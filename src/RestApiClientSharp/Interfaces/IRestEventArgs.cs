using AndreasReitberger.Shared.Core.Models.DTO;

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
        public string? ErrorMessage { get; set; }
        public string? StackTrace { get; set; }
        public ErrorDto? Error { get; set; }

        #endregion
    }
}
