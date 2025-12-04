using AndreasReitberger.API.REST.Interfaces;
using AndreasReitberger.API.REST.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Threading.RateLimiting;

namespace AndreasReitberger.API.REST
{
    public partial class RestApiClient : ObservableObject, IRestApiClient
    {

        #region Variables
        protected int _retries = 0;
        #endregion

        #region Properties

        #region Clients
        
        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial RestClient? RestClient { get; set; }

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial HttpClient? HttpClient { get; set; }
        
        /*
        [JsonIgnore, XmlIgnore]
        protected RestClient? _restClient;

        [JsonIgnore, XmlIgnore]
        protected HttpClient? _httpClient;
        */
#if !NETFRAMEWORK
        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial RateLimitedHandler? RateLimitedHandler { get; set; }

        public static RateLimiter DefaultLimiter = new TokenBucketRateLimiter(new()
        {
            TokenLimit = 5,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = int.MaxValue,
            ReplenishmentPeriod = TimeSpan.FromSeconds(30),
            TokensPerPeriod = 5,
            AutoReplenishment = true,
        });

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial RateLimiter? Limiter { get; set; }
        partial void OnLimiterChanged(RateLimiter? value) => UpdateRestClientInstance();
#endif

        [ObservableProperty]
        public partial bool UseRateLimiter { get; set; } = false;
        partial void OnUseRateLimiterChanged(bool value) => UpdateRestClientInstance();

        [ObservableProperty]
        public partial bool AuthenticationFailed { get; set; } = false;

        [ObservableProperty]
        public partial bool UpdatingClients { get; set; } = false;

        [ObservableProperty]
        public partial string ApiTargetPath { get; set; } = string.Empty;
        partial void OnApiTargetPathChanged(string value) => UpdateRestClientInstance();

        [ObservableProperty]
        public partial string ApiVersion { get; set; } = "v1";
        partial void OnApiVersionChanged(string value) => UpdateRestClientInstance();

        [ObservableProperty]
        public partial ObservableCollection<RestHeader> DefaultHeaders { get; set; } = [];
        partial void OnDefaultHeadersChanged(ObservableCollection<RestHeader> value)
        {
            DefaultHeaders?.CollectionChanged -= DefaultHeaders_CollectionChanged;
            DefaultHeaders?.CollectionChanged += DefaultHeaders_CollectionChanged;
            UpdateRestClientInstance();
        }
        #endregion

        #endregion

        #region Methods

        public virtual void UpdateRestClientInstance()
        {
            if (string.IsNullOrEmpty(ApiTargetPath) || ApiVersion is null || UpdatingClients)
            {
                return;
            }
            UpdatingClients = true;
#if !NETFRAMEWORK
            Limiter ??= DefaultLimiter;
#endif
            Uri target = new(ApiTargetPath);
            if (!string.IsNullOrEmpty(ApiVersion))
                target = new Uri(target, ApiVersion);

            RestClientOptions options = new(target)
            {
                ThrowOnAnyError = false,
                Timeout = TimeSpan.FromSeconds(DefaultTimeout),
                CookieContainer = new CookieContainer(),
            };
            HttpClient?.Dispose();
            HttpClient = null;
            if (EnableProxy && !string.IsNullOrEmpty(ProxyAddress))
            {
                HttpClientHandler httpHandler = new()
                {
                    UseProxy = true,
                    Proxy = GetCurrentProxy(),
                    AllowAutoRedirect = true,
                };
                HttpClient = new(handler: httpHandler, disposeHandler: true);
            }
            else
            {
                HttpClient =
#if !NETFRAMEWORK
                    !UseRateLimiter ? new() : new(new RateLimitedHandler(Limiter));
#else
                    new();
#endif
            }
            RestClient?.Dispose();
            RestClient = null;
            RestClient = new(httpClient: HttpClient, disposeHttpClient: false, options: options);
            if (DefaultHeaders.Count > 0)
            {
                foreach (RestHeader header in DefaultHeaders)
                {
                    RestClient.AddDefaultHeader(header.Name, header.Value);
                }
            }
            UpdatingClients = false;
        }
        #endregion
    }
}
