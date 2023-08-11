using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirabuf.Material;
using SimObjects.MixAndMatch;
using Synthesis.Import;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Utilities;
using UI.Dynamic.Modals.MixAndMatch;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.ColorManager;
using Camera    = UnityEngine.Camera;
using Logger    = SynthesisAPI.Utilities.Logger;
using Object    = UnityEngine.Object;
using Rigidbody = UnityEngine.Rigidbody;
using Transform = UnityEngine.Transform;
using Vector3   = UnityEngine.Vector3;

namespace UI.Dynamic.Panels.MixAndMatch {
    public class RobotEditorPanel : PanelDynamic {
        private const float PANEL_WIDTH  = 300f;
        private const float PANEL_HEIGHT = 400f;

        private const float PART_ROTATION_SPEED = 10f;

        private static readonly int _connectionLayer = LayerMask.NameToLayer("ConnectionPoint");
        private static readonly int _robotLayer      = LayerMask.NameToLayer("MixAndMatchEditor");

        private static readonly int _connectionLayerMask = 1 << _connectionLayer;
        private static readonly int _robotLayerMask      = 1 << _robotLayer;

        private float _scrollViewWidth;
        private float _entryWidth;

        private ScrollView _scrollView;
        private Button _removeButton;

        private readonly MixAndMatchRobotData _robotData;

        private GameObject _robotGameObject;
        private readonly List<(GameObject gameObject, MixAndMatchPartData partData, ParentNodeData parentNodeData)>
            _partGameObjects = new();
        private (GameObject gameObject, MixAndMatchPartData partData, ParentNodeData parentNodeData)? _selectedPart;

        private bool _creationFailed;
        private bool _selectingNode;

        public RobotEditorPanel(MixAndMatchRobotData robotData) : base(new Vector2(PANEL_WIDTH, PANEL_HEIGHT)) {
            _robotData = robotData;
        }

        public override bool Create() {
            RobotSimObject.CurrentlyPossessedRobot = string.Empty;
            MainHUD.ConfigRobot                    = null;

            Title.SetText("Robot Editor");

            AcceptButton.StepIntoLabel(l => l.SetText("Save"))
                .AddOnClickedEvent(
                    _ => {
                        SaveRobotData();
                        DynamicUIManager.ClosePanel<RobotEditorPanel>();
                    });
            CancelButton.RootGameObject.SetActive(false);

            _scrollView = MainContent.CreateScrollView().SetStretch<ScrollView>(bottomPadding: 60f);

            CreateAddRemoveButtons();

            _robotGameObject = new GameObject(_robotData.Name);

            InstantiatePartGameObjects();
            if (_creationFailed)
                return false;

            PopulateScrollView();

            Camera.main!.GetComponent<CameraController>().CameraMode = CameraController.CameraModes["Orbit"];

            OrbitCameraMode.FocusPoint = () =>
                _robotGameObject != null ? _robotGameObject.transform.position : Vector3.up / 2f;

            return true;
        }

        /// <summary>Creates the buttons to add and remove parts from the robot assembly</summary>
        private void CreateAddRemoveButtons() {
            (Content add, Content right) = MainContent.CreateSubContent(new Vector2(PANEL_WIDTH, 50))
                                               .SetBottomStretch<Content>()
                                               .SplitLeftRight((PANEL_WIDTH - 10f) / 3f, 10f);

            (Content remove, Content parent) = right.SplitLeftRight((PANEL_WIDTH - 10f) / 3f, 10f);

            add.CreateButton("Add").SetStretch<Button>().AddOnClickedEvent(
                _ => {
                    DynamicUIManager.CreateModal<SelectPartModal>(
                        args: new Action<MixAndMatchPartData>(AddAdditionalPart));
                });

            _removeButton = remove.CreateButton("Remove").SetStretch<Button>().AddOnClickedEvent(
                _ => DynamicUIManager.CreateModal<RemovePartModal>(args: new Action(RemovePartCallback)));
            UpdateRemoveButton();

            parent.CreateButton("Parent").SetStretch<Button>().AddOnClickedEvent(
                _ => _selectingNode = !_selectingNode);

            void RemovePartCallback() {
                if (_selectedPart == null)
                    return;

                _partGameObjects.Remove(_selectedPart.Value);
                Object.Destroy(_selectedPart.Value.gameObject);
                _selectedPart = null;

                PopulateScrollView();
                UpdateRemoveButton();
            }
        }

        /// <summary>Creates all of the part objects when the panel is first opened</summary>
        private void InstantiatePartGameObjects() {
            if (_robotData.PartTransformData == null)
                return;

            _robotData.PartTransformData.ForEach(InstantiatePartGameObject);
        }

