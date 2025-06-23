﻿using AndreasReitberger.API.REST.Events;
using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;
#if DEBUG
using System.Diagnostics;
#endif
using System.Threading.Tasks;
using Websocket.Client;
using System.Text.RegularExpressions;
using System.Net.WebSockets;
using System.Net;
using System.Threading;

namespace AndreasReitberger.API.REST
{
    public partial class RestApiClient : ObservableObject, IRestApiClient
    {

        #region Properties
        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial WebsocketClient? WebSocket { get; set; }

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial CancellationTokenSource? CtsPinging { get; set; }

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool IsListening { get; set; } = false;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial Func<Task>? OnRefresh { get; set; }

        [ObservableProperty]
        public partial int RefreshInterval { get; set; } = 5;
        partial void OnRefreshIntervalChanged(int value)
        {
            if (IsListening)
            {
                _ = StartListeningAsync(target: WebSocketTargetUri, stopActiveListening: true, refreshFunction: OnRefresh);
            }
        }


        [ObservableProperty]
        public partial int KeepAliveInterval { get; set; } = 5;
        partial void OnKeepAliveIntervalChanged(int value)
        {
            if (IsListening)
            {
                _ = StartListeningAsync(target: WebSocketTargetUri, stopActiveListening: true, refreshFunction: OnRefresh);
            }
        }

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial long LastPingTimestamp { get; set; }

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool EnablePing { get; set; } = true;      

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial int PingInterval { get; set; } = 60;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial long PingCounter { get; set; } = 0;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial long LastRefreshTimestamp { get; set; }

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial int RefreshCounter { get; set; } = 0;

