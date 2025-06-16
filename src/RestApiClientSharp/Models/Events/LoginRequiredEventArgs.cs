using Newtonsoft.Json;

namespace AndreasReitberger.API.REST.Events
{
    public partial class LoginRequiredEventArgs : RestEventArgs
    {
        #region Properties
        public object? LoginData { get; set; }
        public bool LoginSucceeded { get; set; } = false;
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}
