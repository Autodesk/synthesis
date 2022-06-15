using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.UI
{
    public class MenuManager : MonoBehaviour
    {
        //attached to the main menu game object for grid layout scene

        public void ButtonPrint(string s)
        {
            Debug.Log("Button Pressed: " + s);
        }
    }
}