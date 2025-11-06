using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.REST.Events
{
    public partial class WebsocketPingSentEventArgs : WebsocketEventArgs, IWebsocketPingSentEventArgs
    {
        #region Properties
        public DateTimeOffset? Timestamp { get; set; }
        public string PingCommand { get; set; } = string.Empty;
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}
