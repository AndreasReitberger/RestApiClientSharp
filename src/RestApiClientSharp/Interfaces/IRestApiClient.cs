using AndreasReitberger.API.REST.Enums;
using AndreasReitberger.API.REST.Events;
using AndreasReitberger.API.REST.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Websocket.Client;

namespace AndreasReitberger.API.REST.Interfaces
{
    public interface IRestApiClient : INotifyPropertyChanged, IDisposable, ICloneable //, IObservable<RestApiClient>
    {

        #region Properties

        #region General
        public Guid Id { get; set; }
        public int DefaultTimeout { get; set; }
        public int MinimumCooldown { get; set; }
        public int RetriesWhenOffline { get; set; }
        #endregion

        #region Auth
        Dictionary<string, IAuthenticationHeader> AuthHeaders { get; set; }
        public bool AuthenticationFailed { get; set; }
        #endregion

        #region States
        public bool IsOnline { get; set; }
        public bool IsConnecting { get; set; }
        public bool IsAccessTokenValid { get; set; }
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

        #region REST
        public RestClient? RestClient { get; set; }
        public HttpClient? HttpClient { get; set; }
        public ObservableCollection<RestHeader> DefaultHeaders { get; set; }
        public RateLimitedHandler? RateLimitedHandler { get; set; }
        public static RateLimiter? DefaultLimiter { get; }
        public RateLimiter? Limiter { get; set; }
        public bool UpdatingClients { get; set; }
        public bool UseRateLimiter { get; set; }
        #endregion

        #region WebSocket
        public string PingCommand { get; set; }
        public long PingCounter { get; set; }
        public int PingInterval { get; set; }
        public int OnRefreshInterval { get; set; }
        public string WebSocketTargetUri { get; set; }
        public long LastPingTimestamp { get; set; }
        public long LastRefreshTimestamp { get; set; }
        public Func<Task>? OnRefresh { get; set; }
        #endregion

        #endregion

        #region EventHandlers

        public event EventHandler? Error;
        public event EventHandler<RestEventArgs>? RestApiError;
        public event EventHandler<RestEventArgs>? RestApiAuthenticationError;
        public event EventHandler<RestEventArgs>? RestApiAuthenticationSucceeded;
        public event EventHandler<JsonConvertEventArgs>? RestJsonConvertError;

        public event EventHandler<WebsocketPingSentEventArgs>? WebSocketPingSent;
        public event EventHandler<WebsocketEventArgs>? WebSocketConnected;
        public event EventHandler<WebsocketEventArgs>? WebSocketDisconnected;
        public event EventHandler<WebsocketEventArgs>? WebSocketError;
        public event EventHandler<WebsocketEventArgs>? WebSocketMessageReceived;
        public event EventHandler<WebsocketEventArgs>? WebSocketDataReceived;

        public event EventHandler<LoginRequiredEventArgs>? LoginResultReceived;
        public event EventHandler<ListeningChangedEventArgs>? ListeningChanged;
        public event EventHandler<SessionChangedEventArgs>? SessionChanged;
        #endregion

        #region Methods

        #region OnlineCheck
        public Task CheckOnlineAsync(int timeout = 10);
        public Task CheckOnlineAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, int timeout = 10);
        public Task CheckOnlineAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, CancellationTokenSource? cts = default);
        public Task<bool> CheckIfApiIsValidAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, int timeout = 10);
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
            Dictionary<string, IAuthenticationHeader> authHeaders, object? jsonObject = null, RestBodyTarget target = RestBodyTarget.Json, CancellationTokenSource? cts = default, Dictionary<string, string>? urlSegments = null
            //string? contentType = null, string? accept = null
            );

        public Task<IRestApiRequestRespone?> SendMultipartFormDataFileRestApiRequestAsync(string requestTargetUri, Dictionary<string, IAuthenticationHeader> authHeaders, string? fileName, byte[]? file,
            Dictionary<string, string>? parameters = null, string? localFilePath = null,
            string contentType = "multipart/form-data", string fileTargetName = "file", string fileContentType = "application/octet-stream", int timeout = 10
            );

        #endregion

        #region WebSocket
        public string BuildPingCommand(object? data = null);
        public Task StartListeningAsync(bool stopActiveListening = false, string[]? commandsOnConnect = null);
        public Task StartListeningAsync(string target, bool stopActiveListening = false, Func<Task>? refreshFunctions = null, string[]? commandsOnConnect = null);
        public Task StopListeningAsync();
        public Task ConnectWebSocketAsync(string target, string commandOnConnect, CookieContainer? cookies = null);
        public Task ConnectWebSocketAsync(string target, string[]? commandsOnConnect = null, CookieContainer? cookies = null);
        public Task DisconnectWebSocketAsync();
        public Task SendWebSocketCommandAsync(string command);
        public Task SendPingAsync(object? pingObject = null);
        public Task UpdateWebSocketAsync(Func<Task>? refreshFunctions, string[]? commandsOnConnect = null);
        public WebsocketClient? GetWebSocketClient(CookieContainer? cookies = null);
        #endregion

        #region Misc

        public void CancelCurrentRequests();
        public IAuthenticationHeader? GetAuthHeader(string key);
        public void AddOrUpdateAuthHeader(string key, string value, AuthenticationHeaderTarget target, int order = 0, AuthenticationTypeTarget type = AuthenticationTypeTarget.Both);
        #endregion

        #endregion
    }
}
