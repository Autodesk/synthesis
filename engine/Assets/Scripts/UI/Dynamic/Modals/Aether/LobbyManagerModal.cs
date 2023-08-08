using System.Collections;
using System.Collections.Generic;
using System.Text;
using Synthesis.UI.Dynamic;
using TMPro;
using UnityEngine;
using Utilities.ColorManager;

public class LobbyManagerModal : ModalDynamic {

    private static readonly Vector2 CONTENT_SIZE = new Vector2(900, 600);

    private const float PLAYER_LIST_WIDTH = 300f;
    private const float PLAYER_LIST_RIGHT_PADDING = 20f;

    private const float REFRESH_PLAYERS_INTERVAL = 1f;

    private HostMode _mode;

    private Label _playerListLabel;

    private float _lastPlayerRefresh;

    public LobbyManagerModal(HostMode mode): base(CONTENT_SIZE) {
        _mode = mode;
    }

    public override void Create() {
        Title.SetText("Manage Lobby");
        Description.SetText("...");
        var (left, right) = MainContent.SplitLeftRight(PLAYER_LIST_WIDTH, PLAYER_LIST_RIGHT_PADDING);
        left.EnsureImage().StepIntoImage(i => i.SetSprite(null).SetColor(ColorManager.SynthesisColor.BackgroundSecondary)
            .SetCornerRadius(20f));

        AcceptButton.StepIntoLabel(l => l.SetText("Start")).AddOnClickedEvent(b => {
            (var task, var status) = _mode.UploadData();
            DynamicUIManager.CreateModal<NetworkWaitModal>(task, status);
        });
        CancelButton.RootGameObject.SetActive(false);

        _playerListLabel = left.CreateLabel().SetStretch(
            PLAYER_LIST_RIGHT_PADDING,
            PLAYER_LIST_RIGHT_PADDING,
            PLAYER_LIST_RIGHT_PADDING,
            PLAYER_LIST_RIGHT_PADDING
        ).SetVerticalAlignment(VerticalAlignmentOptions.Top);
        RefreshPlayers();
    }

    private void RefreshPlayers() {
        var playerInfo = _mode.AllClients;
        StringBuilder players = new StringBuilder();

        if (playerInfo.Count > 0) {
            playerInfo.ForEach(x => players.Append($"[{x.Guid}] {x.Name}\n"));
        } else {
            players.Append("No joined players");
        }

        _playerListLabel.SetText(players.ToString());
    }

    public override void Update() {
        if (Time.realtimeSinceStartup - _lastPlayerRefresh > REFRESH_PLAYERS_INTERVAL) {
            RefreshPlayers();
            _lastPlayerRefresh = Time.realtimeSinceStartup;
        }
    }
    
    public override void Delete() { }
}
