using AndreasReitberger.API.REST.Interfaces;
using System.Collections.Generic;

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

            #endregion
        }
    }
}
