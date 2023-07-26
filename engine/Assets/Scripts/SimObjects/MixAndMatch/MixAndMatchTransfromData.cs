using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace SimObjects.MixAndMatch {
    public static class MixAndMatchSaveUtil {
        private static readonly char ALT_SEP = Path.AltDirectorySeparatorChar;

        private static readonly string MIX_AND_MATCH_FOLDER_PATH =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
            ALT_SEP + "Autodesk" + ALT_SEP + "Synthesis" + ALT_SEP + "MixAndMatch";

        private static readonly string PART_FOLDER_PATH = MIX_AND_MATCH_FOLDER_PATH + ALT_SEP + "Parts";
        private static readonly string ROBOT_FOLDER_PATH = MIX_AND_MATCH_FOLDER_PATH + ALT_SEP + "Robots";

        public static string[] PartFiles {
            get {
                if (!Directory.Exists(PART_FOLDER_PATH))
                    Directory.CreateDirectory(PART_FOLDER_PATH);
                return Directory.GetFiles(PART_FOLDER_PATH).Select(Path.GetFileNameWithoutExtension).ToArray();
            }
        }

        public static string[] RobotFiles {
            get {
                if (!Directory.Exists(ROBOT_FOLDER_PATH))
                    Directory.CreateDirectory(ROBOT_FOLDER_PATH);
                return Directory.GetFiles(ROBOT_FOLDER_PATH).Select(Path.GetFileNameWithoutExtension).ToArray();
            }
        }

        public static void SavePartData(MixAndMatchPartData part) {
            if (!Directory.Exists(PART_FOLDER_PATH)) {
                Directory.CreateDirectory(PART_FOLDER_PATH);
            }

            var filePath = Path.GetFullPath(PART_FOLDER_PATH) + ALT_SEP + part.Name + ".json";

            File.WriteAllText(filePath, JsonUtility.ToJson(part));
        }

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

        public static void SaveRobotData(MixAndMatchRobotData robot) {
            if (!Directory.Exists(ROBOT_FOLDER_PATH)) {
                Directory.CreateDirectory(ROBOT_FOLDER_PATH);
            }

            var filePath = Path.GetFullPath(ROBOT_FOLDER_PATH) + ALT_SEP + robot.Name + ".json";

            File.WriteAllText(filePath, JsonUtility.ToJson(robot));
        }

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

        /// <summary>Creates a new mix and match part</summary>
        private static MixAndMatchPartData CreateNewPart(string name) {
            return new MixAndMatchPartData(name, "", Array.Empty<(Vector3, Vector3)>());
        }

        /// <summary>Creates a new mix and match robot</summary>
        private static MixAndMatchRobotData CreateNewRobot(string name) {
            return new MixAndMatchRobotData(name, Array.Empty<(string, Vector3, Quaternion)>());
        }
    }

    [Serializable]
    public class MixAndMatchRobotData {
        public string Name;
        private Tuple<string, Vector3, Quaternion>[] _parts;

        [JsonIgnore]
        public (string fileName, Vector3 localPosition, Quaternion localRotation)[] Parts {
            get => _parts?.Select(p => (p.Item1, p.Item2, p.Item3)).ToArray();
            set => _parts = value?.Select(p => p.ToTuple()).ToArray();
        }

        public MixAndMatchRobotData(string name, (string fileName, Vector3 localPosition, Quaternion localRotation)[] parts) {
            Name = name;
            _parts = parts.Select(p => p.ToTuple()).ToArray();
        }
    }

    [Serializable]
    public class MixAndMatchPartData {
        public string MirabufPartFile;
        [JsonIgnore] public string Name; // Ignored because it is the filename

        public ConnectionPointData[] ConnectionPoints;
        
        public string ConnectedPart;

        public MixAndMatchPartData(string name, string mirabufPartFile, (Vector3 position, Vector3 normal)[] connectionPoints) {
            Name = name;
            MirabufPartFile = mirabufPartFile;
            ConnectionPoints = connectionPoints.Select(c => new ConnectionPointData(c.position, c.normal)).ToArray();
        }
    }

    [Serializable]
    public class ConnectionPointData {
        public Vector3 Position;
        public Vector3 Normal;

        public ConnectionPointData(Vector3 position, Vector3 normal) {
            Position = position;
            Normal = normal;
        }

        public ConnectionPointData() {
            Position = Vector3.zero;
            Normal = Vector3.forward;
        }
    }
}