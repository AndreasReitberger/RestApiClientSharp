using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.REST.Events
{
    public partial class LoginRequiredEventArgs : RestEventArgs, ILoginRequiredEventArgs
    {
        #region Properties
        public ILoginData? LoginData { get; set; }
        public bool LoginSucceeded { get; set; } = false;
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}
