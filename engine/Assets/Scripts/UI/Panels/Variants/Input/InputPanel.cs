using System;
using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.InputManager;
using UnityEngine;

namespace Synthesis.UI.Panels {
    public class InputPanel : Panel {

        public GameObject Content;
        public GameObject InputSelection;
        
        private InputSelection awaitingReassignment = null;
        
        public void RequestInput(InputSelection selection) {
            if (awaitingReassignment != null)
                throw new Exception("Should this even be possible?");

            awaitingReassignment = selection;
        }

        public void PopulateInputSelections() {
            foreach (var kvp in InputManager.MappedValueInputs) {
                var selectionObject = Instantiate(InputSelection, Content.transform);
                var selection = selectionObject.GetComponent<InputSelection>();
                // TODO: Probably some parsing on the selection title and some specific ordering of available inputs
                selection.Init(kvp.Key, kvp.Key, kvp.Value.Name, this);
            }
        }

        private void Update() {
            if (awaitingReassignment != null) {
                var input = InputManager.GetAny();
                if (input != null) {
                    InputManager.AssignValueInput(awaitingReassignment.InputKey, input);
                    awaitingReassignment.UpdateUI(input.Name);
                    awaitingReassignment = null;
                }
            }
        }
    }
}
