namespace AndreasReitberger.API.REST.Interfaces
{
    public interface ITaskCanceledEventArgs : IRestEventArgs
    {
        #region Properties
        public string? Source { get; set; }
        public bool CancelationRequested { get; set; }
        public int Timeout { get; set; }
        #endregion
    }
}
