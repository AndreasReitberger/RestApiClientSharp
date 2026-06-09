using AndreasReitberger.API.REST.Events;
using AndreasReitberger.API.REST.TypeConverters;

namespace AndreasReitberger.API.REST.SourceGeneration
{
    [JsonSerializable(typeof(AuthenticationHeader))]
    [JsonSerializable(typeof(QueryActionResult))]
    [JsonSerializable(typeof(RestHeader))]
    [JsonSerializable(typeof(JsonConvertEventArgs))]
    [JsonSerializable(typeof(ListeningChangedEventArgs))]
    [JsonSerializable(typeof(LoginRequiredEventArgs))]
    [JsonSerializable(typeof(RestApiClient))]
    [JsonSerializable(typeof(RestApiRequestRespone))]
    [JsonSerializable(typeof(RestEventArgs))]
    [JsonSerializable(typeof(SessionChangedEventArgs))]
    [JsonSerializable(typeof(TaskCanceledEventArgs))]
    [JsonSerializable(typeof(WebsocketEventArgs))]
    [JsonSerializable(typeof(WebsocketPingSentEventArgs))]
    [JsonSourceGenerationOptions(WriteIndented = true,
        Converters = new Type[] {
            typeof(AuthenticationHeaderConverter),
        }, ReferenceHandler =
#if DEBUG
        JsonKnownReferenceHandler.Preserve
#else
        JsonKnownReferenceHandler.IgnoreCycles
#endif
        )]
    public partial class RestSourceGenerationContext : JsonSerializerContext { }
}
