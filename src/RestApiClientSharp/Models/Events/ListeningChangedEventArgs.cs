using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.REST.Events
{
    public partial class ListeningChangedEventArgs : SessionChangedEventArgs, IListeningChangedEventArgs
    {
        #region Properties
        public bool IsListening { get; set; } = false;
        public bool IsListeningToWebSocket { get; set; } = false;
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}