        /// <summary>Instantiates a part object to position from a part fileName</summary>
        private void InstantiatePartGameObject(RobotPartTransformData partData) {
            var partFile = MixAndMatchSaveUtil.LoadPartData(partData.FileName, false);
            if (partFile == null) {
                Logger.Log($"Part file \"{partData.FileName}\" not found!", LogLevel.Error);
                _creationFailed = true;
                return;
            }

            InstantiatePartGameObject(
                partData.LocalPosition, partData.LocalRotation, partFile, partData.ParentNodeData);
        }

        /// <summary>Instantiates a part object to position from a partData object</summary>
        private GameObject InstantiatePartGameObject(Vector3 localPosition, Quaternion localRotation,
            MixAndMatchPartData partData, ParentNodeData parentNodeData) {
            if (!File.Exists(partData.MirabufPartFile)) {
                Logger.Log($"Mirabuf file \"{partData.MirabufPartFile}\" not found!", LogLevel.Error);
                _creationFailed = true;
                return null;
            }

            MirabufLive miraLive = new MirabufLive(partData.MirabufPartFile);

            GameObject gameObject = new GameObject(partData.Name);
            Transform trf         = gameObject.transform;

            miraLive.GenerateDefinitionObjects(gameObject, dynamicLayer: _robotLayer);

            trf.SetParent(_robotGameObject.transform);

            trf.position = localPosition;
            trf.rotation = localRotation;

            _partGameObjects.Add((gameObject, partData, parentNodeData));

            trf.GetComponentsInChildren<Rigidbody>().ForEach(x => {
                var rc     = x.gameObject.AddComponent<HighlightComponent>();
                rc.Color   = ColorManager.GetColor(ColorManager.SynthesisColor.HighlightHover);
                rc.enabled = false;

                x.isKinematic = true;
            });

            gameObject.AddComponent<PartGuidHolder>().Guid = partData.Guid;

            InstantiatePartConnectionPoints(gameObject, partData);
            return gameObject;
        }

        /// <summary>Instantiates objects for all the connection points of a part</summary>
        private void InstantiatePartConnectionPoints(GameObject partGameObject, MixAndMatchPartData partData) {
            partData.ConnectionPoints.ForEachIndex((_, connection) => {
                var gameObject   = Object.Instantiate(SynthesisAssetCollection.Instance.MixAndMatchConnectionPrefab);
                gameObject.layer = _connectionLayer;
                gameObject.name  = "Connection Point";

                var trf = gameObject.transform;

                trf.SetParent(partGameObject.transform);
                trf.localPosition = connection.LocalPosition;
                trf.localRotation = connection.LocalRotation;

                var color = ColorManager.GetColor(ColorManager.SynthesisColor.HighlightHover);
                color.a   = 0.5f;

                Material mat = new Material(Appearance.DefaultTransparentShader);
                mat.SetColor(Appearance.TRANSPARENT_COLOR, color);
                mat.SetFloat(Appearance.TRANSPARENT_SMOOTHNESS, 0);
                trf.Find("Sphere").GetComponent<MeshRenderer>().material = mat;
            });
        }

        /// <summary>Adds an addition part to the robot anytime during editing</summary>
        private void AddAdditionalPart(MixAndMatchPartData part) {
            EnableConnectionColliders(_selectedPart?.gameObject);
            AddScrollViewEntry((InstantiatePartGameObject(Vector3.zero, Quaternion.identity, part, null), part, null));
            UpdateRemoveButton();
        }

        /// <summary>Clears the scroll view then repopulates it with all the current parts</summary>
        private void PopulateScrollView() {
            _scrollView.Content.DeleteAllChildren();

            _partGameObjects.ForEach(AddScrollViewEntry);
        }

        /// <summary>Adds an entry to the scroll view</summary>
        private void AddScrollViewEntry((GameObject gameObject, MixAndMatchPartData partData, ParentNodeData _) part) {
            var toggle = _scrollView.Content.CreateToggle(label: part.gameObject.name, radioSelect: true)
                             .SetSize<Toggle>(new Vector2(PANEL_WIDTH, 50f))
                             .ApplyTemplate(Toggle.RadioToggleLayout)
                             .StepIntoLabel(l => l.SetFontSize(16f))
                             .SetDisabledColor(ColorManager.SynthesisColor.Background);
            toggle.AddOnStateChangedEvent((t, s) => { SelectPart(part, t, s); });
        }

        /// <summary>Selects which part will be edited, and updates radio select buttons accordingly</summary>
        private void SelectPart(
            (GameObject gameObject, MixAndMatchPartData partData, ParentNodeData _) part, Toggle toggle, bool state) {
            if (state) {
                _scrollView.Content.ChildrenReadOnly.OfType<Toggle>().ForEach(x => { x.SetStateWithoutEvents(false); });
                toggle.SetStateWithoutEvents(true);

                _partGameObjects.ForEach(x => EnableConnectionColliders(x.gameObject));
                DisableConnectionColliders(part.gameObject);

                _selectedPart = part;
            } else
                _selectedPart = null;

            UpdateRemoveButton();
        }

