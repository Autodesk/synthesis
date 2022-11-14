using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Synthesis.Gizmo;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class RioConfigurationModal : ModalDynamic {
    public RioConfigurationModal() : base(new Vector2(1000, 600)) { }

    private const string PWM = "PWM";
    private const string Analog = "Analog";
    private const string Digital = "Digital";

    public static Dictionary<string, string[]> DeviceIDs = new Dictionary<string, string[]>();

    public static List<DeviceEntry> Devices = new List<DeviceEntry>();
    public static List<GroupEntry> Groupings = new List<GroupEntry>();

    private ScrollView _scrollView;

    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset);
        return u;
    };

    static RioConfigurationModal() {
        string[] basicIds = new string[] {
            "0", "1",  "2",  "3",  "4",  "5",  "6",  "7",
            "8", "9", "10", "11", "12", "13", "14", "15" };
        DeviceIDs.Add(PWM, basicIds);
        DeviceIDs.Add(Analog, basicIds);
        DeviceIDs.Add(Digital, basicIds);
    }

    public override void Create() {
        Title.SetText("RoboRIO Configuration");
        Title.SetWidth<Label>(300);
        Description.SetText("Configuring RoboRIO for Synthesis simulation");
        ModalImage.SetSprite(SynthesisAssetCollection.GetSpriteByName("wrench-icon"));
        ModalImage.SetColor(ColorManager.SYNTHESIS_WHITE);

        _scrollView = MainContent.CreateScrollView();
        _scrollView.SetStretch<ScrollView>();

        // for (int i = 0; i < 10; i++) {
        //     CreateItem("PWM " + i);
        // }

        Devices.ForEach(e => {
            CreateItem($"{e.Type} {e.ID}", "Config", () => {
                switch (e.Type) {
                    case PWM:
                        DynamicUIManager.CreateModal<RCConfigurePwmModal>(e);
                        break;
                }
            });
        });
        Groupings.ForEach(g => {
            CreateItem($"[G] {g.Name}", "Edit", () => {
                DynamicUIManager.CreateModal<RCCreateGroupingModal>(g);
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
        (Content left, Content right) = content.SplitLeftRight((1000f / 2f) - (20f / 2f), 20f);
        var deviceButton = left.CreateButton("Create Device");
        deviceButton.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE));
        deviceButton.SetStretch<Button>(leftPadding: 300, topPadding: 20, bottomPadding: 20);
        deviceButton.AddOnClickedEvent(b => {
            DynamicUIManager.CreateModal<RCCreateDeviceModal>();
        });

        var groupingButton = right.CreateButton("Create Grouping");
        groupingButton.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE));
        groupingButton.SetStretch<Button>(rightPadding: 300, topPadding: 20, bottomPadding: 20);
        groupingButton.AddOnClickedEvent(b => {
            DynamicUIManager.CreateModal<RCCreateGroupingModal>();
        });
    }

    public override void Delete() { }

    public override void Update() { }
}

public struct DeviceEntry {
    public string Type;
    public string ID;
}

public struct GroupEntry {
    public string Name;
    public List<string> Signals;
}

public class RCConfigurePwmModal : ModalDynamic {
    public RCConfigurePwmModal(DeviceEntry entry) : base(new Vector2(500, 300)) {
        _entry = entry;
    }

    private DeviceEntry _entry;

    private LabeledDropdown _signalDropdown;

    public override void Create() {
        Title.SetText("Configure PWM");
        Title.SetWidth<Label>(400);
        Description.SetText("Configure PWM Device");
        ModalImage.SetSprite(SynthesisAssetCollection.GetSpriteByName("wrench-icon"));
        ModalImage.SetColor(ColorManager.SYNTHESIS_WHITE);

        AcceptButton.AddOnClickedEvent(b => {
            DynamicUIManager.CreateModal<RioConfigurationModal>();
        }).StepIntoLabel(l => l.SetText("Done"));

        CancelButton.AddOnClickedEvent(b => {
            DynamicUIManager.CreateModal<RioConfigurationModal>();
        });

        var options = new List<string>();

        RioConfigurationModal.Groupings.ForEach(g => options.Add($"[G] {g.Name}"));

        // I apologize in advance
        RobotSimObject.GetCurrentlyPossessedRobot().MiraLive.MiraAssembly.Data.Joints.JointInstances.Values.Where(
            x => !x.Info.Name.Equals("grounded") && RobotSimObject.GetCurrentlyPossessedRobot().MiraLive.MiraAssembly.Data.Joints.JointDefinitions[x.JointReference].JointMotionType
                == Mirabuf.Joint.JointMotion.Revolute
        ).Where(x => !RioConfigurationModal.Groupings.Exists(y => y.Signals.Contains(x.SignalReference)))
            .ForEach(x => options.Add($"{x.Info.Name} ({x.SignalReference})"));

        _signalDropdown = MainContent.CreateLabeledDropdown();
        _signalDropdown.SetTopStretch<LabeledDropdown>().StepIntoLabel(
            l => l.SetText("Signal")
        ).StepIntoDropdown(
            d => d.SetOptions(options.ToArray())
        );
    }

    public override void Delete() { }

    public override void Update() { }
}

public class RCCreateDeviceModal : ModalDynamic {
    public RCCreateDeviceModal() : base(new Vector2(400, 120)) { }

