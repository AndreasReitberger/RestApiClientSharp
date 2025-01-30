using AndreasReitberger.API.REST;
using AndreasReitberger.API.REST.Enums;
using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;
using RestApiClientSharp.Test.NUnit.Model;
using RestSharp;
using System.Drawing;

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
                Assert.Fail($"Error: {args?.ToString()}");
            };
            client.RestApiError += (sender, args) =>
            {
                Assert.Fail($"REST-Error: {args?.ToString()}");
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

        #region Query
        [Test]
        public async Task TestQueryAsync()
        {
            try
            {
                if (client is null) throw new NullReferenceException($"The client was null!");
                // Create a new Invoice object

                IRestApiRequestRespone? result = null;
                string? json = null;
                string targetUri = $"";
                result = await client.SendRestApiRequestAsync(
                        requestTargetUri: targetUri,
                        method: Method.Get,
                        command: "",
                        jsonObject: null,
                        authHeaders: client.AuthHeaders,
                        urlSegments: null,
                        cts: default
                        )
                    .ConfigureAwait(false);
                json = result?.Result;
                Assert.That(!string.IsNullOrEmpty(json));
                TestJson? resultObject = client.GetObjectFromJsonSystem<TestJson>(json);
                Assert.That(resultObject, Is.Not.EqualTo(null));
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
    }
}