using Newtonsoft.Json;
using Synthesis.Gizmo;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using Synthesis.WS;
using Synthesis.WS.Translation;
using SynthesisAPI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using Logger = SynthesisAPI.Utilities.Logger;

#nullable enable

public class RioConfigurationModal : ModalDynamic {
    private const float MODAL_WIDTH  = 1000;
    private const float MODAL_HEIGHT = 600;

    public RioConfigurationModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {
    }

    public RioConfigurationModal(bool reload = false) : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {
        if (reload && RobotSimObject.CurrentlyPossessedRobot != string.Empty) {

            Entries.Clear();

            var trans = RobotSimObject.GetCurrentlyPossessedRobot().SimulationTranslationLayer;

            trans.PWMGroups.ForEach(x => Entries.Add((PWMGroupEntry)x!));

            Entries.AddRange(trans.Encoders.Select<RioTranslationLayer.Encoder, EncoderEntry>(x => (EncoderEntry)x));
        }
    }

    public const string PWM     = "PWM";
    public const string ENCODER = "Encoder";
    public const string Analog  = "Analog";
    public const string Digital = "Digital";

    public static List<RioEntry> Entries = new List<RioEntry>();

    private ScrollView _scrollView;

    public static Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset);
        return u;
    };

    public override void Create() {
        Title.SetText("RoboRIO Configuration");
        Title.SetWidth<Label>(300);
        Description.SetText("Configuring RoboRIO for Synthesis simulation");
        ModalImage.SetSprite(SynthesisAssetCollection.GetSpriteByName("wrench-icon"));
        ModalImage.SetColor(ColorManager.SYNTHESIS_WHITE);

        _scrollView = MainContent.CreateScrollView();
        _scrollView.SetStretch<ScrollView>();

        AcceptButton
            .AddOnClickedEvent(b => {
                Apply();
                DynamicUIManager.CloseActiveModal();
            })
            .StepIntoLabel(l => l.SetText("Save"));

        Entries.ForEach(e => {
            CreateItem($"{e.GetDisplayName()}", "Config",
                () => {
                    if (e.GetType().Name.Equals(typeof(PWMGroupEntry).Name)) {
                        DynamicUIManager.CreateModal<RCConfigPwmGroupModal>(e);
                    }

                    if (e.GetType().Name.Equals(typeof(EncoderEntry).Name)) {
                        DynamicUIManager.CreateModal<RCConfigEncoderModal>(e);
                    } else {
                        Debug.Log($"{e.GetType().Name}");
                    }
                },
                () => {
                    Entries.RemoveAll(x => x.Equals(e));
                    DynamicUIManager.CreateModal<RioConfigurationModal>();
                });
        });

        CreateAddButtons();

        _scrollView.Content.SetTopStretch<Content>().SetHeight<Content>(-_scrollView.Content.RectOfChildren().yMin);
    }

    public void CreateItem(string text, string buttonText, Action onButton, Action onDelete) {
        var content = _scrollView.Content.CreateSubContent(new Vector2(_scrollView.Content.Size.x, 80));
        content.EnsureImage().StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE));
        content.SetTopStretch<Content>(anchoredY: -_scrollView.Content.RectOfChildren(content).yMin);
        content.CreateLabel()
            .SetStretch<Label>(leftPadding: 20, topPadding: 20, bottomPadding: 20)
            .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle)
            .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left)
            .SetText(text)
            .SetColor(ColorManager.SYNTHESIS_BLACK);
        var confButton = content.CreateButton(buttonText);
        confButton.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK));
        confButton.SetPivot<Button>(new Vector2(1, 0.5f))
            .SetRightStretch<Button>(20, 20, 15)
            .SetWidth<Button>(110)
            .SetHeight<Button>(-30);
        confButton.StepIntoLabel(l => l.SetColor(ColorManager.SYNTHESIS_WHITE));
        confButton.AddOnClickedEvent(b => onButton());
        var deleteButton = content.CreateButton("Remove");
        deleteButton.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK));
        deleteButton.SetPivot<Button>(new Vector2(1, 0.5f))
            .SetRightStretch<Button>(20, 20, 140)
            .SetWidth<Button>(110)
            .SetHeight<Button>(-30);
        deleteButton.StepIntoLabel(l => l.SetColor(ColorManager.SYNTHESIS_WHITE));
        deleteButton.AddOnClickedEvent(b => onDelete());
    }

    public void CreateAddButtons() {
        var content = _scrollView.Content.CreateSubContent(new Vector2(_scrollView.Content.Size.x, 80));
        content.Image?.SetColor(new Color(0, 0, 0, 0));
        content.SetTopStretch<Content>(anchoredY: -_scrollView.Content.RectOfChildren(content).yMin)
            .SetAnchorTop<Content>();
        content.SetWidth<Content>(1000);
        // (Content left, Content right) = content.SplitLeftRight((1000f / 2f) - (20f / 2f), 20f);
        var deviceButton = content.CreateButton("Create Device");
        deviceButton.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE));
        deviceButton.SetStretch<Button>(leftPadding: 300, rightPadding: 300, topPadding: 20, bottomPadding: 20);
        deviceButton.AddOnClickedEvent(b => { DynamicUIManager.CreateModal<RCCreateDeviceModal>(); });
    }

    public void Apply() {
        RioTranslationLayer trans = new RioTranslationLayer();

        trans.PWMGroups = new List<RioTranslationLayer.PWMGroup>();

        Entries.ForEach(e => {
            if (e.GetType() == typeof(PWMGroupEntry)) {
                var pwm = (e as PWMGroupEntry)!;
                trans.PWMGroups.Add((RioTranslationLayer.PWMGroup)pwm);
            } else if (e.GetType() == typeof(EncoderEntry)) {
                var encoder = (e as EncoderEntry)!;
                trans.Encoders.Add((RioTranslationLayer.Encoder)encoder);
            }
        });

        RobotSimObject.GetCurrentlyPossessedRobot().SimulationTranslationLayer = trans;
    }

    public override void Delete() {
    }

    public override void Update() {
        if (RobotSimObject.CurrentlyPossessedRobot == string.Empty) {
            Logger.Log("Spawn a robot first", LogLevel.Info);
            DynamicUIManager.CloseActiveModal();
        }
    }
}

