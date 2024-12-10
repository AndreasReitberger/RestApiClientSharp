using Newtonsoft.Json;
using System;

namespace AndreasReitberger.API.REST.Events
{
    public partial class JsonConvertEventArgs : EventArgs
    {
        #region Properties
        public string? Message { get; set; }
        public string? OriginalString { get; set; }
        public string? TargetType { get; set; }
        public Exception? Exception { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}
