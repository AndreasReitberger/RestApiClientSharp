namespace AndreasReitberger.API.REST.Interfaces
{
    public interface IWebsocketEventArgs : IRestEventArgs
    {
        #region Properties
        public string? MessageReceived { get; set; }
        public byte[]? Data { get; set; }
        #endregion
    }
}
