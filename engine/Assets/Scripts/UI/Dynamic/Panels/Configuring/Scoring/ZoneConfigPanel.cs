using System;
using Synthesis.Gizmo;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class ZoneConfigPanel : PanelDynamic
    {
        private const float MODAL_WIDTH = 400f;
        private const float MODAL_HEIGHT = 600f;
    
        private const float VERTICAL_PADDING = 16f;
        
        private InputField _zoneNameInput;
        private Button _zoneAllianceButton;
        private Button _zonePointsDecreaseButton;
        private Button _zonePointsIncreaseButton;
        private Label _zonePointsLabel;
        private Toggle _deleteGamepieceToggle;
        private Slider _xScaleSlider;
        private Slider _yScaleSlider;
        private Slider _zScaleSlider;

        private ScoringZoneData _data = new ScoringZoneData();

        private Func<ScoringZoneData, bool> _callback;
        
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
                // call one last time to update data
                // don't want to update data and call callback on every character typed for name
                _data.Name = _zoneNameInput.Value;
                _callback.Invoke(_data);
                DynamicUIManager.ClosePanel<ZoneConfigPanel>();
            });

            _zoneNameInput = MainContent.CreateInputField().StepIntoLabel(l => l.SetText("Name"))
                .ApplyTemplate(VerticalLayout);
            _zoneAllianceButton = MainContent.CreateButton().StepIntoLabel(l => l.SetText("Blue Alliance")).AddOnClickedEvent(
                b =>
                {
                    if (_data.Alliance == Alliance.Blue)
                    {
                        b.StepIntoLabel(l => l.SetText("Red Alliance")).SetBackgroundColor<Button>(Color.red);
                        _data.Alliance = Alliance.Red;
                    }
                    else
                    {
                        b.StepIntoLabel(l => l.SetText("Blue Alliance")).SetBackgroundColor<Button>(Color.blue);
                        _data.Alliance = Alliance.Blue;
                    }
                    DataUpdated();
                })
                .SetBackgroundColor<Button>(Color.blue)
                .ApplyTemplate(VerticalLayout);

            _zonePointsDecreaseButton = MainContent.CreateButton().StepIntoLabel(l => l.SetText("-")).AddOnClickedEvent(
                b =>
                {
                    _data.Points--;
                    UpdatePointsLabel();
                    DataUpdated();
                }).ApplyTemplate(VerticalLayout);
            _zonePointsLabel = MainContent.CreateLabel().SetText(_data.Points.ToString()).ApplyTemplate(VerticalLayout);
            _zonePointsIncreaseButton = MainContent.CreateButton().StepIntoLabel(l => l.SetText("+")).AddOnClickedEvent(
                b =>
                {
                    _data.Points++;
                    UpdatePointsLabel();
                    DataUpdated();
                }).ApplyTemplate(VerticalLayout);

            _deleteGamepieceToggle = MainContent.CreateToggle(false, "Destroy Gamepiece")
                .AddOnStateChangedEvent((t, v) =>
                {
                    _data.DestroyGamepiece = !_data.DestroyGamepiece;
                    t.State = _data.DestroyGamepiece; // just in case
                }).ApplyTemplate(VerticalLayout);

            _xScaleSlider = MainContent.CreateSlider(label: "X Scale", minValue: 0.1f, maxValue: 10f, currentValue: 1)
                .ApplyTemplate(VerticalLayout).AddOnValueChangedEvent(
                    (s, v) =>
                    {
                        _data.XScale = v;
                        DataUpdated();
                    });
            
            _yScaleSlider = MainContent.CreateSlider(label: "Y Scale", minValue: 0.1f, maxValue: 10f, currentValue: 1)
                .ApplyTemplate(VerticalLayout).AddOnValueChangedEvent(
                    (s, v) =>
                    {
                        _data.YScale = v;
                        DataUpdated();
                    });
            
            _zScaleSlider = MainContent.CreateSlider(label: "Z Scale", minValue: 0.1f, maxValue: 10f, currentValue: 1)
                .ApplyTemplate(VerticalLayout).AddOnValueChangedEvent(
                    (s, v) =>
                    {
                        _data.ZScale = v;
                        DataUpdated();
                    });

            return true;
        }

        private void DataUpdated()
        {
            _callback.Invoke(_data);
        }

        public void SetCallback(Func<ScoringZoneData, bool> callback)
        {
            _callback = callback;
        }

        private void UpdatePointsLabel()
        {
            _zonePointsLabel.SetText(_data.Points.ToString());
        }

        public override void Update()
        {
        }

        public override void Delete()
        {
            GizmoManager.ExitGizmo();
        }
    }