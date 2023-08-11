using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace SimObjects.MixAndMatch {
    public static class MixAndMatchSaveUtil {
        private static readonly char ALT_SEP = Path.AltDirectorySeparatorChar;

        private static readonly string PART_MIRABUF_FOLDER_PATH =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + ALT_SEP + "Autodesk" + ALT_SEP +
            "Synthesis" + ALT_SEP + "MixAndMatch" + ALT_SEP + "Mira";

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

        /// <summary>An array of all part mirabuf files found in the appdata folder</summary>
        public static string[] PartMirabufFiles {
            get {
                if (!Directory.Exists(PART_MIRABUF_FOLDER_PATH))
                    Directory.CreateDirectory(PART_MIRABUF_FOLDER_PATH);
                return Directory.GetFiles(PART_MIRABUF_FOLDER_PATH)
                    .Where(x => Path.GetExtension(x).Equals(".mira"))
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
        public static MixAndMatchPartData LoadPartData(string fileName, bool createNewIfNoExist = true) {
            if (!Directory.Exists(PART_FOLDER_PATH)) {
                Directory.CreateDirectory(PART_FOLDER_PATH);
            }

            var filePath = Path.GetFullPath(PART_FOLDER_PATH) + ALT_SEP + fileName + ".json";

            if (!File.Exists(filePath)) {
                if (createNewIfNoExist)
                    return CreateNewPart(fileName);
                return null;
            }

            var part = JsonUtility.FromJson<MixAndMatchPartData>(File.ReadAllText(filePath));

            if (part.Guid is null or "")
                part.Guid = GUID.Generate().ToString();

            return part;
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
            return new MixAndMatchPartData(
                name, GUID.Generate().ToString(), mirabufFile, Array.Empty<(Vector3, Quaternion)>());
        }

        /// <summary>Creates a new mix and match robot with no parts</summary>
        public static MixAndMatchRobotData CreateNewRobot(string name) {
            return new MixAndMatchRobotData(name, Array.Empty<RobotPartTransformData>());
        }

        /// <summary>Deletes the part if it exists</summary>
        public static void DeletePart(string fileName) {
            var filePath = Path.GetFullPath(PART_FOLDER_PATH) + ALT_SEP + fileName + ".json";
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        /// <summary>Deletes the robot if it exists</summary>
        public static void DeleteRobot(string fileName) {
            var filePath = Path.GetFullPath(ROBOT_FOLDER_PATH) + ALT_SEP + fileName + ".json";
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    /// <summary>Stores info about a robot including the positions of it's parts. Only ever saved in it's own json
    /// file.</summary>
    [Serializable]
    public class MixAndMatchRobotData {
        public string Name;
        public RobotPartTransformData[] PartTransformData;

        public string RobotPreferencesJson;

        [JsonIgnore]
        private MixAndMatchPartData[] _globalPartData;

        [JsonIgnore]
        public MixAndMatchPartData[] GlobalPartData {
            get {
                if (_globalPartData != null)
                    return _globalPartData;

                _globalPartData =
                    PartTransformData.Select(part => MixAndMatchSaveUtil.LoadPartData(part.FileName)).ToArray();

                return _globalPartData;
            }
        }

        public MixAndMatchRobotData(string name, RobotPartTransformData[] transforms) {
            Name                 = name;
            PartTransformData    = transforms;
            RobotPreferencesJson = "";
        }

        public int PartGuidToIndex(string partGuid) {
            int index = 0;

            foreach (var part in GlobalPartData) {
                if (partGuid == part.Guid)
                    return index;

                index++;
            }
            return -1;
        }
    }

    /// <summary>Robot specific part data (position, parent node). Only ever saved in a robot's json file.</summary>
    [Serializable]
    public struct RobotPartTransformData {
        public string FileName;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public ParentNodeData ParentNodeData;

        public RobotPartTransformData(
            string fileName, ParentNodeData parentNodeData, Vector3 position, Quaternion rotation) {
            FileName       = fileName;
            ParentNodeData = parentNodeData;
            LocalPosition  = position;
            LocalRotation  = rotation;
        }

        public (string name, ParentNodeData parentNodeData, Vector3 position, Quaternion rotation) ToTuple() {
            return (FileName, ParentNodeData, LocalPosition, LocalRotation);
        }
    }

    /// <summary>Stores a parts mirabuf file and connection points. Only ever saved in it's own json file.</summary>
    [Serializable]
    public class MixAndMatchPartData {
        [JsonIgnore]
        public string Name; // Ignored because it is the filename
        public string Guid;
        public string MirabufPartFile;

        public ConnectionPointData[] ConnectionPoints;

        public MixAndMatchPartData(string name, string guid, string mirabufPartFile,
            (Vector3 position, Quaternion rotation)[] connectionPoints) {
            Name             = name;
            MirabufPartFile  = mirabufPartFile;
            ConnectionPoints = connectionPoints.Select(c => new ConnectionPointData(c.position, c.rotation)).ToArray();
            Guid             = guid;
        }
    }

    /// <summary>The position and direction of a part's connection point. Only ever saved in a part's json
    /// file.</summary>
    [Serializable]
    public class ConnectionPointData {
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;

        public ConnectionPointData(Vector3 localPosition, Quaternion localRotation) {
            LocalPosition = localPosition;
            LocalRotation = localRotation;
        }

        public ConnectionPointData() {
            LocalPosition = Vector3.zero;
            LocalRotation = Quaternion.identity;
        }
    }

    /// <summary>Stores data about what a part is parented to. Only ever saved in a part's json file</summary>
    [Serializable]
    public class ParentNodeData {
        public string PartGuid;
        public string NodeName;

        public ParentNodeData(string partGuid, string nodeName) {
            PartGuid = partGuid;
            NodeName = nodeName;
        }
    }
}