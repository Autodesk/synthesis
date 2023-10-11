using System;
using System.Collections;
using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using UnityEngine;
using SynthesisAPI.Utilities;
using TMPro;
using Logger = SynthesisAPI.Utilities.Logger;

#nullable enable

public class InitLobbyConnection : ModalDynamic {

    private static readonly Vector2 MODAL_SIZE = new Vector2(300, 200);

    private readonly bool _isHost;
    private string? _ip;
    private string _username = string.Empty;

    private InputField? _ipInputField;
    private InputField? _usernameInputField;

    public InitLobbyConnection(string? ip): base(MODAL_SIZE) {
        _ip = ip;
        _isHost = ip != null;
    }
    
    public override void Create() {

        Title.SetText("Connect");
        Description.SetText("");

        AcceptButton.AddOnClickedEvent(b => {
            if (_isHost) {
                var mode = (ModeManager.CurrentMode as HostMode)!;

                DynamicUIManager.CreateModal<NetworkWaitModal>(
                    true,
                    mode.StartHostClient(_username),
                    typeof(LobbyManagerModal),
                    ModeManager.CurrentMode
                );

            } else {
                var mode = (ModeManager.CurrentMode as ClientMode)!;

                mode.StartClient(_ip ?? "127.0.0.1", _username);
                if (mode.Client.IsAlive) {
                    DynamicUIManager.CreateModal<LobbyManagerModal>(
                        true,
                        ModeManager.CurrentMode
                    );
                }
            }
        });
        CancelButton.RootGameObject.SetActive(false);

        if (!_isHost) {
            _ipInputField = MainContent.CreateInputField().SetTopStretch<InputField>()
                .StepIntoHint(h => h.SetText(_ip ?? "Enter IP...")).AddOnValueChangedEvent((i, v) => {
                    _ip = v;
                }).StepIntoLabel(l => l.SetText("IP Address"));
        }

        _usernameInputField = MainContent.CreateInputField().SetTopStretch<InputField>(anchoredY: _isHost ? 0f : _ipInputField!.Size.y + 15f)
            .StepIntoHint(h => h.SetText("Mary, Tim, Alice, Bob...")).AddOnValueChangedEvent((i, v) => {
                _username = v;
            }).StepIntoLabel(l => l.SetText("Username"));
    }

    public override void Update() {
        if ((_ip ?? string.Empty) == string.Empty || _username == string.Empty) {
            AcceptButton.RootGameObject.SetActive(false);
        } else {
            AcceptButton.RootGameObject.SetActive(true);
        }
    }
    
    public override void Delete() {
        
    }
}
