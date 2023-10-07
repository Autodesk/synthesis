using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Aether;
using SynthesisAPI.Aether.Lobby;
using SynthesisAPI.Utilities;
using TMPro;
using UnityEngine;
using Utilities.ColorManager;

#nullable enable

public class LobbyManagerModal : ModalDynamic {
    
    readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0);
        return u;
    };

    private static readonly Vector2 CONTENT_SIZE = new Vector2(900, 600);

    private const float PLAYER_LIST_WIDTH = 300f;
    private const float PLAYER_LIST_RIGHT_PADDING = 20f;

    private const float REFRESH_PLAYERS_INTERVAL = 1f;

    private readonly bool _isHost;
    private HostMode? _hostMode;
    private ClientMode? _clientMode;
    private Atomic<StringValue> _robotSelectionId = new(string.Empty);
    private Atomic<StringValue> _fieldSelectionId = new(string.Empty);

    private Label? _playerListLabel;
    private LabeledDropdown? _availableRobotsDropdown;
    private LabeledDropdown? _availableFieldsDropdown;

    private float _lastPlayerRefresh;

    private List<SynthesisDataDescriptor> _availableRobots = new();
    private List<SynthesisDataDescriptor> _availableFields = new();

    public LobbyManagerModal(IMode mode): base(CONTENT_SIZE) {
        if (mode is ClientMode) {
            _clientMode = mode as ClientMode;
            _isHost = false;
        } else {
            _hostMode = mode as HostMode;
            _isHost = true;
        }
    }

    public override void Create() {
        
        Title.SetText("Manage Lobby");
        Description.SetText("...");
        var (left, right) = MainContent.SplitLeftRight(PLAYER_LIST_WIDTH, PLAYER_LIST_RIGHT_PADDING);
        left.EnsureImage().StepIntoImage(i => i.SetSprite(null).SetColor(ColorManager.SynthesisColor.BackgroundSecondary)
            .SetCornerRadius(20f));

        if (_isHost) {
            AcceptButton.StepIntoLabel(l => l.SetText("Start")).AddOnClickedEvent(b => {
                var task = _hostMode?.UploadData();
                DynamicUIManager.CreateModal<NetworkWaitModal>(task, null, null);
            });
            CancelButton.RootGameObject.SetActive(false);
        } else {
            // TODO
        }

        _playerListLabel = left.CreateLabel().SetStretch(
            PLAYER_LIST_RIGHT_PADDING,
            PLAYER_LIST_RIGHT_PADDING,
            PLAYER_LIST_RIGHT_PADDING,
            PLAYER_LIST_RIGHT_PADDING
        ).SetVerticalAlignment(VerticalAlignmentOptions.Top);
        
        // Right
        _availableRobotsDropdown = right.CreateLabeledDropdown().SetTopStretch<LabeledDropdown>().StepIntoLabel(l => l.SetText("Robots"));
        if (_isHost) {
            _availableFieldsDropdown = right.CreateLabeledDropdown().SetTopStretch<LabeledDropdown>(anchoredY: _availableRobotsDropdown.Size.y + 10f)
                .StepIntoLabel(l => l.SetText("Fields"));
        }

        float heightAccum = 0f;
        var selectRobotButton = right.CreateButton("Select Robot").SetBottomStretch<Button>().AddOnClickedEvent(
            b => {
                string? currentSelection;
                if ((currentSelection = _robotSelectionId.Value) != string.Empty) {
                    ActiveClient()?.UnselectData(currentSelection).Wait();
                }

                var selection = _availableRobots[_availableRobotsDropdown.Dropdown.Value];
                var selectTask = ActiveClient()?.SelectData(selection.Owner, selection.Guid);
                selectTask?.ContinueWith(x => {
                    Debug.Log($"Robot Selection ID: {x.Result ?? "NULL"}");
                    _robotSelectionId.Value = x.Result ?? string.Empty;
                });
                selectTask?.Wait(); // TODO: don't hold main thread for this
            });
        heightAccum += selectRobotButton.Size.y + 10f;
        if (_isHost) {
            var selectFieldButton = right.CreateButton("Select Field").SetBottomStretch<Button>(anchoredY: heightAccum).AddOnClickedEvent(
                b => {
                    string? currentSelection;
                    if ((currentSelection = _fieldSelectionId.Value) != string.Empty) {
                        ActiveClient()?.UnselectData(currentSelection).Wait();
                    }
                    
                    var selection = _availableFields[_availableFieldsDropdown.Dropdown.Value];
                    var selectTask = ActiveClient()?.SelectData(selection.Owner, selection.Guid);
                    selectTask?.ContinueWith(x => {
                        Debug.Log($"Field Selection ID: {x.Result ?? "NULL"}");
                        _fieldSelectionId.Value = x.Result ?? string.Empty;
                    });
                    selectTask?.Wait(); // TODO: Same here
                });
            heightAccum += selectFieldButton.Size.y + 10f;
        }
        var refreshButton = right.CreateButton("Refresh Selections").SetBottomStretch<Button>(anchoredY: heightAccum).AddOnClickedEvent(
            b => {
                RefreshAvailableData();
            });

        heightAccum += refreshButton.Size.y + 10f;

        RefreshPlayers();
        RefreshAvailableData();
    }

    private void RefreshAvailableData() {
        var allAvailableTask = ActiveClient()!.GetAllAvailableData();
        allAvailableTask.Wait();
        
        _availableRobots.Clear();
        _availableRobots.AddRange(allAvailableTask.Result.Where(x => x.Description.Equals("Robot")));
        _availableRobotsDropdown.StepIntoDropdown(d => d.SetOptions(_availableRobots.Select(x => x.Name).ToArray()));
        
        _availableFields.Clear();
        _availableFields.AddRange(allAvailableTask.Result.Where(x => x.Description.Equals("Field")));
        _availableFieldsDropdown.StepIntoDropdown(d => d.SetOptions(_availableFields.Select(x => x.Name).ToArray()));
    }

    private void RefreshPlayers() {
        var playerInfoTask = ActiveClient()!.GetLobbyInformation();
        playerInfoTask.Wait();
        var playerInfo = playerInfoTask.Result.GetResult()!.FromGetLobbyInformation.LobbyInformation.Clients;
        
        StringBuilder players = new StringBuilder();

        if (playerInfo.Count > 0) {
            playerInfo.ForEach(x => {
                players.Append($"[{x.Guid}] {x.Name}");
                if (x.Selections.Any()) {
                    players.Append("(");
                    foreach (var kvp in x.Selections) {
                        players.Append($"{kvp.Value.Description.Name} ");
                    }
                    players.Append(")");
                }
                players.Append("\n");
            });
        } else {
            players.Append("No joined players");
        }

        _playerListLabel?.SetText(players.ToString());
    }

    public override void Update() {
        if (Time.realtimeSinceStartup - _lastPlayerRefresh > REFRESH_PLAYERS_INTERVAL) {
            RefreshPlayers();
            _lastPlayerRefresh = Time.realtimeSinceStartup;
        }
    }

    private LobbyClient? ActiveClient()
        => _isHost ? _hostMode!.HostClient : _clientMode?.Client;
    
    public override void Delete() { }
}
