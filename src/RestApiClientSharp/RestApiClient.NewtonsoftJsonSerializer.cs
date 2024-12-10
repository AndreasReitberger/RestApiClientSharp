using AndreasReitberger.API.REST.Events;
using AndreasReitberger.API.REST.Interfaces;
using AndreasReitberger.API.REST.JSON.Newtonsoft;
using AndreasReitberger.API.REST;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AndreasReitberger.API.REST
{
    // Documentation: https://finnhub.io/docs/api
    public partial class RestApiClient : ObservableObject, IRestApiClient
    {
#if DEBUG
        #region Debug
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        JsonSerializerSettings newtonsoftJsonSerializerSettings = DefaultNewtonsoftJsonSerializerSettings;

        public static JsonSerializerSettings DefaultNewtonsoftJsonSerializerSettings = new()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            },
            DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffK",
            // Detect if the json respone has more or less properties than the target class
            //MissingMemberHandling = MissingMemberHandling.Error,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
            TypeNameHandling = TypeNameHandling.Auto,
            Converters =
            {
                // Map the converters
                new AbstractConverter<AuthenticationHeader, IAuthenticationHeader>(),
            }
        };
        #endregion
#else
        #region Release
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        JsonSerializerSettings newtonsoftJsonSerializerSettings = DefaultNewtonsoftJsonSerializerSettings;

        public static JsonSerializerSettings DefaultNewtonsoftJsonSerializerSettings = new()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            },
            DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffK",
            // Ignore if the json respone has more or less properties than the target class
            MissingMemberHandling = MissingMemberHandling.Ignore,          
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            Converters =
            {
                // Map the converters
                new AbstractConverter<AuthenticationHeader, IAuthenticationHeader>(),
            }
        };
        #endregion
#endif
        #region Methods

#nullable enable
        public T? GetObjectFromJson<T>(string? json, JsonSerializerSettings? serializerSettings = null)
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
                return JsonConvert.DeserializeObject<T?>(json, serializerSettings ?? NewtonsoftJsonSerializerSettings);
            }
            catch (JsonSerializationException jexc)
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
