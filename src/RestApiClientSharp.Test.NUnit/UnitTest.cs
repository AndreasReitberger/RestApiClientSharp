using AndreasReitberger.API.REST;
using AndreasReitberger.API.REST.Enums;
using Newtonsoft.Json;

namespace RestApiClientSharp.Test.NUnit
{
    public class Tests
    {
        private readonly string tokenString = SecretAppSettingReader.ReadSection<SecretAppSetting>("TestSetup")?.ApiKey ?? "";
        private RestApiClient? client;

        #region Setup
        [SetUp]
        public void Setup()
        {
            client = new RestApiClient.RestApiConnectionBuilder()
                .WithWebAddress("https://jsonplaceholder.typicode.com/todos/")
                .WithVersion("1")
                .WithApiKey("token", new AuthenticationHeader() { Token = tokenString, Target = AuthenticationHeaderTarget.Header})
                .WithTimeout(100 * 1000)
                .WithRateLimiter(true, tokenLimit: 2, tokensPerPeriod: 2, replenishmentPeriod: 1.5)
                .Build();
            client.Error += (sender, args) =>
            {
                Assert.Fail(args?.ToString());
            };
        }
        #endregion

        #region JSON
        [Test]
        public void TestJsonSerialization()
        {
            try
            {
                string? json = JsonConvert.SerializeObject(client, Formatting.Indented, settings: RestApiClient.DefaultNewtonsoftJsonSerializerSettings);
                Assert.That(!string.IsNullOrEmpty(json));

                RestApiClient? client2 = JsonConvert.DeserializeObject<RestApiClient>(json, settings: RestApiClient.DefaultNewtonsoftJsonSerializerSettings);
                Assert.That(client2, Is.Not.EqualTo(null));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        #endregion

        #region Cleanup
        [TearDown]
        public void BaseTearDown()
        {
            client?.Dispose();
        }
        #endregion

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}