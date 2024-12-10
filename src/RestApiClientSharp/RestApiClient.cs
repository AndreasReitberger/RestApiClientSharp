using AndreasReitberger.API.REST.Events;
using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AndreasReitberger.API.REST
{
    // Documentation: https://finnhub.io/docs/api
    public partial class RestApiClient : ObservableObject, IRestApiClient
    {

        #region Instance

        static RestApiClient? _instance = null;
        static readonly object Lock = new();
        public static RestApiClient Instance
        {
            get
            {
                lock (Lock)
                {
                    _instance ??= new RestApiClient();
                }
                return _instance;
            }
            set
            {
                if (_instance == value) return;
                lock (Lock)
                {
                    _instance = value;
                }
            }
        }

        [ObservableProperty]
        Guid id = Guid.Empty;

        [ObservableProperty]
        bool isActive = false;

        [ObservableProperty]
        bool updateInstance = false;

        [ObservableProperty]
        bool isInitialized = false;

        #endregion

        #region Properties

        #region General

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        bool isConnecting = false;
        [JsonIgnore, XmlIgnore]

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        bool isOnline = false;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        bool isAccessTokenValid = false;

        [ObservableProperty]
        int defaultTimeout = 10000;

        [ObservableProperty]
        int minimumCooldown = 0;

        [ObservableProperty]
        int retriesWhenOffline = 2;
        #endregion

        #endregion

        #region Constructor
        public RestApiClient()
        {
            Id = Guid.NewGuid();
            IsInitialized = false;
        }
        public RestApiClient(IAuthenticationHeader authHeader, string tokenName)
        {
            Id = Guid.NewGuid();
            AuthHeaders = new Dictionary<string, IAuthenticationHeader>() { { tokenName, authHeader } };
            IsInitialized = true;
            Instance = this;
        }
        public RestApiClient(IAuthenticationHeader authHeader, string tokenName, string url, string version = "v1")
        {
            Id = Guid.NewGuid();
            AuthHeaders = new Dictionary<string, IAuthenticationHeader>() { { tokenName, authHeader } };
            ApiTargetPath = url;
            ApiVersion = version;
            IsInitialized = true;
            Instance = this;
        }
        #endregion

        #region Public Methods

        #region SetAccessToken
        public void SetAccessToken(string tokenName, IAuthenticationHeader authenticationHeader)
        {
            if (!AuthHeaders.TryAdd(tokenName, authenticationHeader))
            {
                AuthHeaders[tokenName] = authenticationHeader;
            }
            IsInitialized = true;
        }
        #endregion

        #region OnlineCheck
        public virtual async Task CheckOnlineAsync(int timeout = 10000)
        {
            CancellationTokenSource cts = new(timeout);
            await CheckOnlineAsync($"{ApiTargetPath}/{ApiVersion}", AuthHeaders, "", cts).ConfigureAwait(false);
            cts?.Dispose();
        }

        public virtual async Task CheckOnlineAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, int timeout = 10000)
        {
            CancellationTokenSource cts = new(timeout);
            await CheckOnlineAsync(commandBase, authHeaders, command, cts).ConfigureAwait(false);
            cts?.Dispose();
        }

        public virtual async Task CheckOnlineAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, CancellationTokenSource? cts = default)
        {
            if (IsConnecting) return; // Avoid multiple calls
            IsConnecting = true;
            bool isReachable = false;
            try
            {
                string uriString = $"{ApiTargetPath}/{ApiVersion}";
                try
                {
                    // Send a blank api request in order to check if the server is reachable
                    IRestApiRequestRespone? respone = await SendRestApiRequestAsync(
                       requestTargetUri: commandBase,
                       method: Method.Get,
                       command: command,
                       jsonObject: null,
                       authHeaders: authHeaders,
                       cts: cts)
                    .ConfigureAwait(false);
                    isReachable = respone?.IsOnline == true;
                }
                catch (InvalidOperationException iexc)
                {
                    OnError(new UnhandledExceptionEventArgs(iexc, false));
                }
                catch (HttpRequestException rexc)
                {
                    OnError(new UnhandledExceptionEventArgs(rexc, false));
                }
                catch (TaskCanceledException)
                {
                    // Throws an exception on timeout, not actually an error
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
            IsConnecting = false;
            // Avoid offline message for short connection loss
            if (!IsOnline || isReachable || _retries > RetriesWhenOffline)
            {
                // Do not check if the previous state was already offline
                _retries = 0;
                IsOnline = isReachable;
            }
            else
            {
                // Retry with shorter timeout to see if the connection loss is real
                _retries++;
                cts = new(3500);
                await CheckOnlineAsync(commandBase, authHeaders, command, cts).ConfigureAwait(false);
            }
        }

        public virtual async Task<bool> CheckIfApiIsValidAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, int timeout = 10000)
        {
            try
            {
                if (IsOnline)
                {
                    RestApiRequestRespone? respone = await SendRestApiRequestAsync(
                        requestTargetUri: commandBase,
                        method: Method.Get,
                        command: command,
                        authHeaders: authHeaders,
                        cts: new(timeout))
                        .ConfigureAwait(false) as RestApiRequestRespone;
                    if (respone?.HasAuthenticationError is true)
                    {
                        AuthenticationFailed = true;
                        if (respone.EventArgs is RestEventArgs rArgs)
                            OnRestApiAuthenticationError(rArgs);
                    }
                    else
                    {
                        AuthenticationFailed = false;
                        if (respone?.EventArgs is RestEventArgs rArgs)
                            OnRestApiAuthenticationSucceeded(rArgs);
                    }
                    return AuthenticationFailed;
                }
                else
                    return false;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }

        #endregion

        #region Misc
        public virtual void AddOrUpdateAuthHeader(string key, string value, int order = 0)
        {
            if (AuthHeaders?.ContainsKey(key) is true)
            {
                AuthHeaders[key] = new AuthenticationHeader() { Token = value, Order = order };
            }
            else
            {
                AuthHeaders?.Add(key, new AuthenticationHeader() { Token = value, Order = order });
            }
        }

        public virtual IAuthenticationHeader? GetAuthHeader(string key)
        {
            if (AuthHeaders?.ContainsKey(key) is true)
            {
                return AuthHeaders?[key];
            }
            return null;
        }

        public virtual void CancelCurrentRequests()
        {
            try
            {
                if (HttpClient is not null)
                {
                    HttpClient.CancelPendingRequests();
                    UpdateRestClientInstance();
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }
        #endregion

        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public override bool Equals(object? obj)
        {
            if (obj is not RestApiClient item)
                return false;
            return Id.Equals(item.Id);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            // Ordinarily, we release unmanaged resources here;
            // but all are wrapped by safe handles.
            // Release disposable objects.
            if (disposing)
            {

            }
        }
        #endregion

        #region Clone

        public object Clone() => MemberwiseClone();
        
        #endregion
    }
}
