using AndreasReitberger.API.REST.Enums;
using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.REST
{
    public partial class AuthenticationHeader : ObservableObject, IAuthenticationHeader
    {
        #region Properties
        [ObservableProperty]
        public partial string Token { get; set; } = string.Empty;

        [ObservableProperty]
        public partial int Order { get; set; }

        [ObservableProperty]
        public partial string? Format { get; set; }

        [ObservableProperty]
        public partial AuthenticationHeaderTarget Target { get; set; } = AuthenticationHeaderTarget.Header;
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);

        #endregion
    }
}
