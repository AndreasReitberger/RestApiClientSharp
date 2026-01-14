using AndreasReitberger.API.REST.Interfaces;

namespace AndreasReitberger.API.REST.Events
{
    public partial class TaskCanceledEventArgs : RestEventArgs, ITaskCanceledEventArgs
    {
        #region Properties
        public string? Source { get; set; }
        public bool CancelationRequested { get; set; } = false;
        public int Timeout { get; set; } = -1;
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, RestSourceGenerationContext.Default.TaskCanceledEventArgs);
        #endregion
    }
}
