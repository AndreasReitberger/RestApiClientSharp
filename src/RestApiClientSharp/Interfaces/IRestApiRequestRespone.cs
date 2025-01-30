namespace AndreasReitberger.API.REST.Interfaces
{
    public interface IRestApiRequestRespone
    {
        #region Properties
        public string? Result { get; set; }
        public byte[]? RawBytes { get; set; }
        public bool IsOnline { get; set; }
        public bool Succeeded { get; set; }
        public bool HasAuthenticationError { get; set; }
        public IRestEventArgs? EventArgs { get; set; }
        public Exception? Exception { get; set; }
        #endregion
    }
}
