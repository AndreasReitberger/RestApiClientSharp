using AndreasReitberger.API.REST.Enums;
using AndreasReitberger.API.REST.Events;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AndreasReitberger.API.REST.Interfaces
{
    public interface IRestApiClient : INotifyPropertyChanged, IDisposable, ICloneable //, IObservable<RestApiClient>
    {

        #region Properties

        #region Instance
        public Guid Id { get; set; }
        public bool UpdateInstance { get; set; }
        public static IRestApiClient? Instance { get; set; }
        #endregion

        #region Auth
        Dictionary<string, IAuthenticationHeader> AuthHeaders { get; set; }
        public bool AuthenticationFailed { get; set; }
        #endregion

        #region States
        public bool IsOnline { get; set; }
        public bool IsConnecting { get; set; }
        public bool IsInitialized { get; set; }
        #endregion

        #region Api
        public string ApiVersion { get; set; }
        public string ApiTargetPath { get; set; }

        #endregion

        #region Proxy

        public bool EnableProxy { get; set; }
        public bool ProxyUserUsesDefaultCredentials { get; set; }
        public bool SecureProxyConnection { get; set; }
        public string ProxyAddress { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUser { get; set; }
        public string? ProxyPassword { get; set; }

        #endregion

        #endregion

        #region EventHandlers

        public event EventHandler? Error;
        public event EventHandler<RestEventArgs>? RestApiError;
        public event EventHandler<RestEventArgs>? RestApiAuthenticationError;
        public event EventHandler<RestEventArgs>? RestApiAuthenticationSucceeded;
        public event EventHandler<JsonConvertEventArgs>? RestJsonConvertError;
        #endregion

        #region Methods

        #region OnlineCheck
        public Task CheckOnlineAsync(int timeout = 10000);
        public Task CheckOnlineAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, int timeout = 10000);
        public Task CheckOnlineAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, CancellationTokenSource? cts = default);
        public Task<bool> CheckIfApiIsValidAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, int timeout = 10000);
        #endregion

        #region Proxy
        public Uri GetProxyUri();
        public WebProxy GetCurrentProxy();
        public void UpdateRestClientInstance();
        public void SetProxy(bool secure, string address, int port, bool enable = true);
        public void SetProxy(bool secure, string address, int port, string user = "", string? password = null, bool enable = true);
        #endregion

        #region Rest
        public Task<IRestApiRequestRespone?> SendRestApiRequestAsync(string? requestTargetUri, Method method, string? command,
            Dictionary<string, IAuthenticationHeader> authHeaders, object? jsonObject = null, CancellationTokenSource? cts = default, Dictionary<string, string>? urlSegments = null
            );

        public Task<IRestApiRequestRespone?> SendMultipartFormDataFileRestApiRequestAsync(string requestTargetUri, Dictionary<string, IAuthenticationHeader> authHeaders, string? fileName, byte[]? file,
            Dictionary<string, string>? parameters = null, string? localFilePath = null,
            string contentType = "multipart/form-data", string fileTargetName = "file", string fileContentType = "application/octet-stream", int timeout = 100000
            );

        #endregion

        #region Misc

        public void CancelCurrentRequests();
        public IAuthenticationHeader? GetAuthHeader(string key);
        public void AddOrUpdateAuthHeader(string key, string value, AuthenticationHeaderTarget target, int order = 0);
        #endregion

        #endregion
    }
}
