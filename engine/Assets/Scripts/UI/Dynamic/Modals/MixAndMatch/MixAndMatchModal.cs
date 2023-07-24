using System;
using Synthesis.UI.Dynamic;
using UnityEngine;

namespace UI.Dynamic.Modals.MixAndMatch {
    public class MixAndMatchModal : ModalDynamic {
        private const float SPLIT_SPACING = 15;
        private const float CONTENT_WIDTH = 400;
        private const float CHOOSE_MODE_HEIGHT = 55;
        private const float SELECT_PART_HEIGHT = 100;

        public MixAndMatchModal() : base(new Vector2(CONTENT_WIDTH, CHOOSE_MODE_HEIGHT)) {}

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
            Title.SetText("Mix and Match Robot Editor");
            
            AcceptButton.RootGameObject.SetActive(false);

            var (left, right) = MainContent.SplitLeftRight((MainContent.Size.x / 2f) - (SPLIT_SPACING / 2f), SPLIT_SPACING);
            
            var robotEditorButton = left.CreateButton("Robot Editor")
                .ApplyTemplate<Button>(VerticalLayout)
                .AddOnClickedEvent(b => DynamicUIManager.CreateModal<MAMChooseRobotModal>());

            var partEditorButton = right.CreateButton("Part Editor")
                .ApplyTemplate<Button>(VerticalLayout)
                .AddOnClickedEvent(b => ClearAndResizeContent(new Vector2(CONTENT_WIDTH, 200)));
        }

        /*
        private void CreateChooseRobot() {
            MainContent.SetHeight<Content>(SELECT_PART_HEIGHT);

            ClearContent();

            MainContent.CreateDropdown().ApplyTemplate(VerticalLayout);
            var (left, right) = MainContent.CreateSubContent(new Vector2(MainContent.Size.x, 50)).ApplyTemplate(VerticalLayout).SplitLeftRight((MainContent.Size.x / 2f) - (SPLIT_SPACING / 2f), SPLIT_SPACING);
            
            left.CreateButton("Select").ApplyTemplate(VerticalLayout);
            right.CreateButton("New").ApplyTemplate(VerticalLayout);
        }

        private void CreateChoosePart() {
            Title.SetText("Choose a Part");
            ClearContent();
        }*/

        public override void Delete() {}

        public override void Update() {}
    }
}