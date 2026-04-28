using AndreasReitberger.API.REST.Events;
using AndreasReitberger.API.REST.Interfaces;
using AndreasReitberger.API.REST.JSON.System;
using System.Text.Json.Serialization.Metadata;

namespace AndreasReitberger.API.REST
{
    public partial class RestApiClient : ObservableObject, IRestApiClient
    {

        [ObservableProperty]
        [Newtonsoft.Json.JsonIgnore, JsonIgnore, XmlIgnore]
        public partial JsonSerializerOptions JsonSerializerSettings { get; set; } = DefaultJsonSerializerSettings;

        #region SerializerSettings

        public static JsonSerializerOptions DefaultJsonSerializerSettings = new()
        {
            TypeInfoResolver = RestSourceGenerationContext.Default,
#if DEBUG
            ReferenceHandler = ReferenceHandler.Preserve,
#else
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
#endif
            WriteIndented = true,
            Converters =
            {                     
                // Map the converters
                new TypeMappingConverter<IAuthenticationHeader, AuthenticationHeader>(),
            }
        };
        #endregion

        #region Methods

#nullable enable
        public T? GetObjectFromJsonSystem<T>(string? json, JsonSerializerContext serializerContext)
        {
            try
            {
                json ??= string.Empty;
                return (T?)JsonSerializer.Deserialize(json, typeof(T), serializerContext);
            }
            catch (JsonException jexc)
            {
                OnError(new JsonConvertEventArgs()
                {
                    Exception = jexc,
                    OriginalString = json,
                    Message = jexc?.Message,
                    TargetType = nameof(T)
                });
                return default;
            }
        }
        public T? GetObjectFromJsonSystem<T>(string? json, JsonSerializerOptions? serializerSettings = null)
        {
            try
            {
                serializerSettings ??= DefaultJsonSerializerSettings;
                json ??= string.Empty; 
                JsonTypeInfo<T?> typeInfo = (JsonTypeInfo<T?>)serializerSettings.GetTypeInfo(typeof(T));
                return JsonSerializer.Deserialize(json, typeInfo);
            }
            catch (JsonException jexc)
            {
                OnError(new JsonConvertEventArgs()
                {
                    Exception = jexc,
                    OriginalString = json,
                    Message = jexc?.Message,
                    TargetType = nameof(T)
                });
                return default;
            }
        }
#nullable disable
        #endregion
    }
}
