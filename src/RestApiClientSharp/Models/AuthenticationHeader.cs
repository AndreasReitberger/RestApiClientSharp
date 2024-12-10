using AndreasReitberger.API.REST.Enums;
using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.REST
{
    public partial class AuthenticationHeader : ObservableObject, IAuthenticationHeader
    {
        #region Properties
        [ObservableProperty]
        string? token;

        [ObservableProperty]
        int order;

        [ObservableProperty]
        string? format;

        [ObservableProperty]
        AuthenticationHeaderTarget target = AuthenticationHeaderTarget.Header;
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);

        #endregion
    }
}
