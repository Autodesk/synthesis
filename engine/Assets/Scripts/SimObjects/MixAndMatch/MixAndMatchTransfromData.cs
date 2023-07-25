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


        public static void SavePart(MixAndMatchPartData part) {
            if (!Directory.Exists(PART_FOLDER_PATH)) {
                Directory.CreateDirectory(PART_FOLDER_PATH);
            }
            var filePath = Path.GetFullPath(PART_FOLDER_PATH)+ALT_SEP+part.Name+".json";

            File.WriteAllText(filePath, JsonUtility.ToJson(part));
        }

        public static MixAndMatchPartData LoadPart(string fileName) {
            var filePath = Path.GetFullPath(PART_FOLDER_PATH)+ALT_SEP+fileName+".json";
            
            if (!Directory.Exists(PART_FOLDER_PATH)) {
                // TODO: Create an empty part
                throw new Exception($"Part {fileName} not found");
            }
            
            return JsonUtility.FromJson<MixAndMatchPartData>(File.ReadAllBytes(filePath).ToString());
        }
        
        public static void SaveRobot(MixAndMatchRobotData robot) {
            if (!Directory.Exists(ROBOT_FOLDER_PATH)) {
                Directory.CreateDirectory(ROBOT_FOLDER_PATH);
            }
            var filePath = Path.GetFullPath(ROBOT_FOLDER_PATH)+ALT_SEP+robot.Name+".json";

            File.WriteAllText(filePath, JsonUtility.ToJson(robot));
        }
        
        public static MixAndMatchRobotData LoadRobot(string fileName) {
            var filePath = Path.GetFullPath(ROBOT_FOLDER_PATH)+ALT_SEP+fileName+".json";
            
            if (!Directory.Exists(ROBOT_FOLDER_PATH)) {
                // TODO: Create an empty part
                throw new Exception($"Robot {fileName} not found");
            }
            
            return JsonUtility.FromJson<MixAndMatchRobotData>(File.ReadAllBytes(filePath).ToString());
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
        
        public ConnectionPoint[] ConnectionPoints;
        
        public MixAndMatchPartData ConnectedPart;
        
        // TODO: figure out how to handle part rotations
        // TODO: store which mesh this corresponds to without just using the index
        [JsonIgnore] public int PartIndex;
        [JsonIgnore] public string Name;

        public MixAndMatchPartData(string name, Vector3 localPosition, Quaternion localRotation, (Vector3 position, Vector3 normal)[] connectionPoints) {
            LocalPosition = localPosition;
            ConnectionPoints = connectionPoints.Select(c => new ConnectionPoint(c.position, c.normal)).ToArray();
            LocalRotation = localRotation;
            Name = name;
        }
    }

    [Serializable]
    public class ConnectionPoint {
        public Vector3 Position;
        public Vector3 Normal;

        public ConnectionPoint(Vector3 position, Vector3 normal) {
            Position = position;
            Normal = normal;
        }
    }
}