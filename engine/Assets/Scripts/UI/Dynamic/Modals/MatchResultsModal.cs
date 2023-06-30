using System;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Dynamic.Modals {
    public class MatchResultsModal : ModalDynamic {
        private const float MODAL_WIDTH = 500f;
        private const float MODAL_HEIGHT = 600f;
        
        private const float VERTICAL_PADDING = 16f;
        private const float HORIZONTAL_PADDING = 16f;
        private const float SCROLLBAR_WIDTH = 10f;
        private const float BUTTON_WIDTH = 64f;
        private const float ROW_HEIGHT = 64f;
        
        private float _scrollViewWidth;
        private float _entryWidth;
        
        private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0, rightPadding: 0);
            return u;
        };
    
        private readonly Func<UIComponent,  UIComponent> ListVerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: HORIZONTAL_PADDING, rightPadding: HORIZONTAL_PADDING);
            return u;
        };
        
        public MatchResultsModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) { }

        public override void Create() {

            bool isOnMainMenu = SceneManager.GetActiveScene().name != "MainScene";

            Title.SetText("Match Results");
            Description.SetText("Placeholder panel to show match results");
            Description.SetText("");
            AcceptButton.AddOnClickedEvent(x => {
                MatchStateMachine.Instance.SetState(MatchStateMachine.StateName.None);

            }).StepIntoLabel(l => l.SetText("Exit"));
            
            var _scrollView = MainContent.CreateScrollView().SetRightStretch<ScrollView>().ApplyTemplate(VerticalLayout)
                .SetHeight<ScrollView>(MODAL_HEIGHT - VERTICAL_PADDING * 2 - 50);

            MatchMode.MatchResultsTracker.TrackedResults.ForEach(x =>
            {
                var entry = x.Value;

                Debug.Log($"{entry.GetName()}: {entry.ToString()}");

                _scrollViewWidth = _scrollView.Parent!.RectOfChildren().width - SCROLLBAR_WIDTH;
                _entryWidth = _scrollViewWidth - HORIZONTAL_PADDING * 2;

                //_scrollView.Content.CreateSubContent((new Vector2(_entryWidth, ROW_HEIGHT)));


                /*(Content leftContent, Content rightContent) = _scrollView.Content
                    .CreateSubContent(new Vector2(_entryWidth, ROW_HEIGHT))
                    .ApplyTemplate(ListVerticalLayout)
                    .SplitLeftRight(BUTTON_WIDTH, HORIZONTAL_PADDING);

                (Content labelsContent, Content buttonsContent) =
                    rightContent.SplitLeftRight(_entryWidth - (HORIZONTAL_PADDING + BUTTON_WIDTH) * 3,
                        HORIZONTAL_PADDING);*/
                
                //(Content topContent, Content bottomContent) = labelsContent.SplitTopBottom(ROW_HEIGHT / 2, 0);
                /*topContent.CreateLabel().SetText(entry.GetName())
                    .ApplyTemplate(VerticalLayout)
                    /*  
                    .SetAnchorLeft<Label>()
                    #1#
                    .SetAnchoredPosition<Label>(new Vector2(0, -ROW_HEIGHT / 8));*/

                Content _entryContent = _scrollView.Content
                    .CreateSubContent(new Vector2(_entryWidth, ROW_HEIGHT))
                    .ApplyTemplate(ListVerticalLayout);
                
                _entryContent.Image?.SetColor(ColorManager.SYNTHESIS_BLACK);
                    


                _entryContent.CreateLabel().SetText($"{entry.GetName()}: {entry.ToString()}")
                    .ApplyTemplate(VerticalLayout)
                    /*.SetAnchorLeft<Label>()*/
                    .SetAnchoredPosition<Label>(new Vector2(0, -ROW_HEIGHT / 8));
            });

            /*(Content editButtonContent, Content deleteButtonContent) = buttonsContent.SplitLeftRight(BUTTON_WIDTH, HORIZONTAL_PADDING);
            editButtonContent.CreateButton().StepIntoLabel(l => l.SetText("Edit"))
                .AddOnClickedEvent(b => OpenScoringZoneGizmo(zone))
                .ApplyTemplate(VerticalLayout).SetSize<Button>(new Vector2(BUTTON_WIDTH, ROW_HEIGHT)).SetStretch<Button>();
            deleteButtonContent.CreateButton().StepIntoLabel(l => l.SetText("Delete"))
                .AddOnClickedEvent(b => {
                    FieldSimObject.CurrentField.ScoringZones.Remove(zone);
                    GameObject.Destroy(zone.GameObject);
                    AddZoneEntries();
                })
                .ApplyTemplate(VerticalLayout).SetSize<Button>(new Vector2(BUTTON_WIDTH, ROW_HEIGHT)).SetStretch<Button>();
                });*/
        }

        public override void Update() { }
        public override void Delete() { }
    }
}
