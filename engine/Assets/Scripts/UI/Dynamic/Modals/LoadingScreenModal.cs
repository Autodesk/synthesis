using Synthesis.UI.Dynamic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenModal : ModalDynamic {
    public LoadingScreenModal() : base(new Vector2(300, -80)) {
    }

    public override void Create() {
        MainContent.CreateLabel(40)
            .SetFontSize(50)
            .SetText("Loading...")
            .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
            .SetAnchoredPosition<Label>(new Vector2(0, 10));
        Description.RootGameObject.SetActive(false);
        Title.RootGameObject.SetActive(false);
        AcceptButton.RootGameObject.SetActive(false);
        CancelButton.RootGameObject.SetActive(false);
        ModalImage.RootGameObject.SetActive(false);
    }

    public override void Update() {
    }

    public override void Delete() {
    }
}
