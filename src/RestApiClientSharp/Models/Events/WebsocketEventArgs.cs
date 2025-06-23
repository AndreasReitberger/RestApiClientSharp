using Newtonsoft.Json;

namespace AndreasReitberger.API.REST.Events
{
    public partial class WebsocketEventArgs : RestEventArgs
    {
        #region Properties
        public string? MessageReceived { get; set; }
        public byte[]? Data { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}
