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

            public RestApiClient Build()
            {
                return _client;
            }

            public RestApiConnectionBuilder WithWebAddress(string webAddress)
            {
                _client.AppBaseUrl = webAddress;
                return this;
            }

            public RestApiConnectionBuilder WithVersion(string version)
            {
                _client.ApiVersion = version;
                return this;
            }
            public RestApiConnectionBuilder WithApiKey(string apiKey)
            {
                _client.AccessToken = apiKey;
                return this;
            }

            public RestApiConnectionBuilder WithWebAddressAndApiKey(string webAddress, string apiKey)
            {
                _client.AppBaseUrl = webAddress;
                _client.AccessToken = apiKey;
                return this;
            }

            #endregion
        }
    }
}