public abstract class RioEntry {
    public string Name;

    protected RioEntry(string name) {
        Name = name;
    }

    public virtual string GetDisplayName() => Name;

    public override int GetHashCode() => Name.GetHashCode() * 849583721;

    public override sealed bool Equals(object obj) {
        if (ReferenceEquals(obj, null)) {
            return false;
        }

        if (obj.GetType() != this.GetType()) {
            return false;
        }

        return obj.GetHashCode() == GetHashCode();
    }
}

public class PWMGroupEntry : RioEntry {
    public string[] Ports;
    public string[] Signals;

    public PWMGroupEntry(string name, string[] ports, string[] signals) : base(name) {
        Ports   = ports;
        Signals = signals;
    }

    public override string GetDisplayName() => $"{Name} (PWM)";

    public override int
    GetHashCode() => Ports.GetHashCode() * 342564752 + Signals.GetHashCode() * 980451232 + base.GetHashCode();

    public static explicit operator PWMGroupEntry(RioTranslationLayer.PWMGroup group) => new PWMGroupEntry(
        group.Name, group.Ports.ToArray(), group.Signals.ToArray());
    public static explicit operator RioTranslationLayer.PWMGroup(
        PWMGroupEntry group) => new RioTranslationLayer.PWMGroup(group.Name, group.Ports, group.Signals);
}

public class EncoderEntry : RioEntry {
    public string Signal;
    public string ChannelA;
    public string ChannelB;
    public float Mod;

    public EncoderEntry(string name, string signal, string channelA, string channelB, float mod) : base(name) {
        Signal   = signal;
        ChannelA = channelA;
        ChannelB = channelB;
        Mod      = mod;
    }

    public override string GetDisplayName() => $"{Name} (Encoder)";

    public override int
    GetHashCode() => Signal.GetHashCode() * 342564752 + ChannelA.GetHashCode() * 980451232 +
                     ChannelB.GetHashCode() * 453678690 + Mod.GetHashCode() * 213434321 + base.GetHashCode();

    public static explicit operator EncoderEntry(RioTranslationLayer.Encoder enc) => new EncoderEntry(
        enc.GUID, enc.Signal, enc.ChannelA, enc.ChannelB, enc.Mod);
    public static explicit operator RioTranslationLayer.Encoder(EncoderEntry enc) => new RioTranslationLayer.Encoder(
        enc.Name, enc.ChannelA, enc.ChannelB, enc.Signal, enc.Mod);
}
