using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;
using System;

namespace AndreasReitberger.API.REST.Events
{
    public partial class RestEventArgs : EventArgs, IRestEventArgs
    {
        #region Properties
        public string? Message { get; set; }
        public string? Status { get; set; }
        public Uri? Uri { get; set; }
        public Exception? Exception { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}
