using System.Collections;
using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using UnityEngine;

namespace Synthesis.UI.Dynamic {
    public class TestModal : ModalDynamic {
        public TestModal() : base(new Vector2(500, 500)) { }

        public override void Create() {
            (var left, var right) = base.MainContent.SplitLeftRight(250, 100);
            base.AcceptButton.ButtonText.text = "Hello";
            base.AcceptButton.UnityButton.onClick.AddListener(() => Debug.Log("Hello from the Test Modal"));
        }
    }
}
