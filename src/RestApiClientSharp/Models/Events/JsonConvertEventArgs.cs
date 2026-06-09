using AndreasReitberger.API.REST.Interfaces;

namespace AndreasReitberger.API.REST.Events
{
    public partial class JsonConvertEventArgs : EventArgs, IJsonConvertEventArgs
    {
        #region Properties
        public string? Message { get; set; }
        public string? OriginalString { get; set; }
        public string? TargetType { get; set; }

        [JsonIgnore]
        public Exception? Exception { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, RestSourceGenerationContext.Default.JsonConvertEventArgs);
        #endregion
    }
}
