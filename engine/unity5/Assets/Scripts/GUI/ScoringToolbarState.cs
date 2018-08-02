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
    public class ScoringToolbarState : State
    {
        GameObject canvas;

        GameObject helpMenu;
        GameObject toolbar;
        GameObject overlay;
        GameObject tabs;
        public override void Start()
        {
            canvas = GameObject.Find("Canvas");

            tabs = Auxiliary.FindObject(canvas, "Tabs");
            toolbar = Auxiliary.FindObject(canvas, "ScoringToolbar");
            helpMenu = Auxiliary.FindObject(canvas, "Help");
            overlay = Auxiliary.FindObject(canvas, "Overlay");

            Button helpButton = Auxiliary.FindObject(helpMenu, "CloseHelpButton").GetComponent<Button>();
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(CloseHelpMenu);
        }

        public void OnHelpButtonPressed()
        {
            helpMenu.SetActive(true);
            Auxiliary.FindObject(helpMenu, "Type").GetComponent<Text>().text = "ScoringToolbar";
            overlay.SetActive(true);
            tabs.transform.Translate(new Vector3(200, 0, 0));
            foreach (Transform t in toolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(200, 0, 0));
                else t.gameObject.SetActive(false);
            }
        }
        private void CloseHelpMenu()
        {
            helpMenu.SetActive(false);
            overlay.SetActive(false);
            tabs.transform.Translate(new Vector3(-200, 0, 0));
            foreach (Transform t in toolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(-200, 0, 0));
                else t.gameObject.SetActive(true);
            }
        }

    }
}