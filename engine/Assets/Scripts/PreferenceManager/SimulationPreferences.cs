using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;
using Synthesis.Import;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Utilities;
using UnityEngine;
using Synthesis.WS.Translation;
using UnityEngine.Windows;
using ITD = RobotSimObject.IntakeTriggerData;
using STD = RobotSimObject.ShotTrajectoryData;

#nullable enable

namespace Synthesis.PreferenceManager {
    // Funky inner setup so I can control the lifetime of the instance
    public static class SimulationPreferences {
        public const string ALL_ROBOT_DATA_KEY = "all_robot_data";
        public const string ALL_FIELD_DATA_KEY = "all_field_data";

        // Why, again?
        public static void DestroyInstance() {
            _instance = null;
        }

        public static void LoadRobotFromMira(MirabufLive live) => Instance.LoadRobotFromMira(live);
        public static void LoadFieldFromMira(MirabufLive live) => Instance.LoadFieldFromMira(live);

        public static Analog GetRobotInput(string robot, string input) => Instance.GetRobotInput(robot, input);

        public static Dictionary<string, Analog> GetRobotInputs(string robot) => Instance.GetRobotInputs(robot);

        public static JointMotor? GetRobotJointMotor(string robot, string motorKey) => Instance.GetRobotJointMotor(
            robot, motorKey);

        public static float? GetRobotJointSpeed(string robot, string speedKey) => Instance.GetRobotJointSpeed(
            robot, speedKey);

        public static ITD? GetRobotIntakeTriggerData(string robot) => Instance.GetRobotIntakeTriggerData(robot);

        public static STD? GetRobotTrajectoryData(string robot) => Instance.GetTrajectoryData(robot);

        public static RioTranslationLayer? GetRobotSimTranslationLayer(string robot) => Instance.GetSimTranslationLayer(
            robot);

        public static RobotSimObject.DrivetrainType GetRobotDrivetrain(string robot) => Instance.GetRobotDrivetrainType(
            robot);

        public static void SetRobotInput(string robot, string inputKey, Analog inputValue) {
            Instance.SetRobotInput(robot, inputKey, inputValue);
        }

        public static void SetRobotJointMotor(string robot, string motorKey, JointMotor motor) {
            Instance.SetRobotJointMotor(robot, motorKey, motor);
        }

        public static void SetRobotJointSpeed(string robot, string speedKey, float speed) {
            Instance.SetRobotJointSpeed(robot, speedKey, speed);
        }

        public static void SetRobotIntakeTriggerData(string robot, ITD? data) {
            Instance.SetRobotIntakeTriggerData(robot, data);
        }

        public static void SetRobotTrajectoryData(string robot, STD? data) {
            Instance.SetRobotTrajectoryData(robot, data);
        }

        public static void SetRobotSimTranslationLayer(string robot, RioTranslationLayer layer) {
            Instance.SetRobotSimTranslationLayer(robot, layer);
        }

        public static void SetRobotDrivetrainType(string robot,
            RobotSimObject.DrivetrainType drivetrainType) => Instance.SetRobotDrivetrainType(robot, drivetrainType);

        public static IReadOnlyCollection<ScoringZoneData>? GetFieldScoringZones(string field)
            => Instance.GetFieldScoringZones(field);

        public static void SetFieldScoringZones(string field, List<ScoringZoneData> zones)
            => Instance.SetFieldScoringZones(field, zones);
        
        private class Inner {
            public const string USER_DATA_KEY = "saved-data";

            private readonly Dictionary<string, RobotData> _allRobotData;
            private readonly Dictionary<string, FieldData> _allFieldData;

