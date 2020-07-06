using System;
using System.Text;

namespace SynthesisAPI.EventBus
{
    public interface IEvent 
    {
        public object[] GetArguments();
    }

    // ReSharper disable once InconsistentNaming
    public static class IEventExtension
    {
        public static string Name(this IEvent e) => e.GetType().FullName;
    }

}
