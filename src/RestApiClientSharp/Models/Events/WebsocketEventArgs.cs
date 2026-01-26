using AndreasReitberger.API.REST.Interfaces;

namespace AndreasReitberger.API.REST.Events
{
    public partial class WebsocketEventArgs : RestEventArgs, IWebsocketEventArgs
    {
        #region Properties
        public string? MessageReceived { get; set; }
        public byte[]? Data { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, RestSourceGenerationContext.Default.WebsocketEventArgs);
        #endregion
    }
}
