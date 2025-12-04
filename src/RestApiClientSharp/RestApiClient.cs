using AndreasReitberger.API.REST.Enums;
using AndreasReitberger.API.REST.Events;
using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AndreasReitberger.API.REST
{
    public partial class RestApiClient : ObservableObject, IRestApiClient
    {

        #region Variables
        private bool _isDisposed;
        const string defaultApiVersion = "v1";
        #endregion

        #region Properties

        #region General

        [ObservableProperty]
        public partial Guid Id { get; set; } = Guid.Empty;

        [ObservableProperty]
        public partial bool IsActive { get; set; } = false;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool IsConnecting { get; set; } = false;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool IsOnline { get; set; } = false;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool IsAccessTokenValid { get; set; } = false;

        [ObservableProperty]
        public partial int DefaultTimeout { get; set; } = 10;
        partial void OnDefaultTimeoutChanged(int value)
        {
            if (value > 1000)
                throw new ArgumentOutOfRangeException(nameof(DefaultTimeout), "The property has been changed from ms to seconds! Provide a value less than 1000!");
            UpdateRestClientInstance();
        }

        [ObservableProperty]
        public partial int MinimumCooldown { get; set; } = 0;

        [ObservableProperty]
        public partial int RetriesWhenOffline { get; set; } = 2;
        #endregion

        #endregion

        #region Ctor
        public RestApiClient()
        {
            Id = Guid.NewGuid();
            DefaultHeaders?.CollectionChanged += DefaultHeaders_CollectionChanged;
        }
        public RestApiClient(string url, string version = defaultApiVersion)
        {
            Id = Guid.NewGuid();
            AuthHeaders = [];
            ApiTargetPath = url;
            ApiVersion = version;
            DefaultHeaders?.CollectionChanged += DefaultHeaders_CollectionChanged;
        }

        public RestApiClient(IAuthenticationHeader authHeader, string tokenName)
        {
            Id = Guid.NewGuid();
            AuthHeaders = new Dictionary<string, IAuthenticationHeader>() { { tokenName, authHeader } };
            DefaultHeaders?.CollectionChanged += DefaultHeaders_CollectionChanged;
        }
        public RestApiClient(IAuthenticationHeader authHeader, string tokenName, string url, string version = defaultApiVersion)
        {
            Id = Guid.NewGuid();
            AuthHeaders = new Dictionary<string, IAuthenticationHeader>() { { tokenName, authHeader } };
            ApiTargetPath = url;
            ApiVersion = version;
            DefaultHeaders?.CollectionChanged += DefaultHeaders_CollectionChanged;
        }

        private void DefaultHeaders_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateRestClientInstance();
        #endregion

        #region Dtor
        ~RestApiClient()
        {
            DefaultHeaders?.CollectionChanged -= DefaultHeaders_CollectionChanged;
            Dispose();
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
        }
        #endregion

        #region OnlineCheck
        public virtual async Task CheckOnlineAsync(int timeout = 10)
        {
            CancellationTokenSource? cts = new(TimeSpan.FromSeconds(timeout));
            await CheckOnlineAsync($"{ApiTargetPath}/{ApiVersion}", AuthHeaders, "", cts).ConfigureAwait(false);
            cts?.Dispose();
        }

        public virtual async Task CheckOnlineAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, int timeout = 10)
        {
            CancellationTokenSource? cts = new(TimeSpan.FromSeconds(timeout));
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
                //string uriString = $"{ApiTargetPath}/{ApiVersion}";
                try
                {
                    // Send a blank api request in order to check if the server is reachable
                    IRestApiRequestRespone? respone = await SendRestApiRequestAsync(
                       requestTargetUri: commandBase,
                       method: Method.Get,
                       command: command,
                       body: null,
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
                catch (TaskCanceledException texp)
                {
                    // Throws an exception on timeout, not actually an error
                    OnTaskCanceled(new()
                    {
                        Message = texp.Message,
                        Uri = new(commandBase),
                        Source = nameof(CheckOnlineAsync),
                        CancelationRequested = cts?.IsCancellationRequested ?? false,
                        Exception = texp
                    });
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
                cts?.Dispose();
                cts = new(TimeSpan.FromSeconds(3));
                await CheckOnlineAsync(commandBase, authHeaders, command, cts).ConfigureAwait(false);
            }
        }

        public virtual async Task<bool> CheckIfApiIsValidAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, int timeout = 10)
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
                        cts: new(TimeSpan.FromSeconds(timeout)))
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
        public virtual void AddOrUpdateAuthHeader(string key, string value, AuthenticationHeaderTarget target, int order = 0, AuthenticationTypeTarget type = AuthenticationTypeTarget.Both)
        {
            if (AuthHeaders?.ContainsKey(key) is true)
            {
                AuthHeaders[key] = new AuthenticationHeader() { Token = value, Order = order, Target = target, Type = type };
            }
            else
            {
                AuthHeaders?.Add(key, new AuthenticationHeader() { Token = value, Order = order, Target = target, Type = type });
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
        public override int GetHashCode() => Id.GetHashCode();
        
        #endregion

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            // Ordinarily, we release unmanaged resources here;
            // but all are wrapped by safe handles.
            // Release disposable objects.
            if (disposing)
            {
                RestClient?.Dispose();
                RestClient = null;
                HttpClient?.Dispose();
                HttpClient = null;
            }
        }
        #endregion

        #region Clone

        public object Clone() => MemberwiseClone();
        
        #endregion
    }
}
