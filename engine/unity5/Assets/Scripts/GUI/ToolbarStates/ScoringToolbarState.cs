using Synthesis.DriverPractice;
using Synthesis.FSM;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GUI
{
    /// <summary>
    /// The state that controls the scoring toolbar and related state functions. Because of the nature of the specific functions,
    /// the scoring toolbar buttons are tethered within Unity.
    /// </summary>
    public class ScoringToolbarState : State
    {
        GameObject canvas;
        GameObject toolbar;

        public override void Start()
        {
            canvas = GameObject.Find("Canvas");
            toolbar = Auxiliary.FindObject(canvas, "ScoringToolbar");
        }

        public override void ToggleHidden()
        {
            toolbar.SetActive(!toolbar.activeSelf);
        }
    }
}