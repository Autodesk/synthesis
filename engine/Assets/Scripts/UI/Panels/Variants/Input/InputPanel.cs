using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using UnityEngine;

namespace Synthesis.UI.Panels {
    // public class InputPanel : Panel {
    //
    //     public GameObject Content;
    //     public GameObject InputSelection;
    //     
    //     private InputSelection awaitingReassignment = null;
    //
    //     public bool RequestInput(InputSelection selection)
    //     {
    //         if (awaitingReassignment != null)
    //             return false;
    //         awaitingReassignment = selection;
    //         return true;
    //     }
    //
    //     public void PopulateInputSelections() {
    //         if (RobotSimObject.CurrentlyPossessedRobot.Equals(string.Empty))
    //             return;
    //
    //         foreach (var inputKey in SimulationManager.SimulationObjects[RobotSimObject.CurrentlyPossessedRobot]?.GetAllReservedInputs()) {
    //             var val = InputManager.MappedValueInputs[inputKey];
    //             var selectionObject = Instantiate(InputSelection, Content.transform);
    //             var selection = selectionObject.GetComponent<InputSelection>();
    //             // TODO: Probably some parsing on the selection title and some specific ordering of available inputs
    //             if(val is Digital)
    //                 selection.Init(inputKey, inputKey, val.Name, val.Modifier, this);
    //             else
    //                 selection.Init(inputKey, inputKey,
    //                     val.UsePositiveSide ? $"(+) {val.Name}" : $"(-) {val.Name}", val.Modifier, this);
    //         }
    //     }
    //
    //     private void Start() {
    //         PopulateInputSelections();
    //     }
    //
    //     private void Update() {
    //         if (awaitingReassignment != null) {
    //             var input = InputManager.GetAny();
    //             var modKeys = InputManager.ModifierInputs;
    //             // if (input != null)
    //             //     SynthesisAPI.Utilities.Logger.Log(input.Name, SynthesisAPI.Utilities.LogLevel.Debug);
    //             if (input != null && !Regex.IsMatch(input.Name, ".*Mouse.*")) {
    //                 InputManager.AssignValueInput(awaitingReassignment.InputKey, input);
    //                 if(input is Digital)
    //                     awaitingReassignment.UpdateUI(input.Name, input.Modifier);
    //                 else
    //                     awaitingReassignment.UpdateUI(input.UsePositiveSide ? $"(+) {input.Name}" : $"(-) {input.Name}", input.Modifier);
    //                 awaitingReassignment = null;
    //             }
    //         }
    //     }
    // }
}
