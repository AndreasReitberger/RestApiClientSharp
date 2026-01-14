using AndreasReitberger.API.REST.Interfaces;

namespace AndreasReitberger.API.REST
{
    public partial class RestHeader : ObservableObject, IRestHeader
    {
        #region Properties
        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Value { get; set; } = string.Empty;
        #endregion

        #region Ctor
        public RestHeader() { }
        public RestHeader(string name, string value)
        {
            Name = name;
            Value = value;
        }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, RestSourceGenerationContext.Default.RestHeader);

        #endregion
    }
}
