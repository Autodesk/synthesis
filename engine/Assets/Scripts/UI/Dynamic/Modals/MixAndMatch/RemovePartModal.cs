using System;
using Synthesis.UI.Dynamic;
using UnityEngine;

namespace UI.Dynamic.Modals.MixAndMatch {
    public class RemovePartModal : ModalDynamic {
        private const float CONTENT_WIDTH  = 400;
        private const float CONTENT_HEIGHT = 0;

        private readonly Action _callback;

        public RemovePartModal(Action callback) : base(new Vector2(CONTENT_WIDTH, CONTENT_HEIGHT)) {
            _callback = callback;
        }

        public override void Create() {
            ModalIcon.UnityImage.sprite = SynthesisAssetCollection.GetSpriteByName("trash-icon");
            Title.SetText("Remove Part?");

            AcceptButton.AddOnClickedEvent(
                _ => {
                    _callback();
                    DynamicUIManager.CloseActiveModal();
                });
        }

        public override void Update() {}

        public override void Delete() {}
    }
}