    private LabeledDropdown _typeDropdown;
    private LabeledDropdown _idDropdown;

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
        ModalImage.SetColor(ColorManager.SYNTHESIS_WHITE);

        AcceptButton.AddOnClickedEvent(b => {
            RioConfigurationModal.Devices.Add(new DeviceEntry { Type = _typeDropdown.Dropdown.SelectedOption.text, ID = _idDropdown.Dropdown.SelectedOption.text });
            DynamicUIManager.CreateModal<RioConfigurationModal>();
        }).StepIntoLabel(l => l.SetText("Create"));
        CancelButton.AddOnClickedEvent(b => {
            DynamicUIManager.CreateModal<RioConfigurationModal>();
        });

        _typeDropdown = MainContent.CreateLabeledDropdown().StepIntoLabel(l => l.SetText("Type"));
        _typeDropdown.StepIntoDropdown(d => d.SetOptions(RioConfigurationModal.DeviceIDs.Keys.ToArray()));
        _typeDropdown.SetTopStretch<LabeledDropdown>();
        _typeDropdown.StepIntoDropdown(d => d.AddOnValueChangedEvent(UpdateIDSelection));

        _idDropdown = MainContent.CreateLabeledDropdown().StepIntoLabel(l => l.SetText("ID"));
        _idDropdown.SetTopStretch<LabeledDropdown>(anchoredY: MainContent.RectOfChildren(_idDropdown).height);
        _idDropdown.StepIntoDropdown(d => d.SetOptions(RioConfigurationModal.DeviceIDs.Values.ElementAt(0)));
        // _idDropdown.StepIntoDropdown(d => d.AddOnValueChangedEvent((d, i, data) => {
        //     _selectedId = data.text;
        // }));
    }

    private void UpdateIDSelection(Dropdown dropdown, int index, TMPro.TMP_Dropdown.OptionData data) {
        _idDropdown.StepIntoDropdown(d => d.SetOptions(RioConfigurationModal.DeviceIDs[data.text]).SetValue(0));
    }

    public override void Delete() { }

    public override void Update() { }
}

public class RCCreateGroupingModal : ModalDynamic {

    private const int WIDTH = 600;
    private const int HEIGHT = 400;

    public RCCreateGroupingModal() : base(new Vector2(WIDTH, HEIGHT)) { }

    public RCCreateGroupingModal(GroupEntry existingEntry) : base(new Vector2(WIDTH, HEIGHT)) {
        _entry = existingEntry;
    }

    private GroupEntry? _entry;

    private InputField _nameInput;
    private ScrollView _signalsSelection;
    private Dictionary<string, Toggle> _signalToggles;

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
        ModalImage.SetColor(ColorManager.SYNTHESIS_WHITE);

        AcceptButton.AddOnClickedEvent(b => {
            var entry = new GroupEntry { Name = _nameInput.Value, Signals = new List<string>() };
            _signalToggles.Where(x => x.Value.State).ForEach(x => entry.Signals.Add(x.Key));

            RioConfigurationModal.Groupings.RemoveAll(x => x.Name.Equals(entry.Name));

            RioConfigurationModal.Groupings.Add(entry);
            DynamicUIManager.CreateModal<RioConfigurationModal>();
        }).StepIntoLabel(l => l.SetText("Done"));

        CancelButton.AddOnClickedEvent(b => {
            DynamicUIManager.CreateModal<RioConfigurationModal>();
        });

        _nameInput = MainContent.CreateInputField().StepIntoLabel(l => l.SetText("Name")).StepIntoHint(h => h.SetText("Left Drivetrain..."));
        _nameInput.SetTopStretch<InputField>();
        if (_entry.HasValue)
            _nameInput.SetValue(_entry.Value.Name);

        _signalsSelection = MainContent.CreateScrollView();
        _signalsSelection.SetHeight<ScrollView>(340);
        _signalsSelection.SetTopStretch<ScrollView>(anchoredY: MainContent.RectOfChildren(_signalsSelection).height + 10);

        _signalToggles = new Dictionary<string, Toggle>();

        RobotSimObject.GetCurrentlyPossessedRobot().MiraLive.MiraAssembly.Data.Joints.JointInstances.Values.Where(
            x => !x.Info.Name.Equals("grounded") && RobotSimObject.GetCurrentlyPossessedRobot().MiraLive.MiraAssembly.Data.Joints.JointDefinitions[x.JointReference].JointMotionType
                == Mirabuf.Joint.JointMotion.Revolute
        ).ForEach(j => {
            var container = _signalsSelection.Content.CreateSubContent(new Vector2(600, 40));
            container.SetTopStretch<Content>(leftPadding: 10, rightPadding: 10, anchoredY: -_signalsSelection.Content.RectOfChildren(container).yMin);
            // container.EnsureImage().StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE));

            var toggle = container.CreateToggle(false, $"{j.Info.Name} ({j.SignalReference})");
            toggle.SetStretch<Toggle>().SetEnabledColor(ColorManager.SYNTHESIS_ORANGE).SetDisabledColor(ColorManager.SYNTHESIS_BLACK);
            if (_entry.HasValue && _entry.Value.Signals.Contains(j.SignalReference))
                toggle.SetState(true);

            _signalToggles.Add(j.SignalReference, toggle);
        });
        
        _signalsSelection.Content.SetTopStretch<Content>().SetHeight<Content>(-_signalsSelection.Content.RectOfChildren().yMin);
    }

    public override void Delete() { }

    public override void Update() { }
}
