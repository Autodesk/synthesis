using System;
using System.Collections.Generic;
using Synthesis.Gizmo;
using Synthesis.Physics;
using Synthesis.PreferenceManager;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Utilities;
using UnityEngine;
using UnityEngine.XR;
using Logger = SynthesisAPI.Utilities.Logger;

public class PracticeMode : IMode {
    public static Vector3 GamepieceSpawnpoint = new Vector3(0, 10, 0);
    private static GameObject _gamepieceSpawnpointObject;

    private bool _lastEscapeValue   = false;
    private bool _escapeMenuOpen    = false;
    private bool _showingScoreboard = false;

    public static GamepieceData ChosenGamepiece { get; set; }
    public static PrimitiveType ChosenPrimitive { get; set; }

    public const string TOGGLE_ESCAPE_MENU_INPUT = "escape_menu";

    private static Dictionary<GameObject, Vector3> _initialPositions    = new Dictionary<GameObject, Vector3>();
    private static Dictionary<GameObject, Quaternion> _initialRotations = new Dictionary<GameObject, Quaternion>();

    // for resetting the robot in practice mode
    public static Dictionary<GameObject, Vector3> InitialPositions {
        get => _initialPositions;
        set => _initialPositions = value;
    }

    public static Dictionary<GameObject, Quaternion> InitialRotations {
        get => _initialRotations;
        set => _initialRotations = value;
    }

    private static List<GamepieceSimObject> _gamepieces = new List<GamepieceSimObject>();

    public void Start() {
        DynamicUIManager.CreateModal<AddFieldModal>();

        // var mira = new
        // MirabufLive("C:\\Users\\hunte\\AppData\\Roaming\\Autodesk\\Synthesis\\Mira\\BrokenLinksRobot_v1.mira");
        //
        // GameObject container = new GameObject();
        // mira.GenerateDefinitionObjects(container, false);

        InputManager.AssignValueInput(
            TOGGLE_ESCAPE_MENU_INPUT, TryGetSavedInput(TOGGLE_ESCAPE_MENU_INPUT,
                                          new Digital("Escape", context: SimulationRunner.RUNNING_SIM_CONTEXT)));

        EventBus.NewTypeListener<OnScoreUpdateEvent>(HandleScoreEvent);

        ConfigureMainHUD();
    }

    /// Adds buttons to the main hud (panel on left side)
    public void ConfigureMainHUD() {
        MainHUD.AddItemToDrawer("Spawn", b => DynamicUIManager.CreateModal<SpawningModal>(),
            icon: SynthesisAssetCollection.GetSpriteByName("PlusIcon"));
        if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
            MainHUD.AddItemToDrawer("Configure", b => DynamicUIManager.CreateModal<ConfiguringModal>(),
                icon: SynthesisAssetCollection.GetSpriteByName("wrench-icon"));

        MainHUD.AddItemToDrawer("Multibot", b => DynamicUIManager.CreatePanel<RobotSwitchPanel>());

        MainHUD.AddItemToDrawer("Controls", b => DynamicUIManager.CreateModal<ChangeInputsModal>(),
            icon: SynthesisAssetCollection.GetSpriteByName("DriverStationView"));
        MainHUD.AddItemToDrawer("Camera View", b => DynamicUIManager.CreateModal<ChangeViewModal>(),
            icon: SynthesisAssetCollection.GetSpriteByName("CameraIcon"));
        MainHUD.AddItemToDrawer("Download Asset", b => DynamicUIManager.CreateModal<DownloadAssetModal>(),
            icon: SynthesisAssetCollection.GetSpriteByName("DownloadIcon"));

        MainHUD.AddItemToDrawer("Settings", b => DynamicUIManager.CreateModal<SettingsModal>(),
            icon: SynthesisAssetCollection.GetSpriteByName("settings"));
        MainHUD.AddItemToDrawer("RoboRIO Conf.", b => DynamicUIManager.CreateModal<RioConfigurationModal>(true),
            icon: SynthesisAssetCollection.GetSpriteByName("rio-config-icon"));

        MainHUD.AddItemToDrawer("DriverStation",
            b => DynamicUIManager.CreatePanel<BetaWarningPanel>(
                false, (Action) (() => DynamicUIManager.CreatePanel<DriverStationPanel>(true))),
            icon: SynthesisAssetCollection.GetSpriteByName("driverstation-icon"));

        MainHUD.AddItemToDrawer("Drivetrain", b => DynamicUIManager.CreateModal<ChangeDrivetrainModal>());
        MainHUD.AddItemToDrawer("Scoring Zones", b => {
            if (FieldSimObject.CurrentField == null) {
                Logger.Log("No field loaded!", LogLevel.Info);
            } else {
                if (!DynamicUIManager.PanelExists<ScoringZonesPanel>())
                    DynamicUIManager.CreatePanel<ScoringZonesPanel>();
            }
        });

        EventBus.NewTypeListener<OnScoreUpdateEvent>(HandleScoreEvent);
    }

