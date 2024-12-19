using AndreasReitberger.API.REST;
using AndreasReitberger.API.REST.Enums;

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
                .WithWebAddress("")
                .WithVersion("v1")
                .WithApiKey("token", new AuthenticationHeader() { Token = tokenString, Target = AuthenticationHeaderTarget.Header})
                .Build();
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