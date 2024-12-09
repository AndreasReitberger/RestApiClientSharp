using AndreasReitberger.API.REST;

namespace RestApiClientSharp.Test.NUnit
{
    public class Tests
    {
        private readonly string tokenString = SecretAppSettingReader.ReadSection<SecretAppSetting>("TestSetup").ApiKey ?? "";
        private RestApiClient? client;

        #region Setup
        [SetUp]
        public void Setup()
        {
            client = new RestApiClient.RestApiConnectionBuilder()
                .WithWebAddress("")
                .WithVersion("v1")
                .WithApiKey(tokenString)
                .Build();
        }
        #endregion

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}