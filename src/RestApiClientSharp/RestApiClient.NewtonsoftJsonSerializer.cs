using AndreasReitberger.API.REST.Events;
using AndreasReitberger.API.REST.Interfaces;
using AndreasReitberger.API.REST.JSON.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace AndreasReitberger.API.REST
{
    public partial class RestApiClient : ObservableObject, IRestApiClient
    {
        [ObservableProperty]
        [Obsolete("Due to trimming use `System.Json.JsonSerializerSettings` implementations")]
        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial JsonSerializerSettings NewtonsoftJsonSerializerSettings { get; set; } = DefaultNewtonsoftJsonSerializerSettings;

        #region SerializerSettings
        [Obsolete("Due to trimming use `System.Json.JsonSerializerSettings` implementations")]
#if NET5_0_OR_GREATER
        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
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
#if DEBUG
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
#else
            // Ignore if the json respone has more or less properties than the target class
            MissingMemberHandling = MissingMemberHandling.Ignore,          
            NullValueHandling = NullValueHandling.Ignore,
#endif
            TypeNameHandling = TypeNameHandling.Auto,
            Converters =
            {
                // Map the converters
                new AbstractConverter<AuthenticationHeader, IAuthenticationHeader>(),
            }
        };
        #endregion

        #region Methods

#nullable enable
        [Obsolete("Due to trimming use `System.Json.JsonSerializerSettings` implementations")]
#if NET5_0_OR_GREATER
        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
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