using AndreasReitberger.API.REST.Interfaces;
using AndreasReitberger.Shared.Core.DTO;

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
        public string? ErrorMessage { get; set; }
        public string? StackTrace { get; set; }
        public ErrorDto? Error { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, RestSourceGenerationContext.Default.RestEventArgs);
        #endregion
    }
}
