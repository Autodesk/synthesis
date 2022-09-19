using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.WS;
using SynthesisAPI.RoboRIO;
using System;
using Newtonsoft.Json;
using System.IO;

namespace Synthesis.WS {
    public static class WebSocketManager {

        private static bool _initialized = false;
        public static bool Initialized => _initialized;
        private static WebSocketServer _server;
        public static RoboRIOState RioState;

        private static StreamWriter _writer;

        public static void Init(bool force = false) {
            if (_initialized && !force)
                return;

            if (_server != null) {
                _server.Close();
            }

            RioState = new RoboRIOState();
            _writer = new StreamWriter(File.Open("C:\\Users\\hunte\\dump.txt", FileMode.OpenOrCreate));

            _server = new WebSocketServer("127.0.0.1", 3300);
            _server.OnMessage += OnMessage;

            _initialized = true;
        }

        private static void OnMessage(Guid guid, string message) {
            var jsonData = JsonConvert.DeserializeObject<RioJsonData>(message);
            RioState.UpdateData(jsonData.type, jsonData.device, jsonData.data);
            if (jsonData.type == "PWM" && jsonData.device == "0") {
                _writer.WriteLine($"{message}\n\n");
                _writer.Flush();
            }
        }

        public static void Teardown() {
            if (_server != null) {
                _server.Close();
            }
            _server = null;

            _initialized = false;
        }
    }

    public struct RioJsonData {
        public string type;
        public string device;
        public Dictionary<string, object> data;
    }
}
