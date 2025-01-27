using AndreasReitberger.API.REST.Interfaces;
using AndreasReitberger.API.REST.Utilities;
using Newtonsoft.Json;
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
        [JsonIgnore, XmlIgnore]
        public partial RestClient? RestClient { get; set; }

        [ObservableProperty]
        [JsonIgnore, XmlIgnore]
        public partial HttpClient? HttpClient { get; set; }

#if !NETFRAMEWORK
        [ObservableProperty]
        [JsonIgnore, XmlIgnore]
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
        [JsonIgnore, XmlIgnore]
        public partial RateLimiter? Limiter { get; set; }
        partial void OnLimiterChanged(RateLimiter? value) => UpdateRestClientInstance();
#endif

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
        #endregion

        #endregion
    }
}
