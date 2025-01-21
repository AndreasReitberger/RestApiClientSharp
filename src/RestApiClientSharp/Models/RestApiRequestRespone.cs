using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;

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
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);

        #endregion
    }
}
