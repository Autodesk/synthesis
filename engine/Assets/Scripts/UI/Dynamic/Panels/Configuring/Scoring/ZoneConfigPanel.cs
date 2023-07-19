using System;
using System.Collections.Generic;
using Analytics;
using Synthesis.Gizmo;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;
using Utilities.ColorManager;

using Logger = SynthesisAPI.Utilities.Logger;

public class ZoneConfigPanel : PanelDynamic {
    private const float MODAL_WIDTH  = 500f;
    private const float MODAL_HEIGHT = 600f;

    private const float VERTICAL_PADDING = 16f;

    private InputField _zoneNameInput;
    private Button _zoneAllianceButton;
    private LabeledButton _zoneParentButton;
    private NumberInputField _pointsInputField;
    private Toggle _deleteGamepieceToggle;
    private Toggle _persistentPointsToggle;
    private Slider _xScaleSlider;
    private Slider _yScaleSlider;
    private Slider _zScaleSlider;

    private ScoringZoneData _data        = new ScoringZoneData();
    private ScoringZoneData _initialData = new ScoringZoneData();
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private ScoringZone _zone;
    private string _initialParent;

    private const float MIN_XYZ_SCALE = 0.025f;
    private const float MAX_XYZ_SCALE = 10f;

    private bool _isNewZone = true;

    private bool _pressedButtonToClose = false;

    private bool _selectingNode;

    private HighlightComponent _hoveringNode = null;
    private HighlightComponent _selectedNode = null;

    private Action<ScoringZone, bool> _callback;

    private SortedDictionary<int, bool> _initialFieldCollisions = new SortedDictionary<int, bool>();

