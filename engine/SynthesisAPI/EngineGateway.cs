using System;

namespace SynthesisAPI
{
    public class EngineGateway
    {
        internal static IEngineHandle handle = null;

        public static void AttachHandle(IEngineHandle h)
        {
            if (handle == null) handle = h;
            else throw new InvalidOperationException("Engine handle already exists");
        }
    }
}
