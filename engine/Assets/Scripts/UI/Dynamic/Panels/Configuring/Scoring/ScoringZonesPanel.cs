using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Synthesis.Gizmo;
using Synthesis.UI.Dynamic;
using Synthesis.PreferenceManager;
using UnityEngine;

public record ScoringZoneData()
{
    public string Name { get; set; } = "";
    public Alliance Alliance { get; set; } = Alliance.Blue;
    public Transform Parent { get; set; } = null;
    public int Points { get; set; } = 0;
    public bool DestroyGamepiece { get; set; } = false;
    public float XScale { get; set; } = 1.0f;
    public float YScale { get; set; } = 1.0f;
    public float ZScale { get; set; } = 1.0f;
}

public class ScoringZonesPanel : PanelDynamic
{
    private const float MODAL_WIDTH = 500f;
    private const float MODAL_HEIGHT = 600f;
    
    private const float VERTICAL_PADDING = 16f;
    private const float HORIZONTAL_PADDING = 16f;
    private const float SCROLLBAR_WIDTH = 10f;
    private const float BUTTON_WIDTH = 64f;
    private const float ROW_HEIGHT = 64f;

    private float _scrollViewWidth;
    private float _entryWidth;

    private ScrollView _zonesScrollView;
    private Button _addZoneButton;

    private bool _initiallyVisible = true;
    
    private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0, rightPadding: 0);
        return u;
    };
    
    private readonly Func<UIComponent,  UIComponent> ListVerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: HORIZONTAL_PADDING, rightPadding: HORIZONTAL_PADDING);
        return u;
    };

    public ScoringZonesPanel(): base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) { }
    public override bool Create() {
        Title.SetText("Scoring Zones");
        
        AcceptButton
            .StepIntoLabel(l => l.SetText("Close"))
            .AddOnClickedEvent(b => DynamicUIManager.ClosePanel<ScoringZonesPanel>());
        CancelButton.RootGameObject.SetActive(false);

        _zonesScrollView = MainContent.CreateScrollView().SetRightStretch<ScrollView>().ApplyTemplate(VerticalLayout)
            .SetHeight<ScrollView>(MODAL_HEIGHT - VERTICAL_PADDING * 2 - 50);
        _scrollViewWidth = _zonesScrollView.Parent!.RectOfChildren().width - SCROLLBAR_WIDTH;
        _entryWidth = _scrollViewWidth - HORIZONTAL_PADDING * 2;


        _addZoneButton = MainContent.CreateButton()
            .SetTopStretch<Button>()
            .StepIntoLabel(l => l.SetText("Add Zone"))
            .AddOnClickedEvent(_ =>
            {
                OpenScoringZoneGizmo();
            })
            .ApplyTemplate(VerticalLayout);
        
        AddZoneEntries();

        _initiallyVisible = PreferenceManager.GetPreference<bool>(SettingsModal.RENDER_SCORE_ZONES);
        
        if (!_initiallyVisible)
            FieldSimObject.CurrentField.ScoringZones.ForEach(z => z.GameObject.GetComponent<MeshRenderer>().enabled = true);

        return true;
    }

    private void AddZoneEntries()
    {
        _zonesScrollView.Content.DeleteAllChildren();
        foreach (ScoringZone zone in FieldSimObject.CurrentField.ScoringZones)
        {
            AddZoneEntry(zone, true);
        }
    }

    private void AddZoneEntry(ScoringZone zone, bool isNew) {
        if (!isNew) {
            AddZoneEntries();
            return;
        }
        (Content leftContent, Content rightContent) = _zonesScrollView.Content.CreateSubContent(new Vector2(_entryWidth, ROW_HEIGHT))
            .ApplyTemplate(ListVerticalLayout)
            .SplitLeftRight(BUTTON_WIDTH, HORIZONTAL_PADDING);
        leftContent.SetBackgroundColor<Content>(zone.Alliance == Alliance.Red ? Color.red : Color.blue);

        (Content labelsContent, Content buttonsContent) = rightContent.SplitLeftRight(_entryWidth- (HORIZONTAL_PADDING + BUTTON_WIDTH) * 3, HORIZONTAL_PADDING);
        (Content topContent, Content bottomContent) = labelsContent.SplitTopBottom(ROW_HEIGHT / 2, 0);
        topContent.CreateLabel().SetText(zone.Name)
            .ApplyTemplate(VerticalLayout)
            .SetAnchorLeft<Label>()
            .SetAnchoredPosition<Label>(new Vector2(0, -ROW_HEIGHT / 8));
        bottomContent.CreateLabel().SetText($"{zone.Points} points")
            .ApplyTemplate(VerticalLayout)
            .SetAnchorLeft<Label>()
            .SetAnchoredPosition<Label>(new Vector2(0, -ROW_HEIGHT / 8));

        (Content editButtonContent, Content deleteButtonContent) = buttonsContent.SplitLeftRight(BUTTON_WIDTH, HORIZONTAL_PADDING);
        editButtonContent.CreateButton().StepIntoLabel(l => l.SetText("Edit"))
            .AddOnClickedEvent(b => OpenScoringZoneGizmo(zone))
            .ApplyTemplate(VerticalLayout).SetSize<Button>(new Vector2(BUTTON_WIDTH, ROW_HEIGHT)).SetStretch<Button>();
        deleteButtonContent.CreateButton().StepIntoLabel(l => l.SetText("Delete"))
            .AddOnClickedEvent(b => {
                FieldSimObject.CurrentField.ScoringZones.Remove(zone);
                GameObject.Destroy(zone.GameObject);
                AddZoneEntries();
            })
            .ApplyTemplate(VerticalLayout).SetSize<Button>(new Vector2(BUTTON_WIDTH, ROW_HEIGHT)).SetStretch<Button>();
    }

    private void OpenScoringZoneGizmo(ScoringZone zone = null)    {
        DynamicUIManager.CreatePanel<ZoneConfigPanel>(persistent: false, zone);
        ZoneConfigPanel panel = DynamicUIManager.GetPanel<ZoneConfigPanel>();
        panel.SetCallback(AddZoneEntry);
    }
    
    public override void Update() { }

    public override void Delete()
    {
        if (!_initiallyVisible)
            FieldSimObject.CurrentField.ScoringZones.ForEach(z => z.GameObject.GetComponent<MeshRenderer>().enabled = _initiallyVisible);
    }
}