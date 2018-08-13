using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Assets.Scripts.Network
{
    public class ServerStatus
    {
        public enum ServerStatusCode
        {
            Active = 0,
            Offline = 1,
            Full = 2
        }

        String serverName;

        ServerStatusCode statusCode;

        int currentClients = 0;
        int maxClients = 6;

        String serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static ServerStatus deserialize(String jsonData)
        {
            ServerStatus serverStatus = JsonConvert.DeserializeObject<ServerStatus>(jsonData);
            return serverStatus;
        }

    }
}