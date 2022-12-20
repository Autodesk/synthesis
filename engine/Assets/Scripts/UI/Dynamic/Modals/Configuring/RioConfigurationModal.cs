using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Synthesis.Gizmo;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;
using SynthesisAPI.Utilities;

using Logger = SynthesisAPI.Utilities.Logger;
using Synthesis.WS.Translation;

#nullable enable

public class RioConfigurationModal : ModalDynamic {

    private const float MODAL_WIDTH = 1000;
    private const float MODAL_HEIGHT = 600;

    public RioConfigurationModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) { }
    public RioConfigurationModal(bool reload = false) : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {
        if (reload && RobotSimObject.CurrentlyPossessedRobot != string.Empty) {

            Entries.Clear();

            var trans = RobotSimObject.GetCurrentlyPossessedRobot().SimulationTranslationLayer;

            trans.MotorGroups.ForEach(x => {
                if (x.GetType() == typeof(RioTranslationLayer.PWMGroup)) {
                    Entries.Add((PWMGroupEntry)x!);
                }
            });
        }
    }

    public const string PWM = "PWM";
    public const string Analog = "Analog";
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

        AcceptButton.AddOnClickedEvent(b => {
            Apply();
            DynamicUIManager.CloseActiveModal();
        }).StepIntoLabel(l => l.SetText("Save"));

        Entries.ForEach(e => {
            CreateItem($"{e.Name}", "Config", () => {
                if (e.GetType().Name.Equals(typeof(PWMGroupEntry).Name)) {
                    DynamicUIManager.CreateModal<RCConfigPwmGroupModal>(e);
                } else {
                    Debug.Log($"{e.GetType().Name}");
                }
            });
        });

        CreateAddButtons();

        _scrollView.Content.SetTopStretch<Content>().SetHeight<Content>(-_scrollView.Content.RectOfChildren().yMin);
    }

    public void CreateItem(string text, string buttonText, Action onButton) {
        var content = _scrollView.Content.CreateSubContent(new Vector2(_scrollView.Content.Size.x, 80));
        content.EnsureImage().StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE));
        content.SetTopStretch<Content>(anchoredY: -_scrollView.Content.RectOfChildren(content).yMin);
        content.CreateLabel().SetStretch<Label>(leftPadding: 20, topPadding: 20, bottomPadding:20)
            .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle).SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left)
            .SetText(text).SetColor(ColorManager.SYNTHESIS_BLACK);
        var button = content.CreateButton(buttonText);
        button.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK));
        button.SetPivot<Button>(new Vector2(1, 0.5f)).SetRightStretch<Button>(20, 20, 15).SetWidth<Button>(150).SetHeight<Button>(-30);
        button.StepIntoLabel(l => l.SetColor(ColorManager.SYNTHESIS_WHITE));
        button.AddOnClickedEvent(b => onButton());
    }

    public void CreateAddButtons() {
        var content = _scrollView.Content.CreateSubContent(new Vector2(_scrollView.Content.Size.x, 80));
        content.Image?.SetColor(new Color(0, 0, 0, 0));
        content.SetTopStretch<Content>(anchoredY: -_scrollView.Content.RectOfChildren(content).yMin).SetAnchorTop<Content>();
        content.SetWidth<Content>(1000);
        // (Content left, Content right) = content.SplitLeftRight((1000f / 2f) - (20f / 2f), 20f);
        var deviceButton = content.CreateButton("Create Device");
        deviceButton.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE));
        deviceButton.SetStretch<Button>(leftPadding: 300, rightPadding: 300, topPadding: 20, bottomPadding: 20);
        deviceButton.AddOnClickedEvent(b => {
            DynamicUIManager.CreateModal<RCCreateDeviceModal>();
        });
    }

    public void Apply() {
        RioTranslationLayer trans = new RioTranslationLayer();

        trans.MotorGroups = new List<RioTranslationLayer.MotorGroup>();

        Entries.ForEach(e => {
            if (e.GetType() == typeof(PWMGroupEntry)) {
                var pwm = (e as PWMGroupEntry)!;
                trans.MotorGroups.Add(new RioTranslationLayer.PWMGroup(pwm.Name, pwm.Ports, pwm.Signals));
            }
        });

        RobotSimObject.GetCurrentlyPossessedRobot().SimulationTranslationLayer = trans;
    }

    public override void Delete() { }

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

    public virtual string GetDisplayName()
        => Name;

    public override int GetHashCode()
        => Name.GetHashCode() * 849583721;
    
    public override sealed bool Equals(object obj) {
        if (ReferenceEquals(obj, null))
            return false;

        if (obj.GetType() != this.GetType())
            return false;
        return obj.GetHashCode() == GetHashCode();
    }
}

public class PWMGroupEntry : RioEntry {
    public string[] Ports;
    public string[] Signals;

    public PWMGroupEntry(string name, string[] ports, string[] signals) : base(name) {
        Ports = ports;
        Signals = signals;
    }

    public override string GetDisplayName()
        => $"{Name} (PWM)";

    public override int GetHashCode()
        => Ports.GetHashCode() * 342564752
        + Signals.GetHashCode() * 980451232
        + base.GetHashCode();

    public static explicit operator PWMGroupEntry(RioTranslationLayer.PWMGroup group)
        => new PWMGroupEntry(group.GUID, group.Ports.ToArray(), group.Signals.ToArray());
    public static explicit operator RioTranslationLayer.PWMGroup(PWMGroupEntry group)
        => new RioTranslationLayer.PWMGroup(group.Name, group.Ports, group.Signals);
}
