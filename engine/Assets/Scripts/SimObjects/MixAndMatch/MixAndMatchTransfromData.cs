using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace SimObjects.MixAndMatch {
    public static class MixAndMatchSaveUtil {
        private static readonly char ALT_SEP = Path.AltDirectorySeparatorChar;
        
        private static readonly string PART_MIRABUF_FOLDER_PATH = 
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + ALT_SEP + "Autodesk" + ALT_SEP +
                                                                  "Synthesis" + ALT_SEP + "Mira";

        private static readonly string MIX_AND_MATCH_FOLDER_PATH =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + ALT_SEP + "Autodesk" + ALT_SEP +
            "Synthesis" + ALT_SEP + "MixAndMatch";

        private static readonly string PART_FOLDER_PATH  = MIX_AND_MATCH_FOLDER_PATH + ALT_SEP + "Parts";
        private static readonly string ROBOT_FOLDER_PATH = MIX_AND_MATCH_FOLDER_PATH + ALT_SEP + "Robots";

        /// <summary>An array of all part files found in the appdata folder</summary>
        public static string[] PartFiles {
            get {
                if (!Directory.Exists(PART_FOLDER_PATH))
                    Directory.CreateDirectory(PART_FOLDER_PATH);
                return Directory.GetFiles(PART_FOLDER_PATH).Select(Path.GetFileNameWithoutExtension).ToArray();
            }
        }

        /// <summary>An array of all robot files found in the appdata folder</summary>
        public static string[] RobotFiles {
            get {
                if (!Directory.Exists(ROBOT_FOLDER_PATH))
                    Directory.CreateDirectory(ROBOT_FOLDER_PATH);
                return Directory.GetFiles(ROBOT_FOLDER_PATH).Select(Path.GetFileNameWithoutExtension).ToArray();
            }
        }

        // TODO: Create a separate folder for mix and match mirabuf files
        /// <summary>An array of all part mirabuf files found in the appdata folder</summary>
        public static string[] PartMirabufFiles {
            get {
                if (!Directory.Exists(PART_MIRABUF_FOLDER_PATH))
                    Directory.CreateDirectory(PART_MIRABUF_FOLDER_PATH);
                return Directory.GetFiles(PART_MIRABUF_FOLDER_PATH).Where(x => Path.GetExtension(x).Equals(".mira"))
                    .ToArray();
            }
        }

        /// <summary>Save a part to the appdata folder. Overrides an existing part with the same name</summary>
        public static void SavePartData(MixAndMatchPartData part) {
            if (!Directory.Exists(PART_FOLDER_PATH)) {
                Directory.CreateDirectory(PART_FOLDER_PATH);
            }

            var filePath = Path.GetFullPath(PART_FOLDER_PATH) + ALT_SEP + part.Name + ".json";

            File.WriteAllText(filePath, JsonUtility.ToJson(part));
        }

        /// <summary>Load a part from the appdata folder. If no found, a blank part with be created</summary>
        public static MixAndMatchPartData LoadPartData(string fileName) {
            if (!Directory.Exists(PART_FOLDER_PATH)) {
                Directory.CreateDirectory(PART_FOLDER_PATH);
            }

            var filePath = Path.GetFullPath(PART_FOLDER_PATH) + ALT_SEP + fileName + ".json";

            if (!File.Exists(filePath)) {
                return CreateNewPart(fileName);
            }

            return JsonUtility.FromJson<MixAndMatchPartData>(File.ReadAllText(filePath));
        }

        /// <summary>Save a robot to the appdata folder. Overrides an existing robot with the same name</summary>
        public static void SaveRobotData(MixAndMatchRobotData robot) {
            if (!Directory.Exists(ROBOT_FOLDER_PATH)) {
                Directory.CreateDirectory(ROBOT_FOLDER_PATH);
            }

            var filePath = Path.GetFullPath(ROBOT_FOLDER_PATH) + ALT_SEP + robot.Name + ".json";

            File.WriteAllText(filePath, JsonUtility.ToJson(robot));
        }

        /// <summary>Load a robot from the appdata folder. If no found, a new robot with be created</summary>
        public static MixAndMatchRobotData LoadRobotData(string fileName) {
            if (!Directory.Exists(ROBOT_FOLDER_PATH)) {
                Directory.CreateDirectory(ROBOT_FOLDER_PATH);
            }

            var filePath = Path.GetFullPath(ROBOT_FOLDER_PATH) + ALT_SEP + fileName + ".json";

            if (!File.Exists(filePath)) {
                return CreateNewRobot(fileName);
            }

            return JsonUtility.FromJson<MixAndMatchRobotData>(File.ReadAllText(filePath));
        }

        /// <summary>Creates a new mix and match part with no connection points</summary>
        public static MixAndMatchPartData CreateNewPart(string name, string mirabufFile = "") {
            return new MixAndMatchPartData(name, mirabufFile, Array.Empty<(Vector3, Quaternion)>());
        }

        /// <summary>Creates a new mix and match robot with no parts</summary>
        public static MixAndMatchRobotData CreateNewRobot(string name) {
            return new MixAndMatchRobotData(name, Array.Empty<(string, Vector3, Quaternion)>());
        }
    }

    /// <summary>Stores info about a robot including the positions of it's parts. Always saved in it's own json
    /// file.</summary>
    [Serializable]
    public class MixAndMatchRobotData {
        public string Name;
        public SerializableTransformData[] SerializablePartData;

        [JsonIgnore]
        public (string fileName, Vector3 localPosition, Quaternion localRotation)[] PartData {
            get => SerializablePartData?.Select(p => p.ToTuple()).ToArray();
            set {
                SerializablePartData =
                    value?.Select(p => new SerializableTransformData((p.fileName, p.localPosition, p.localRotation)))
                        .ToArray();
            }
        }

        public MixAndMatchRobotData(
            string name, (string fileName, Vector3 localPosition, Quaternion localRotation)[] parts) {
            Name = name;
            SerializablePartData =
                parts.Select(p => new SerializableTransformData((p.fileName, p.localPosition, p.localRotation)))
                    .ToArray();
        }
    }

    /// <summary>Stores where a part is positioned relative to a robot, not specific info about it. Always saved within
    /// a robot's json file.</summary>
    [Serializable]
    public struct SerializableTransformData {
        public string FileName;
        public Vector3 Position;
        public Quaternion Rotation;

        public SerializableTransformData((string name, Vector3 position, Quaternion rotation) partData) {
            FileName = partData.name;
            Position = partData.position;
            Rotation = partData.rotation;
        }

        public (string name, Vector3 position, Quaternion rotation) ToTuple() {
            return (FileName, Position, Rotation);
        }
    }

    /// <summary>Stores a parts mirabuf file and connection points. Always saved in it's own json file.</summary>
    [Serializable]
    public class MixAndMatchPartData {
        public string MirabufPartFile;
        [JsonIgnore]
        public string Name; // Ignored because it is the filename

        public ConnectionPointData[] ConnectionPoints;

        public string ConnectedPart;

        public MixAndMatchPartData(
            string name, string mirabufPartFile, (Vector3 position, Quaternion rotation)[] connectionPoints) {
            Name             = name;
            MirabufPartFile  = mirabufPartFile;
            ConnectionPoints = connectionPoints.Select(c => new ConnectionPointData(c.position, c.rotation)).ToArray();
        }
    }

    /// <summary>The position and direction of a part's connection point. Always saved in a part's json file.</summary>
    [Serializable]
    public class ConnectionPointData {
        public Vector3 LocalPosition;
        [FormerlySerializedAs("Rotation")] public Quaternion LocalRotation;

        public ConnectionPointData(Vector3 localPosition, Quaternion localRotation) {
            LocalPosition = localPosition;
            LocalRotation   = localRotation;
        }

        public ConnectionPointData() {
            LocalPosition = Vector3.zero;
            LocalRotation   = Quaternion.identity;
        }
    }
}