using Newtonsoft.Json;

namespace RestApiClientSharp.Test.NUnit.Model
{
    public partial class TestJson
    {
        [JsonProperty("userId")]
        public long UserId { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("completed")]
        public bool Completed { get; set; }
    }
}
