using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.REST
{
    public partial class QueryActionResult : ObservableObject, IQueryActionResult
    {
        #region Properties
        [ObservableProperty]
        [JsonProperty("ok")]
        public partial bool Ok { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);

        #endregion
    }
}