        /// <summary>Enables the colliders of a part's connection points so other parts can snap to it</summary>
        private void EnableConnectionColliders(GameObject part) {
            if (part == null)
                return;

            part.GetComponentsInChildren<SphereCollider>(true).ForEach(c => c.enabled = true);
        }

        /// <summary>Disables the colliders of a part's connection points so other parts can't snap to it</summary>
        private void DisableConnectionColliders(GameObject part) {
            if (part == null)
                return;

            part.GetComponentsInChildren<SphereCollider>().ForEach(c => c.enabled = false);
        }

        /// Updates the remove button based on if a part is selected
        private void UpdateRemoveButton() {
            _removeButton.ApplyTemplate(
                (_partGameObjects.Count > 0 && _selectedPart != null) ? Button.EnableButton : Button.DisableButton);
        }

        private float _axisRotation;

        /// <summary>Handles raycasts to find connection points and part rotation</summary>
        private void PartPlacement() {
            if (EventSystem.current.IsPointerOverGameObject() || _selectedPart == null)
                return;

            Ray ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100, _connectionLayerMask)) {
                var selectedTrf      = _selectedPart.Value.gameObject.transform;
                var selectedPartData = _selectedPart.Value.partData;

                // Align the connection points normals
                selectedTrf.position = hit.transform.position;
                selectedTrf.rotation = Quaternion.LookRotation(-hit.transform.forward, Vector3.up);
                selectedTrf.Rotate(-selectedPartData.ConnectionPoints[0].LocalRotation.eulerAngles);

                // Handle rotation about the connection points normals
                if (Input.GetKey(KeyCode.R)) {
                    _axisRotation += Time.deltaTime * PART_ROTATION_SPEED * (Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
                }

                Vector3 axis = selectedTrf.localToWorldMatrix.rotation *
                               (selectedPartData.ConnectionPoints[0].LocalRotation * Vector3.forward);

                // It says rotate around is obsolete but .Rotate didn't work in the same way
                selectedTrf.RotateAround(axis, _axisRotation);

                // Offset so that the connection points are overlapping
                selectedTrf.Translate(-selectedPartData.ConnectionPoints[0].LocalPosition);
            }
        }

        private HighlightComponent _hoveringNode;
        private HighlightComponent _selectedNode;

        /// <summary>Raycast to select a part node</summary>
        private void NodeSelection() {
            if (_selectedPart == null)
                return;

            Ray ray  = Camera.main!.ScreenPointToRay(Input.mousePosition);
            bool hit = Physics.Raycast(ray, out var hitInfo, 100, _robotLayerMask);

            if (hit && hitInfo.rigidbody != null &&
                hitInfo.transform.GetComponent<HighlightComponent>() != _selectedNode) {
                if (_hoveringNode != null &&
                    (_selectedNode == null || !_selectedNode.name.Equals(_hoveringNode.name))) {
                    _hoveringNode.enabled = false;
                }

                _hoveringNode         = hitInfo.rigidbody.GetComponent<HighlightComponent>();
                _hoveringNode.enabled = true;
                _hoveringNode.Color   = ColorManager.GetColor(ColorManager.SynthesisColor.HighlightHover);

                if (Input.GetKeyDown(KeyCode.Mouse0)) {
                    if (_selectedNode != null)
                        _selectedNode.enabled = false;

                    _selectedNode         = _hoveringNode;
                    _selectedNode.enabled = true;
                    _selectedNode.Color   = ColorManager.GetColor(ColorManager.SynthesisColor.HighlightSelect);
                    _hoveringNode         = null;

                    _selectingNode = false;

                    int selectedPartIndex = _partGameObjects.FindIndex(x => x.Equals(_selectedPart));

                    var part            = _partGameObjects[selectedPartIndex];
                    part.parentNodeData = new ParentNodeData(
                        _selectedNode.transform.parent.GetComponent<PartGuidHolder>().Guid, _selectedNode.name);
                    _partGameObjects[selectedPartIndex] = part;
                }
            } else {
                if (_hoveringNode != null) {
                    _hoveringNode.enabled = false;
                    _hoveringNode         = null;
                }
            }
        }

        /// <summary>Saves all edits that have been made</summary>
        private void SaveRobotData() {
            List<RobotPartTransformData> parts = new();
            _partGameObjects.ForEach(part => {
                parts.Add(new RobotPartTransformData(part.gameObject.name, part.parentNodeData,
                    part.gameObject.transform.position, part.gameObject.transform.rotation));
            });

            _robotData.PartTransformData = parts.ToArray();

            MixAndMatchSaveUtil.SaveRobotData(_robotData);
        }

        public override void Update() {
            if (_selectingNode)
                NodeSelection();
            else
                PartPlacement();
        }

        public override void Delete() {
            Object.Destroy(_robotGameObject);
        }
    }

    /// <summary>Contains a parts GUID to reference during node selection</summary>
    public class PartGuidHolder : MonoBehaviour {
        [HideInInspector]
        public string Guid;
    }
}