            public Inner() {
                EventBus.NewTypeListener<PrePreferenceSaveEvent>(PreSaveDump);

                if (!PreferenceManager.AnyPrefs)
                    PreferenceManager.Load(); // A way of making sure data is loaded first?

                if (PreferenceManager.ContainsPreference(ALL_ROBOT_DATA_KEY)) {
                    _allRobotData = PreferenceManager.GetPreference<Dictionary<string, RobotData>>(ALL_ROBOT_DATA_KEY);
                } else {
                    _allRobotData = new Dictionary<string, RobotData>();
                    PreferenceManager.SetPreference(ALL_ROBOT_DATA_KEY, _allRobotData);
                }

                if (PreferenceManager.ContainsPreference(ALL_FIELD_DATA_KEY)) {
                    _allFieldData = PreferenceManager.GetPreference<Dictionary<string, FieldData>>(ALL_FIELD_DATA_KEY);
                } else {
                    _allFieldData = new Dictionary<string, FieldData>();
                    PreferenceManager.SetPreference(ALL_FIELD_DATA_KEY, _allFieldData);
                }
            }

            /// <summary>
            /// Load all the necessary data into PreferenceManager before it is saved
            /// </summary>
            public void PreSaveDump(IEvent _) {
                // PreferenceManager.SetPreference(ALL_ROBOT_DATA_KEY, _allRobotData);
                if (RobotSimObject.CurrentlyPossessedRobot != string.Empty) {
                    var live = RobotSimObject.GetCurrentlyPossessedRobot().MiraLive;
                    live.MiraAssembly.Data.Parts.UserData ??= new Mirabuf.UserData();
                    live.MiraAssembly.Data.Parts.UserData.Data[USER_DATA_KEY] =
                        JsonConvert.SerializeObject(_allRobotData[live.MiraAssembly.Info.GUID]);
                    live.Save();
                }

                if (FieldSimObject.CurrentField != null) {
                    var live = FieldSimObject.CurrentField.MiraLive;
                    live.MiraAssembly.Data.Parts.UserData ??= new Mirabuf.UserData();
                    live.MiraAssembly.Data.Parts.UserData.Data[USER_DATA_KEY] =
                        JsonConvert.SerializeObject(_allFieldData[live.MiraAssembly.Info.GUID]);
                    live.Save();
                }
            }

            public void LoadRobotFromMira(MirabufLive live) {
                if (live.MiraAssembly.Data.Parts.UserData != null &&
                    live.MiraAssembly.Data.Parts.UserData.Data.ContainsKey(USER_DATA_KEY)) {
                    _allRobotData[live.MiraAssembly.Info.GUID] = JsonConvert.DeserializeObject<RobotData>(
                        live.MiraAssembly.Data.Parts.UserData.Data[USER_DATA_KEY])!;
                }
            }

            public void LoadFieldFromMira(MirabufLive live) {
                if (live.MiraAssembly.Data.Parts.UserData != null &&
                    live.MiraAssembly.Data.Parts.UserData.Data.ContainsKey(USER_DATA_KEY)) {
                    _allFieldData[live.MiraAssembly.Info.GUID] = JsonConvert.DeserializeObject<FieldData>(
                        live.MiraAssembly.Data.Parts.UserData.Data[USER_DATA_KEY])!;
                }
            }
            
#region Robot Data

#region Getters

            public Analog GetRobotInput(string robot, string input) {
                if (!_allRobotData.ContainsKey(robot))
                    return null;

                var rData = _allRobotData[robot];
                if (!rData.InputData.ContainsKey(input))
                    return null;

                return rData.InputData[input].GetInput();
            }

            public Dictionary<string, Analog> GetRobotInputs(string robot) {
                if (!_allRobotData.ContainsKey(robot))
                    return null;

                var inputs = new Dictionary<string, Analog>();
                var rData  = _allRobotData[robot];
                rData.InputData.ForEach(x => inputs.Add(x.Key, x.Value.GetInput()));
                return inputs;
            }

            public JointMotor? GetRobotJointMotor(string robot, string motorKey) {
                if (!_allRobotData.ContainsKey(robot))
                    return null;

                var motors = _allRobotData[robot].JointMotors;
                if (!motors.ContainsKey(motorKey))
                    return null;

                return motors[motorKey];
            }

