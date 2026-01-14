using AndreasReitberger.API.REST.Interfaces;

namespace AndreasReitberger.API.REST.Events
{
    public partial class RestEventArgs : EventArgs, IRestEventArgs
    {
        #region Properties
        public string? Message { get; set; }
        public string? Status { get; set; }
        public Uri? Uri { get; set; }
        public long CallbackId { get; set; }
        public string? SessionId { get; set; }
        public string? AuthToken { get; set; }
        [JsonIgnore, Newtonsoft.Json.JsonIgnore]
        public Exception? Exception { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, RestSourceGenerationContext.Default.RestEventArgs);
        #endregion
    }
}
