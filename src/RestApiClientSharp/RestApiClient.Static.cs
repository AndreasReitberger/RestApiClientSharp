using AndreasReitberger.API.REST.Interfaces;

namespace AndreasReitberger.API.REST
{
    public partial class RestApiClient : ObservableObject, IRestApiClient
    {
        #region Methods

#if NET6_0_OR_GREATER
        /// <summary>
        /// Checks if a uri string is valid (http / ws).
        /// </summary>
        /// <param name="uri">The url to be checked</param>
        /// <returns>true|false</returns>
        public static bool IsUriValid(string uri)
        {
            return Uri.TryCreate(uri, UriKind.Absolute, out Uri? uriResult)
             && (
                 uriResult.Scheme == Uri.UriSchemeHttp || 
                 uriResult.Scheme == Uri.UriSchemeHttps || 
                 uriResult.Scheme == Uri.UriSchemeWs || 
                 uriResult.Scheme == Uri.UriSchemeWss
             );
        }
#endif

        #endregion
    }
}
