using AndreasReitberger.API.REST.Events;
using AndreasReitberger.API.REST.Interfaces;
using AndreasReitberger.API.REST.JSON.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AndreasReitberger.API.REST
{
    // Documentation: https://finnhub.io/docs/api
    public partial class RestApiClient : ObservableObject, IRestApiClient
    {
        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial JsonSerializerSettings NewtonsoftJsonSerializerSettings { get; set; } = DefaultNewtonsoftJsonSerializerSettings;
#if DEBUG
        #region Debug

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
