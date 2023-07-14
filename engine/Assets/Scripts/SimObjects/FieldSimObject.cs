using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Analytics;
using MathNet.Numerics;
using Mirabuf;
using Synthesis.Gizmo;
using Synthesis.Import;
using Synthesis.Physics;
using Synthesis.PreferenceManager;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Controller;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;
using Utilities.ColorManager;
using Bounds    = UnityEngine.Bounds;
using Transform = Mirabuf.Transform;
using Vector3   = UnityEngine.Vector3;

public class FieldSimObject : SimObject, IPhysicsOverridable {
    public static FieldSimObject CurrentField { get; private set; }

    private readonly List<ScoringZone> _scoringZones;
    public IReadOnlyCollection<ScoringZone> ScoringZones => _scoringZones.AsReadOnly();

    public MirabufLive MiraLive { get; private set; }
    public GameObject GroundedNode { get; private set; }
    public GameObject FieldObject { get; private set; }
    public Bounds FieldBounds { get; private set; }
    public List<GamepieceSimObject> Gamepieces { get; private set; }

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    private bool _isFrozen;
    public bool isFrozen() => _isFrozen;

    public void Freeze() {
        if (_isFrozen)
            return;
        FieldObject.GetComponentsInChildren<Rigidbody>()
            .Where(e => e.name != "grounded")
            .Concat(
                Gamepieces.Where(e => !e.IsCurrentlyPossessed).Select(e => e.GamepieceObject.GetComponent<Rigidbody>()))
            .ForEach(e => {
                e.isKinematic      = true;
                e.detectCollisions = false;
            });

        _isFrozen = true;
    }

    public void Unfreeze() {
        if (!_isFrozen)
            return;

        FieldObject.GetComponentsInChildren<Rigidbody>()
            .Where(e => e.name != "grounded")
            .Concat(
                Gamepieces.Where(e => !e.IsCurrentlyPossessed).Select(e => e.GamepieceObject.GetComponent<Rigidbody>()))
            .ForEach(e => {
                e.isKinematic      = false;
                e.detectCollisions = true;
            });

        _isFrozen = false;
    }

    public List<Rigidbody>
    GetAllRigidbodies() => FieldObject.GetComponentsInChildren<Rigidbody>().Where(e => e.name != "grounded").ToList();

    public GameObject GetRootGameObject() {
        return FieldObject;
    }

    public FieldSimObject(string name, ControllableState state, MirabufLive miraLive, GameObject groundedNode,
        List<GamepieceSimObject> gamepieces)
        : base(name, state) {
        MiraLive     = miraLive;
        GroundedNode = groundedNode;
        // grounded node is what gets grabbed in god mode so it needs field tag to not get moved
        GroundedNode.transform.tag = "field";
        FieldObject                = groundedNode.transform.parent.gameObject;
        FieldBounds                = groundedNode.transform.GetBounds();
        Gamepieces                 = gamepieces;
        SimulationPreferences.LoadFieldFromMira(MiraLive);

        _scoringZones = new List<ScoringZone>();

        PhysicsManager.Register(this);

        // Level the field
        FieldObject.transform.position =
            new Vector3(-FieldBounds.center.x, FieldBounds.extents.y - FieldBounds.center.y, -FieldBounds.center.z);

        _initialPosition = FieldObject.transform.position;

        CurrentField = this;
        Gamepieces.ForEach(gp => {
            UnityEngine.Transform gpTransform = gp.GamepieceObject.transform;
            gp.InitialPosition                = gpTransform.position;
            gp.InitialRotation                = gpTransform.rotation;
        });

        SynthesisAPI.EventBus.EventBus.NewTypeListener<PostPreferenceSaveEvent>(e => {
            bool visible = PreferenceManager.GetPreference<bool>(SettingsModal.RENDER_SCORE_ZONES);
            ScoringZones.ForEach(zone => zone.VisibilityCounter = zone.VisibilityCounter);
        });

        FieldObject.transform.GetComponentsInChildren<Rigidbody>().ForEach(x => {
            var rc     = x.gameObject.AddComponent<HighlightComponent>();
            rc.Color   = ColorManager.GetColor(ColorManager.SynthesisColor.HighlightSelect);
            rc.enabled = false;
        });
    }

    public void ResetField() {
        SpawnField(MiraLive);
    }

    public bool RemoveScoringZone(ScoringZone zone) {
        var res = _scoringZones.Remove(zone);
        if (res)
            UpdateSavedScoringZones();
        return res;
    }

    public void AddScoringZone(ScoringZone zone) {
        _scoringZones.Add(zone);
        UpdateSavedScoringZones();
    }

    public void UpdateSavedScoringZones() {
        SimulationPreferences.SetFieldScoringZones(
            MiraLive.MiraAssembly.Info.GUID, _scoringZones.Select(x => x.ZoneData).ToList());
        PreferenceManager.Save();
    }

    public void InitializeScoreZones() {
        _scoringZones.Clear();
        bool visible     = PreferenceManager.GetPreference<bool>(SettingsModal.RENDER_SCORE_ZONES);
        var scoringZones = SimulationPreferences.GetFieldScoringZones(MiraLive.MiraAssembly.Info.GUID);
        if (scoringZones != null) {
            scoringZones.ForEach(x => {
                var zone = new ScoringZone(
                    GameObject.CreatePrimitive(PrimitiveType.Cube), "temp scoring zone", Alliance.Blue, 0, false, true);
                zone.ZoneData          = x;
                zone.VisibilityCounter = zone.VisibilityCounter;
                _scoringZones.Add(zone);
            });
        }
    }

    public static bool DeleteField() {
        if (CurrentField == null)
            return false;

        if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
            RobotSimObject.GetCurrentlyPossessedRobot().ClearGamepieces();

        CurrentField._scoringZones.Clear();
        CurrentField.Gamepieces.ForEach(x => x.DeleteGamepiece());
        CurrentField.Gamepieces.Clear();
        GameObject.Destroy(CurrentField.FieldObject);
        SimulationManager.RemoveSimObject(CurrentField);
        CurrentField = null;
        return true;
    }

    public static void SpawnField(string filePath, bool spawnRobotGizmo = true) {
        DeleteField();

        var mira = Importer.MirabufAssemblyImport(filePath);
        mira.MainObject.transform.SetParent(GameObject.Find("Game").transform);
        mira.MainObject.tag = "field";

        FieldSimObject.CurrentField.InitializeScoreZones();

        if (spawnRobotGizmo && RobotSimObject.CurrentlyPossessedRobot != string.Empty) {
            GizmoManager.SpawnGizmo(RobotSimObject.GetCurrentlyPossessedRobot());
            // TODO: Move robot to default spawn location for field
        }
        AnalyticsManager.LogCustomEvent(AnalyticsEvent.FieldSpawned, ("FieldName", mira.MainObject.name));
    }

    public static void SpawnField(MirabufLive miraAssem, bool spawnRobotGizmo = true) {
        DeleteField();

        var mira = Importer.MirabufAssemblyImport(miraAssem);
        mira.MainObject.transform.SetParent(GameObject.Find("Game").transform);
        mira.MainObject.tag = "field";

        if (spawnRobotGizmo && RobotSimObject.CurrentlyPossessedRobot != string.Empty) {
            GizmoManager.SpawnGizmo(RobotSimObject.GetCurrentlyPossessedRobot());
            // TODO: Move robot to default spawn location for field
        }
    }

    public override void Destroy() {
        PhysicsManager.Unregister(this);
    }
}