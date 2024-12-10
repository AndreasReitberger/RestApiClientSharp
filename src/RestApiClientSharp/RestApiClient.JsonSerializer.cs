using AndreasReitberger.API.REST.Events;
using AndreasReitberger.API.REST.Interfaces;
using AndreasReitberger.API.REST.JSON.System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AndreasReitberger.API.REST
{
    // Documentation: https://finnhub.io/docs/api
    public partial class RestApiClient : ObservableObject, IRestApiClient
    {
#if DEBUG
        #region Debug
        [ObservableProperty]
        [property: Newtonsoft.Json.JsonIgnore, JsonIgnore, XmlIgnore]
        JsonSerializerOptions jsonSerializerSettings = DefaultJsonSerializerSettings;

        public static JsonSerializerOptions DefaultJsonSerializerSettings = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            WriteIndented = true,
            Converters =
            {                     
                // Map the converters
                new TypeMappingConverter<IAuthenticationHeader, AuthenticationHeader>(),
            }
        };
        #endregion
#else
        #region Release
        public static JsonSerializerOptions DefaultJsonSerializerSettings = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Converters =
            {                     
                // Map the converters
                new TypeMappingConverter<IAuthenticationHeader, AuthenticationHeader>(),
            }
        };
        #endregion
#endif
        #region Methods

#nullable enable
        public T? GetObjectFromJsonSystem<T>(string? json, JsonSerializerOptions? serializerSettings = null)
        {
            try
            {
                // Workaround
                // The HttpClient on net7-android seems to missing the char for the json respone
                // Seems to be only on a specific simulator, further investigation
#if DEBUG
                if ((json?.StartsWith("{") ?? false) && (!json?.EndsWith("}") ?? false))
                {
                    //json += $"}}"; 
                }
                else if ((json?.StartsWith("[") ?? false) && (!json?.EndsWith("]") ?? false))
                {
                    //json += $"]";
                }
#endif
                json ??= string.Empty;
                return JsonSerializer.Deserialize<T?>(json, serializerSettings ?? DefaultJsonSerializerSettings);
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
