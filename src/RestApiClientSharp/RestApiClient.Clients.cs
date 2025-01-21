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
        [property: JsonIgnore, XmlIgnore]
        RestClient? restClient;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        HttpClient? httpClient;

#if !NETFRAMEWORK
        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        RateLimitedHandler? rateLimitedHandler;

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
        [property: JsonIgnore, XmlIgnore]
        RateLimiter? limiter;
        partial void OnLimiterChanged(RateLimiter? value) => UpdateRestClientInstance();
#endif
        [ObservableProperty]
        bool authenticationFailed = false;

        [ObservableProperty]
        bool updatingClients = false;

        [ObservableProperty]
        string apiTargetPath = string.Empty;
        partial void OnApiTargetPathChanged(string value) => UpdateRestClientInstance();

        [ObservableProperty]
        string apiVersion = "v1";
        partial void OnApiVersionChanged(string value) => UpdateRestClientInstance();
        #endregion

        #endregion
    }
}
