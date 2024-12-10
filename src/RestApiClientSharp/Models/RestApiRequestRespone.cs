using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.REST
{
    public partial class RestApiRequestRespone : ObservableObject, IRestApiRequestRespone
    {
        #region Properties
        [ObservableProperty]
        string? result;

        [ObservableProperty]
        byte[]? rawBytes = [];

        [ObservableProperty]
        bool isOnline = false;

        [ObservableProperty]
        bool succeeded = false;

        [ObservableProperty]
        bool hasAuthenticationError = false;

        [ObservableProperty]
        IRestEventArgs? eventArgs;
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);

        #endregion
    }
}
