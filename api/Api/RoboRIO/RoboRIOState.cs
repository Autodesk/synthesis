using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SynthesisAPI.RoboRIO {
    public class RoboRIOState {
        private Dictionary<string, Type> _registeredTypes = new Dictionary<string, Type>() {
            { "AI", typeof(AIData) },
            { "PWM", typeof(PWMData) }
        };

        private Mutex _dataMutex = new Mutex();

        private Dictionary<string, Dictionary<string, IHardwareTypeData>> _allHardware = new Dictionary<string, Dictionary<string, IHardwareTypeData>>();

        public RoboRIOState() {
            _allHardware.Add("AI", new Dictionary<string, IHardwareTypeData>());
            _allHardware.Add("PWM", new Dictionary<string, IHardwareTypeData>());
        }

        public bool UpdateData(string type, string device, Dictionary<string, object> jsonData) {
            if (!_allHardware.ContainsKey(type))
                return false;

            _dataMutex.WaitOne();

            if (!_allHardware[type].ContainsKey(device)) {
                _allHardware[type][device] = (IHardwareTypeData)Activator.CreateInstance(_registeredTypes[type]);
            }
            _allHardware[type][device].UpdateData(jsonData);

            _dataMutex.ReleaseMutex();

            return true;
        }

        public bool UpdateData<T>(string type, string device, Action<T> updateAct) where T : IHardwareTypeData {
            if (!_allHardware.ContainsKey(type))
                return false;

            _dataMutex.WaitOne();

            if (!_allHardware[type].ContainsKey(device)) {
                _allHardware[type][device] = (IHardwareTypeData)Activator.CreateInstance(_registeredTypes[type]);
            }

            updateAct((T)_allHardware[type][device]);

            _dataMutex.ReleaseMutex();

            return true;
        }

        /// <summary>
        /// Get Data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="device"></param>
        /// <returns>Data if available. If type doesn't exist, returns null</returns>
        public T GetData<T>(string type, string device) where T : class, IHardwareTypeData {
            if (!_allHardware.ContainsKey(type))
                return null;

            _dataMutex.WaitOne();

            if (!_allHardware[type].ContainsKey(device))
                _allHardware[type][device] = (T)Activator.CreateInstance(typeof(T));

            var res = (T)Activator.CreateInstance(typeof(T), _allHardware[type][device]);

            _dataMutex.ReleaseMutex();

            return res;
        }
    }

    public class AIData : IHardwareTypeData {

        public bool init = false;
        public double voltage = 0.0;

        public AIData() { }

        public AIData(AIData data) {
            init = data.init;
            voltage = data.voltage;
        }

        public string GetData() {
            Dictionary<string, object> data = new Dictionary<string, object>() {
                { "<init", init },
                { ">voltage", voltage }
            };
            return JsonConvert.SerializeObject(data);
        }

        public void UpdateData(Dictionary<string, object> data) {
            // var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);

            if (data.ContainsKey("<init"))
                init = (bool)data["<init"];
            if (data.ContainsKey(">voltage"))
                voltage = (double)data[">voltage"];
        }
    }

    public class PWMData : IHardwareTypeData {

        public bool init = false;
        public double speed = 0.0;
        public double position = 0.0;

        public PWMData() { }

        public PWMData(PWMData data) {
            init = data.init;
            speed = data.speed;
            position = data.position;
        }

        public string GetData() {
            Dictionary<string, object> data = new Dictionary<string, object>() {
                { "<init", init },
                { "<speed", speed },
                { "<position", position }
            };
            return JsonConvert.SerializeObject(data);
        }

        public void UpdateData(Dictionary<string, object> data) {

            if (data.ContainsKey("<init"))
                init = (bool)data["<init"];
            if (data.ContainsKey("<speed"))
                speed = (double)data["<speed"];
            if (data.ContainsKey("<position"))
                speed = (double)data["<position"];
        }
    }

    public interface IHardwareTypeData {
        /// <summary>
        /// Converts data to JSON
        /// </summary>
        /// <returns></returns>
        string GetData();
        /// <summary>
        /// Updates existing data with JSON
        /// </summary>
        void UpdateData(Dictionary<string, object> data);
    }
}
