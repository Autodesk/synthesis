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
            DynamicUIManager.CreateModal<TestModal>();
        }

        public void OpenSettingsPanel() {
            DynamicUIManager.CreateModal<SettingsModal>();
        }

        public void Singleplayer() {
            ModeManager.isSinglePlayer = true;
            SceneManager.LoadScene("MainScene");
        }

        public void Multiplayer() {
            ModeManager.isSinglePlayer = false;
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

        public void QuitSim() {
            DynamicUIManager.CreateModal<ExitSynthesisModal>();
        }

        public void ShowSocials() {
            Transform linksTransform =
                GameObject.Find("Canvas").transform.Find("MenuPanel").transform.Find("Links").transform;
            CanvasGroup cGroup  = linksTransform.Find("SocialsDropDownHide").gameObject.GetComponent<CanvasGroup>();
            cGroup.alpha        = 0;
            cGroup.interactable = false;
            linksTransform.Find("SocialsDropDownHide").gameObject.SetActive(false);

            cGroup              = linksTransform.Find("SocialsShadow").gameObject.GetComponent<CanvasGroup>();
            cGroup.alpha        = 1;
            cGroup.interactable = true;

            cGroup =
                linksTransform.Find("Socials").transform.Find("SocialsDropDown").gameObject.GetComponent<CanvasGroup>();
            cGroup.alpha        = 1;
            cGroup.interactable = true;
        }

        public void HideSocials() {
            Transform linksTransform =
                GameObject.Find("Canvas").transform.Find("MenuPanel").transform.Find("Links").transform;
            CanvasGroup cGroup  = linksTransform.Find("SocialsShadow").gameObject.GetComponent<CanvasGroup>();
            cGroup.alpha        = 0;
            cGroup.interactable = false;

            cGroup =
                linksTransform.Find("Socials").transform.Find("SocialsDropDown").gameObject.GetComponent<CanvasGroup>();
            cGroup.alpha        = 0;
            cGroup.interactable = false;

            linksTransform.Find("SocialsDropDownHide").gameObject.SetActive(true);
            cGroup              = linksTransform.Find("SocialsDropDownHide").gameObject.GetComponent<CanvasGroup>();
            cGroup.alpha        = 1;
            cGroup.interactable = true;
        }
    }
}