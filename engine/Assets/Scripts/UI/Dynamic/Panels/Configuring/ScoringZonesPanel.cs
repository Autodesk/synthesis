using System;
using System.Collections.Generic;
using System.Linq;
using Synthesis.Gizmo;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class ScoringZonesPanel : PanelDynamic
{
    private const float MODAL_WIDTH = 400f;
    private const float MODAL_HEIGHT = 400f;
    
    private const float VERTICAL_PADDING = 16f;

    private float _scrollViewWidth;

    private ScrollView _zonesScrollView;
    private Button _addZoneButton;

    private List<GameObject> _scoringZones = new List<GameObject>();

    public enum Alliance
    {
        Blue,
        Red
    }
    
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
            .AddOnClickedEvent(b => DynamicUIManager.CloseAllPanels());
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
        foreach (GameObject gameObject in _scoringZones)
        {
            AddZoneEntry(gameObject);
        }
    }

    private void AddZoneEntry(GameObject gameObject)
    {
        (Content leftContent, Content rightContent) = _zonesScrollView.Content.CreateSubContent(new Vector2(_scrollViewWidth, 64f)).ApplyTemplate(VerticalLayout).SplitLeftRight(_scrollViewWidth / 2, 0);
        leftContent.CreateLabel().SetText(gameObject.name).ApplyTemplate(ListVerticalLayout);
        rightContent.CreateLabel().SetText(gameObject.tag).ApplyTemplate(ListVerticalLayout);
    }

    private void OpenScoringZoneGizmo()
    {
        GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Renderer renderer = zone.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Shader Graphs/DefaultSynthesisTransparentShader"));
        renderer.material.color = Color.green;
        GizmoManager.SpawnGizmo(zone.transform,
            t => zone.transform.position = t.Position,
            t => {
                _scoringZones.Add(zone);
                AddZoneEntry(zone);
                DynamicUIManager.CreatePanel<ZoneConfigPanel>();
                ZoneConfigPanel panel = DynamicUIManager.GetPanel<ZoneConfigPanel>();
                panel.SetCallback((name, alliance, points) => OnConfigModalExit(zone, name, alliance, points));
            });
    }

    private bool OnConfigModalExit(GameObject zone, string name, Alliance alliance, int points)
    {
        zone.name = name;
        zone.tag = alliance.ToString();
        zone.GetComponent<Renderer>().material.color = alliance == Alliance.Blue ? Color.blue : Color.red;
        return true;
    }
    
    public override void Update() { }

    public override void Delete()
    {
        foreach (GameObject zone in _scoringZones)
        {
            zone.GetComponent<Renderer>().enabled = false;
        }
    }

    private class ZoneConfigPanel : PanelDynamic
    {
        private const float MODAL_WIDTH = 400f;
        private const float MODAL_HEIGHT = 400f;
        
        private InputField _zoneNameInput;
        private Button _zoneAllianceButton;
        private Button _zonePointsDecreaseButton;
        private Button _zonePointsIncreaseButton;
        private Label _zonePointsLabel;

        private Alliance _alliance = Alliance.Blue;
        private int _points = 0;

        private Func<string, Alliance, int, bool> _callback;
        
        private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0, rightPadding: 0);
            return u;
        };

        public ZoneConfigPanel() : base(
            new Vector2(MODAL_WIDTH, MODAL_HEIGHT))
        {
        }

        public override bool Create()
        {
            Title.SetText("Scoring Zone Config");

            AcceptButton.AddOnClickedEvent(b =>
            {
                _callback.Invoke(_zoneNameInput.Value, _alliance, _points);

                DynamicUIManager.ClosePanel(GetType());
            });

            _zoneNameInput = MainContent.CreateInputField().StepIntoLabel(l => l.SetText("Name"))
                .ApplyTemplate(VerticalLayout);
            _zoneAllianceButton = MainContent.CreateButton().StepIntoLabel(l => l.SetText("Blue Alliance")).AddOnClickedEvent(
                b =>
                {
                    if (_alliance == Alliance.Blue)
                    {
                        b.StepIntoLabel(l => l.SetText("Red Alliance")).SetBackgroundColor<Button>(Color.red);
                        _alliance = Alliance.Red;
                    }
                    else
                    {
                        b.StepIntoLabel(l => l.SetText("Blue Alliance")).SetBackgroundColor<Button>(Color.blue);
                        _alliance = Alliance.Blue;
                    }
                })
                .SetBackgroundColor<Button>(Color.blue)
                .ApplyTemplate(VerticalLayout);

            _zonePointsDecreaseButton = MainContent.CreateButton().StepIntoLabel(l => l.SetText("-")).AddOnClickedEvent(
                b =>
                {
                    _points--;
                    UpdatePointsLabel();
                }).ApplyTemplate(VerticalLayout);
            _zonePointsLabel = MainContent.CreateLabel().SetText(_points.ToString()).ApplyTemplate(VerticalLayout);
            _zonePointsIncreaseButton = MainContent.CreateButton().StepIntoLabel(l => l.SetText("+")).AddOnClickedEvent(
                b =>
                {
                    _points++;
                    UpdatePointsLabel();
                }).ApplyTemplate(VerticalLayout);

            return true;
        }

        public void SetCallback(Func<string, Alliance, int, bool> callback)
        {
            _callback = callback;
        }

        private void UpdatePointsLabel()
        {
            _zonePointsLabel.SetText(_points.ToString());
        }

        public override void Update()
        {
        }

        public override void Delete()
        {
            
        }
    }
}