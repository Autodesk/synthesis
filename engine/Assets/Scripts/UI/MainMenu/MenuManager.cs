using System.Diagnostics;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Debug = UnityEngine.Debug;

namespace Synthesis.UI {
    public class MenuManager : MonoBehaviour {
        public void ButtonPrint(string s) {
            Debug.Log("Button Pressed: " + s);
            DynamicUIManager.CreateModal<TestModal>();
        }

        public void Singleplayer() {
            DynamicUIManager.CreateModal<ChooseModeModal>();
        }

        public void Feedback() {
            Process.Start(new ProcessStartInfo() {
                FileName = "https://github.com/Autodesk/synthesis/issues/new/choose",
                UseShellExecute = true
            });
        }

        public void Help() {
            Process.Start(new ProcessStartInfo() {
                FileName = "https://github.com/Autodesk/synthesis/issues/784",
                UseShellExecute = true
            });
        }
    }
}