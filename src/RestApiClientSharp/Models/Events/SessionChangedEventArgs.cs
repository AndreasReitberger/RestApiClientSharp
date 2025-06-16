using Newtonsoft.Json;

namespace AndreasReitberger.API.REST.Events
{
    public partial class SessionChangedEventArgs : RestEventArgs
    {
        #region Properties
        public string? Session { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}