            public float? GetRobotJointSpeed(string robot, string speedKey) {
                if (!_allRobotData.ContainsKey(robot))
                    return null;

                var speeds = _allRobotData[robot].JointSpeeds;
                if (!speeds.ContainsKey(speedKey))
                    return null;

                return speeds[speedKey];
            }

            public ITD? GetRobotIntakeTriggerData(string robot) {
                if (!_allRobotData.ContainsKey(robot))
                    return null;

                return _allRobotData[robot].IntakeTrigger;
            }

            public STD? GetTrajectoryData(string robot) {
                if (!_allRobotData.ContainsKey(robot))
                    return null;

                return _allRobotData[robot].TrajectoryPointer;
            }

            public RioTranslationLayer? GetSimTranslationLayer(string robot) {
                if (!_allRobotData.ContainsKey(robot))
                    return null;

                return _allRobotData[robot].SimTranslationLayer;
            }

            public RobotSimObject.DrivetrainType GetRobotDrivetrainType(string robot) {
                if (!_allRobotData.ContainsKey(robot))
                    return RobotSimObject.DrivetrainType.ARCADE;
                return _allRobotData[robot].DrivetrainType ?? RobotSimObject.DrivetrainType.ARCADE;
            }
            
#endregion
            
#region Setters

            public void SetRobotInput(string robot, string inputKey, Analog inputValue) {
                if (!_allRobotData.ContainsKey(robot))
                    _allRobotData[robot] = new RobotData(robot);
                var rData                 = _allRobotData[robot];
                rData.InputData[inputKey] = new InputData(inputValue);
                // _allRobotData[robot] = rData; // I changed it to a class so Im not sure if this is needed
            }

            public void SetRobotJointMotor(string robot, string motorKey, JointMotor m) {
                if (!_allRobotData.ContainsKey(robot))
                    _allRobotData[robot] = new RobotData(robot);
                var rData                   = _allRobotData[robot];
                rData.JointMotors[motorKey] = m;
                // _allRobotData[robot] = rData;
            }

            public void SetRobotJointSpeed(string robot, string speedKey, float speed) {
                if (!_allRobotData.ContainsKey(robot))
                    _allRobotData[robot] = new RobotData(robot);
                var rData                   = _allRobotData[robot];
                rData.JointSpeeds[speedKey] = speed;
                // _allRobotData[robot] = rData;
            }

            public void SetRobotIntakeTriggerData(string robot, ITD? data) {
                if (!_allRobotData.ContainsKey(robot))
                    _allRobotData[robot] = new RobotData(robot);
                _allRobotData[robot].IntakeTrigger = data;
            }

            public void SetRobotTrajectoryData(string robot, STD? data) {
                if (!_allRobotData.ContainsKey(robot))
                    _allRobotData[robot] = new RobotData(robot);
                _allRobotData[robot].TrajectoryPointer = data;
            }

            public void SetRobotSimTranslationLayer(string robot, RioTranslationLayer layer) {
                if (!_allRobotData.ContainsKey(robot))
                    _allRobotData[robot] = new RobotData(robot);
                _allRobotData[robot].SimTranslationLayer = layer;
            }

            public void SetRobotDrivetrainType(string robot, RobotSimObject.DrivetrainType drivetrainType) {
                if (!_allRobotData.ContainsKey(robot))
                    _allRobotData[robot] = new RobotData(robot);
                _allRobotData[robot].DrivetrainType = drivetrainType;
            }

#endregion
            
#endregion

#region Field Data

#region Getters

            public IReadOnlyCollection<ScoringZoneData>? GetFieldScoringZones(string field) {
                if (!_allFieldData.ContainsKey(field))
                    return null;
                
                return _allFieldData[field].ScoringZones.AsReadOnly();
            }

#endregion

#region Setters

