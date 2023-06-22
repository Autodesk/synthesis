using Newtonsoft.Json;
using SynthesisAPI.RoboRIO;
using SynthesisAPI.WS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#nullable enable

namespace Synthesis.WS {
    public static class WebSocketManager {
        private static bool _initialized  = false;
        public static bool Initialized   => _initialized;
        private static WebSocketServer? _server;
        public static RoboRIOState RioState = new RoboRIOState();

        public static bool HasClient => _server?.Clients.Count > 0;

        public static void Init(bool force = false) {
            if (_initialized && !force) {
                return;
            }

            if (_server != null) {
                _server.Dispose();
                Debug.Log("WebSocketServer closed by manager");
            }

            RioState = new RoboRIOState();

            _server = new WebSocketServer("127.0.0.1", 3300);
            _server.AddOnMessageListener(OnMessage);

            _initialized = true;
        }

        private static void OnMessage(Guid guid, string message) {
            var jsonData = JsonConvert.DeserializeObject<RioJsonData>(message);
            RioState.UpdateData(jsonData.type, jsonData.device, jsonData.data);
        }

        /// <summary>
        /// Get Rio data from the RioState
        /// </summary>
        /// <typeparam name="T">Data requested</typeparam>
        public static T GetData<T>(string device)
            where T : HardwareTypeData {
            return RioState.GetData<T>(device);
        }

        /// <summary>
        /// Update data in the RioState and upload it to a currently connected websocket client
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        public static void UpdateData<T>(string device, Action<T> change)
            where T : HardwareTypeData {
            RioState.UpdateData(device, change);

            if (_server == null || _server.Clients.Count == 0) {
                return;
            }

            var copy = new Dictionary<string, object>();
            RioState.GetData<T>(device).GetData().ForEach(kvp => copy[kvp.Key] = kvp.Value);

            var data = new RioJsonData { type = HardwareTypeData.GetMetaData<T>().RegisteredTypeName, device = device,
                data = copy };
            var json = JsonConvert.SerializeObject(data);
            _server.SendToClient(_server.Clients[0], json);
        }

        public static void ForceUpdate() {
            throw new NotImplementedException();
        }

        public static void ForceUpdate(string type, string device) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Execute to prepare the sim for
        /// </summary>
        public static void Teardown() {
            if (_server != null) {
                _server.Dispose();
            }

            _server = null;
            _initialized = false;

            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/riostate.json",
                JsonConvert.SerializeObject(RioState));
        }
    }

    public struct RioJsonData {
        public string type;
        public string device;
        public Dictionary<string, object> data;
    }
}
