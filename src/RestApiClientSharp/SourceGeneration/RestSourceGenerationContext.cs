using AndreasReitberger.API.REST.Events;

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
    [JsonSourceGenerationOptions(WriteIndented = true)]
    public partial class RestSourceGenerationContext : JsonSerializerContext { }
}
