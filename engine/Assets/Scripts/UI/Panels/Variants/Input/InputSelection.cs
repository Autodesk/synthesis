using SynthesisAPI.InputManager.Inputs;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

// TODO: Remove whole thing?
namespace Synthesis.UI.Panels {
    // public class InputSelection : MonoBehaviour {
    //     private string _inputKey;
    //
    //     public string InputKey {
    //         get => _inputKey;
    //     }
    //     public TMP_Text Title;
    //     public TMP_Text SelectedValue;
    //     public Button Button;
    //     private InputPanel _panel;
    //
    //     public void Init(string title, string inputKey, string inputValue, int inputMod, InputPanel panel) {
    //         Title.text = title;
    //         _inputKey = inputKey;
    //         _panel = panel;
    //
    //         UpdateUI(inputValue, inputMod);
    //
    //         Button.onClick.AddListener(() =>
    //         {
    //             if (panel.RequestInput(this))
    //             {
    //                 UpdateUI("Press anything");
    //             }
    //         });
    //     }
    //
    //     public void UpdateUI(string newKeyValue, int modifier = 0) {
    //         if ((modifier & (int)ModKey.LeftShift) != 0) {
    //             newKeyValue += " + Left Shift";
    //         }
    //         if ((modifier & (int)ModKey.LeftCommand) != 0) {
    //             newKeyValue += " + Left Command";
    //         }
    //         // Idk the difference
    //         if ((modifier & (int)ModKey.LeftApple) != 0) {
    //             newKeyValue += " + Left Command";
    //         }
    //         if ((modifier & (int)ModKey.LeftAlt) != 0) {
    //             newKeyValue += " + Left Alt";
    //         }
    //         if ((modifier & (int)ModKey.RightShift) != 0) {
    //             newKeyValue += " + Right Shift";
    //         }
    //         if ((modifier & (int)ModKey.RightCommand) != 0) {
    //             newKeyValue += " + Right Control";
    //         }
    //         if ((modifier & (int)ModKey.RightApple) != 0) {
    //             newKeyValue += " + Right Command";
    //         }
    //         if ((modifier & (int)ModKey.RightAlt) != 0) {
    //             newKeyValue += " + Right Alt";
    //         }
    //         if ((modifier & (int)ModKey.LeftControl) != 0) {
    //             newKeyValue += " + Left Control";
    //         }
    //         if ((modifier & (int)ModKey.RightControl) != 0) {
    //             newKeyValue += " + Right Control";
    //         }
    //         SelectedValue.text = newKeyValue;
    //     }
    // }
}