    private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0, rightPadding: 0);
        return u;
    };

    public ZoneConfigPanel(ScoringZone zone) : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {
        _zone = zone;
        if (zone is not null) {
            _isNewZone            = false;
            _initialData.Name     = zone.Name;
            _initialData.Alliance = zone.Alliance;
            var parent = zone.GameObject.transform.parent.name == "grounded" ? null : zone.GameObject.transform.parent;
            if (parent is not null) {
                _initialParent               = parent.name;
                _initialData.Parent          = parent.name;
                HighlightComponent highlight = parent.GetComponent<Rigidbody>().GetComponent<HighlightComponent>();

                highlight.Color   = ColorManager.GetColor(ColorManager.SynthesisColor.HighlightSelect);
                highlight.enabled = true;
            }

            _initialData.DestroyGamepiece = zone.DestroyGamepiece;
            _initialData.PersistentPoints = zone.PersistentPoints;
            _initialData.Points           = zone.Points;
            var scale                     = zone.GameObject.transform.localScale;
            _initialData.LocalScale       = (scale.x, scale.y, scale.z);
            _initialPosition              = zone.GameObject.transform.position;
            _initialRotation              = zone.GameObject.transform.rotation;
            _data                         = _initialData;
        }
    }

    public override bool Create() {
        Title.SetText("Scoring Zone Config");

        AcceptButton.AddOnClickedEvent(b => {
            _pressedButtonToClose = true;
            // call one last time to update data
            // don't want to update data and call callback on every character typed for name
            _data.Name          = _zoneNameInput.Value;
            var parentTransform = FieldSimObject.CurrentField.FieldObject.transform.Find(_data.Parent ?? "grounded") ??
                                  FieldSimObject.CurrentField.FieldObject.transform.Find("grounded");
            var localPosition =
                parentTransform.worldToLocalMatrix.MultiplyPoint3x4(_zone.GameObject.transform.position);
            var localRotation   = Quaternion.Inverse(parentTransform.rotation) * _zone.GameObject.transform.rotation;
            _data.LocalPosition = (localPosition.x, localPosition.y, localPosition.z);
            _data.LocalRotation = (localRotation.x, localRotation.y, localRotation.z, localRotation.w);
            _zone.ZoneData      = _data;

            if (_isNewZone)
                FieldSimObject.CurrentField.AddScoringZone(_zone);

            if (!DynamicUIManager.PanelExists<ScoringZonesPanel>())
                DynamicUIManager.CreatePanel<ScoringZonesPanel>();

            if (DynamicUIManager.PanelExists<ScoringZonesPanel>())
                _callback.Invoke(_zone, _isNewZone);

            DynamicUIManager.ClosePanel<ZoneConfigPanel>();

            AnalyticsManager.LogCustomEvent(AnalyticsEvent.ScoringZoneUpdated, ("AllianceColor", _data.Alliance),
                ("ScoringZonePoints", _data.Points), ("ScoringZoneName", _data.Name));
        });

        CancelButton.AddOnClickedEvent(b => {
            _pressedButtonToClose = true;
            DoCancel();
            DynamicUIManager.ClosePanel<ZoneConfigPanel>();
        });

        _zoneNameInput =
            MainContent.CreateInputField()
                .StepIntoLabel(l => l.SetText("Name"))
                .StepIntoHint(h => h.SetText(_initialData.Name is not null ? _initialData.Name : "Zone Name"))
                .SetCharacterLimit(16)
                .ApplyTemplate(VerticalLayout);
        _zoneAllianceButton = MainContent.CreateButton()
                                  .StepIntoLabel(l => l.SetText("Blue Alliance"))
                                  .AddOnClickedEvent(b => {
                                      _data.Alliance = _data.Alliance == Alliance.Blue ? Alliance.Red : Alliance.Blue;
                                      ConfigureAllianceButton();
                                      DataUpdated();
                                  })
                                  .SetBackgroundColor<Button>(Color.blue)
                                  .ApplyTemplate(VerticalLayout);
        ConfigureAllianceButton();

        _zoneParentButton =
            MainContent.CreateLabeledButton()
                .StepIntoLabel(l => l.SetText(_initialParent is not null && _initialParent != "" ? _initialParent
                                                                                                 : "Parent Object"))
                .StepIntoButton(b => {
                    b.StepIntoLabel(l => l.SetText(_initialParent is not null ? "Remove" : "Click to select..."))
                        .AddOnClickedEvent(SelectParentButton);
                })
                .ApplyTemplate(VerticalLayout);

        _pointsInputField = MainContent.CreateNumberInputField()
                                .StepIntoLabel(l => l.SetText("Points"))
                                .StepIntoHint(l => l.SetText("Points"))
                                .ApplyTemplate(VerticalLayout)
                                .SetValue(_initialData.Points)
                                .AddOnValueChangedEvent((f, n) => _data.Points = n);

        _deleteGamepieceToggle = MainContent.CreateToggle(_initialData.DestroyGamepiece, "Destroy Gamepiece")
                                     .AddOnStateChangedEvent((t, v) => {
                                         _data.DestroyGamepiece = !_data.DestroyGamepiece;
                                         t.State                = _data.DestroyGamepiece; // just in case
                                     })
                                     .ApplyTemplate(VerticalLayout);

        _persistentPointsToggle = MainContent.CreateToggle(_initialData.PersistentPoints, "Persistent Points")
                                      .AddOnStateChangedEvent((t, v) => {
                                          _data.PersistentPoints = !_data.PersistentPoints;
                                          t.State                = _data.PersistentPoints;
                                      })
                                      .ApplyTemplate(VerticalLayout);

        _xScaleSlider = MainContent
                            .CreateSlider(label: "X Scale", minValue: MIN_XYZ_SCALE, maxValue: MAX_XYZ_SCALE,
                                currentValue: _initialData.LocalScale.x)
                            .ApplyTemplate(VerticalLayout)
                            .AddOnValueChangedEvent((s, v) => {
                                _data.LocalScale = (v, _data.LocalScale.y, _data.LocalScale.z);
                                DataUpdated();
                            });

        _yScaleSlider = MainContent
                            .CreateSlider(label: "Y Scale", minValue: MIN_XYZ_SCALE, maxValue: MAX_XYZ_SCALE,
                                currentValue: _initialData.LocalScale.y)
                            .ApplyTemplate(VerticalLayout)
                            .AddOnValueChangedEvent((s, v) => {
                                _data.LocalScale = (_data.LocalScale.x, v, _data.LocalScale.z);
                                DataUpdated();
                            });

        _zScaleSlider = MainContent
                            .CreateSlider(label: "Z Scale", minValue: MIN_XYZ_SCALE, maxValue: MAX_XYZ_SCALE,
                                currentValue: _initialData.LocalScale.z)
                            .ApplyTemplate(VerticalLayout)
                            .AddOnValueChangedEvent((s, v) => {
                                _data.LocalScale = (_data.LocalScale.x, _data.LocalScale.y, v);
                                DataUpdated();
                            });

        GameObject obj;
        if (_zone is null) {
            obj   = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _zone = new ScoringZone(obj, "temp scoring zone", Alliance.Blue, 0, false, true);
            _zone.VisibilityCounter++;
        } else {
            obj = _zone.GameObject;
            UseZone(_zone);
        }

        GizmoManager.SpawnGizmo(obj.transform,
            t => {
                obj.transform.position = t.Position;
                obj.transform.rotation = t.Rotation;
            },
            t => {});

        return true;
    }

    private void DoCancel() {
        if (_isNewZone)
            GameObject.Destroy(_zone.GameObject);
        else {
            _zone.ZoneData                      = _initialData;
            _zone.GameObject.transform.position = _initialPosition;
            _zone.GameObject.transform.rotation = _initialRotation;
            if (_initialParent is null || _initialParent == "")
                _zone.GameObject.transform.parent = null;
            else
                _zone.GameObject.transform.parent = GameObject.Find(_initialParent).transform;
        }
    }

    private void ConfigureAllianceButton() {
        if (_data.Alliance == Alliance.Blue) {
            _zoneAllianceButton.StepIntoLabel(l => l.SetText("Blue Alliance")).SetBackgroundColor<Button>(Color.blue);
        } else {
            _zoneAllianceButton.StepIntoLabel(l => l.SetText("Red Alliance")).SetBackgroundColor<Button>(Color.red);
        }
    }

    public void UseZone(ScoringZone zone) {
        _zone      = zone;
        _data.Name = _zone.Name;
        _zoneNameInput.SetValue(_zone.Name);

        _data.Alliance = zone.Alliance;
        ConfigureAllianceButton();

        _data.Points = zone.Points;
        _pointsInputField.SetValue(_zone.Points);

        _data.DestroyGamepiece = zone.DestroyGamepiece;
        _deleteGamepieceToggle.SetState(_zone.DestroyGamepiece, notify: false);

        var localScale   = zone.GameObject.transform.localScale;
        _data.LocalScale = (localScale.x, localScale.y, localScale.z);
        _xScaleSlider.SetValue(_data.LocalScale.x);
        _yScaleSlider.SetValue(_data.LocalScale.y);
        _zScaleSlider.SetValue(_data.LocalScale.z);
    }

    private void DataUpdated() {
        var data            = _zone.ZoneData;
        data.Alliance       = _data.Alliance;
        data.Points         = _data.Points;
        data.LocalScale     = _data.LocalScale;
        var parentTransform = FieldSimObject.CurrentField.FieldObject.transform.Find(_data.Parent);
        var localPosition   = parentTransform.worldToLocalMatrix.MultiplyPoint3x4(_zone.GameObject.transform.position);
        var localRotation   = Quaternion.Inverse(parentTransform.rotation) * _zone.GameObject.transform.rotation;
        data.LocalPosition  = (localPosition.x, localPosition.y, localPosition.z);
        data.LocalRotation  = (localRotation.x, localRotation.y, localRotation.z, localRotation.w);
        _zone.ZoneData      = data;
    }

    public void SetCallback(Action<ScoringZone, bool> callback) {
        _callback = callback;
    }

    public void SelectParentButton(Button b) {
        if (!_selectingNode) {
            if (_selectedNode || _data.Parent != "grounded") {
                if (_selectedNode)
                    _selectedNode.enabled = false;
                _selectedNode = null;
                _hoveringNode = null;
                _data.Parent  = "grounded";
            } else {
                // I don't like this; do we just want all field rigidbodies to detect collisions?
                if (_initialFieldCollisions.Count == 0)
                    FieldSimObject.CurrentField.FieldObject.GetComponentsInChildren<Rigidbody>().ForEach(x => {
                        _initialFieldCollisions.Add(x.GetHashCode(), x.detectCollisions);
                        x.detectCollisions = true;
                    });
                _selectingNode = true;
            }
        } else {
            FieldSimObject.CurrentField.FieldObject.GetComponentsInChildren<Rigidbody>().ForEach(
                x => x.detectCollisions = _initialFieldCollisions[x.GetHashCode()]);
            _initialFieldCollisions.Clear();
            _selectingNode = false;
        }

        SetSelectUIState(_selectingNode);
    }

    private void SetSelectUIState(bool isUserSelecting) {
        if (isUserSelecting) {
            _zoneParentButton.StepIntoLabel(l => l.SetText("Selecting..."));
            _zoneParentButton.StepIntoButton(b => b.StepIntoImage(i => i.SetColor(
                    ColorManager.SynthesisColor.BackgroundSecondary))
                .StepIntoLabel(l => l.SetText("...")));
        }
        else {
            if (_selectedNode is null)
                _data.Parent = "grounded";
            _zoneParentButton.StepIntoLabel(
                l => l.SetText(_selectedNode is not null ? _selectedNode.name : "Parent Object"));

            _zoneParentButton.StepIntoButton(
                b => b.StepIntoImage(
                        i => i.SetColor(ColorManager.SynthesisColor.InteractiveElementLeft,
                            ColorManager.SynthesisColor.InteractiveElementRight))
                    .StepIntoLabel(l => l.SetText(_selectedNode is not null ? "Remove" : "Click to select...")));
        }
    }

    public override void Update() {
        if (_selectingNode) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(ray, out hitInfo);

            if (hit && hitInfo.rigidbody != null &&
                hitInfo.rigidbody.transform.parent == FieldSimObject.CurrentField.FieldObject.transform &&
                !hitInfo.rigidbody.transform.CompareTag("field")) {
                if (_hoveringNode is not null &&
                    (_selectedNode is null || !_selectedNode.name.Equals(_hoveringNode.name))) {
                    _hoveringNode.enabled = false;
                }

                _hoveringNode = hitInfo.rigidbody.GetComponent<HighlightComponent>();
                if (_selectedNode is null || hitInfo.rigidbody.name != _selectedNode.name) {
                    _hoveringNode.enabled = true;
                    _hoveringNode.Color   = ColorManager.GetColor(ColorManager.SynthesisColor.HighlightHover);
                }

                if (Input.GetKeyDown(KeyCode.Mouse0)) {
                    if (_selectedNode is not null) {
                        _selectedNode.enabled = false;
                    }

                    _selectedNode         = _hoveringNode;
                    _selectedNode.enabled = true;
                    _selectedNode.Color   = ColorManager.GetColor(ColorManager.SynthesisColor.HighlightSelect);

                    _data.Parent  = _selectedNode.gameObject.transform.name;
                    _hoveringNode = null;

                    _selectingNode = false;
                    SetSelectUIState(false);
                }
            } else if (_hoveringNode is not null &&
                       (_selectedNode is null || !_selectedNode.name.Equals(_hoveringNode.name))) {
                _hoveringNode.enabled = false;
                _hoveringNode         = null;
            } else if (hit && hitInfo.rigidbody is null) {
                if (Input.GetKeyDown(KeyCode.Mouse0)) {
                    _data.Parent   = null;
                    _selectingNode = false;
                    SetSelectUIState(false);
                }
            }
        }
    }

    public override void Delete() {
        if (_hoveringNode is not null) {
            _hoveringNode.enabled = false;
        }

        if (_selectedNode is not null) {
            _selectedNode.enabled = false;
        }

        if (_data.Parent is not null && _data.Parent != "grounded") {
            HighlightComponent highlight = FieldSimObject.CurrentField.FieldObject.transform.Find(_data.Parent)
                                               .GetComponent<Rigidbody>()
                                               .GetComponent<HighlightComponent>();
            highlight.enabled = false;
        }

        if (!_pressedButtonToClose)
            DoCancel();
        GizmoManager.ExitGizmo();
        FieldSimObject.CurrentField.UpdateSavedScoringZones();
    }
}
