using System;

namespace BxDRobotExporter.Exporter
{
    public class Exporter
    {
        public class EmptyAssemblyException : ApplicationException
        {
            public EmptyAssemblyException() : base("No parts in assembly.") { }
        }

        public class InvalidJointException : ApplicationException
        {
            public InvalidJointException(string message) : base(message) { }
        }

        public class NoGroundException : ApplicationException
        {
            public NoGroundException() : base("Assembly has no ground.") { }
        }
    }
}
