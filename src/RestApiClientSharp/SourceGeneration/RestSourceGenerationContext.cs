using System.Text.Json.Serialization;

namespace AndreasReitberger.API.REST.SourceGeneration
{
    [JsonSerializable(typeof(AuthenticationHeader))]
    [JsonSerializable(typeof(QueryActionResult))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    public partial class RestSourceGenerationContext : JsonSerializerContext { }
}
