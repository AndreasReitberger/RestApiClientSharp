using AndreasReitberger.API.REST.Interfaces;
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

            /// <summary>
            /// Builds the <c>RestApiClient</c> instance with the specified configuration. After calling this method, the builder should not be used anymore, as it does not create a new instance of the client but returns the same one.
            /// </summary>
            /// <returns><c>RestApiClient</c></returns>
            public RestApiClient Build() => _client;

            /// <summary>
            /// Sets the web address for the connection. This is the base address to which the method paths will be appended when making requests. 
            /// For instance, if the web address is `https://api.example.com` and a method has the path `get-data`, the full request URI will be `https://api.example.com/get-data` 
            /// (or `https://api.example.com/version/get-data` if an API version is set). 
            /// If not set, the client will throw an exception when trying to make a request, as it won't have a valid URI to send the request to.
            /// </summary>
            /// <param name="webAddress">The web address for the connection</param>
            /// <returns>The updated connection builder</returns>
            public RestApiConnectionBuilder WithWebAddress(string webAddress)
            {
                _client.ApiTargetPath = webAddress;
                return this;
            }

            /// <summary>
            /// Sets the API version for the connection. This is used to build the full request URI, which is usually in the format `webAddress/version/method`. If not set, the version will be omitted from the request URI.
            /// </summary>
            /// <param name="version">The API version</param>
            /// <returns><c>RestApiConnectionBuilder</c></returns>
            public RestApiConnectionBuilder WithVersion(string version)
            {
                _client.ApiVersion = version;
                return this;
            }

            /// <summary>
            /// Sets the API key for the connection. The token name is the name of the header in which the token will be sent (for instance `apikey`).
            /// The `AuthenticationHeader` contains the token and other information about how to send it.
            /// </summary>
            /// <param name="tokenName">The name of the API key</param>
            /// <param name="authHeader">The authentication header for the API key</param>
            /// <returns><c>RestApiConnectionBuilder</c></returns>
            public RestApiConnectionBuilder WithApiKey(string tokenName, IAuthenticationHeader authHeader)
            {
                _client.AuthHeaders = new Dictionary<string, IAuthenticationHeader>() { { tokenName, authHeader } };
                return this;
            }

            /// <summary>
            /// Sets the web address and the api key for the connection. This is a shortcut for setting both values at once.
            /// </summary>
            /// <param name="webAddress">The rest API web address</param>
            /// <param name="tokenName">The name for the API key</param>
            /// <param name="authHeader">The authentication header for the API key</param>
            /// <returns><c>RestApiConnectionBuilder</c></returns>
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
                _client.UseRateLimiter = true;
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
            /// Set the timeout for the connection in s (default is 10 m)
            /// </summary>
            /// <param name="timeout">The timeout in seconds</param>
            /// <returns><c>RestApiConnectionBuilder</c></returns>
            public RestApiConnectionBuilder WithTimeout(int timeout = 10)
            {
                _client.DefaultTimeout = timeout;
                return this;
            }

            /// <summary>
            /// Sets the WebSocket address for the connection
            /// </summary>
            /// <param name="webSocketAddress">The full web address for the WebSocket</param>
            /// <param name="tokenName">The name for the token (for instance `apikey`)</param>
            /// <param name="authentication">The `AuthenticationHeader` for the token</param>
            /// <param name="pingCommand">The command sent on each ping action</param>
            /// <param name="pingInterval">The keep alive interval in seconds. 0 disables it</param>
            /// <param name="enablePing">Enables the custom ping command sending</param>
            /// <returns><c>RestApiConnectionBuilder</c></returns>
            public RestApiConnectionBuilder WithWebSocket(string webSocketAddress, string? tokenName = null, IAuthenticationHeader? authentication = null, string pingCommand = "", int pingInterval = 5, bool enablePing = true)
            {
                if (!string.IsNullOrEmpty(tokenName) && authentication is not null)
                    _client.AuthHeaders.Add(tokenName, authentication);
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
            public RestApiConnectionBuilder WithWebSocket(string webSocketAddress, string pingCommand, string? tokenName = null, IAuthenticationHeader? authentication = null, int pingInterval = 0, bool enablePing = true)
                => WithWebSocket(webSocketAddress, tokenName, authentication, pingCommand, pingInterval, enablePing);

            /// <summary>
            /// Sets default headers for the connection. These headers will be sent with every request.
            /// </summary>
            /// <param name="headers">The headers to set</param>
            /// <returns><c>RestApiConnectionBuilder</c></returns>
            public RestApiConnectionBuilder WithDefaultHeaders(params RestHeader[] headers)
            {
                _client.DefaultHeaders = [.. headers];
                return this;
            }
            /// <summary>
            /// Sets the <c>JsonSerializerContext</c> for the rest api methods. If not set, the default options will be used.
            /// </summary>
            /// <param name="serializerContext">The json serializer context</param>
            /// <returns><c>RestApiConnectionBuilder</c></returns>
            public RestApiConnectionBuilder WithJsonSerializerContext(JsonSerializerContext serializerContext)
            {
                _client.JsonSerializerContext = serializerContext;
                return this;
            }
            #endregion
        }
    }
}
