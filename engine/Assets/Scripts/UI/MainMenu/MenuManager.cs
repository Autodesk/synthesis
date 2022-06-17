using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.UI {
    public class MenuManager : MonoBehaviour {
        public void ButtonPrint(string s) {
            Debug.Log("Button Pressed: " + s);
        }
    }
}