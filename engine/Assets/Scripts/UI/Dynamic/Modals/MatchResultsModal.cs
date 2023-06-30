using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Dynamic.Modals {
    public class MatchResultsModal : ModalDynamic {
        private const float MODAL_WIDTH = 500f;
        private const float MODAL_HEIGHT = 600f;
        public MatchResultsModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) { }

        public override void Create() {

            bool isOnMainMenu = SceneManager.GetActiveScene().name != "MainScene";

            Title.SetText("Match Results");
            Description.SetText("Placeholder panel to show match results");
            Description.SetText("");
            AcceptButton.AddOnClickedEvent(x => {
                MatchStateMachine.Instance.SetState(MatchStateMachine.StateName.None);

            }).StepIntoLabel(l => l.SetText("Exit"));
            
            
        }

        public override void Update() { }
        public override void Delete() { }
    }
}
