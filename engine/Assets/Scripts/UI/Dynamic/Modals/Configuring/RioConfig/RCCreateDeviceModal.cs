using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;
using Utilities.ColorManager;

#nullable enable

public class RCCreateDeviceModal : ModalDynamic {
    public RCCreateDeviceModal() : base(new Vector2(400, 120)) {}

    private LabeledDropdown _typeDropdown;

    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset);
        return u;
    };

    public static readonly string[] EntryTypes =
        new string[] { RioConfigurationModal.PWM, RioConfigurationModal.ENCODER };

    public override void Create() {
        Title.SetText("Create Device");
        Title.SetWidth<Label>(400);
        Description.SetText("Create a Motor Controller, Encoder, Etc.");
        
        ModalImage.SetSprite(SynthesisAssetCollection.GetSpriteByName("wrench-icon"));
        ModalImage.SetColor(ColorManager.SynthesisColor.MainText);

        AcceptButton
            .AddOnClickedEvent(b => {
                switch (_typeDropdown.Dropdown.SelectedOption.text) {
                    case RioConfigurationModal.PWM:
                        DynamicUIManager.CreateModal<RCConfigPwmGroupModal>();
                        break;
                    case RioConfigurationModal.ENCODER:
                        DynamicUIManager.CreateModal<RCConfigEncoderModal>();
                        break;
                    default:
                        break;
                }
            })
            .StepIntoLabel(l => l.SetText("Next"));
        CancelButton.AddOnClickedEvent(b => { DynamicUIManager.CreateModal<RioConfigurationModal>(); });

        _typeDropdown = MainContent.CreateLabeledDropdown().StepIntoLabel(l => l.SetText("Type"));
        _typeDropdown.StepIntoDropdown(d => d.SetOptions(EntryTypes));
        _typeDropdown.SetTopStretch<LabeledDropdown>();
        // _typeDropdown.StepIntoDropdown(d => d.AddOnValueChangedEvent(UpdateIDSelection));

        // _idDropdown.StepIntoDropdown(d => d.AddOnValueChangedEvent((d, i, data) => {
        //     _selectedId = data.text;
        // }));
    }

    public override void Delete() {}

    public override void Update() {}
}

public class RCConfigPwmGroupModal : ModalDynamic {
    private const int MODAL_WIDTH = 900, MODAL_HEIGHT = 400;

    public RCConfigPwmGroupModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

    public RCConfigPwmGroupModal(PWMGroupEntry entry) : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {
        _entry = entry;
    }

    private PWMGroupEntry? _entry;

    private InputField _nameInput;
    private ScrollView _signalSelection;
    private Dictionary<string, Toggle> _signalToggles;
    private ScrollView _portSelection;
    private Dictionary<string, Toggle> _portToggles;

