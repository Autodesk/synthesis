using Synthesis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthesis
{
    public class StandardMessage
    {
        public const string Resume = "Resume";
        public const string Exit = "Exit";
        public const string Stop = "Stop";
        public const string Restart = "Restart";
        public const string Start = "Start";
        public const string Pause = "Pause";

        public const string ThreadStopped = "ThreadStopped";

        public class ResumeMessage : IMessage
        {
            public string GetName()
            {
                return Resume;
            }
        }


        public class ExitMessage : IMessage
        {
            public string GetName()
            {
                return Exit;
            }
        }

        public class StopMessage : IMessage
        {
            public string GetName()
            {
                return Stop;
            }
        }

        public class RestartMessage : IMessage
        {
            public string GetName()
            {
                return Restart;
            }
        }

        public class ThreadStoppedMessage : IMessage
        {
            public string GetName()
            {
                return ThreadStopped;
            }
        }

    }
}