using Synthesis.UI.Dynamic;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;

public class PracticeMode : MonoBehaviour
{
    private bool _lastEscapeValue = false;
    private bool _escapeMenuOpen = false;

    private void Start()
    {
        if (ModeManager.CurrentMode != ModeManager.Mode.Practice) return;
        DynamicUIManager.CreateModal<SelectFieldModal>();
        InputManager.AssignValueInput("escape_menu", new Digital("Escape"));
    }

    private void Update()
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
}