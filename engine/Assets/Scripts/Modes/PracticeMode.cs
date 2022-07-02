using Synthesis.UI.Dynamic;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;

public class PracticeMode : GameMode
{
    private bool _lastEscapeValue = false;
    private bool _escapeMenuOpen = false;
    
    public static GamepieceSimObject ChosenGamepiece { get; set; }
    public static PrimitiveType ChosenPrimitive { get; set; }

    public override void Start()
    {
        if (ModeManager.CurrentMode != ModeManager.Mode.Practice) return;
        DynamicUIManager.CreateModal<SelectFieldModal>();
        InputManager.AssignValueInput("escape_menu", new Digital("Escape"));
    }

    public override void Update()
    {
        if (ModeManager.CurrentMode != ModeManager.Mode.Practice) return;
        bool openEscapeMenu = InputManager.MappedValueInputs["escape_menu"].Value == 1.0f;
        // TODO close button doesn't update last escape and escape menu open
        // so you have to press space twice again to open menu
        if (openEscapeMenu && !_lastEscapeValue)
        {
            if (_escapeMenuOpen)
            {
                _escapeMenuOpen = false;
                DynamicUIManager.CloseActivePopup();
            }
            else
            {
                _escapeMenuOpen = true;
                DynamicUIManager.CreatePopup<PracticeSettingsPopup>();
            }
        }

        _lastEscapeValue = openEscapeMenu;
    }

    public override void End(){}
}