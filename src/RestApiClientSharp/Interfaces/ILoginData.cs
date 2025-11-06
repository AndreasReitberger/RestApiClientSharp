namespace AndreasReitberger.API.REST.Interfaces
{
    public interface ILoginData : IRestEventArgs
    {
        #region Properties
        public string? Username { get; set; }
        public long? Permissions { get; set; }
        #endregion
    }
}
