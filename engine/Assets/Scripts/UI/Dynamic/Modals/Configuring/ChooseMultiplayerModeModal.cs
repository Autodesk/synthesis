using System;
using Modes.MatchMode;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UI.EventListeners;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.ColorManager;
using Object = UnityEngine.Object;

public class ChooseMultiplayerModeModal : ModalDynamic {
    readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0);
        return u;
    };

    public ChooseMultiplayerModeModal() : base(new Vector2(230, 80)) {}

    public override void Create() {
        Title.SetText("Choose Mode");
        Description.SetText("Choose a mode to play in.");

        ModalIcon.SetSprite(SynthesisAssetCollection.GetSpriteByName("settings"));

        AcceptButton.RootGameObject.SetActive(false);
        CancelButton.RootGameObject.SetActive(false);
        MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Server Test Mode"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b => {
                ModeManager.CurrentMode = new ServerTestMode();
                if (SceneManager.GetActiveScene().name != "MainScene")
                    SceneManager.LoadScene("MainScene");
            });

        var comingSoonButton = MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Coming Soon"))
            .ApplyTemplate(VerticalLayout)
            .StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.InteractiveBackground))
                .StepIntoLabel(l => l.SetColor(ColorManager.SynthesisColor.InteractiveElementText))
                .DisableEvents<Button>();
        
        Object.Destroy(comingSoonButton.RootGameObject.transform.Find("Button").GetComponent<HoverEventListener>());
    }

    public override void Update() {}

    public override void Delete() {}
}