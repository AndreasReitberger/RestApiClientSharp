using AndreasReitberger.API.REST.Events;
using AndreasReitberger.API.REST.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace AndreasReitberger.API.REST
{
    // Documentation: https://finnhub.io/docs/api
    public partial class RestApiClient : ObservableObject, IRestApiClient
    {

        #region EventHandlers
        public event EventHandler? Error;
        protected virtual void OnError()
        {
            Error?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnError(UnhandledExceptionEventArgs e)
        {
            Error?.Invoke(this, e);
        }
        protected virtual void OnError(JsonConvertEventArgs e)
        {
            Error?.Invoke(this, e);
        }
        public event EventHandler<RestEventArgs>? RestApiError;
        protected virtual void OnRestApiError(RestEventArgs e)
        {
            RestApiError?.Invoke(this, e);
        }

        public event EventHandler<RestEventArgs>? RestApiAuthenticationError;
        protected virtual void OnRestApiAuthenticationError(RestEventArgs e)
        {
            RestApiAuthenticationError?.Invoke(this, e);
        }
        public event EventHandler<RestEventArgs>? RestApiAuthenticationSucceeded;
        protected virtual void OnRestApiAuthenticationSucceeded(RestEventArgs e)
        {
            RestApiAuthenticationSucceeded?.Invoke(this, e);
        }

        public event EventHandler<JsonConvertEventArgs>? RestJsonConvertError;
        protected virtual void OnRestJsonConvertError(JsonConvertEventArgs e)
        {
            RestJsonConvertError?.Invoke(this, e);
        }
        #endregion

    }
}
