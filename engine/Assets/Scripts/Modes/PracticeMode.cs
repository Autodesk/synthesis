using System.Collections.Generic;
using Synthesis.PreferenceManager;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;

public class PracticeMode : IMode
{
    public static Vector3 GamepieceSpawnpoint = new Vector3(0, 10, 0);
    private static GameObject _gamepieceSpawnpointObject;
  
    private bool _lastEscapeValue = false;
    private bool _escapeMenuOpen = false;
    
    public static GamepieceSimObject ChosenGamepiece { get; set; }
    public static PrimitiveType ChosenPrimitive { get; set; }

    public const string TOGGLE_ESCAPE_MENU_INPUT = "input/escape_menu";
    
    private static Dictionary<GameObject, Vector3> _initialPositions = new Dictionary<GameObject, Vector3>();
    private static Dictionary<GameObject, Quaternion> _initialRotations = new Dictionary<GameObject, Quaternion>();

    // for resetting the robot in practice mode
    public static Dictionary<GameObject, Vector3> InitialPositions
    {
        get => _initialPositions;
        set => _initialPositions = value;
    }

    public static Dictionary<GameObject, Quaternion> InitialRotations
    {
        get => _initialRotations;
        set => _initialRotations = value;
    }

    private static List<GamepieceSimObject> _gamepieces = new List<GamepieceSimObject>();

    public void Start()
    {
        DynamicUIManager.CreateModal<SelectFieldModal>();
        InputManager.AssignValueInput(TOGGLE_ESCAPE_MENU_INPUT, TryGetSavedInput(TOGGLE_ESCAPE_MENU_INPUT, new Digital("Escape", context: SimulationRunner.RUNNING_SIM_CONTEXT)));
    }
    
    public static void SetInitialState(GameObject robot)
    {
        InitialPositions.Clear();
        InitialRotations.Clear();
        robot.GetComponentsInChildren<Rigidbody>().ForEach(rb =>
        {
            GameObject go = rb.gameObject;
            InitialPositions.Add(go, go.transform.position);
            InitialRotations.Add(go, go.transform.rotation);
        });
    }

    private Analog TryGetSavedInput(string key, Analog defaultInput) {
        if (PreferenceManager.ContainsPreference(key)) {
            var input = (Digital)PreferenceManager.GetPreference<InputData[]>(key)[0].GetInput();
            input.ContextBitmask = defaultInput.ContextBitmask;
            return input;
        } 
        return defaultInput;
    }

    public void Update()
    {
        bool openEscapeMenu = InputManager.MappedValueInputs[TOGGLE_ESCAPE_MENU_INPUT].Value == 1.0F;
        if (openEscapeMenu && !_lastEscapeValue)
        {
            if (_escapeMenuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }
        
        _lastEscapeValue = openEscapeMenu;
    }

    public void OpenMenu()
    {
        DynamicUIManager.CreateModal<PracticeSettingsModal>();
        _escapeMenuOpen = true;
    }

    public void CloseMenu()
    {
        DynamicUIManager.CloseActiveModal();
        _escapeMenuOpen = false;
    }

    public void End()
    {
        InputManager._mappedValueInputs.Remove(TOGGLE_ESCAPE_MENU_INPUT);
    }
    
    public static void ConfigureGamepieceSpawnpoint()
    {
        _gamepieceSpawnpointObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _gamepieceSpawnpointObject.transform.localScale = new Vector3(1, 1, 1);
        _gamepieceSpawnpointObject.transform.position = GamepieceSpawnpoint;
        _gamepieceSpawnpointObject.transform.tag = "gamepiece";

        _gamepieceSpawnpointObject.GetComponent<Collider>().enabled = false;
        
        FieldSimObject currentField = FieldSimObject.CurrentField;
        if (currentField != null)
        {
            _gamepieceSpawnpointObject.transform.parent = currentField.FieldObject.transform;
        }
        
        // make it transparent
        Renderer renderer = _gamepieceSpawnpointObject.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Shader Graphs/DefaultSynthesisTransparentShader"));

        GizmoManager.SpawnGizmo(GizmoStore.GizmoPrefabStatic, _gamepieceSpawnpointObject.transform, _gamepieceSpawnpointObject.transform.position);
    }

    public static void EndConfigureGamepieceSpawnpoint()
    {
        GamepieceSpawnpoint = _gamepieceSpawnpointObject.transform.position;
        GameObject.Destroy(_gamepieceSpawnpointObject);
        _gamepieceSpawnpointObject = null;
    }

    public static void ResetRobot()
    {
        RobotSimObject robot = RobotSimObject.GetCurrentlyPossessedRobot();
        if (robot == null) return;
        robot.RobotNode.GetComponentsInChildren<Rigidbody>().ForEach(rb =>
        {
            GameObject go = rb.gameObject;
            go.transform.position = InitialPositions[go];
            go.transform.rotation = InitialRotations[go];
        });
    }

    public static void ResetField()
    {
        FieldSimObject field = FieldSimObject.CurrentField;
        if (field != null)
            FieldSimObject.CurrentField.ResetField();
        ResetGamepieces();
    }

    public static void ResetGamepieces()
    {
        _gamepieces.ForEach(gp => { GameObject.Destroy(gp.GamepieceObject); });
        _gamepieces.Clear();
        FieldSimObject currentField = FieldSimObject.CurrentField;
        if (currentField != null)
        {
            FieldSimObject.CurrentField.Gamepieces.ForEach(gp => gp.Reset());
        }
    }

    public static void ResetAll()
    {
        ResetField();
        ResetRobot();
    }

    public static void SpawnGamepiece(float scale = 1.0f, PrimitiveType type = PrimitiveType.Sphere)
    {
        SpawnGamepiece(GamepieceSpawnpoint, scale, type);
    }

    public static void SpawnGamepiece(int x, int y, int z, float scale = 1.0f,
        PrimitiveType type = PrimitiveType.Sphere)
    {
        SpawnGamepiece(new Vector3(x, y, z), scale, type);
    }

    public static void SpawnGamepiece(Vector3 spawnPosition, float scale = 1.0f,
        PrimitiveType type = PrimitiveType.Sphere)
    {
        FieldSimObject currentField = FieldSimObject.CurrentField;
        GamepieceSimObject gamepiece = PracticeMode.ChosenGamepiece;
        if (currentField == null || gamepiece == null)
        {
            GameObject gameObject = GameObject.CreatePrimitive(type);
            gameObject.transform.position = spawnPosition;
            gameObject.AddComponent<Rigidbody>();
            gamepiece = new GamepieceSimObject(type + " Gamepiece", gameObject);
        }
        else
        {
            GameObject go = GameObject.Instantiate(gamepiece.GamepieceObject);
            go.transform.parent = gamepiece.GamepieceObject.transform.parent;
            go.transform.position = Vector3.zero;
            Transform childTransform = go.transform.GetChild(0);

            if (childTransform != null)
            {
                childTransform.position = spawnPosition;
            }
            
            gamepiece =
                new GamepieceSimObject(gamepiece.Name, go);
        }

        _gamepieces.Add(gamepiece);
    }
}