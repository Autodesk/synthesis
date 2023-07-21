using System;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UI.Dynamic.Modals.MixAndMatch {
    public class MixAndMatchModal : ModalDynamic {
        private const float SPLIT_SPACING = 15;
        
        public MixAndMatchModal() : base(new Vector2(400, 55)) {}

        public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
            u.SetTopStretch<UIComponent>(anchoredY: offset);
            return u;
        };

        public override void Create() {
            Description.RootGameObject.SetActive(false);
            CreateChoosePartOrRobot();
        }

        private void CreateChoosePartOrRobot() {
            Title.SetText("Mix and Match Editor");
            
            AcceptButton.RootGameObject.SetActive(false);

            var (left, right) = SplitLeftRight();
            
            var robotEditorButton = left.CreateButton("Robot Editor")
                .ApplyTemplate<Button>(VerticalLayout)
                .AddOnClickedEvent(b => CreateChooseRobot());

            var partEditorButton = right.CreateButton("Part Editor")
                .ApplyTemplate<Button>(VerticalLayout)
                .AddOnClickedEvent(b => CreateChoosePart());
        }

        private void CreateChooseRobot() {
            Title.SetText("Choose a Robot");
            ClearContent();
            
            MainContent.CreateDropdown().ApplyTemplate(VerticalLayout);
            var (left, right) = SplitLeftRight();
            
            left.CreateButton("Select").ApplyTemplate(VerticalLayout);
            right.CreateButton("New").ApplyTemplate(VerticalLayout);
        }

        private void CreateChoosePart() {
            Title.SetText("Choose a Part");
            ClearContent();
            
            var (left, right) = SplitLeftRight();
            MainContent.CreateButton("Test").ApplyTemplate(VerticalLayout);
        }

        private (Content left, Content right) SplitLeftRight() =>
            MainContent.SplitLeftRight((MainContent.Size.x / 2f) - (SPLIT_SPACING / 2f), SPLIT_SPACING);

        public override void Delete() {}

        public override void Update() {}
    }
}