    private List<(string name, string guid)> _options;

    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset);
        return u;
    };

    public override void Create() {
        Title.SetText("Create Device");
        Title.SetWidth<Label>(400);
        Description.SetText("Create a Motor Controller, Encoder, Etc.");
        
        ModalImage.SetSprite(SynthesisAssetCollection.GetSpriteByName("wrench-icon"));
        ModalImage.SetColor(ColorManager.SynthesisColor.MainText);

        AcceptButton
            .AddOnClickedEvent(b => {
                var entry = new PWMGroupEntry(_nameInput.Value,
                    _portToggles.Where(x => x.Value.State).Select(x => x.Key).ToArray(),
                    _signalToggles.Where(x => x.Value.State).Select(x => x.Key).ToArray());
                if (_entry != null) {
                    int n = RioConfigurationModal.Entries.RemoveAll(e => e.Equals(_entry));
                }
                RioConfigurationModal.Entries.Add(entry);

                DynamicUIManager.CreateModal<RioConfigurationModal>();
            })
            .StepIntoLabel(l => l.SetText("Done"));
        CancelButton.AddOnClickedEvent(b => { DynamicUIManager.CreateModal<RioConfigurationModal>(); });

        _nameInput = MainContent.CreateInputField();
        _nameInput.SetTopStretch<InputField>().StepIntoLabel(l => l.SetText("Name"));
        _nameInput.StepIntoHint(h => h.SetText("..."));
        _nameInput.AddOnValueChangedEvent((i, v) => UpdateAcceptButton());

        var subContent = MainContent.CreateSubContent(new Vector2(900, 400));
        subContent.SetStretch<Content>(topPadding: 60f);
        (Content left, Content right) = subContent.SplitLeftRight(445, 10);

        // Left Side
        left.SetLeftStretch<Content>();
        var leftLabel = left.CreateLabel(height: 30f);
        leftLabel.SetText("Ports").SetTopStretch<Label>().SetFontSize(20f);

        _portSelection = left.CreateScrollView();
        _portSelection.SetHeight<ScrollView>(340);
        _portSelection.SetStretch<Content>(topPadding: 30f);
        _portToggles = new Dictionary<string, Toggle>();
        for (int i = 0; i < 10; i++) {
            var container = _portSelection.Content.CreateSubContent(new Vector2(600, 40));
            container.SetTopStretch<Content>(
                leftPadding: 10, rightPadding: 10, anchoredY: -_portSelection.Content.RectOfChildren(container).yMin);

            var toggle = container.CreateToggle(false, $"{i}");
            toggle.SetStretch<Toggle>()
                .SetEnabledColor(ColorManager.SynthesisColor.InteractiveElement)
                .SetDisabledColor(ColorManager.SynthesisColor.Background);

            _portToggles.Add($"{i}", toggle);
        }
        _portSelection.Content.SetTopStretch<Content>().SetHeight<Content>(
            -_portSelection.Content.RectOfChildren().yMin);

        // Right Side
        right.SetRightStretch<Content>();
        var rightLabel = right.CreateLabel(height: 30f);
        rightLabel.SetText("Signals").SetTopStretch<Label>().SetFontSize(20f);

        // Signal Selection
        _signalSelection = right.CreateScrollView();
        _signalSelection.SetHeight<ScrollView>(340);
        _signalSelection.SetStretch<Content>(topPadding: 30f);
        _signalToggles = new Dictionary<string, Toggle>();
        RobotSimObject.GetCurrentlyPossessedRobot()
            .MiraLive.MiraAssembly.Data.Joints.JointInstances.Values
            .Where(x => !x.Info.Name.Equals("grounded") &&
                        RobotSimObject.GetCurrentlyPossessedRobot()
                                .MiraLive.MiraAssembly.Data.Joints.JointDefinitions[x.JointReference]
                                .JointMotionType == Mirabuf.Joint.JointMotion.Revolute)
            .ForEach(j => {
                var container = _signalSelection.Content.CreateSubContent(new Vector2(600, 40));
                container.SetTopStretch<Content>(leftPadding: 10, rightPadding: 10,
                    anchoredY: -_signalSelection.Content.RectOfChildren(container).yMin);

                var toggle = container.CreateToggle(false, $"{j.Info.Name} ({j.SignalReference})");
                toggle.SetStretch<Toggle>()
                    .SetEnabledColor(ColorManager.SynthesisColor.InteractiveElement)
                    .SetDisabledColor(ColorManager.SynthesisColor.Background);

                toggle.AddOnStateChangedEvent((t, s) => UpdateAcceptButton());

                _signalToggles.Add(j.SignalReference, toggle);
            });
        _signalSelection.Content.SetTopStretch<Content>().SetHeight<Content>(
            -_signalSelection.Content.RectOfChildren().yMin);

        if (_entry != null) {
            _nameInput.SetValue(_entry.Name);

            _entry.Ports.ForEach(x => _portToggles[x].SetState(true));
            _entry.Signals.ForEach(x => _signalToggles[x].SetState(true));
        }

        UpdateAcceptButton();
    }

    public void UpdateAcceptButton() {
        if (_nameInput.Value.Length != 0 && _signalToggles.Where(x => x.Value.State).Count() != 0 &&
            _portToggles.Where(x => x.Value.State).Count() != 0) {
            AcceptButton.RootGameObject.SetActive(true);
        } else {
            AcceptButton.RootGameObject.SetActive(false);
        }
    }

    public override void Delete() {}

    public override void Update() {}
}

public class RCConfigEncoderModal : ModalDynamic {
    private const int MODAL_WIDTH = 400, MODAL_HEIGHT = 400;

    public RCConfigEncoderModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

    public RCConfigEncoderModal(EncoderEntry entry) : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {
        _entry = entry;
    }

    private EncoderEntry? _entry;

    private InputField _nameInput;
    private LabeledDropdown _signalDropdown;
    private LabeledDropdown _channelADropdown;
    private LabeledDropdown _channelBDropdown;
    private InputField _modInput;

