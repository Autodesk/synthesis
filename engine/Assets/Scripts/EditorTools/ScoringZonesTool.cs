// This is a tool only to be used in the unity editor. It should not be included in any builds
#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using EditorTools;
using Newtonsoft.Json;
using Synthesis.Import;
using Synthesis.PreferenceManager;
using UI.Dynamic.Modals.Spawning;
using UnityEditor;
using UnityEngine;

namespace EditorTools {
    [ExecuteAlways]
    public class ScoringZonesTool : MonoBehaviour {
        private const string FOLDER = "Mira/Fields";
        private const string FIELD_TAG = "field";

        public string[] FieldFiles {
            get { 
                var root = AddRobotModal.ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", FOLDER), '/');
                
                if (!Directory.Exists(root))
                    Directory.CreateDirectory(root);

                return Directory.GetFiles(root).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();
            }
        }

        private MirabufLive _fieldMira;
        public static GameObject FieldObject;
        private FieldData _fieldData;

        public void Awake() {
            UnloadField();
        }

        /// <summary>Loads a field and it's scoring zones. Will revert any unsaved progress on another field.</summary>
        /// <param name="fieldIndex">The index of the field file in the fields mira folder</param>
        public void LoadField(int fieldIndex) {
            UnloadField();
            
            _fieldMira = new MirabufLive(FieldFiles[fieldIndex]);
            FieldObject = new GameObject(_fieldMira.MiraAssembly.Info.Name) { tag = FIELD_TAG };
            _fieldMira.GenerateDefinitionObjects(FieldObject, false, false);

            var gamepieces = new List<GameObject>();
            foreach (Transform child in FieldObject.transform) {
                if (child.name.StartsWith("gamepiece"))
                    gamepieces.Add(child.gameObject);
                else {
                    var nodeObjsParent = new GameObject("Node Group Objects").transform;
                    
                    var nodeObjs = new List<Transform>();
                    foreach (Transform nodeChild in child) {
                        nodeObjs.Add(nodeChild);
                    }
                    nodeObjs.ForEach(o => o.parent = nodeObjsParent);
                    
                    nodeObjsParent.parent = child;
                }
            }
            gamepieces.ForEach(DestroyImmediate);
            
            LoadScoringZones();
            ZoneEditorDebug($"Field {Path.GetFileNameWithoutExtension(FieldFiles[fieldIndex])} loaded");
        }
        
        /// <summary>Loads all of the scoring zones in a field from mira</summary>
        private void LoadScoringZones() {
            if (_fieldMira?.MiraAssembly.Data.Parts.UserData == null)
                return;

            _fieldMira.MiraAssembly.Data.Parts.UserData.Data.TryGetValue("saved-data", out var rawData);
            if (rawData == null)
                return;

            _fieldData = JsonConvert.DeserializeObject<FieldData>(rawData);

            var scoringZones = _fieldData.ScoringZones;
            scoringZones.ForEach(AddZone);
        }

        /// <summary>Loads all of the scoring zones in a field to mira</summary>
        public void SaveScoringZones() {
            if (_fieldMira?.MiraAssembly.Data.Parts.UserData == null)
                return;

            _fieldMira.MiraAssembly.Data.Parts.UserData.Data.TryGetValue("saved-data", out var rawData);
            if (rawData == null)
                return;
            
            _fieldData.ScoringZones = FindObjectsOfType<ScoringZoneConfigListener>()
                .Select(zone => zone.ToZoneData()).ToList();

            _fieldMira.MiraAssembly.Data.Parts.UserData.Data["saved-data"] =
                JsonConvert.SerializeObject(_fieldData);
            _fieldMira.Save();
            
            ZoneEditorDebug($"{_fieldData.ScoringZones.Count} Scoring zones saved");
        }

        /// <summary>Adds a zone. If no zone data is provided, a new zone will be created</summary>
        public void AddZone(ScoringZoneData zoneData = null) {
            if (FieldObject == null) {
                ZoneEditorDebug("Load or reload a field first");
                return;
            }

            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.parent = FieldObject.transform.Find("grounded");
            obj.name = "New Scoring Zone";

            var listener = obj.AddComponent<ScoringZoneConfigListener>();
            
            if (zoneData != null)
                listener.FromZoneData(zoneData);
            
            ZoneEditorDebug("New scoring zone added");
        }

