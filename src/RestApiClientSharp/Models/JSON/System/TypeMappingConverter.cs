using System.Diagnostics.CodeAnalysis;
#if NET5_0_OR_GREATER
using System.Text.Json.Serialization.Metadata;
#endif

namespace AndreasReitberger.API.REST.JSON.System
{
    /// <summary>
    /// Source: https://stackoverflow.com/a/64636093/10083577
    /// License: CC BY-SA 4.0 (https://creativecommons.org/licenses/by-sa/4.0/)
    /// Author: Shimmy Weitzhandler (https://stackoverflow.com/users/75500/shimmy-weitzhandler)
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>

    public sealed class TypeMappingConverter<
#if NET5_0_OR_GREATER
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicParameterlessConstructor |
            DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
            TType, TImplementation
            >
     : JsonConverter<TType> where TImplementation : TType
    {
        [return: MaybeNull]
#if NET5_0_OR_GREATER
        public override TType Read(
          ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonTypeInfo<TImplementation> typeInfo = (JsonTypeInfo<TImplementation>)options.GetTypeInfo(typeof(TImplementation));
            return JsonSerializer.Deserialize<TImplementation>(ref reader, typeInfo);
        }
#else
        public override TType Read(
          ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            JsonSerializer.Deserialize<TImplementation>(ref reader, options);
#endif

#if NET5_0_OR_GREATER
        public override void Write(
          Utf8JsonWriter writer, TType value, JsonSerializerOptions options)
        {
            JsonTypeInfo<TImplementation> typeInfo = (JsonTypeInfo<TImplementation>)options.GetTypeInfo(typeof(TImplementation));
            JsonSerializer.Serialize(writer, (TImplementation)value!, typeInfo);
        }
#else
        public override void Write(
          Utf8JsonWriter writer, TType value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, (TImplementation)value!, options);
#endif

    }
}
