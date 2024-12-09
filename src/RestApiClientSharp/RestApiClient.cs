using AndreasReitberger.API.Finnhub.Utilities;
using AndreasReitberger.API.REST.Interfaces;
using AndreasReitberger.Core.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
        bool isActive = false;

        [ObservableProperty]
        bool isInitialized = false;

        #endregion

        #region Properties

        #region Clients

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        RestClient? restClient;
        //partial void OnRestClientChanged(RestClient? value) => UpdateRestClientInstance();

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        HttpClient? httpClient;
        //partial void OnHttpClientChanged(HttpClient? value) => UpdateRestClientInstance();

#if !NETFRAMEWORK
        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        RateLimitedHandler? rateLimitedHandler;

        public static RateLimiter DefaultLimiter = new TokenBucketRateLimiter(new()
        {
            TokenLimit = 2,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = int.MaxValue,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            TokensPerPeriod = 1,
            AutoReplenishment = true,
        });

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        RateLimiter? limiter;
        partial void OnLimiterChanged(RateLimiter? value) => UpdateRestClientInstance();
#endif
        [ObservableProperty]
        bool updatingClients = false;

        [ObservableProperty]
        string appBaseUrl = "";
        partial void OnAppBaseUrlChanged(string value) => UpdateRestClientInstance();

        [ObservableProperty]
        string apiVersion = "v1";
        partial void OnApiVersionChanged(string value) => UpdateRestClientInstance();
        #endregion

        #region SerializerSettings

        [ObservableProperty]
        JsonSerializerSettings jsonSerializerSettings = new()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            },
            DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffK"
        };

        #endregion

        #region General

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        string? accessToken = null;

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

        #endregion

        #region Proxy

        [ObservableProperty]
        bool enableProxy = false;
        partial void OnEnableProxyChanged(bool value) => UpdateRestClientInstance();

        [ObservableProperty]
        bool proxyUseDefaultCredentials = true;
        partial void OnProxyUseDefaultCredentialsChanged(bool value) => UpdateRestClientInstance();

        [ObservableProperty]
        bool secureProxyConnection = true;
        partial void OnSecureProxyConnectionChanged(bool value) => UpdateRestClientInstance();

        [ObservableProperty]
        string proxyAddress = string.Empty;
        partial void OnProxyAddressChanged(string value) => UpdateRestClientInstance();

        [ObservableProperty]
        int proxyPort = 443;
        partial void OnProxyPortChanged(int value) => UpdateRestClientInstance();

        [ObservableProperty]
        string proxyUser = string.Empty;
        partial void OnProxyUserChanged(string value) => UpdateRestClientInstance();

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        string? proxyPassword;
        partial void OnProxyPasswordChanged(string? value) => UpdateRestClientInstance();

        #endregion

        #endregion

        #region EventHandlers
        public event EventHandler? Error;
        protected virtual void OnError()
        {
            Error?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnError(UnhandledExceptionEventArgs e)
        {
            Error?.Invoke(this, e);
        }
        #endregion

        #region Constructor
        public RestApiClient()
        {
            IsInitialized = false;
        }
        public RestApiClient(string accessToken)
        {
            AccessToken = accessToken;
            IsInitialized = true;
            Instance = this;
        }
        public RestApiClient(string accessToken, string url, string version = "v1")
        {
            AccessToken = accessToken;
            AppBaseUrl = url;
            ApiVersion = version;
            IsInitialized = true;
            Instance = this;
        }
        #endregion

        #region Methods
        void UpdateRestClientInstance()
        {
            if (string.IsNullOrEmpty(AppBaseUrl) || string.IsNullOrEmpty(ApiVersion) || UpdatingClients)
            {
                return;
            }
            UpdatingClients = true;
#if !NETFRAMEWORK
            Limiter ??= DefaultLimiter;
#endif
            RestClientOptions options = new($"{AppBaseUrl}{ApiVersion}/")
            {
                ThrowOnAnyError = true,
                Timeout = TimeSpan.FromSeconds(DefaultTimeout / 1000),
            };
            if (EnableProxy && !string.IsNullOrEmpty(ProxyAddress))
            {
                HttpClientHandler httpHandler = new()
                {
                    UseProxy = true,
                    Proxy = GetCurrentProxy(),
                    AllowAutoRedirect = true,
                };

                HttpClient = new(handler: httpHandler, disposeHandler: true);
                //RestClient = new(httpClient: HttpClient, options: options);
            }
            else
            {
                HttpClient =
#if !NETFRAMEWORK
                    new(new RateLimitedHandler(Limiter));
#else
                    new();
#endif
                //RestClient = new(baseUrl: $"{AppBaseUrl}{ApiVersion}/");
            }
            RestClient = new(httpClient: HttpClient, options: options);
            UpdatingClients = false;
        }

        public async Task<T?> CallApiAsync<T>(
            string command, 
            Method method = Method.Get, 
            string body = "", 
            Dictionary<string, string>? headers = null,
            Dictionary<string, string>? urlSegments = null,
            CancellationTokenSource? cts = default) where T : class
        {
            if (cts == default)
            {
                cts = new(DefaultTimeout);
            }
            if (RestClient is null)
            {
                UpdateRestClientInstance();
            }

            RestRequest request = new(command, method)
            {
                RequestFormat = DataFormat.Json
            };
            if (headers is not null)
            {
                foreach (KeyValuePair<string, string> item in headers)
                {
                    request.AddHeader(item.Key, item.Value);
                }
            }
            if (urlSegments != null)
            {
                foreach (KeyValuePair<string, string> pair in urlSegments)
                {
                    request.AddParameter(pair.Key, pair.Value, ParameterType.QueryString);
                }
            }
            /*
            else if (!string.IsNullOrEmpty(AccessToken))
                request.AddHeader("Authorization", $"Bearer {AccessToken}");
            */
            if (!string.IsNullOrEmpty(body))
            {
                request.AddJsonBody(body);
            }
            if (RestClient is not null)
            {
                RestResponse? response = await RestClient.ExecuteAsync(request, cts.Token).ConfigureAwait(false);

                if ((response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created) &&
                    response.ResponseStatus == ResponseStatus.Completed)
                {
                    if (typeof(T) == typeof(byte[]))
                    {
                        return response.RawBytes as T;
                    }
                    else if (typeof(T) == typeof(string))
                    {
                        return response.Content as T;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unsupported return type: {typeof(T).Name}");
                    }
                }
                else
                {
                    string errorMessage = $"Request failed with status code {(int)response.StatusCode} ({response.StatusCode}).";

                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        errorMessage += $" Response content: {response.Content}";
                    }

                    throw new HttpRequestException(errorMessage);
                }
            }
            return default;
        }

        #endregion

        #region Public Methods

        #region Proxy
        Uri GetProxyUri() =>
            ProxyAddress.StartsWith("http://") || ProxyAddress.StartsWith("https://") ? new Uri($"{ProxyAddress}:{ProxyPort}") : new Uri($"{(SecureProxyConnection ? "https" : "http")}://{ProxyAddress}:{ProxyPort}");

        WebProxy GetCurrentProxy()
        {
            WebProxy proxy = new()
            {
                Address = GetProxyUri(),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = ProxyUseDefaultCredentials,
            };
            if (ProxyUseDefaultCredentials && !string.IsNullOrEmpty(ProxyUser))
            {
                proxy.Credentials = new NetworkCredential(ProxyUser, ProxyPassword);
            }
            else
            {
                proxy.UseDefaultCredentials = ProxyUseDefaultCredentials;
            }
            return proxy;
        }
        #endregion

        #region SetAccessToken
        public void SetAccessToken(string token)
        {
            AccessToken = token;
            IsInitialized = true;
        }
        #endregion

        #region OnlineCheck
        public async Task CheckOnlineAsync(int timeout = 10000)
        {
            if (IsConnecting) return; // Avoid multiple calls
            IsConnecting = true;
            bool isReachable = false;
            try
            {
                // Cancel after timeout
                CancellationTokenSource cts = new(new TimeSpan(0, 0, 0, 0, timeout));
                string uriString = $"{AppBaseUrl}";
                try
                {
                    if (HttpClient is not null)
                    {
                        HttpResponseMessage? response = await HttpClient.GetAsync(uriString, cts.Token).ConfigureAwait(false);
                        response.EnsureSuccessStatusCode();
                        if (response != null)
                        {
                            isReachable = response.IsSuccessStatusCode;
                        }
                    }
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
                    // Throws exception on timeout, not actually an error
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
            IsConnecting = false;
            IsOnline = isReachable;
            //return isReachable;
        }
        #endregion

        #endregion
    }
}
