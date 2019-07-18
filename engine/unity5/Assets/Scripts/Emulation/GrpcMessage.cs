using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthesis
{
    public class GrpcMessage
    {

        public const string Connect = "Connect";
        public const string ConnectionError = "ConnectionError";
        public class ConnectionErrorMessage : IMessage
        {
            public string GetName()
            {
                return ConnectionError;
            }
        }

        public class ConnectMessage : IMessage
        {
            public string GetName()
            {
                return Connect;
            }
        }
    }
}
