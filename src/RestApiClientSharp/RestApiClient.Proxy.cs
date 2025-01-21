﻿using AndreasReitberger.API.REST.Interfaces;
using AndreasReitberger.API.REST.Utilities;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace AndreasReitberger.API.REST
{
    // Documentation: https://finnhub.io/docs/api
    public partial class RestApiClient : ObservableObject, IRestApiClient
    {
        #region Properties

        [ObservableProperty]
        public partial bool EnableProxy { get; set; } = false;
        partial void OnEnableProxyChanged(bool value) => UpdateRestClientInstance();

        [ObservableProperty]
        public partial bool ProxyUserUsesDefaultCredentials { get; set; } = true;
        partial void OnProxyUserUsesDefaultCredentialsChanged(bool value) => UpdateRestClientInstance();

        [ObservableProperty]
        public partial bool ProxyUseDefaultCredentials { get; set; } = true;
        partial void OnProxyUseDefaultCredentialsChanged(bool value) => UpdateRestClientInstance();

        [ObservableProperty]
        public partial bool SecureProxyConnection { get; set; } = true;
        partial void OnSecureProxyConnectionChanged(bool value) => UpdateRestClientInstance();

        [ObservableProperty]
        public partial string ProxyAddress { get; set; } = string.Empty;
        partial void OnProxyAddressChanged(string value) => UpdateRestClientInstance();

        [ObservableProperty]
        public partial int ProxyPort { get; set; } = 443;
        partial void OnProxyPortChanged(int value) => UpdateRestClientInstance();

        [ObservableProperty]
        public partial string ProxyUser { get; set; } = string.Empty;
        partial void OnProxyUserChanged(string value) => UpdateRestClientInstance();

        [ObservableProperty]
        [JsonIgnore, XmlIgnore]
        public partial string? ProxyPassword { get; set; }
        partial void OnProxyPasswordChanged(string? value) => UpdateRestClientInstance();

        #endregion

        #region Methods

        public virtual void SetProxy(bool secure, string address, int port, bool enable = true)
        {
            EnableProxy = enable;
            ProxyUserUsesDefaultCredentials = true;
            ProxyAddress = address;
            ProxyPort = port;
            ProxyUser = string.Empty;
            ProxyPassword = null;
            SecureProxyConnection = secure;
            UpdateRestClientInstance();
        }

        public virtual void SetProxy(bool secure, string address, int port, string user = "", string? password = null, bool enable = true)
        {
            EnableProxy = enable;
            ProxyUserUsesDefaultCredentials = false;
            ProxyAddress = address;
            ProxyPort = port;
            ProxyUser = user;
            ProxyPassword = password;
            SecureProxyConnection = secure;
            UpdateRestClientInstance();
        }

        public virtual Uri GetProxyUri() =>
            ProxyAddress.StartsWith("http://") || ProxyAddress.StartsWith("https://") ? new Uri($"{ProxyAddress}:{ProxyPort}") : new Uri($"{(SecureProxyConnection ? "https" : "http")}://{ProxyAddress}:{ProxyPort}");

        public virtual WebProxy GetCurrentProxy()
        {
            WebProxy proxy = new()
            {
                Address = GetProxyUri(),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = ProxyUseDefaultCredentials,
            };
            if (ProxyUseDefaultCredentials && !string.IsNullOrEmpty(ProxyUser))
            {
                proxy.Credentials = new NetworkCredential(ProxyUser, ProxyPassword);
            }
            else
            {
                proxy.UseDefaultCredentials = ProxyUseDefaultCredentials;
            }
            return proxy;
        }

        public virtual void UpdateRestClientInstance()
        {
            if (string.IsNullOrEmpty(ApiTargetPath) || string.IsNullOrEmpty(ApiVersion) || UpdatingClients)
            {
                return;
            }
            UpdatingClients = true;
#if !NETFRAMEWORK
            Limiter ??= DefaultLimiter;
#endif
            RestClientOptions options = new($"{ApiTargetPath}{ApiVersion}/")
            {
                ThrowOnAnyError = true,
                Timeout = TimeSpan.FromSeconds(DefaultTimeout / 1000),
            };
            if (EnableProxy && !string.IsNullOrEmpty(ProxyAddress))
            {
                HttpClientHandler httpHandler = new()
                {
                    UseProxy = true,
                    Proxy = GetCurrentProxy(),
                    AllowAutoRedirect = true,
                };

                HttpClient = new(handler: httpHandler, disposeHandler: true);
            }
            else
            {
                HttpClient =
#if !NETFRAMEWORK
                    new(new RateLimitedHandler(Limiter));
#else
                    new();
#endif
            }
            RestClient = new(httpClient: HttpClient, options: options);
            UpdatingClients = false;
        }

        #endregion

    }
}
