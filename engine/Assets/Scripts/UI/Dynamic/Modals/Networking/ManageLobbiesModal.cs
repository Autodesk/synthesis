using System;
using Synthesis.Networking;
using Synthesis.UI.Dynamic;
using UnityEngine;
using System.Threading.Tasks;
using Synthesis.UI;
using System.Collections.Generic;
using SynthesisServer.Proto;

#nullable enable

public class ManageLobbiesModal : ModalDynamic {

    public const float CHECK_INFO_DELAY = 1.5f;

    public ManageLobbiesModal() : base(new Vector2(1200, 800)) { }

    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset);
        return u;
    };

    private string _ip = "127.0.0.1";
    private string _username = "Epic Gamer";
    private InputField _ipInput;
    private InputField _nameInput;
    private Button _connectButton;
    private ScrollView _lobbiesScrollView;

    private Task<bool>? _connectionStatus;
    private Task<ServerInfoResponse?>? _serverInfoResponse = null;

    public override void Create() {
        Title.SetText("Manage Lobbies");
        Description.SetText("See and manage all lobbies on controlling server");

        _ipInput = MainContent.CreateInputField().StepIntoHint(l => l.SetText("Server IP"))
            // .SetTopStretch<InputField>()
            .StepIntoLabel(l => l.SetText("IP to LobbyHub"))
            .SetContentType(TMPro.TMP_InputField.ContentType.Standard)
            .AddOnValueChangedEvent((i, v) => _ip = v)
            .SetAnchor<InputField>(new Vector2(0, 1), new Vector2(0, 1))
            .SetPivot<InputField>(new Vector2(0, 1))
            .SetAnchoredPosition<InputField>(new Vector2(0, 0))
            .SetWidth<InputField>(400f)
            .SetValue(_ip);
        
        _nameInput = MainContent.CreateInputField().StepIntoHint(l => l.SetText("Name"))
            // .SetTopStretch<InputField>()
            .StepIntoLabel(l => l.SetText("Username"))
            .SetContentType(TMPro.TMP_InputField.ContentType.Alphanumeric)
            .AddOnValueChangedEvent((i, v) => _username = v)
            .SetAnchor<InputField>(new Vector2(0, 1), new Vector2(0, 1))
            .SetPivot<InputField>(new Vector2(0, 1))
            .SetAnchoredPosition<InputField>(new Vector2(410f, 0))
            .SetWidth<InputField>(400f)
            .SetValue(_username);
        
        _connectButton = MainContent.CreateButton().StepIntoLabel(l => l.SetText("Connect"))
            .ApplyTemplate<Button>(VerticalLayout)
            .AddOnClickedEvent(Connect);

        _lobbiesScrollView = MainContent.CreateScrollView();
        _lobbiesScrollView.SetStretch<ScrollView>(topPadding: -MainContent.RectOfChildren(_lobbiesScrollView).yMin + 7.5f, bottomPadding: 47.5f);

        var footerButtons = MainContent.CreateSubContent(new Vector2(MainContent.Size.x, 40f))
            .SetBottomStretch<Content>();
        footerButtons.CreateButton("Create Lobby").SetWidth<Button>(200f).SetRightStretch<Button>()
            .AddOnClickedEvent(b => NetworkManager.CreateLobby($"Lobby{(int)UnityEngine.Random.Range(1, 500)}"));
        footerButtons.CreateButton("Delete Lobby").SetWidth<Button>(200f).SetRightStretch<Button>(anchoredX: 210f)
            .AddOnClickedEvent(b => Debug.Log("TODO: Remove lobby"));
    }

    private void Connect(Button b) {

        switch (GetCurrentConnectionStatus()) {
            case ConnectionStatus.Connected:
                NetworkManager.DisconnectFromServer();
                break;
            case ConnectionStatus.NotConnected:
                _connectionStatus = NetworkManager.ConnectToServer(_ip, _username);
                break;
            default:
                return;
        }

        // Check if IP is valid
        
    }

    public override void Delete() { }

    private ConnectionStatus GetCurrentConnectionStatus() {
        ConnectionStatus currentConStatus = ConnectionStatus.NotConnected;
        if (_connectionStatus != null) {
            if (_connectionStatus.IsCompleted) {
                currentConStatus = _connectionStatus.Result ? ConnectionStatus.Connected : ConnectionStatus.NotConnected;
            } else {
                currentConStatus = ConnectionStatus.Idk;
            }
        }
        return currentConStatus;
    }

    private ConnectionStatus _lastConStatus = ConnectionStatus.NotConnected;
    private float _lastInfoRequest = Time.realtimeSinceStartup;
    public override void Update() {
        var currentConStatus = GetCurrentConnectionStatus();

        if (_lastConStatus != currentConStatus) {

            switch (currentConStatus) {
                case ConnectionStatus.Idk:
                    _connectButton.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK_ACCENT))
                        .StepIntoLabel(l => l.SetText("Connecting").SetFontStyle(TMPro.FontStyles.Italic));
                    break;
                case ConnectionStatus.Connected:
                    _connectButton.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE))
                        .StepIntoLabel(l => l.SetText("Disconnect").SetFontStyle(TMPro.FontStyles.Normal));
                    break;
                case ConnectionStatus.NotConnected:
                    _connectButton.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE))
                        .StepIntoLabel(l => l.SetText("Connect").SetFontStyle(TMPro.FontStyles.Normal));
                    break;
            }

            _lastConStatus = currentConStatus;
        }

        // Maybe throttle this down a bit
        if (_serverInfoResponse != null && _serverInfoResponse.IsCompleted) {

            // Debug.Log("Server Info Response received");
            // Update to data
            var info = _serverInfoResponse.Result;
            if (info != null) {
                UpdateLobbies(info);
                // DebugServerInfo(info);
            }

            _serverInfoResponse = null;
        }

        if (_serverInfoResponse == null && NetworkManager.IsConnected && Time.realtimeSinceStartup - _lastInfoRequest > CHECK_INFO_DELAY) {
            _lastInfoRequest = Time.realtimeSinceStartup;
            // Debug.Log("Making Info Request");
            _serverInfoResponse = NetworkManager.GetServerInfo();
        }
    }

    private List<Content> lobbyContents = new List<Content>();
    public void UpdateLobbies(ServerInfoResponse info) {
        // Purge
        _lobbiesScrollView.Content.DeleteAllChildren();
        lobbyContents.Clear();

        // Make new
        foreach (var lobby in info.Lobbies) {
            lobbyContents.Add(CreateLobbyInfo(lobby, _lobbiesScrollView.Content));
        }

        // Order
        float height = 0f;
        for (int i = 0; i < lobbyContents.Count; i++) {
            var c = lobbyContents[i];
            c.SetAnchoredPosition<Content>(new Vector2(0f, -height));
            height += c.Size.y;
        }
        _lobbiesScrollView.Content.SetHeight<ScrollView>(height);
    }

    public Content CreateLobbyInfo(Lobby lobby, Content parent) {
        var content = parent.CreateSubContent(new Vector2(50, 100))
            .SetTopStretch<Content>();
        content.EnsureImage().StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_WHITE));
        content.CreateLabel(30).SetTopStretch<Label>(leftPadding: 20f, anchoredY: 10f)
            .SetText($"Lobby Name: {lobby.LobbyName}")
            .SetColor(ColorManager.SYNTHESIS_BLACK)
            .SetFont(SynthesisAssetCollection.GetFont("Roboto-Regular SDF"));
        var clientsLabel = content.CreateLabel(30).SetTopStretch<Label>(leftPadding: 20f, anchoredY: 47.5f)
            .SetColor(ColorManager.SYNTHESIS_BLACK)
            .SetFont(SynthesisAssetCollection.GetFont("Roboto-Regular SDF"));
        clientsLabel.SetText("Clients: ");
        lobby.Clients.ForEach(x => {
            clientsLabel.SetText($"{clientsLabel.Text} '{x}'");
        });
        return content;
    }

    public void DebugServerInfo(ServerInfoResponse info) {
        Debug.Log($"Current Lobby: {info.CurrentLobby}");
        foreach (var lobby in info.Lobbies) {
            Debug.Log($"Lobby '{lobby.LobbyName}' with '{lobby.Clients.Count}' client(s)");
        }
    }

    private enum ConnectionStatus {
        Idk = 0, Connected = 1, NotConnected = 2
    }
}
