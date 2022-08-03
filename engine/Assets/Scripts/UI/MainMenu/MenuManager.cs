using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Synthesis.UI {
    public class MenuManager : MonoBehaviour {
        private void Start()
        {
            SettingsModal.LoadSettings();
        }
        public void ButtonPrint(string s) {
            Debug.Log("Button Pressed: " + s);
            DynamicUIManager.CreateModal<TestModal>();
        }

        public void OpenSettingsPanel()
        {
            DynamicUIManager.CreateModal<SettingsModal>();
        }

        public void Singleplayer()
        {
            DynamicUIManager.CreateModal<ChooseModeModal>();
        }
    }
}