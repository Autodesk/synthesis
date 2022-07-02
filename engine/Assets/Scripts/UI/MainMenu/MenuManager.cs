using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Synthesis.UI {
    public class MenuManager : MonoBehaviour {
        public void ButtonPrint(string s) {
            Debug.Log("Button Pressed: " + s);
            DynamicUIManager.CreateModal<TestModal>();
        }

        public void Singleplayer()
        {
            DynamicUIManager.CreatePopup<ChooseModePopup>();
        }
    }
}