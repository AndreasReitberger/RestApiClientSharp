using AndreasReitberger.API.REST.Interfaces;

namespace AndreasReitberger.API.REST.Events
{
    public partial class SessionChangedEventArgs : RestEventArgs, ISessionChangedEventArgs
    {
        #region Properties
        public string? Session { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, RestSourceGenerationContext.Default.SessionChangedEventArgs);
        #endregion
    }
}
