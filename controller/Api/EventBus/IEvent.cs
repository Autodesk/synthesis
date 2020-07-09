namespace SynthesisAPI.EventBus
{
    public interface IEvent 
    {
        // public object[] GetArguments(); // TODO delete this?
    }

    // ReSharper disable once InconsistentNaming
    public static class IEventExtension
    {
        public static string Name(this IEvent e) => e.GetType().FullName;
    }

}
