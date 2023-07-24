using System;
using Synthesis.UI.Dynamic;
using UnityEngine;

namespace UI.Dynamic.Modals.MixAndMatch {
    public class MAMChoosePartModal : ModalDynamic {
        private const float SPLIT_SPACING = 15;
        private const float CONTENT_WIDTH = 400;
        private const float CONTENT_HEIGHT = 120;

        public MAMChoosePartModal() : base(new Vector2(CONTENT_WIDTH, CONTENT_HEIGHT)) {}

        public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
            u.SetTopStretch<UIComponent>(anchoredY: offset);
            return u;
        };

        public override void Create() {
            Title.SetText("Select or Create a Part");
            
            AcceptButton.RootGameObject.SetActive(false);
            Description.RootGameObject.SetActive(false);
            
            MainContent.CreateDropdown().ApplyTemplate(VerticalLayout);
            var (left, right) = MainContent.CreateSubContent(new Vector2(MainContent.Size.x, 50)).ApplyTemplate(VerticalLayout).SplitLeftRight((MainContent.Size.x / 2f) - (SPLIT_SPACING / 2f), SPLIT_SPACING);
            
            left.CreateButton("Select").ApplyTemplate(VerticalLayout);
            right.CreateButton("New").ApplyTemplate(VerticalLayout);
        }

        public override void Delete() {}

        public override void Update() {}
    }
}