            public void SetFieldScoringZones(string field, List<ScoringZoneData> zones) {
                _allFieldData.TryGetValue(field, out var data);
                data ??= new FieldData(field);
                data.ScoringZones.Clear();
                data.ScoringZones.AddRange(zones);
                _allFieldData[field] = data;
            }

#endregion

#endregion

        }

        private static Inner _instance;
        private static Inner Instance {
            get {
                if (_instance == null) {
                    _instance = new Inner();
                }
                return _instance;
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class RobotData {
        [JsonConstructor]
        public RobotData() {
            AssemblyGuid = string.Empty;
            InputData    = new Dictionary<string, InputData>();
            JointMotors  = new Dictionary<string, JointMotor>();
            JointSpeeds  = new Dictionary<string, float>();
        }

        public RobotData(string guid) {
            AssemblyGuid = guid;
            InputData    = new Dictionary<string, InputData>();
            JointMotors  = new Dictionary<string, JointMotor>();
            JointSpeeds  = new Dictionary<string, float>();
        }

        [JsonProperty] public string AssemblyGuid;
        [JsonProperty] public Dictionary<string, InputData> InputData;
        [JsonProperty] public Dictionary<string, JointMotor> JointMotors;
        [JsonProperty] public Dictionary<string, float> JointSpeeds;
        [JsonProperty] public ITD? IntakeTrigger;
        [JsonProperty] public STD? TrajectoryPointer;
        [JsonProperty] public RioTranslationLayer? SimTranslationLayer;
        [JsonProperty] public RobotSimObject.DrivetrainType? DrivetrainType;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class FieldData {
        [JsonConstructor]
        public FieldData() {
            AssemblyGuid = string.Empty;
            ScoringZones = new List<ScoringZoneData>();
        }
        
        public FieldData(string guid) {
            AssemblyGuid = guid;
            ScoringZones = new List<ScoringZoneData>();
        }
        
        [JsonProperty] public string AssemblyGuid;
        [JsonProperty] public List<ScoringZoneData> ScoringZones;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class InputData {
        [JsonProperty] public Type Type;
        [JsonProperty] public string Data;

        [JsonConstructor]
        public InputData() {}

        public InputData(Analog input) {
            this.Type = input.GetType();
            Data      = JsonConvert.SerializeObject(input);
        }

        private static MethodInfo _deserializeMethod;
        private static MethodInfo DeserializeMethod {
            get {
                if (_deserializeMethod == null)
                    _deserializeMethod = typeof(JsonConvert)
                                             .GetMethods()
                                             .First(y => y.IsGenericMethod && y.Name.Equals("DeserializeObject"));
                return _deserializeMethod;
            }
        }

        public Analog GetInput() => (Analog) DeserializeMethod.MakeGenericMethod(this.Type).Invoke(
            null, new string[] { Data });
    }

    // [JsonObject(MemberSerialization.OptIn)]
    // public class JsonFriendlyData<T> {
    //     [JsonProperty]
    //     public Type Type;
    //     [JsonProperty]
    //     public string Data;

    //     [JsonConstructor]
    //     public JsonFriendlyData() { }

    //     public JsonFriendlyData(T input) {
    //         this.Type = input.GetType();
    //         Data = JsonConvert.SerializeObject(input);
    //     }

    //     private static MethodInfo _deserializeMethod;
    //     private static MethodInfo DeserializeMethod {
    //         get {
    //             if (_deserializeMethod == null)
    //                 _deserializeMethod = typeof(JsonConvert).GetMethods().First(y => y.IsGenericMethod &&
    //                 y.Name.Equals("DeserializeObject"));
    //             return _deserializeMethod;
    //         }
    //     }

    //     public T GetData()
    //         => (T)DeserializeMethod.MakeGenericMethod(this.Type).Invoke(null, new string[] { Data });
    // }
}