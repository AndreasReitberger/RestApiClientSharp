using Newtonsoft.Json;

namespace AndreasReitberger.API.REST.JSON.Newtonsoft
{
    /// <summary>
    /// Source: https://stackoverflow.com/a/48923314/10083577
    /// License: CC BY-SA 4.0 (https://creativecommons.org/licenses/by-sa/4.0/)
    /// Author: Simone S. (https://stackoverflow.com/users/1255023/simone-s)
    /// </summary>
    /// <typeparam name="TReal"></typeparam>
    /// <typeparam name="TAbstract"></typeparam>
    public class AbstractConverter<TReal, TAbstract>
     : JsonConverter where TReal : TAbstract
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(TAbstract);

        public override object? ReadJson(JsonReader reader, Type type, object? value, JsonSerializer jser)
            => jser.Deserialize<TReal>(reader);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer jser)
            => jser.Serialize(writer, value);
    }
}
