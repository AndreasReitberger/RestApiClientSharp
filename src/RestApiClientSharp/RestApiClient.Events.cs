using AndreasReitberger.API.REST.Events;
using AndreasReitberger.API.REST.Interfaces;
using System.IO;

namespace AndreasReitberger.API.REST
{
    public partial class RestApiClient : ObservableObject, IRestApiClient
    {

        #region General

        public event EventHandler? Error;
        [ObservableProperty]
        public partial bool ReThrowOnError { get; set; } = false;

        protected virtual void OnError()
        {
            Error?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnError(UnhandledExceptionEventArgs e)
        {
            Error?.Invoke(this, e);
            if (ReThrowOnError)
            {
                throw (Exception)e.ExceptionObject;
            }
        }
        protected virtual void OnError(JsonConvertEventArgs e)
        {
            Error?.Invoke(this, e);
            if (ReThrowOnError)
            {
                throw e.Exception ?? throw new Exception(e.Message);
            }
        }
        public event EventHandler<RestEventArgs>? RestApiError;
        protected virtual void OnRestApiError(RestEventArgs e)
        {
            RestApiError?.Invoke(this, e);
            if (ReThrowOnError)
            {
                throw e.Exception ?? throw new Exception(e.Message);
            }
        }

        public event EventHandler<RestEventArgs>? RestApiAuthenticationError;
        protected virtual void OnRestApiAuthenticationError(RestEventArgs e)
        {
            RestApiAuthenticationError?.Invoke(this, e);
            if (ReThrowOnError)
            {
                throw e.Exception ?? throw new Exception(e.Message);
            }
        }
        public event EventHandler<RestEventArgs>? RestApiAuthenticationSucceeded;
        protected virtual void OnRestApiAuthenticationSucceeded(RestEventArgs e)
        {
            RestApiAuthenticationSucceeded?.Invoke(this, e);
            if (ReThrowOnError)
            {
                throw e.Exception ?? throw new Exception(e.Message);
            }
        }

        public event EventHandler<JsonConvertEventArgs>? RestJsonConvertError;
        protected virtual void OnRestJsonConvertError(JsonConvertEventArgs e)
        {
            RestJsonConvertError?.Invoke(this, e);
            if (ReThrowOnError)
            {
                throw e.Exception ?? throw new Exception(e.Message);
            }
        }
        #endregion

        #region WebSocket

        public event EventHandler<RestEventArgs>? WebSocketConnected;
        protected virtual void OnWebSocketConnected(RestEventArgs e)
        {
            WebSocketConnected?.Invoke(this, e);
        }

        public event EventHandler<RestEventArgs>? WebSocketDisconnected;
        protected virtual void OnWebSocketDisconnected(RestEventArgs e)
        {
            WebSocketDisconnected?.Invoke(this, e);
        }

        public event EventHandler<ErrorEventArgs>? WebSocketError;
        protected virtual void OnWebSocketError(ErrorEventArgs e)
        {
            WebSocketError?.Invoke(this, e);
        }

        public event EventHandler<WebsocketEventArgs>? WebSocketMessageReceived;
        protected virtual void OnWebSocketMessageReceived(WebsocketEventArgs e)
        {
            WebSocketMessageReceived?.Invoke(this, e);
        }

        public event EventHandler<WebsocketEventArgs>? WebSocketDataReceived;
        protected virtual void OnWebSocketDataReceived(WebsocketEventArgs e)
        {
            WebSocketDataReceived?.Invoke(this, e);
        }

        public event EventHandler<LoginRequiredEventArgs>? LoginResultReceived;
        protected virtual void OnLoginResultReceived(LoginRequiredEventArgs e)
        {
            LoginResultReceived?.Invoke(this, e);
        }

        public event EventHandler<SessionChangedEventArgs>? SessionChanged;
        protected virtual void OnSessionChangedEvent(SessionChangedEventArgs e)
        {
            SessionChanged?.Invoke(this, e);
        }
        #endregion
    }
}
