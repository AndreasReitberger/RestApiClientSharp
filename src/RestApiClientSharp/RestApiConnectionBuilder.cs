using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.RateLimiting;

namespace AndreasReitberger.API.REST
{
    public partial class RestApiClient
    {
        public class RestApiConnectionBuilder
        {
            #region Instance
            readonly RestApiClient _client = new();
            #endregion

            #region Methods

            public RestApiClient Build() => _client;
            
            public RestApiConnectionBuilder WithWebAddress(string webAddress)
            {
                _client.ApiTargetPath = webAddress;
                return this;
            }

            public RestApiConnectionBuilder WithVersion(string version)
            {
                _client.ApiVersion = version;
                return this;
            }

            public RestApiConnectionBuilder WithApiKey(string tokenName, IAuthenticationHeader authHeader)
            {
                _client.AuthHeaders = new Dictionary<string, IAuthenticationHeader>() { { tokenName, authHeader } };
                return this;
            }

            public RestApiConnectionBuilder WithWebAddressAndApiKey(string webAddress, string tokenName, IAuthenticationHeader authHeader)
            {
                _client.ApiTargetPath = webAddress;
                _client.AuthHeaders = new Dictionary<string, IAuthenticationHeader>() { { tokenName, authHeader } };
                return this;
            }

            /// <summary>
            /// Set the rate limiter for the rest api connection
            /// </summary>
            /// <param name="autoReplenishment"></param>
            /// <param name="tokenLimit">Maximum number of tokens that can be in the bucket at any time</param>
            /// <param name="tokensPerPeriod">Maximum number of tokens to be restored in each replenishment</param>
            /// <param name="replenishmentPeriod">Enable auto replenishment</param>
            /// <param name="queueLimit">Size of the queue</param>
            /// <returns><c>RestApiConnectionBuilder</c></returns>
            public RestApiConnectionBuilder WithRateLimiter(bool autoReplenishment, int tokenLimit, int tokensPerPeriod, double replenishmentPeriod, int queueLimit = int.MaxValue)
            {
                _client.Limiter = new TokenBucketRateLimiter(new()
                {
                    TokenLimit = tokenLimit,
                    TokensPerPeriod = tokensPerPeriod,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = queueLimit,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(replenishmentPeriod),
                    AutoReplenishment = autoReplenishment,
                });
                return this;
            }
            /// <summary>
            /// Set the timeout for the connection in ms (default is 10000 ms)
            /// </summary>
            /// <param name="timeout">The timeout in ms</param>
            /// <returns><c>RestApiConnectionBuilder</c></returns>
            public RestApiConnectionBuilder WithTimeout(int timeout = 10000)
            {
                _client.DefaultTimeout = timeout;
                return this;
            }

            /// <summary>
            /// Sets the WebSocket address for the connection
            /// </summary>
            /// <param name="webSocketAddress">The full web address for the WebSocket</param>
            /// <param name="pingCommand">The command sent on each ping action</param>
            /// <param name="pingInterval">The keep alive interval in seconds. 0 disables it</param>
            /// <param name="enablePing">Enables the custom ping command sending</param>
            /// <returns><c>RestApiConnectionBuilder</c></returns>
            public RestApiConnectionBuilder WithWebSocket(string webSocketAddress, Dictionary<string, IAuthenticationHeader>? authentication = null, string pingCommand = "", int pingInterval = 0, bool enablePing = true)
            {
                if (authentication is not null)
                    foreach (KeyValuePair<string, IAuthenticationHeader> item in authentication)
                    {
                        _client.AuthHeaders.Add(item.Key, item.Value);
                    }
                _client.EnablePing = enablePing;
                _client.PingCommand = pingCommand;
                _client.PingInterval = pingInterval;
                _client.WebSocketTargetUri = webSocketAddress;
                return this;
            }

            /// <summary>
            /// Sets the WebSocket address for the connection
            /// </summary>
            /// <param name="webSocketAddress">The full web address for the WebSocket</param>
            /// <param name="pingCommand">The command object parsed to JSON and sent on each ping action</param>
            /// <param name="pingInterval">The keep alive interval in seconds. 0 disables it</param>
            /// <param name="enablePing">Enables the custom ping command sending</param>
            /// <returns><c>RestApiConnectionBuilder</c></returns>
            public RestApiConnectionBuilder WithWebSocket(string webSocketAddress, object pingCommand, Dictionary<string, IAuthenticationHeader>? authentication = null, int pingInterval = 0, bool enablePing = true)
                => WithWebSocket(webSocketAddress, authentication, JsonConvert.SerializeObject(pingCommand), pingInterval, enablePing);
            

            #endregion
        }
    }
}