    private void HandleScoreEvent(IEvent e) {
        if (e.GetType() != typeof(OnScoreUpdateEvent))
            return;
        OnScoreUpdateEvent scoreUpdateEvent = e as OnScoreUpdateEvent;
        if (scoreUpdateEvent == null)
            return;

        ScoringZone zone = scoreUpdateEvent.Zone;
        int points       = zone.Points * (scoreUpdateEvent.IncreaseScore ? 1 : -1);

        switch (zone.Alliance) {
            case Alliance.Blue:
                Scoring.blueScore += points;
                break;
            case Alliance.Red:
                Scoring.redScore += points;
                break;
        }
    }

    public static void SetInitialState(GameObject robot) {
        InitialPositions.Clear();
        InitialRotations.Clear();
        robot.GetComponentsInChildren<Rigidbody>().ForEach(rb => {
            GameObject go = rb.gameObject;
            InitialPositions.Add(go, go.transform.position);
            InitialRotations.Add(go, go.transform.rotation);
        });
    }

    private Analog TryGetSavedInput(string key, Analog defaultInput) {
        if (PreferenceManager.ContainsPreference(key)) {
            var input            = (Digital) PreferenceManager.GetPreference<InputData[]>(key) [0].GetInput();
            input.ContextBitmask = defaultInput.ContextBitmask;
            return input;
        }
        return defaultInput;
    }

    public void Update() {
        if (!_showingScoreboard && FieldSimObject.CurrentField != null &&
            FieldSimObject.CurrentField.ScoringZones.Count > 0) {
            _showingScoreboard = true;
            DynamicUIManager.CreatePanel<ScoreboardPanel>(true, false);
        }
        bool openEscapeMenu = InputManager.MappedValueInputs[TOGGLE_ESCAPE_MENU_INPUT].Value == 1.0F;
        if (openEscapeMenu && !_lastEscapeValue) {
            if (_escapeMenuOpen) {
                CloseMenu();
            } else {
                OpenMenu();
            }
        }

        _lastEscapeValue = openEscapeMenu;
    }

    public void OpenMenu() {
        DynamicUIManager.CreateModal<PracticeSettingsModal>();
        _escapeMenuOpen = true;
    }

    public void CloseMenu() {
        DynamicUIManager.CloseActiveModal();
        _escapeMenuOpen = false;
    }

    public void End() {
        InputManager._mappedValueInputs.Remove(TOGGLE_ESCAPE_MENU_INPUT);
    }

    public static void ConfigureGamepieceSpawnpoint() {
        _gamepieceSpawnpointObject                      = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _gamepieceSpawnpointObject.transform.localScale = new Vector3(1, 1, 1);
        _gamepieceSpawnpointObject.transform.position   = GamepieceSpawnpoint;
        _gamepieceSpawnpointObject.transform.tag        = "gamepiece";

        _gamepieceSpawnpointObject.GetComponent<Collider>().enabled = false;

        FieldSimObject currentField = FieldSimObject.CurrentField;
        if (currentField != null) {
            _gamepieceSpawnpointObject.transform.parent = currentField.FieldObject.transform;
        }

        // make it transparent
        Renderer renderer = _gamepieceSpawnpointObject.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Shader Graphs/DefaultSynthesisTransparentShader"));

        GizmoManager.SpawnGizmo(_gamepieceSpawnpointObject.transform,
            t => { _gamepieceSpawnpointObject.transform.position = t.Position; },
            t => { EndConfigureGamepieceSpawnpoint(); });
        // GizmoManager.SpawnGizmo(GizmoStore.GizmoPrefabStatic, _gamepieceSpawnpointObject.transform,
        // _gamepieceSpawnpointObject.transform.position);
    }

    public static void EndConfigureGamepieceSpawnpoint() {
        GamepieceSpawnpoint = _gamepieceSpawnpointObject.transform.position;
        GameObject.Destroy(_gamepieceSpawnpointObject);
        _gamepieceSpawnpointObject = null;
    }

