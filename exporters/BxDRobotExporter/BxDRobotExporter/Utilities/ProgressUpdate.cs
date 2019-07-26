namespace BxDRobotExporter.Utilities
{
    public class ProgressUpdate
    {
        public readonly string Message;
        public readonly int CurrentProgress;
        public readonly int MaxProgress;

        public ProgressUpdate(string message, int currentProgress, int maxProgress)
        {
            Message = message;
            CurrentProgress = currentProgress;
            MaxProgress = maxProgress;
        }
    }
}