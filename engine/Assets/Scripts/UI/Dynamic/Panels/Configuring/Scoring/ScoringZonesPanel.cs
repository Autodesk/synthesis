using System;
using System.Collections.Generic;
using System.Linq;
using Synthesis.Gizmo;
using Synthesis.UI.Dynamic;
using UnityEngine;

public record ScoringZoneData()
{
    public string Name { get; set; } = "";
    public Alliance Alliance { get; set; } = Alliance.Blue;
    public int Points { get; set; } = 0;
    public bool DestroyGamepiece { get; set; } = false;
    public float XScale { get; set; } = 1.0f;
    public float YScale { get; set; } = 1.0f;
    public float ZScale { get; set; } = 1.0f;
}

public class ScoringZonesPanel : PanelDynamic
{
    private const float MODAL_WIDTH = 400f;
    private const float MODAL_HEIGHT = 400f;
    
    private const float VERTICAL_PADDING = 16f;

    private float _scrollViewWidth;

    private ScrollView _zonesScrollView;
    private Button _addZoneButton;
    
    private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0, rightPadding: 0);
        return u;
    };
    
    private readonly Func<UIComponent, UIComponent> ListVerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 15f, rightPadding: 15f);
        return u;
    };

    public ScoringZonesPanel(): base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) { }
    public override bool Create() {
        Title.SetText("Scoring Zones");
        
        AcceptButton
            .StepIntoLabel(l => l.SetText("Close"))
            .AddOnClickedEvent(b => DynamicUIManager.ClosePanel<ScoringZonesPanel>());
        CancelButton.RootGameObject.SetActive(false);

        _zonesScrollView = MainContent.CreateScrollView().SetRightStretch<ScrollView>().ApplyTemplate(VerticalLayout);
        _scrollViewWidth = _zonesScrollView.Parent!.RectOfChildren().width;


        _addZoneButton = MainContent.CreateButton()
            .SetTopStretch<Button>()
            .StepIntoLabel(l => l.SetText("Add Zone"))
            .AddOnClickedEvent(button =>
            {
                OpenScoringZoneGizmo();
            })
            .ApplyTemplate(VerticalLayout);
        
        AddZoneEntries();

        return true;
    }

    private void AddZoneEntries()
    {
        foreach (ScoringZone zone in FieldSimObject.CurrentField.ScoringZones)
        {
            AddZoneEntry(zone);
        }
    }

    private void AddZoneEntry(ScoringZone zone)
    {
        (Content leftContent, Content rightContent) = _zonesScrollView.Content.CreateSubContent(new Vector2(_scrollViewWidth, 64f)).ApplyTemplate(VerticalLayout).SplitLeftRight(_scrollViewWidth / 2, 0);
        leftContent.CreateLabel().SetText(zone.Name).ApplyTemplate(ListVerticalLayout);
        rightContent.CreateLabel().SetText(zone.Alliance.ToString()).ApplyTemplate(ListVerticalLayout);
    }

    private void OpenScoringZoneGizmo()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ScoringZone zone = new ScoringZone(obj, "temp scoring zone", Alliance.Blue, 0, false);
        DynamicUIManager.CreatePanel<ZoneConfigPanel>();
        ZoneConfigPanel panel = DynamicUIManager.GetPanel<ZoneConfigPanel>();
        panel.SetCallback(data => OnConfigModalUpdate(zone, data));
        GizmoManager.SpawnGizmo(obj.transform,
            t =>
            {
                obj.transform.position = t.Position;
                obj.transform.rotation = t.Rotation;
            },
            t => {
                FieldSimObject.CurrentField.ScoringZones.Add(zone);
                AddZoneEntry(zone);
            });
    }

    private bool OnConfigModalUpdate(ScoringZone zone, ScoringZoneData data)
    {
        zone.Name = data.Name;
        zone.GameObject.name = data.Name;
        zone.GameObject.tag = data.Alliance == Alliance.Red ? "red zone" : "blue zone";
        zone.Alliance = data.Alliance;
        zone.Points = data.Points;
        zone.GameObject.transform.localScale = new Vector3(data.XScale, data.YScale, data.ZScale);
        zone.DestroyObject = data.DestroyGamepiece;
        return true;
    }
    
    public override void Update() { }

    public override void Delete()
    {
        // foreach (ScoringZone zone in _scoringZones)
        // {
            // zone.GameObject.GetComponent<Renderer>().enabled = false;
        // }
    }
}