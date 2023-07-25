using System.Diagnostics;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

using Debug = UnityEngine.Debug;

namespace Synthesis.UI {
    public class MenuManager : MonoBehaviour {
        bool socialDropDownHover = false;

        private void Start() {
            SettingsModal.LoadSettings();
            SettingsModal.ApplySettings();
        }

        public void ButtonPrint(string s) {
            Debug.Log("Button Pressed: " + s);
            DynamicUIManager.CreateModal<TestModal>();
        }

        public void OpenSettingsPanel() {
            DynamicUIManager.CreateModal<SettingsModal>();
        }

        public void Singleplayer() {
            SceneManager.LoadScene("MainScene");
        }

        public void Feedback() {
            Process.Start(new ProcessStartInfo() { FileName = "https://github.com/Autodesk/synthesis/issues/new/choose",
                UseShellExecute                             = true });
        }

        public void Help() {
            Process.Start(
                new ProcessStartInfo() { FileName = "https://www.discord.gg/hHcF9AVgZA", UseShellExecute = true });
        }

        public void ShowSocials() {
            GameObject[] objects = GameObject.FindGameObjectsWithTag("socials-drop-down");
            foreach (GameObject gObject in objects) {
                CanvasGroup cGroup = gObject.GetComponent<CanvasGroup>();
                cGroup.alpha = 1;
                cGroup.interactable = true;
            };
        }

        public void HideSocials() {
            if (socialDropDownHover) return;
            GameObject[] objects = GameObject.FindGameObjectsWithTag("socials-drop-down");
            foreach (GameObject gObject in objects) {
                CanvasGroup cGroup = gObject.GetComponent<CanvasGroup>();
                cGroup.alpha = 0;
                cGroup.interactable = false;
            };
        }

        public void EnterSocialsDrop() {
            socialDropDownHover = true;
        }

        public void ExitSocialsDrop() {
            socialDropDownHover = false;
        }
    }
}