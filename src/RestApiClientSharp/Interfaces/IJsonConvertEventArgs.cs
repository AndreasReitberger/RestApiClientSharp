namespace AndreasReitberger.API.REST.Interfaces
{
    public interface IJsonConvertEventArgs
    {
        #region Properties
        public string? Message { get; set; }
        public string? OriginalString { get; set; }
        public string? TargetType { get; set; }
        public Exception? Exception { get; set; }
        #endregion
    }
}
