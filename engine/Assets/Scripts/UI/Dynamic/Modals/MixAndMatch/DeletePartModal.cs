using System;
using Synthesis.UI.Dynamic;
using UnityEngine;

namespace UI.Dynamic.Modals.MixAndMatch {
    public class DeletePartModal : ModalDynamic {
        public DeletePartModal(Vector2 mainContentSize) : base(mainContentSize) { }

        public override void Create() {
            Title.SetText("Delete Part?");

            // TODO: Delete the part or robots json file
            AcceptButton.AddOnClickedEvent(_ => throw new NotImplementedException());
        }

        public override void Update() { }

        public override void Delete() { }
    }
}