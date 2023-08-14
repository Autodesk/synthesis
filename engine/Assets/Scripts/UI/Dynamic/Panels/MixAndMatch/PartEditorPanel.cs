using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirabuf.Material;
using SimObjects.MixAndMatch;
using Synthesis.Gizmo;
using Synthesis.Import;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Utilities;
using UnityEngine;
using Utilities;
using Utilities.ColorManager;
using Logger  = SynthesisAPI.Utilities.Logger;
using Object  = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;

namespace UI.Dynamic.Panels.MixAndMatch {
    public class PartEditorPanel : PanelDynamic {
        private const float PANEL_WIDTH  = 300f;
        private const float PANEL_HEIGHT = 400f;

        private float _scrollViewWidth;
        private float _entryWidth;

        private ScrollView _scrollView;
        private Button _removeButton;

        private GameObject _partGameObject;
        private readonly List<GameObject> _connectionGameObjects = new();
        private GameObject _selectedConnection;

        private readonly MixAndMatchPartData _partData;

        private Vector3 centerOffset;

        public PartEditorPanel(MixAndMatchPartData partData) : base(new Vector2(PANEL_WIDTH, PANEL_HEIGHT)) {
            _partData = partData;
        }

        public override bool Create() {
            SceneHider.IsHidden = true;

            RobotSimObject.CurrentlyPossessedRobot = string.Empty;
            MainHUD.ConfigRobot                    = null;

            Title.SetText("Part Editor");

            AcceptButton.StepIntoLabel(l => l.SetText("Save"))
                .AddOnClickedEvent(
                    _ => {
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

            Camera.main!.GetComponent<CameraController>().CameraMode = CameraController.CameraModes["Orbit"];
            OrbitCameraMode.FocusPoint = () => Vector3.up * 0.5f;

            return true;
        }

        /// <summary>Creates the buttons to add and remove connection points </summary>
        private void CreateAddRemoveButtons() {
            (Content left, Content right) = MainContent.CreateSubContent(new Vector2(PANEL_WIDTH, 50))
                                                .SetBottomStretch<Content>()
                                                .SplitLeftRight((PANEL_WIDTH - 10f) / 2f, 10f);

            left.CreateButton("Add").SetStretch<Button>().AddOnClickedEvent(
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

            _connectionGameObjects.ForEach(AddScrollViewEntry);
        }

        /// <summary>Adds an entry to the scroll view</summary>
        private void AddScrollViewEntry(GameObject point) {
            var toggle = _scrollView.Content.CreateToggle(label: "Connection Point", radioSelect: true)
                             .SetSize<Toggle>(new Vector2(PANEL_WIDTH, 50f))
                             .ApplyTemplate(Toggle.RadioToggleLayout)
                             .StepIntoLabel(l => l.SetFontSize(16f))
                             .SetDisabledColor(ColorManager.SynthesisColor.Background);
            toggle.AddOnStateChangedEvent((t, s) => { SelectConnectionPoint(point, t, s); });
        }

        /// <summary>Instantiates the main part object</summary>
        private GameObject InstantiatePartGameObject() {
            if (!File.Exists(_partData.MirabufPartFile)) {
                Logger.Log($"Part file {_partData.MirabufPartFile} not found!", LogLevel.Error);
                return null;
            }

            MirabufLive miraLive = new MirabufLive(_partData.MirabufPartFile);

            GameObject assemblyObject = new GameObject(miraLive.MiraAssembly.Info.Name);
            miraLive.GenerateDefinitionObjects(assemblyObject, false, false);

            // Center part
            var groundedTransform = assemblyObject.transform.Find("grounded");
            centerOffset          = Vector3.down * 0.5f + groundedTransform.transform.localToWorldMatrix.MultiplyPoint(
                                                     groundedTransform.GetBounds().center);
            assemblyObject.transform.position = -centerOffset;
            return assemblyObject;
        }

        /// <summary>Instantiates all of the connection point objects</summary>
        private void InstantiateConnectionGameObjects() {
            if (_partData.ConnectionPoints == null)
                return;

            _partData.ConnectionPoints.ForEach(connection => { InstantiateConnectionGameObject(connection); });
        }

        /// <summary>Instantiates a single connection point object</summary>
        private GameObject InstantiateConnectionGameObject(ConnectionPointData connection) {
            var connectionObj  = Object.Instantiate(SynthesisAssetCollection.Instance.MixAndMatchConnectionPrefab);
            connectionObj.name = "Connection Point";

            var connectionTrf = connectionObj.transform;
            connectionTrf.SetParent(_partGameObject.transform);

            connectionTrf.position = connection.LocalPosition - centerOffset;
            connectionTrf.rotation = connection.LocalRotation;

            Object.Destroy(connectionTrf.GetComponent<Collider>());

            var color = ColorManager.GetColor(ColorManager.SynthesisColor.HighlightHover);
            color.a   = 0.5f;

            Material mat = new Material(Appearance.DefaultTransparentShader);
            mat.SetColor(Appearance.TRANSPARENT_COLOR, color);
            mat.SetFloat(Appearance.TRANSPARENT_SMOOTHNESS, 0);

            connectionTrf.Find("Sphere").GetComponent<MeshRenderer>().material = mat;

            _connectionGameObjects.Add(connectionObj);
            return connectionObj;
        }

        /// <summary>Selects a connection point to edit</summary>
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

        /// <summary>Updates the remove connection point button based on if one is selected</summary>
        private void UpdateRemoveButton() {
            _removeButton.ApplyTemplate((_connectionGameObjects.Count > 0 && _selectedConnection != null)
                                            ? Button.EnableButton
                                            : Button.DisableButton);
        }

        /// <summary>Saves all edits to the part</summary>
        private void SavePartData() {
            if (_partGameObject == null)
                return;

            List<ConnectionPointData> connectionPoints = new();
            _connectionGameObjects.ForEach(point => {
                connectionPoints.Add(
                    new ConnectionPointData(point.transform.position + centerOffset, point.transform.rotation));
            });

            _partData.ConnectionPoints = connectionPoints.ToArray();

            MixAndMatchSaveUtil.SavePartData(_partData);
        }

        public override void Update() {}

        public override void Delete() {
            SceneHider.IsHidden = false;
            Object.Destroy(_partGameObject);
        }
    }
}