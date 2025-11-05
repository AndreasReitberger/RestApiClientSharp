using AndreasReitberger.API.REST;
using AndreasReitberger.API.REST.Enums;
using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;
using RestApiClientSharp.Test.NUnit.Model;
using RestSharp;

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
                if (!client.ReThrowOnError)
                {
                    Assert.Fail($"Error: {args?.ToString()}");
                }
            };
            client.RestApiError += (sender, args) =>
            {
                if (!client.ReThrowOnError)
                {
                    Assert.Fail($"REST-Error: {args?.ToString()}");
                }
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

        #region WebSocket
        [Test]
        public async Task TestWebSocketAsync()
        {
            try
            {
                if (client is null) throw new NullReferenceException($"The client was null!");
                // Create a new Invoice object
                client.WebSocketTargetUri = "wss://";
                client.WebSocketError += (sender, args) =>
                {
                    Assert.Fail($"WebSocket Error: {args?.ToString()}");
                };
                client.WebSocketMessageReceived += (sender, args) =>
                {
                   // Handle incoming WebSocket messages here
                    Console.WriteLine($"WebSocket Message Received: {args?.Message}");
                };

                await client.ConnectWebSocketAsync(client.WebSocketTargetUri!).ConfigureAwait(false);
                Assert.That(client.IsListening);
                CancellationTokenSource cts = new(new TimeSpan(0, 15, 0));
                while (cts.IsCancellationRequested == false)
                {
                    // Keep the WebSocket connection alive for 15 minutes
                    await Task.Delay(1000, cts.Token).ConfigureAwait(false);
                }
                Assert.That(client.IsListening);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        #endregion

        #region Exception
        [Test]
        public async Task TestExecption()
        {
            try
            {
                client.ReThrowOnError = true;
                var result = await client.SendRestApiRequestAsync(
                        requestTargetUri: "https://github.com/AndreasReitberger/LexOfficeClientSharp/-1",
                        method: Method.Get,
                        command: "",
                        jsonObject: null,
                        authHeaders: client.AuthHeaders,
                        urlSegments: null,
                        cts: default
                        )
                    .ConfigureAwait(false);

                Assert.Fail("Should Throw");
            }
            catch (Exception ex)
            {
                Assert.That(ex.Message == "Request failed with status code NotFound");
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