using System.Collections;
using System.Collections.Generic;
using Analytics;
using Modes.MatchMode;
using Synthesis.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.ColorManager;

namespace Synthesis.UI.Dynamic {
    public class ExitSynthesisModal : ModalDynamic {
        public ExitSynthesisModal() : base(new Vector2(350, 0)) {}

        public override void Create() {
            bool isOnMainMenu = SceneManager.GetActiveScene().name != "MainScene";

            Title.SetText(isOnMainMenu ? "Exit Synthesis" : "Leave Simulation");

            AcceptButton
                .AddOnClickedEvent(x => {
                    if (isOnMainMenu) {
                        Application.Quit();
                    } else {
                        SimulationRunner.InSim = false;
                        DynamicUIManager.CloseAllPanels(true);
                        MatchMode.ResetMatchConfiguration();
                        ModeManager.CurrentMode = null;
                        SceneManager.LoadScene("GridMenuScene", LoadSceneMode.Single);

                        AnalyticsManager.LogCustomEvent(AnalyticsEvent.ExitedToMenu);
                    }
                })
                .StepIntoLabel(l => l.SetText(isOnMainMenu ? "Exit" : "Leave"));

            ModalIcon.SetSprite(SynthesisAssetCollection.GetSpriteByName("CloseIcon"))
                .SetColor(ColorManager.SynthesisColor.MainText);
        }

        public override void Update() {}

        public override void Delete() {}
    }
}
