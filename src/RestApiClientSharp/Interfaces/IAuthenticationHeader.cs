using AndreasReitberger.API.REST.Enums;

namespace AndreasReitberger.API.REST.Interfaces
{
    public interface IAuthenticationHeader
    {
        #region Properties
        public string Token { get; set; }
        public int Order { get; set; }
        public string? Format { get; set; }
        public AuthenticationHeaderTarget Target { get; set; }
        #endregion

    }
}
