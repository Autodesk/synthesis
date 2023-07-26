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
                return Directory.GetFiles(ROBOT_FOLDER_PATH).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();
            }
        }

        public static void SavePartData(MixAndMatchPartData part) {
            if (!Directory.Exists(PART_FOLDER_PATH)) {
                Directory.CreateDirectory(PART_FOLDER_PATH);
            }
            var filePath = Path.GetFullPath(PART_FOLDER_PATH)+ALT_SEP+part.Name+".json";

            File.WriteAllText(filePath, JsonUtility.ToJson(part));
        }

        public static MixAndMatchPartData LoadPartData(string fileName) {
            var filePath = Path.GetFullPath(PART_FOLDER_PATH)+ALT_SEP+fileName+".json";
            
            if (!Directory.Exists(PART_FOLDER_PATH)) {
                // TODO: Create a new part if it does not already exist
                throw new Exception($"Part {fileName} not found");
            }
            
            return JsonUtility.FromJson<MixAndMatchPartData>(File.ReadAllText(filePath));
        }
        
        public static void SaveRobotData(MixAndMatchRobotData robot) {
            if (!Directory.Exists(ROBOT_FOLDER_PATH)) {
                Directory.CreateDirectory(ROBOT_FOLDER_PATH);
            }
            var filePath = Path.GetFullPath(ROBOT_FOLDER_PATH)+ALT_SEP+robot.Name+".json";

            File.WriteAllText(filePath, JsonUtility.ToJson(robot));
        }
        
        public static MixAndMatchRobotData LoadRobotData(string fileName) {
            var filePath = Path.GetFullPath(ROBOT_FOLDER_PATH)+ALT_SEP+fileName+".json";
            
            if (!Directory.Exists(ROBOT_FOLDER_PATH)) {
                // TODO: Create a new robot if it does not already exist
                throw new Exception($"Robot {fileName} not found");
            }
            
            return JsonUtility.FromJson<MixAndMatchRobotData>(File.ReadAllText(filePath));
        }
    }

    public class MixAndMatchRobotData {
        public string Name;
        public MixAndMatchPartData[] Parts;

        public MixAndMatchRobotData(string name, MixAndMatchPartData[] parts) {
            Name = name;
            Parts = parts;
            
            parts.ForEachIndex((i, p) => {
                p.PartIndex = i;
            });
        }
    }

    [Serializable]
    public class MixAndMatchPartData {
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        
        public ConnectionPointData[] ConnectionPoints;
        
        public MixAndMatchPartData ConnectedPart;
        
        // TODO: figure out how to handle part rotations
        // TODO: store which mesh this corresponds to without just using the index
        [JsonIgnore] public int PartIndex;
        [JsonIgnore] public string Name;

        public MixAndMatchPartData(string name, Vector3 localPosition, Quaternion localRotation, (Vector3 position, Vector3 normal)[] connectionPoints) {
            LocalPosition = localPosition;
            ConnectionPoints = connectionPoints.Select(c => new ConnectionPointData(c.position, c.normal)).ToArray();
            LocalRotation = localRotation;
            Name = name;
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