using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SynthesisAPI.RoboRIO {

    [JsonObject(MemberSerialization.OptIn)]
    public class RoboRIOState {
        private Dictionary<string, Type> _registeredTypes = new Dictionary<string, Type>() {
            { "AI", typeof(AIData) },
            { "PWM", typeof(PWMData) },
            { "DriverStation", typeof(DriverStationData) },
            { "dPWM", typeof(DPWMData) },
            { "Encoder", typeof(EncoderData) }
        };

        private Mutex _dataMutex = new Mutex();

        [JsonProperty]
        private Dictionary<string, Dictionary<string, HardwareTypeData>> _allHardware = new Dictionary<string, Dictionary<string, HardwareTypeData>>();

        public RoboRIOState() {
            _registeredTypes.ForEach(kvp => _allHardware[kvp.Key] = new Dictionary<string, HardwareTypeData>());
        }

        /// <summary>
        /// Change data in the RioState.
        /// This overload of UpdateData is meant for the WebSocket server to pass nearly raw data into, not really meant for API users.
        /// </summary>
        /// <param name="type">Websocket type alias</param>
        /// <param name="device">Device ID (Could be number or something else)</param>
        /// <param name="jsonData">Data you wish to change. This will not dump keys that aren't included in jsonData</param>
        /// <returns>Whether or not the data you tried to modify was successfully modified</returns>
        public bool UpdateData(string type, string device, Dictionary<string, object> jsonData) {
            if (!_allHardware.ContainsKey(type))
                return false;

            _dataMutex.WaitOne();

            if (!_allHardware[type].ContainsKey(device)) {
                _allHardware[type][device] = (HardwareTypeData)Activator.CreateInstance(_registeredTypes[type]);
            }
            _allHardware[type][device].Update(jsonData);

            _dataMutex.ReleaseMutex();

            return true;
        }

        /// <summary>
        /// Change data in the RioState.
        /// This overload of UpdateData is meant to be used by users of the API. The wonky design is due to thread safety, sorry. -Hunter
        /// </summary>
        /// <typeparam name="T">HardwareTypeData type associated with the data you are modifying</typeparam>
        /// <param name="device">Device ID (Could be number or something else)</param>
        /// <param name="updateAct">Action in which to do your modifications. They will be ran within a mutex</param>
        /// <exception cref="Exception">Will throw if the type alias you use isn't registered to the T type you passed in</exception>
        /// <returns>Whether or not the data you tried to modify was successfully modified</returns>
        public bool UpdateData<T>(string device, Action<T> updateAct) where T : HardwareTypeData {

            string type = HardwareTypeData.GetMetaData<T>().RegisteredTypeName;

            if (!_allHardware.ContainsKey(type))
                return false;

            if (!_registeredTypes.ContainsKey(type) || _registeredTypes[type] != typeof(T))
                throw new Exception($"Data type '{type}' isn't registered to C# type '{typeof(T).Name}'");

            _dataMutex.WaitOne();

            if (!_allHardware[type].ContainsKey(device)) {
                _allHardware[type][device] = (HardwareTypeData)Activator.CreateInstance(_registeredTypes[type]);
            }

            updateAct((T)_allHardware[type][device]);

            _dataMutex.ReleaseMutex();

            return true;
        }

        /// <summary>
        /// Get data from the RioState
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        /// <param name="device">Device ID</param>
        /// <exception cref="Exception">Will throw if the type alias you use isn't registered to the T type you passed in</exception>
        /// <returns>Data if available. If type doesn't exist, returns null</returns>
        public T GetData<T>(string device) where T : HardwareTypeData {

            string type = HardwareTypeData.GetMetaData<T>().RegisteredTypeName;

            if (!_allHardware.ContainsKey(type))
                return null;

            if (!_registeredTypes.ContainsKey(type) || _registeredTypes[type] != typeof(T))
                throw new Exception($"Data type '{type}' isn't registered to C# type '{typeof(T).FullName}'");

            _dataMutex.WaitOne();

            if (!_allHardware[type].ContainsKey(device))
                _allHardware[type][device] = (T)Activator.CreateInstance(typeof(T));

            var res = (T)Activator.CreateInstance(typeof(T));
            res.Update(_allHardware[type][device].GetData());

            _dataMutex.ReleaseMutex();

            return res;
        }
    }

    [RioData("DriverStation")]
    public class DriverStationData : HardwareTypeData {
        public bool NewData {
            get => (bool)_rawData.TryGetDefault(">new_data", false);
            set {
                _rawData[">new_data"] = value;
            }
        }
        public bool Enabled {
            get => (bool)_rawData.TryGetDefault(">enabled", false);
            set {
                _rawData[">enabled"] = value;
            }
        }

        public override RioDataAttribute GetMetaData()
            => GetMetaData<DriverStationData>();
    }

    [RioData("AI")]
    public class AIData : HardwareTypeData {
        public bool Init {
            get => (bool)_rawData.TryGetDefault("<init", false);
            set {
                _rawData["<init"] = value;
            }
        }
        public double Voltage {
            get => (double)_rawData.TryGetDefault(">voltage", 0.0);
            set {
                _rawData[">voltage"] = value;
            }
        }

        public override RioDataAttribute GetMetaData()
            => GetMetaData<AIData>();
    }

    [RioData("PWM")]
    public class PWMData : HardwareTypeData {
        public bool Init {
            get => (bool)_rawData.TryGetDefault("<init", false);
            set {
                _rawData["<init"] = value;
            }
        }
        public double Speed {
            get => (double)_rawData.TryGetDefault("<speed", 0.0);
            set {
                _rawData["<speed"] = value;
            }
        }
        public double Position {
            get => (double)_rawData.TryGetDefault("<position", 0.0);
            set {
                _rawData["<position"] = value;
            }
        }
        public int Raw {
            get => (int)_rawData.TryGetDefault("<raw", 0);
            set {
                _rawData["<raw"] = value;
            }
        }
        public int PeriodScale {
            get => (int)_rawData.TryGetDefault("<period_scale", 0);
            set {
                _rawData["<period_scale"] = value;
            }
        }
        public bool ZeroLatch {
            get => (bool)_rawData.TryGetDefault("<zero_latch", false);
            set {
                _rawData["<zero_latch"] = value;
            }
        }

        public override RioDataAttribute GetMetaData()
            => GetMetaData<PWMData>();
    }

    [RioData("dPWM")]
    public class DPWMData : HardwareTypeData {
        public bool Init {
            get => (bool)_rawData.TryGetDefault("<init", false);
            set {
                _rawData["<init"] = value;
            }
        }
        public double DutyCycle {
            get => (double)_rawData.TryGetDefault("<duty_cycle", 0.0);
            set {
                _rawData["<duty_cycle"] = value;
            }
        }
        public int DIOPin {
            get => (int)_rawData.TryGetDefault("<dio_pin", 0);
            set {
                _rawData["<dio_pin"] = value;
            }
        }

        public override RioDataAttribute GetMetaData()
            => GetMetaData<DPWMData>();
    }

    [RioData("Encoder")]
    public class EncoderData : HardwareTypeData {
        public bool Init {
            get => (bool)_rawData.TryGetDefault("<init", false);
            set {
                _rawData["<init"] = value;
            }
        }
        public long ChannelA {
            get => (long)_rawData.TryGetDefault("<channel_a", -1L);
            set {
                _rawData["<channel_a"] = value;
            }
        }
        public long ChannelB {
            get => (long)_rawData.TryGetDefault("<channel_b", -1L);
            set {
                _rawData["_channel_b"] = value;
            }
        }
        public int SamplesToAvg {
            get => (int)_rawData.TryGetDefault("<samples_to_avg", 1);
            set {
                _rawData["<samples_to_avg"] = value;
            }
        }
        public int Count {
            get => (int)_rawData.TryGetDefault(">count", 0);
            set {
                _rawData[">count"] = value;
            }
        }
        public float Period {
            get => (float)_rawData.TryGetDefault(">period", 0f);
            set {
                _rawData[">period"] = value;
            }
        }

        public override RioDataAttribute GetMetaData()
            => GetMetaData<EncoderData>();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public abstract class HardwareTypeData {

        [JsonProperty]
        protected Dictionary<string, object> _rawData = new Dictionary<string, object>();

        public HardwareTypeData() { }

        /// <summary>
        /// Converts data to JSON
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, object> GetData() {
            return (IReadOnlyDictionary<string, object>)_rawData;
        }

        public void Update(IReadOnlyDictionary<string, object> data) {
            data.ForEach(kvp => _rawData[kvp.Key] = kvp.Value);
        }

        public static RioDataAttribute GetMetaData<T>() where T : HardwareTypeData {
            var attr = Attribute.GetCustomAttributes(typeof(T)).Where(x => x is RioDataAttribute);
            if (!attr.Any() || !(attr.First() is RioDataAttribute))
                throw new Exception($"Data type incorrectly registered: \"{typeof(T).FullName}\"");

            return (RioDataAttribute)attr.First();
        }

        public abstract RioDataAttribute GetMetaData();
    }

    public class RioDataAttribute : Attribute {
        private readonly string _typeName;
        public string RegisteredTypeName => _typeName;

        internal RioDataAttribute(string registeredTypeName) {
            _typeName = registeredTypeName;
        }
    }
}
