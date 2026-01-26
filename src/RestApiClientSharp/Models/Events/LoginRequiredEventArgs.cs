using AndreasReitberger.API.REST.Interfaces;

namespace AndreasReitberger.API.REST.Events
{
    public partial class LoginRequiredEventArgs : RestEventArgs, ILoginRequiredEventArgs
    {
        #region Properties
        public ILoginData? LoginData { get; set; }
        public bool LoginSucceeded { get; set; } = false;
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, RestSourceGenerationContext.Default.LoginRequiredEventArgs);
        #endregion
    }
}
