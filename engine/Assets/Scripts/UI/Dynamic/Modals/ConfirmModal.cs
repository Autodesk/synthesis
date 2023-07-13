using Synthesis.UI.Dynamic;
using UnityEngine;

public class ConfirmModal : ModalDynamic {
    private const float _width  = 300f;
    private const float _height = 0f;

    private string _message;

    public ConfirmModal(string message) : base(new Vector2(_width, _height)) {
        _message = message;
    }

    public override void Create() {
        Title.SetText(_message);
        Description.RootGameObject.SetActive(false);
        MainContent.RootGameObject.SetActive(false);

        AcceptButton.StepIntoLabel(l => l.SetText("Confirm"));
    }

    public override void Update() {}

    public override void Delete() {}
}