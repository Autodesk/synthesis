using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Mirabuf;
using SimObjects.MixAndMatch;
using Synthesis.Gizmo;
using Synthesis.Import;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using Synthesis.Util;
using SynthesisAPI.Utilities;
using UnityEngine;
using Utilities.ColorManager;
using Color   = UnityEngine.Color;
using Logger = SynthesisAPI.Utilities.Logger;
using Object  = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;

namespace UI.Dynamic.Panels.MixAndMatch {
    public class PartEditorPanel : PanelDynamic {
        private const float PANEL_WIDTH  = 400f;
        private const float PANEL_HEIGHT = 400f;

        private MixAndMatchPartData _partData;

        private GameObject _partGameObject;
        private readonly List<GameObject> _connectionGameObjects = new();

        private float _scrollViewWidth;
        private float _entryWidth;

        private ScrollView _scrollView;

        private Button _removeButton;

        private GameObject _selectedConnection;

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
            if (_partGameObject == null)
                return false;

            InstantiateConnectionGameObjects();
            PopulateScrollView();

            return true;
        }

        private void CreateAddRemoveButtons() {
            (Content left, Content right) = MainContent.CreateSubContent(new Vector2(400, 50))
                                                .SetBottomStretch<Content>()
                                                .SplitLeftRight((PANEL_WIDTH - 10f) / 2f, 10f);

            var addButton = left.CreateButton("Add").SetStretch<Button>().AddOnClickedEvent(
                _ => {
                    AddScrollViewEntry(InstantiateConnectionGameObject(new ConnectionPointData()));
                    UpdateRemoveButton();
                });

            _removeButton = right.CreateButton("Remove").SetStretch<Button>().AddOnClickedEvent(
                _ => {
                    if (_selectedConnection != null) {
                        _connectionGameObjects.Remove(_selectedConnection);
                        Object.Destroy(_selectedConnection);
                        _selectedConnection = null;
                    }
                    PopulateScrollView();
                    GizmoManager.ExitGizmo();
                    UpdateRemoveButton();
                });
            UpdateRemoveButton();
        }

        private void PopulateScrollView() {
            _scrollView.Content.DeleteAllChildren();

            _connectionGameObjects.ForEach(connectionGameObject => { AddScrollViewEntry(connectionGameObject); });
        }

        private GameObject InstantiatePartGameObject() {
            if (!File.Exists(_partData.MirabufPartFile)) {
                Logger.Log($"Part file {_partData.MirabufPartFile} not found!", LogLevel.Error);
                return null;
            }
            
            MirabufLive miraLive = new MirabufLive(_partData.MirabufPartFile);

            GameObject assemblyObject = new GameObject(miraLive.MiraAssembly.Info.Name);
            miraLive.GenerateDefinitionObjects(assemblyObject, false);

            // Center part
            var groundedTransform = assemblyObject.transform.Find("grounded");
            assemblyObject.transform.position = Vector3.up 
                                                - groundedTransform.transform.localToWorldMatrix.MultiplyPoint(groundedTransform.GetBounds().center);
            return assemblyObject;
        }

        private void InstantiateConnectionGameObjects() {
            if (_partData.ConnectionPoints == null)
                return;

            _partData.ConnectionPoints.ForEach(connection => { InstantiateConnectionGameObject(connection); });
        }

        private GameObject InstantiateConnectionGameObject(ConnectionPointData connection) {
            var gameObject  = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.name = "ConnectionPoint";

            var trf         = gameObject.transform;
            trf.SetParent(_partGameObject.transform);
            
            trf.position   = connection.LocalPosition;
            trf.rotation   = connection.LocalRotation;
            trf.localScale = Vector3.one * 0.25f;
            
            trf.GetComponent<MeshRenderer>().material.color = ColorManager.GetColor(
                ColorManager.SynthesisColor.HighlightHover);
            
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
                _selectedConnection = point;
                GizmoManager.SpawnGizmo(point.transform,
                    t => {
                        point.transform.position = t.Position;
                        point.transform.rotation = t.Rotation;
                    },
                    _ => {});

                _scrollView.Content.ChildrenReadOnly.OfType<Toggle>().ForEach(x => { x.SetStateWithoutEvents(false); });
                toggle.SetStateWithoutEvents(true);
            } else {
                _selectedConnection = null;
                GizmoManager.ExitGizmo();
            }
            UpdateRemoveButton();
        }

        private void UpdateRemoveButton() {
            _removeButton.ApplyTemplate((_connectionGameObjects.Count > 0 && _selectedConnection != null)
                                            ? Button.EnableButton
                                            : Button.DisableButton);
        }

        public override void Update() {}

        public override void Delete() {
            Object.Destroy(_partGameObject);
        }

        private void SavePartData() {
            if (_partGameObject == null)
                return;
            
            List<ConnectionPointData> connectionPoints = new();
            _connectionGameObjects.ForEach(point => {
                connectionPoints.Add(new ConnectionPointData(point.transform.position, point.transform.rotation));
            });

            _partData.ConnectionPoints = connectionPoints.ToArray();

            MixAndMatchSaveUtil.SavePartData(_partData);
        }
    }
}