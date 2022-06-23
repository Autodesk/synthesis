using System;
using System.Collections.Generic;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;

public class PracticeMode : MonoBehaviour
{
    private bool _active = true;
    private bool _lastEscapeValue = false;
    private bool _escapeMenuOpen = false;

    private void Start()
    {
        if (!_active) return;
        InputManager.AssignValueInput("escape_menu", new Digital("ESCAPE"));
        for (int i = 0; i < 10; i++)
        {
            ModeManager.SpawnGamepiece(0, i, 0, (10 - i) / 10f);
        }
    }

    private void Update()
    {
        bool openEscapeMenu = InputManager.MappedValueInputs["escape_menu"].Value == 1.0f;
        if (openEscapeMenu && openEscapeMenu != _lastEscapeValue)
        {
            if (_escapeMenuOpen)
            {
                _escapeMenuOpen = false;
            }
            else
            {
                _escapeMenuOpen = true;
                // open menu using dynamic ui stuff
            }
        }

        _lastEscapeValue = openEscapeMenu;
    }
}