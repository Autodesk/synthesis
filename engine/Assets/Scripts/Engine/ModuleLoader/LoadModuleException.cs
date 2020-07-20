using System;

namespace Engine.ModuleLoader
{
    public class LoadModuleException : Exception
    {
        public LoadModuleException() { }
        public LoadModuleException(string message) : base(message) { }
        public LoadModuleException(string message, Exception inner) : base(message, inner) { }
    }
}