        /// <summary>Destroy gameobjects of any loaded field and it's zones</summary>
        private void UnloadField() {
            var fields = GameObject.FindGameObjectsWithTag(FIELD_TAG);
            fields.ForEach(DestroyImmediate);
            _fieldMira = null;
            FieldObject = null;
        }

        private static void ZoneEditorDebug(string message) {
            Debug.Log($"<color=#4df080>Zone Editor: </color>{message}");
        }
    }

    [CustomEditor(typeof(ScoringZonesTool))]
    public class ScoringZonesToolCustomEditor : Editor {
        private ScoringZonesTool _scoringZonesTool;
        
        private string[] _fieldDropdownOptions;
        private int _selectedFieldIndex = 0;

        void OnEnable() {
            _scoringZonesTool = (ScoringZonesTool)target;
            _fieldDropdownOptions = _scoringZonesTool.FieldFiles
                .Select(Path.GetFileNameWithoutExtension).ToArray();
        }

        /// <summary>Called whenever the unity GUI is changed</summary>
        public override void OnInspectorGUI() {
            serializedObject.Update();

            // Field dropdown
            _selectedFieldIndex = EditorGUILayout.Popup("Choose Field",
                _selectedFieldIndex, _fieldDropdownOptions);

            // Buttons
            if (GUILayout.Button("Load Field"))
                _scoringZonesTool.LoadField(_selectedFieldIndex);
            
            if (GUILayout.Button("Add Zone"))
                _scoringZonesTool.AddZone();

            if (GUILayout.Button("Save Zones"))
                _scoringZonesTool.SaveScoringZones();

            serializedObject.ApplyModifiedProperties();
        }
    }
}

public class ScoringZoneConfigListener : MonoBehaviour {
    [SerializeField] private Alliance _alliance;
    [SerializeField] private int _points;
    [SerializeField] private bool _destroyGamepiece;
    [SerializeField] private bool _persistentPoints;

    /// <summary>Updates all scoring zone settings to match a ScoringZoneData instance</summary>
    public void FromZoneData(ScoringZoneData zoneData) {
        _alliance = zoneData.Alliance;
        _points = zoneData.Points;
        _destroyGamepiece = zoneData.DestroyGamepiece;
        _persistentPoints = zoneData.PersistentPoints;
        
        name = zoneData.Name;

        var trf = transform;
        
        trf.localPosition =
            new Vector3(zoneData.LocalPosition.x, zoneData.LocalPosition.y, zoneData.LocalPosition.z);
        
        trf.localRotation = new Quaternion(zoneData.LocalRotation.x, zoneData.LocalRotation.y,
            zoneData.LocalRotation.z, zoneData.LocalRotation.w);
        
        trf.localScale =
            new Vector3(zoneData.LocalScale.x, zoneData.LocalScale.y, zoneData.LocalScale.z);
        
        transform.parent = ScoringZonesTool.FieldObject.transform.Find(zoneData.Parent);
    }

    /// <summary>Creates a ScoringZoneData instance based on settings</summary>
    public ScoringZoneData ToZoneData() {
        var zoneData = new ScoringZoneData {
            Alliance = _alliance,
            Points = _points,
            DestroyGamepiece = _destroyGamepiece,
            PersistentPoints = _persistentPoints,
            Name = name
        };

        var trf = transform;
        Vector3 pos = trf.localPosition;
        Quaternion rot = trf.localRotation;
        Vector3 scl = trf.localScale;

        zoneData.LocalPosition = (pos.x, pos.y, pos.z);
        zoneData.LocalRotation = (rot.x, rot.y, rot.z, rot.w);
        zoneData.LocalScale = (scl.x, scl.y, scl.z);

        zoneData.Parent = trf.parent.name;

        return zoneData;
    }
}

#endif