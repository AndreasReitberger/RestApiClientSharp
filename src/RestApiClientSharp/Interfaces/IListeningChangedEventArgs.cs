namespace AndreasReitberger.API.REST.Interfaces
{
    public interface IListeningChangedEventArgs : ISessionChangedEventArgs
    {
        #region Properties
        public bool IsListening { get; set; }
        public bool IsListeningToWebSocket { get; set; }
        #endregion
    }
}
