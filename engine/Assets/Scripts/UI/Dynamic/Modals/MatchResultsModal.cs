using System;
using Modes.MatchMode;
using Synthesis.Runtime;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static MatchResultsTracker;

namespace UI.Dynamic.Modals {
    public class MatchResultsModal : ModalDynamic {
        private const float MODAL_WIDTH  = 500f;
        private const float MODAL_HEIGHT = 600f;

        private const float VERTICAL_PADDING         = 16f;
        private const float HORIZONTAL_PADDING       = 16f;
        private const float SCROLLBAR_WIDTH          = 10f;
        private const float ROW_HEIGHT               = 64f;
        private static readonly Color ENTRY_BG_COLOR = ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK);

        private float _scrollViewWidth;
        private float _entryWidth;

        private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0, rightPadding: 0);
            return u;
        };

        private readonly Func<UIComponent, UIComponent> ListVerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(
                anchoredY: offset, leftPadding: HORIZONTAL_PADDING, rightPadding: HORIZONTAL_PADDING);
            return u;
        };

        public MatchResultsModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

        public override void Create() {
            Title.SetText("Match Results");
            Description.SetText("Statistics about the match");

            CancelButton
                .AddOnClickedEvent(x => {
                    SimulationRunner.InSim  = false;
                    ModeManager.CurrentMode = null;

                    DynamicUIManager.CloseAllPanels(true);
                    DynamicUIManager.CloseActiveModal();

                    SceneManager.LoadScene("GridMenuScene", LoadSceneMode.Single);
                })
                .StepIntoLabel(l => l.SetText("Exit"));

            MiddleButton
                .AddOnClickedEvent(x => {
                    DynamicUIManager.CloseActiveModal();
                    MatchStateMachine.Instance.SetState(MatchStateMachine.StateName.Reconfigure);
                })
                .StepIntoLabel(l => l.SetText("Configure"));

            AcceptButton
                .AddOnClickedEvent(x => {
                    DynamicUIManager.CloseActiveModal();
                    MatchStateMachine.Instance.SetState(MatchStateMachine.StateName.Restart);
                })
                .StepIntoLabel(l => l.SetText("Restart"));

            CreateScrollMenu();
        }

        private RectTransform _middleButtonObject;

        /// Creates the main scroll menu and adds all of the match result entries
        public void CreateScrollMenu() {
            var scrollView = MainContent.CreateScrollView()
                                 .SetRightStretch<ScrollView>()
                                 .ApplyTemplate(VerticalLayout)
                                 .SetHeight<ScrollView>(MODAL_HEIGHT - VERTICAL_PADDING * 2 - 50);

            MatchMode.MatchResultsTracker.MatchResultEntries.ForEach(x => {
                var entry = x.Value;

                _scrollViewWidth = scrollView.Parent!.RectOfChildren().width - SCROLLBAR_WIDTH;
                _entryWidth      = _scrollViewWidth - HORIZONTAL_PADDING * 2;

                Content entryContent = scrollView.Content.CreateSubContent(new Vector2(_entryWidth, ROW_HEIGHT))
                                           .ApplyTemplate(ListVerticalLayout);

                (Content left, Content right) = entryContent.ApplyTemplate(ListVerticalLayout)
                                                    .SplitLeftRight(_entryWidth * (2 / 3f), HORIZONTAL_PADDING);

                left.SetBackgroundColor<Content>(ENTRY_BG_COLOR)
                    .CreateLabel()
                    .SetAnchoredPosition<Label>(new Vector2(HORIZONTAL_PADDING, 0))
                    .SetText(entry.GetName());
                right.SetBackgroundColor<Content>(ENTRY_BG_COLOR)
                    .CreateLabel()
                    .SetAnchoredPosition<Label>(new Vector2(HORIZONTAL_PADDING, 0))
                    .SetText(entry.ToString());
            });
        }

        public override void Update() {}

        public override void Delete() {}
    }
}
