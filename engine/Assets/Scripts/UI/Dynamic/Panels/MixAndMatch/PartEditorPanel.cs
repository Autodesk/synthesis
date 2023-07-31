using System;
using System.Collections.Generic;
using System.Linq;
using Mirabuf;
using SimObjects.MixAndMatch;
using Synthesis.Gizmo;
using Synthesis.Import;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;
using Utilities.ColorManager;
using Color   = UnityEngine.Color;
using Object  = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;

namespace UI.Dynamic.Panels.MixAndMatch {
    public class PartEditorPanel : PanelDynamic {
        private const float PANEL_WIDTH  = 400f;
        private const float PANEL_HEIGHT = 400f;

        private const float VERTICAL_PADDING   = 7f;
        private const float HORIZONTAL_PADDING = 16f;

        private MixAndMatchPartData _partData;

        private GameObject _partGameObject;
        private readonly List<GameObject> _connectionGameObjects = new();

        private float _scrollViewWidth;
        private float _entryWidth;

        private ScrollView _scrollView;

        private Button _removeButton;

        private GameObject _selectedPoint;

        public PartEditorPanel(MixAndMatchPartData partData) : base(new Vector2(PANEL_WIDTH, PANEL_HEIGHT)) {
            _partData = partData;
        }

        public override bool Create() {
            Title.SetText("Part Editor");

            AcceptButton.StepIntoLabel(l => l.SetText("Save")).AddOnClickedEvent(b => {
                SavePartData();
                GizmoManager.ExitGizmo();
                DynamicUIManager.ClosePanel<PartEditorPanel>();
            });
            CancelButton.RootGameObject.SetActive(false);

            _scrollView = MainContent.CreateScrollView().SetStretch<ScrollView>(bottomPadding: 60f);

            CreateAddRemoveButtons();

            _partGameObject = InstantiatePartGameObject();

            InstantiatePointGameObjects();
            PopulateScrollView();

            return true;
        }

        private void CreateAddRemoveButtons() {
            (Content left, Content right) = MainContent.CreateSubContent(new Vector2(400, 50))
                                                .SetBottomStretch<Content>()
                                                .SplitLeftRight((PANEL_WIDTH - 10f) / 2f, 10f);

            var addButton = left.CreateButton("Add").SetStretch<Button>().AddOnClickedEvent(
                _ => {
                    AddScrollViewEntry(InstantiatePointGameObject(new ConnectionPointData()));
                    UpdateRemoveButton();
                });

            _removeButton = right.CreateButton("Remove").SetStretch<Button>().AddOnClickedEvent(
                _ => {
                    if (_selectedPoint != null) {
                        _connectionGameObjects.Remove(_selectedPoint);
                        Object.Destroy(_selectedPoint);
                        _selectedPoint = null;
                    }
                    PopulateScrollView();
                    GizmoManager.ExitGizmo();
                    UpdateRemoveButton();
                });
            UpdateRemoveButton();
        }

        private void PopulateScrollView() {
            _scrollView.Content.DeleteAllChildren();

            _connectionGameObjects.ForEach(pointGameObject => { AddScrollViewEntry(pointGameObject); });
        }

        private GameObject InstantiatePartGameObject() {
            // TODO: check if part file exists
            MirabufLive miraLive = new MirabufLive(_partData.MirabufPartFile);

            GameObject assemblyObject = new GameObject(miraLive.MiraAssembly.Info.Name);
            // TODO: set parent
            miraLive.GenerateDefinitionObjects(assemblyObject, false);
            // TODO: Center part
            return assemblyObject;
        }

        private void InstantiatePointGameObjects() {
            if (_partData.ConnectionPoints == null)
                return;

            _partData.ConnectionPoints.ForEach(point => { InstantiatePointGameObject(point); });
        }

        private GameObject InstantiatePointGameObject(ConnectionPointData point) {
            var gameObject  = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var trf         = gameObject.transform;
            gameObject.name = "ConnectionPoint";
            trf.SetParent(_partGameObject.transform);

            trf.position   = point.LocalPosition;
            trf.rotation   = point.LocalRotation;
            trf.localScale = Vector3.one * 0.25f;
            // TODO: after merge, use color manager
            trf.GetComponent<MeshRenderer>().material.color = Color.green;
            Object.Destroy(trf.GetComponent<Collider>());
            _connectionGameObjects.Add(gameObject);
            return gameObject;
        }

        private void AddScrollViewEntry(GameObject point) {
            var toggle = _scrollView.Content.CreateToggle(label: "Connection Point", radioSelect: true)
                             .SetSize<Toggle>(new Vector2(PANEL_WIDTH, 50f))
                             .ApplyTemplate(Toggle.RadioToggleLayout)
                             .StepIntoLabel(l => l.SetFontSize(16f))
                             .SetDisabledColor(ColorManager.SynthesisColor.Background);
            toggle.AddOnStateChangedEvent((t, s) => { SelectConnectionPoint(point, t, s); });
        }

        private void SelectConnectionPoint(GameObject point, Toggle toggle, bool state) {
            if (state) {
                _selectedPoint = point;
                GizmoManager.SpawnGizmo(point.transform,
                    t => {
                        point.transform.position = t.Position;
                        point.transform.rotation = t.Rotation;
                    },
                    _ => {});

                _scrollView.Content.ChildrenReadOnly.OfType<Toggle>().ForEach(x => { x.SetStateWithoutEvents(false); });
                toggle.SetStateWithoutEvents(true);
            } else {
                _selectedPoint = null;
                GizmoManager.ExitGizmo();
            }
            UpdateRemoveButton();
        }

        private void UpdateRemoveButton() {
            _removeButton.ApplyTemplate((_connectionGameObjects.Count > 0 && _selectedPoint != null)
                                            ? Button.EnableButton
                                            : Button.DisableButton);
        }

        public override void Update() {}

        public override void Delete() {
            Object.Destroy(_partGameObject);
        }

        private void SavePartData() {
            List<ConnectionPointData> connectionPoints = new();
            _connectionGameObjects.ForEach(point => {
                connectionPoints.Add(new ConnectionPointData(point.transform.position, point.transform.rotation));
            });

            _partData.ConnectionPoints = connectionPoints.ToArray();

            MixAndMatchSaveUtil.SavePartData(_partData);
        }
    }
}