#nullable enable

using System;
using Synthesis.UI.Dynamic;
using UnityEngine;
public class ManageLobbiesModal : ModalDynamic {
    public const float CHECK_INFO_DELAY = 1.5f;
    private const float MODAL_WIDTH     = 1200f;
    private const float MODAL_HEIGHT    = 800f;

    public ManageLobbiesModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset);
        return u;
    };

    private string _ip       = "127.0.0.1";
    private string _username = "Epic Gamer";
    private InputField _ipInput;
    private InputField _nameInput;
    private Button _connectButton;
    private ScrollView _lobbiesScrollView;

    public override void Create() {
        Title.SetText("Manage Lobbies");
        Description.SetText("See and manage all lobbies on controlling server");
    }

    public override void Update() {}

    public override void Delete() {}
}
