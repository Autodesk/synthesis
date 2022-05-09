using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;

namespace Synthesis.UI.Panels {
    public class InputPanel : Panel {

        public GameObject Content;
        public GameObject InputSelection;
        
        private InputSelection awaitingReassignment = null;

        public bool RequestInput(InputSelection selection)
        {
            if (awaitingReassignment != null)
                return false;
            awaitingReassignment = selection;
            return true;
        }

        public void PopulateInputSelections() {
            foreach (var kvp in InputManager.MappedValueInputs) {
                var selectionObject = Instantiate(InputSelection, Content.transform);
                var selection = selectionObject.GetComponent<InputSelection>();
                // TODO: Probably some parsing on the selection title and some specific ordering of available inputs
                if(kvp.Value is Digital)
                    selection.Init(kvp.Key, kvp.Key, kvp.Value.Name, kvp.Value.Modifier, this);
                else
                    selection.Init(kvp.Key, kvp.Key,
                        kvp.Value.UsePositiveSide ? $"(+) {kvp.Value.Name}" : $"(-) {kvp.Value.Name}", kvp.Value.Modifier, this);
            }
        }

        private void Start() {
            PopulateInputSelections();
        }

        private void Update() {
            if (awaitingReassignment != null) {
                var input = InputManager.GetAny();
                var modKeys = InputManager.ModifierInputs;
                // if (input != null)
                //     SynthesisAPI.Utilities.Logger.Log(input.Name, SynthesisAPI.Utilities.LogLevel.Debug);
                if (input != null && !Regex.IsMatch(input.Name, ".*Mouse.*")) {
                    InputManager.AssignValueInput(awaitingReassignment.InputKey, input);
                    if(input is Digital)
                        awaitingReassignment.UpdateUI(input.Name, input.Modifier);
                    else
                        awaitingReassignment.UpdateUI(input.UsePositiveSide ? $"(+) {input.Name}" : $"(-) {input.Name}", input.Modifier);
                    awaitingReassignment = null;
                }
            }
        }
    }
}