    public static void ResetRobot() {
        RobotSimObject robot = RobotSimObject.GetCurrentlyPossessedRobot();
        if (robot == null)
            return;
        robot.ClearGamepieces();
        robot.RobotNode.GetComponentsInChildren<Rigidbody>().ForEach(rb => {
            GameObject go         = rb.gameObject;
            go.transform.position = InitialPositions[go];
            go.transform.rotation = InitialRotations[go];
        });
    }

    public static void ResetGamepieces() {
        if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
            RobotSimObject.GetCurrentlyPossessedRobot().ClearGamepieces();

        _gamepieces.ForEach(gp => { GameObject.Destroy(gp.GamepieceObject); });
        _gamepieces.Clear();
        FieldSimObject currentField = FieldSimObject.CurrentField;
        if (currentField != null) {
            FieldSimObject.CurrentField.Gamepieces.ForEach(gp => gp.Reset());
        }

        Scoring.ResetScore();
    }

    public static void ResetAll() {
        ResetRobot();
        ResetGamepieces();
    }

    public static void SpawnGamepiece(float scale = 1.0f, PrimitiveType type = PrimitiveType.Sphere) {
        SpawnGamepiece(GamepieceSpawnpoint, scale, type);
    }

    public static void SpawnGamepiece(
        int x, int y, int z, float scale = 1.0f, PrimitiveType type = PrimitiveType.Sphere) {
        SpawnGamepiece(new Vector3(x, y, z), scale, type);
    }

    public static void SpawnGamepiece(
        Vector3 spawnPosition, float scale = 1.0f, PrimitiveType type = PrimitiveType.Sphere) {
        FieldSimObject currentField = FieldSimObject.CurrentField;
        GamepieceData data          = ChosenGamepiece;

        GamepieceSimObject gamepiece;

        if (currentField == null || data == null) {
            GameObject gameObject         = GameObject.CreatePrimitive(type);
            gameObject.transform.position = spawnPosition;
            gameObject.AddComponent<Rigidbody>();
            gamepiece = new GamepieceSimObject(type + " Gamepiece", gameObject);
        } else {
            GameObject parent = new GameObject(data.Name);
            Rigidbody rb      = parent.AddComponent<Rigidbody>();
            rb.mass           = data.Mass;
            parent.AddComponent<ContactRecorder>();

            GameObject childWithTransform       = new GameObject(data.Name);
            childWithTransform.transform.parent = parent.transform;

            GameObject childWithMesh                            = new GameObject(data.Name);
            childWithMesh.transform.parent                      = childWithTransform.transform;
            childWithMesh.AddComponent<MeshFilter>().mesh       = data.Mesh;
            childWithMesh.AddComponent<MeshRenderer>().material = data.Material;
            MeshCollider collider                               = childWithMesh.AddComponent<MeshCollider>();
            collider.convex                                     = true;
            collider.material                                   = data.ColliderMaterial;

            parent.transform.parent               = data.Parent;
            parent.transform.position             = Vector3.zero;
            childWithTransform.transform.position = spawnPosition;
            // GameObject.Instantiate(parent);

            // if (childTransform != null)
            // {
            //     childTransform.position = spawnPosition;
            // }

            gamepiece = new GamepieceSimObject(data.Name, parent);
        }

        _gamepieces.Add(gamepiece);
    }

    public class GamepieceData {
        public string Name;
        public Mesh Mesh;
        public Material Material;
        public PhysicMaterial ColliderMaterial;
        public Transform Parent;
        public float Mass;

        public GamepieceData(GameObject gameObject) {
            if (gameObject == null)
                return;

            if (gameObject.transform.childCount > 0)
                // standard game objects have children named block:# or balls:#
                // so I use that for the name of the gamepiece
                Name = gameObject.transform.GetChild(0).name.Split(':')[0];
            else
                Name = gameObject.name;

            Parent = gameObject.transform.parent;

            MeshFilter meshFilter = gameObject.GetComponentInChildren<MeshFilter>();
            if (meshFilter != null)
                Mesh = meshFilter.mesh;

            MeshCollider collider = gameObject.GetComponentInChildren<MeshCollider>();
            if (collider != null)
                ColliderMaterial = collider.material;

            Rigidbody rb = gameObject.GetComponentInChildren<Rigidbody>();
            if (rb != null)
                Mass = rb.mass;

            MeshRenderer meshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null)
                Material = meshRenderer.material;
        }
    }
}