        [ObservableProperty]
        public partial string PingCommand { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string SessionId { get; set; } = string.Empty;
        partial void OnSessionIdChanged(string value)
        {
            OnSessionChangedEvent(new()
            {
                SessionId = value,
                Session = value,
            });
        }

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial string WebSocketTargetUri { get; set; } = string.Empty;

        #endregion

        #region Methods

        /// <summary>
        /// Source: https://github.com/Z0rdak/RepetierSharp/blob/main/RepetierConnection.cs
        /// </summary>
        /// <returns></returns>
        public virtual WebsocketClient? GetWebSocketClient(CookieContainer? cookies = null)
        {
            if (string.IsNullOrEmpty(WebSocketTargetUri)) return null;
            Func<ClientWebSocket> factory = new(() => new ClientWebSocket
            {
                Options =
                {
                    KeepAliveInterval = KeepAliveInterval > 0 ? TimeSpan.FromSeconds(KeepAliveInterval) : TimeSpan.Zero,
                    Cookies = cookies ?? new(),
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                },
            });
            WebsocketClient client = new(new Uri(WebSocketTargetUri), factory)
            {
                ReconnectTimeout = TimeSpan.FromSeconds(15),
                IsReconnectionEnabled = true,
            };
            client.ReconnectionHappened.Subscribe(info =>
            {
                if (info.Type == ReconnectionType.Initial)
                {
                    IsListening = true;
                    // Only query messages at this point when using a api-key or no auth
                    /*
                    if (Session.AuthType != AuthenticationType.Credentials)
                    {
                        this.QueryOpenMessages();
                    }
                    */
                }
                if (EnablePing)
                    Task.Run(async () => await SendPingAsync());
            });
            client.DisconnectionHappened.Subscribe(WebSocket_Closed);
            client.MessageReceived.Subscribe(WebSocket_MessageReceived);
            return client;
        }

        public virtual async Task SendPingAsync()
        {
            string pingCmd = BuildPingCommand();
#if DEBUG
            Debug.WriteLine($"WebSocket: Ping sent '{pingCmd}' at {DateTime.Now}");
#endif
            await SendWebSocketCommandAsync(pingCmd);
        }

        public virtual async Task PingingAsync(CancellationTokenSource cts, int interval)
        {
            while (cts.IsCancellationRequested is false && WebSocket?.IsRunning is true)
            {
                long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                if (EnablePing && LastPingTimestamp + PingInterval < DateTimeOffset.Now.ToUnixTimeSeconds())
                {
                    PingCounter++;
                    LastPingTimestamp = timestamp;
                    await SendPingAsync();
                }
            }
        }

        public virtual Task SendWebSocketCommandAsync(string command) => Task.Run(() => WebSocket?.Send(command));

        public virtual string BuildPingCommand(object? data = null)
        {
            data = new
            {
                jsonrpc = "2.0",
                method = "server.info",
                @params = new { },
                id = PingCounter,
            };
            return JsonConvert.SerializeObject(data);
        }

        public virtual async Task UpdateWebSocketAsync(Func<Task>? refreshFunction = null, string[]? commandsOnConnect = null)
        {
            if (!string.IsNullOrEmpty(WebSocketTargetUri) && IsInitialized)
            {
                await StartListeningAsync(target: WebSocketTargetUri, stopActiveListening: true, refreshFunction: refreshFunction, commandsOnConnect: commandsOnConnect)
                    .ConfigureAwait(false);
            }
        }
        public virtual Task StartListeningAsync(bool stopActiveListening = false, string[]? commandsOnConnect = null)
            => StartListeningAsync(WebSocketTargetUri, stopActiveListening, OnRefresh, commandsOnConnect: commandsOnConnect);

        public virtual async Task StartListeningAsync(string target, bool stopActiveListening = false, Func<Task>? refreshFunction = null, string[]? commandsOnConnect = null)
        {
            if (IsListening)// avoid multiple sessions
            {
                if (stopActiveListening)
                {
                    await StopListeningAsync();
                }
                else
                {
                    return; // StopListening();
                }
            }
            OnRefresh = refreshFunction;
            await ConnectWebSocketAsync(target, commandsOnConnect: commandsOnConnect).ConfigureAwait(false);
            IsListening = true;
        }

        public virtual async Task StopListeningAsync()
        {
            CancelCurrentRequests();
            if (IsListening)
            {
                await DisconnectWebSocketAsync().ConfigureAwait(false);
            }
            IsListening = false;
        }

        public virtual Task ConnectWebSocketAsync(string target, string commandOnConnect, CookieContainer? cookies = null)
            => ConnectWebSocketAsync(target: target, commandsOnConnect: commandOnConnect is not null ? [commandOnConnect] : null, cookies: cookies);
        public virtual async Task ConnectWebSocketAsync(string target, string[]? commandsOnConnect = null, CookieContainer? cookies = null)
        {
            try
            {
#if NET6_0_OR_GREATER
                bool targetValid = Uri.TryCreate(target, UriKind.Absolute, out var uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeWs || uriResult.Scheme == Uri.UriSchemeWss);
                if (!targetValid) return;
#else
                if (!string.IsNullOrEmpty(target) && Regex.IsMatch(target, @"/^(wss?:\/\/)([0-9]{1,3}(?:\.[0-9]{1,3}){3}|[a-zA-Z]+):([0-9]{1,5})$/"))
                {
                    return;
                }
#endif            
                await DisconnectWebSocketAsync().ConfigureAwait(false);
                WebSocket = GetWebSocketClient(cookies);
                if (WebSocket is null) return;
                await WebSocket.StartOrFail().ContinueWith(async t =>
                {
#if DEBUG
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        Debug.WriteLine($"WebSocket: Start failed or canceled! {t}");
                    }
#endif
                    for (int i = 0; i < commandsOnConnect?.Length; i++)
                    {
                        WebSocket.Send(commandsOnConnect[i]);
#if DEBUG
                        Debug.WriteLine($"WebSocket: Sent onConnection command '{commandsOnConnect[i]}' at {DateTime.Now}");
#endif
                    }
                    if (EnablePing)
                        await SendPingAsync();
                });
                if (EnablePing)
                {
                    CtsPinging?.Cancel();
                    CtsPinging = new();
                    Task.Run(async () => await PingingAsync(CtsPinging, PingInterval));
                }
                /*
                if (EnablePing)
                {
                    await WebSocket.StartOrFail().ContinueWith(t => SendPingAsync());
                }
                else
                {
                    await WebSocket.StartOrFail().ContinueWith(t =>
                    {
                        for (int i = 0; i < commandsOnConnect?.Length; i++)
                        {
                            WebSocket.Send(commandsOnConnect[i]);
#if DEBUG
                            Debug.WriteLine($"WebSocket: Sent onConnection command '{commandsOnConnect[i]}' at {DateTime.Now}");
#endif
                        }
                    });
                }
                */
#if DEBUG
                Debug.WriteLine($"WebSocket: Connection established at {DateTime.Now}");
#endif
                /*
                if (EnablePing && commandsOnConnect is not null && WebSocket is not null)
                {
                    // Send command
                    for (int i = 0; i < commandsOnConnect?.Length; i++)
                    {
                        WebSocket.Send(commandsOnConnect[i]);
#if DEBUG
                        Debug.WriteLine($"WebSocket: Sent onConnection command '{commandsOnConnect[i]}' at {DateTime.Now}");
#endif
                    }
                }
                */
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }
        public virtual async Task DisconnectWebSocketAsync()
        {
            try
            {
                if (WebSocket is not null)
                {
                    await Task.Delay(10);
                    WebSocket.Dispose();
                    WebSocket = null;
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }

        protected virtual void WebSocket_MessageReceived(ResponseMessage? msg)
        {
            try
            {
                if (msg?.MessageType != WebSocketMessageType.Text || string.IsNullOrEmpty(msg?.Text))
                {
                    return;
                }
                long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                if (EnablePing && LastPingTimestamp + PingInterval < DateTimeOffset.Now.ToUnixTimeSeconds())
                {
                    PingCounter++;
                    LastPingTimestamp = timestamp;
                    Task.Run(SendPingAsync);
#if DEBUG
                    Debug.WriteLine($"WebSocket: Ping sent: {DateTime.Now}");
#endif
                }
                // Handle refreshing more often the pinging
                if (LastRefreshTimestamp + RefreshInterval < DateTimeOffset.Now.ToUnixTimeSeconds())
                {
                    LastRefreshTimestamp = timestamp;
                    Task.Run(async () =>
                    {
                        // Check online state on each 5. ping
                        if (RefreshCounter > 5)
                        {
                            RefreshCounter = 0;
                            await CheckOnlineAsync(commandBase: ApiTargetPath, authHeaders: AuthHeaders, timeout: 3500).ConfigureAwait(false);
                        }
                        else RefreshCounter++;
                        if (IsOnline)
                        {
                            if (OnRefresh is not null)
                            {
                                await OnRefresh.Invoke().ConfigureAwait(false);
#if DEBUG
                                Debug.WriteLine($"Data refreshed: {DateTime.Now} - On refresh done");
#endif
                            }
                        }
                        else if (IsListening)
                        {
                            await StopListeningAsync().ConfigureAwait(false); // StopListening();
                        }
                    });
                }

                OnWebSocketMessageReceived(new WebsocketEventArgs()
                {
                    CallbackId = PingCounter,
                    Message = msg.Text,
                    SessionId = SessionId,
                });
            }
            catch (JsonException jecx)
            {
                OnError(new JsonConvertEventArgs()
                {
                    Exception = jecx,
                    OriginalString = msg?.Text,
                    Message = jecx.Message,
                });
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }

        protected virtual void WebSocket_Closed(DisconnectionInfo? info)
        {
            try
            {
                IsListening = false;
                CtsPinging?.Cancel();
                //StopPingTimer();
                OnWebSocketDisconnected(new RestEventArgs()
                {
                    Message =
                    $"WebSocket connection to {WebSocket} closed. Connection state while closing was '{(IsOnline ? "online" : "offline")}'" +
                    $"\n-- Connection closed: {info?.Type} | {info?.CloseStatus} | {info?.CloseStatusDescription}",
                });
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }

        #endregion
    }
}
