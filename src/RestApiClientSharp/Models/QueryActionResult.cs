using AndreasReitberger.API.REST.Interfaces;

namespace AndreasReitberger.API.REST
{
    public partial class QueryActionResult : ObservableObject, IQueryActionResult
    {
        #region Properties
        [ObservableProperty]
        [JsonPropertyName("ok")]
        public partial bool Ok { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, RestSourceGenerationContext.Default.QueryActionResult);

        #endregion
    }
}
