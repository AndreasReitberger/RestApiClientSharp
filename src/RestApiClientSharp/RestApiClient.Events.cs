using AndreasReitberger.API.REST.Events;
using AndreasReitberger.API.REST.Interfaces;

namespace AndreasReitberger.API.REST
{
    public partial class RestApiClient : ObservableObject, IRestApiClient
    {

        #region EventHandlers
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

    }
}
