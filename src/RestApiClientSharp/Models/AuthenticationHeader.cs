using AndreasReitberger.API.REST.Enums;
using AndreasReitberger.API.REST.Interfaces;

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

        [ObservableProperty]
        public partial AuthenticationTypeTarget Type { get; set; } = AuthenticationTypeTarget.Rest;
        #endregion

        #region Overrides

        public override string ToString() => JsonSerializer.Serialize(this!, RestSourceGenerationContext.Default.AuthenticationHeader);

        #endregion
    }
}
