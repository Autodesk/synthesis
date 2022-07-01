using Synthesis.UI.Dynamic;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;

public class PracticeMode : GameMode
{
    private bool _lastEscapeValue = false;
    private bool _escapeMenuOpen = false;

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
        if (openEscapeMenu && !_lastEscapeValue)
        {
            if (_escapeMenuOpen)
            {
                _escapeMenuOpen = false;
                DynamicUIManager.CloseActiveModal();
            }
            else
            {
                _escapeMenuOpen = true;
                DynamicUIManager.CreateModal<PracticeSettingsModal>();
            }
        }

        _lastEscapeValue = openEscapeMenu;
    }

    public override void End(){}
}