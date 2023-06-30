using System.Collections;
using System.Collections.Generic;
using Synthesis.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Synthesis.UI.Dynamic {
    public class MatchResultsModal : ModalDynamic {
        public MatchResultsModal() : base(new Vector2(350, 50)) {}

        public override void Create() {
            bool isOnMainMenu = SceneManager.GetActiveScene().name != "MainScene";

            Title.SetText("Match Results");
            Description.SetText("Placeholder panel to show match results");
            Description.SetText("");
            AcceptButton
                .AddOnClickedEvent(x => {
                    MatchStateMachine.Instance.SetState(MatchStateMachine.StateName.None);
                })
                .StepIntoLabel(l => l.SetText("Exit"));
        }

        public override void Update() {}

        public override void Delete() {}
    }
}
