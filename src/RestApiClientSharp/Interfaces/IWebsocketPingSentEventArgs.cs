namespace AndreasReitberger.API.REST.Interfaces
{
    public interface IWebsocketPingSentEventArgs : IWebsocketEventArgs
    {
        #region Properties
        public DateTimeOffset? Timestamp { get; set; }
        public string PingCommand { get; set; }
        #endregion
    }
}
