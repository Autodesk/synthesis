using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirabuf;
using Synthesis.Gizmo;
using Synthesis.Import;
using Synthesis.Physics;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

using Bounds = UnityEngine.Bounds;
using Transform = Mirabuf.Transform;
using Vector3 = UnityEngine.Vector3;

public class FieldSimObject : SimObject, IPhysicsOverridable {

    public static FieldSimObject CurrentField { get; private set; }

    public MirabufLive MiraLive { get; private set; }
    public GameObject GroundedNode { get; private set; }
    public GameObject FieldObject { get; private set; }
    public Bounds FieldBounds { get; private set; }
    public List<GamepieceSimObject> Gamepieces { get; private set; }
    
    public List<ScoringZone> ScoringZones { get; private set; }

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    private bool _isFrozen;
    public bool isFrozen()
        => _isFrozen;

    public void Freeze() {
        if (_isFrozen)
            return;
        FieldObject.GetComponentsInChildren<Rigidbody>()
            .Where(e => e.name != "grounded").Concat(
                Gamepieces.Where(e => !e.IsCurrentlyPossessed)
                    .Select(e => e.GamepieceObject.GetComponent<Rigidbody>())).ForEach(e =>
            {
                e.isKinematic = true;
                e.detectCollisions = false;
            });

        _isFrozen = true;
    }
    public void Unfreeze() {
        if (!_isFrozen)
            return;

        FieldObject.GetComponentsInChildren<Rigidbody>()
            .Where(e => e.name != "grounded").Concat(
                Gamepieces.Where(e => !e.IsCurrentlyPossessed)
                    .Select(e => e.GamepieceObject.GetComponent<Rigidbody>())).ForEach(e =>
            {
                e.isKinematic = false;
                e.detectCollisions = true;
            });

        _isFrozen = false;
    }

    public List<Rigidbody> GetAllRigidbodies() =>
        FieldObject.GetComponentsInChildren<Rigidbody>()
            .Where(e => e.name != "grounded").ToList();

    public GameObject GetRootGameObject()
    {
        return FieldObject;
    }

    public FieldSimObject(string name, ControllableState state, MirabufLive miraLive, GameObject groundedNode, List<GamepieceSimObject> gamepieces) : base(name, state) {
        MiraLive = miraLive;
        GroundedNode = groundedNode;
        // grounded node is what gets grabbed in god mode so it needs field tag to not get moved
        GroundedNode.transform.tag = "field";
        FieldObject = groundedNode.transform.parent.gameObject;
        FieldBounds = groundedNode.transform.GetBounds();
        Gamepieces = gamepieces;
        ScoringZones = new List<ScoringZone>();

        PhysicsManager.Register(this);

        // Level the field
        var position = FieldObject.transform.position;
        position.y -= FieldBounds.center.y - FieldBounds.extents.y;
        FieldObject.transform.position = position;
        Debug.Log($"{FieldObject.transform.position.y}");

        _initialPosition = FieldObject.transform.position;

        CurrentField = this;
        Gamepieces.ForEach(gp =>
        {
            UnityEngine.Transform gpTransform = gp.GamepieceObject.transform;
            gp.InitialPosition = gpTransform.position;
            gp.InitialRotation = gpTransform.rotation;
        });
        // Shooting.ConfigureGamepieces();
    }

    public void ResetField() {
        SpawnField(MiraLive);
        // FieldObject.transform.position = _initialPosition;
        // FieldObject.transform.rotation = _initialRotation;
    }

    public static bool DeleteField() {
        if (CurrentField == null)
            return false;

        // Debug.Log($"GP count: {CurrentField.Gamepieces.Count}");

        if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
            RobotSimObject.GetCurrentlyPossessedRobot().ClearGamepieces();

        CurrentField.Gamepieces.ForEach(x => x.DeleteGamepiece());
        CurrentField.Gamepieces.Clear();
        GameObject.Destroy(CurrentField.FieldObject);
        SimulationManager.RemoveSimObject(CurrentField);
        CurrentField = null;
        return true;
        // SynthesisAssetCollection.DefaultFloor.SetActive(true);
    }

    public static void SpawnField(string filePath) {
        DeleteField();

        var mira = Importer.MirabufAssemblyImport(filePath);
        mira.MainObject.transform.SetParent(GameObject.Find("Game").transform);
        mira.MainObject.tag = "field";

        if (RobotSimObject.CurrentlyPossessedRobot != string.Empty) {
            GizmoManager.SpawnGizmo(RobotSimObject.GetCurrentlyPossessedRobot());
            // TODO: Move robot to default spawn location for field
        }
    }

    public static void SpawnField(MirabufLive miraAssem) {
        DeleteField();

        var mira = Importer.MirabufAssemblyImport(miraAssem);
        mira.MainObject.transform.SetParent(GameObject.Find("Game").transform);
        mira.MainObject.tag = "field";

        if (RobotSimObject.CurrentlyPossessedRobot != string.Empty) {
            GizmoManager.SpawnGizmo(RobotSimObject.GetCurrentlyPossessedRobot());
            // TODO: Move robot to default spawn location for field
        }
    }

    public void CreateScoringZone(Alliance alliance, int points, bool destroyObject = true)
    {
        GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ScoringZones.Add(new ScoringZone(zone, alliance, points, destroyObject));
    }

    public void CreateTestGamepiece(PrimitiveType type)
    {
        GameObject gamepiece = GameObject.CreatePrimitive(type);
        gamepiece.tag = "gamepiece";
        gamepiece.AddComponent<Rigidbody>();
    }

    public override void Destroy()
    {
        PhysicsManager.Unregister(this);
    }

}
