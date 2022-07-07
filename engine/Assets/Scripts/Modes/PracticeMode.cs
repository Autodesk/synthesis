using Synthesis.PreferenceManager;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.InputEvents;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;

public class PracticeMode : GameMode
{
    private bool _lastEscapeValue = false;
    private bool _escapeMenuOpen = false;
    
    public static GamepieceSimObject ChosenGamepiece { get; set; }
    public static PrimitiveType ChosenPrimitive { get; set; }

    public const string TOGGLE_ESCAPE_MENU_INPUT = "input/escape_menu";

    public override void Start()
    {
        if (ModeManager.CurrentMode != ModeManager.Mode.Practice) return;
        DynamicUIManager.CreateModal<SelectFieldModal>();
        InputManager.AssignValueInput(TOGGLE_ESCAPE_MENU_INPUT, TryGetSavedInput(TOGGLE_ESCAPE_MENU_INPUT, new Digital("Escape", context: SimulationRunner.RUNNING_SIM_CONTEXT)));
    }

    private Analog TryGetSavedInput(string key, Analog defaultInput) {
        if (PreferenceManager.ContainsPreference(key)) {
            var input = (Digital)PreferenceManager.GetPreference<InputData[]>(key)[0].GetInput();
            input.ContextBitmask = defaultInput.ContextBitmask;
            return input;
        } 
        return defaultInput;
    }

    public override void Update()
    {
        if (ModeManager.CurrentMode != ModeManager.Mode.Practice) return;
        bool openEscapeMenu = InputManager.MappedValueInputs[TOGGLE_ESCAPE_MENU_INPUT].Value == 1.0f;
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

    public override void End(){}
}