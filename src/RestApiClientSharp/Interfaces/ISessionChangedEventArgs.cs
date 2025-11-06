namespace AndreasReitberger.API.REST.Interfaces
{
    public interface ISessionChangedEventArgs : IRestEventArgs
    {
        #region Properties
        public string? Session { get; set; }
        #endregion
    }
}