    private List<(string name, string guid)> _signals;
    private string[] _channelPorts = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset);
        return u;
    };

    public override void Create() {
        Title.SetText("Create Device");
        Title.SetWidth<Label>(400);
        Description.SetText("Create a Motor Controller, Encoder, Etc.");
        
        ModalImage.SetSprite(SynthesisAssetCollection.GetSpriteByName("wrench-icon"));
        ModalImage.SetColor(ColorManager.SynthesisColor.MainText);

        AcceptButton
            .AddOnClickedEvent(b => {
                var entry = new EncoderEntry(_nameInput.Value, _signals[_signalDropdown.Dropdown.Value].guid,
                    _channelADropdown.Dropdown.SelectedOption.text, _channelBDropdown.Dropdown.SelectedOption.text,
                    float.Parse(_modInput.Value));
                if (_entry != null) {
                    int n = RioConfigurationModal.Entries.RemoveAll(e => e.Equals(_entry));
                }
                RioConfigurationModal.Entries.Add(entry);

                DynamicUIManager.CreateModal<RioConfigurationModal>();
            })
            .StepIntoLabel(l => l.SetText("Done"));
        CancelButton.AddOnClickedEvent(b => { DynamicUIManager.CreateModal<RioConfigurationModal>(); });

        _nameInput = MainContent.CreateInputField();
        _nameInput.SetTopStretch<InputField>().StepIntoLabel(l => l.SetText("Name"));
        _nameInput.StepIntoHint(h => h.SetText("..."));
        _nameInput.AddOnValueChangedEvent((i, v) => UpdateAcceptButton());

        _signalDropdown = MainContent.CreateLabeledDropdown();
        _signalDropdown.ApplyTemplate(VerticalLayout);
        _signalDropdown.StepIntoLabel(l => l.SetText("Signal"));
        _signals = new List<(string name, string guid)>();
        _signals.AddRange(
            RobotSimObject.GetCurrentlyPossessedRobot()
                .MiraLive.MiraAssembly.Data.Joints.JointInstances.Values
                .Select<Mirabuf.Joint.JointInstance, (string name, string guid)>(x => (x.Info.Name, x.SignalReference))
                .Where(x => RobotSimObject.GetCurrentlyPossessedRobot().State.CurrentSignals.ContainsKey(
                           $"{x.guid}_encoder"))
                .Select(x => ($"{x.name} ({x.guid})", x.guid)));
        _signalDropdown.StepIntoDropdown(d => d.SetOptions(_signals.Select(x => x.name).ToArray()));
        _signalDropdown.Dropdown.AddOnValueChangedEvent((a, b, c) => UpdateAcceptButton());

        _channelADropdown = MainContent.CreateLabeledDropdown();
        _channelADropdown.ApplyTemplate(VerticalLayout);
        _channelADropdown.StepIntoLabel(l => l.SetText("Channel A"));
        _channelADropdown.StepIntoDropdown(d => d.SetOptions(_channelPorts));
        _channelADropdown.Dropdown.AddOnValueChangedEvent((a, b, c) => UpdateAcceptButton());

        _channelBDropdown = MainContent.CreateLabeledDropdown();
        _channelBDropdown.ApplyTemplate(VerticalLayout);
        _channelBDropdown.StepIntoLabel(l => l.SetText("Channel B"));
        _channelBDropdown.StepIntoDropdown(d => d.SetOptions(_channelPorts));
        _channelBDropdown.Dropdown.AddOnValueChangedEvent((a, b, c) => UpdateAcceptButton());

        _modInput = MainContent.CreateInputField();
        _modInput.ApplyTemplate(VerticalLayout);
        _modInput.StepIntoLabel(l => l.SetText("Conversion Factor"));
        _modInput.SetContentType(TMPro.TMP_InputField.ContentType.DecimalNumber);
        _modInput.SetValue("1");
        _modInput.AddOnValueChangedEvent((a, b) => UpdateAcceptButton());

        if (_entry != null) {
            _nameInput.SetValue(_entry.Name);

            int signalIndex = _signals.FindIndex(0, _signals.Count, x => x.guid == _entry.Signal);
            _signalDropdown.Dropdown.SetValue(signalIndex, true);

            int channelAIndex = -1;
            int.TryParse(_entry.ChannelA, out channelAIndex);
            _channelADropdown.Dropdown.SetValue(channelAIndex);

            int channelBIndex = -1;
            int.TryParse(_entry.ChannelB, out channelBIndex);
            _channelBDropdown.Dropdown.SetValue(channelBIndex);

            _modInput.SetValue(_entry.Mod.ToString());
        }

        UpdateAcceptButton();
    }

    public void UpdateAcceptButton() {
        if (_nameInput.Value.Length != 0 && _channelADropdown.Dropdown.Value != _channelBDropdown.Dropdown.Value &&
            Mathf.Abs(int.Parse(_modInput.Value)) > 0.001) {
            AcceptButton.RootGameObject.SetActive(true);
        } else {
            AcceptButton.RootGameObject.SetActive(false);
        }
    }

    public override void Delete() {}

    public override void Update() {}
}
