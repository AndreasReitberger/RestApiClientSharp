using AndreasReitberger.API.REST.Interfaces;
using System.Net;

namespace AndreasReitberger.API.REST
{
    public partial class RestApiRequestRespone : ObservableObject, IRestApiRequestRespone
    {
        #region Properties
        [ObservableProperty]
        public partial string? Result { get; set; }

        [ObservableProperty]
        public partial byte[]? RawBytes { get; set; } = [];

        [ObservableProperty]
        public partial bool IsOnline { get; set; } = false;

        [ObservableProperty]
        public partial bool Succeeded { get; set; } = false;

        [ObservableProperty]
        public partial bool HasAuthenticationError { get; set; } = false;

        [ObservableProperty]
        public partial IRestEventArgs? EventArgs { get; set; }

        [ObservableProperty]
        [JsonIgnore, Newtonsoft.Json.JsonIgnore]
        public partial Exception? Exception { get; set; }

        [ObservableProperty]
        public partial CookieCollection? Cookies { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, RestSourceGenerationContext.Default.RestApiRequestRespone);

        #endregion
    }
}
