namespace AndreasReitberger.API.REST.Interfaces
{
    public interface IRestEventArgs
    {
        #region Properties
        public string? Message { get; set; }
        public string? Status { get; set; }
        public string? SessionId { get; set; }
        public string? AuthToken { get; set; }
        public long CallbackId { get; set; }
        public Uri? Uri { get; set; }
        public Exception? Exception { get; set; }
        #endregion
    }
}
