namespace AndreasReitberger.API.REST.Interfaces
{
    public interface ILoginRequiredEventArgs : IRestEventArgs
    {
        #region Properties
        public ILoginData? LoginData { get; set; }
        public bool LoginSucceeded { get; set; }
        #endregion
    }
}
