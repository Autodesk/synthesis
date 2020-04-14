using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.Simulator.UI
{
    public class MainTabUI : MonoBehaviour
    {
        public MainTabUI() { }

        public void onLoadRobotClicked()
        {
            Debug.Log("Clicked on Load Robot");
        }

        public void onLoadFieldClicked()
        {
            Debug.Log("Clicked on Load Field");
        }
    }
}