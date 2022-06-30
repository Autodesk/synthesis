using System;
using System.Collections;
using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Synthesis.UI {
    public class MenuManager : MonoBehaviour {
        public void ButtonPrint(string s) {
            Debug.Log("Button Pressed: " + s);
        }

        public void PracticeMode()
        {
            ModeManager.CurrentMode = ModeManager.Mode.Practice;
            SceneManager.LoadScene("MainScene");
        }
    }
}