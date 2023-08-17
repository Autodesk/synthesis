using System;
using System.Collections.Generic;
using System.Linq;
using Mirabuf.Material;
using SimObjects.MixAndMatch;
using Synthesis.Import;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Utilities;
using UI.Dynamic.Modals.MixAndMatch;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities;
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

        private const float PART_ROTATION_SPEED = 3.75f;

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
        private readonly List<LocalPartData> _parts = new();
        private LocalPartData? _selectedPart;

        private bool _creationFailed;
        private bool _selectingNode;

        public RobotEditorPanel(MixAndMatchRobotData robotData) : base(new Vector2(PANEL_WIDTH, PANEL_HEIGHT)) {
            _robotData = robotData;
        }

        public override bool Create() {
            SceneHider.IsHidden = true;

            RobotSimObject.CurrentlyPossessedRobot = string.Empty;
            MainHUD.SelectedRobot                    = null;

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

            OrbitCameraMode.FocusPoint = () => Vector3.zero;

            // Center the robot in the world
            var centerOffset = _robotGameObject.transform.localToWorldMatrix.MultiplyPoint(
                _robotGameObject.transform.GetBounds().center);
            _robotGameObject.transform.position = -centerOffset;

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
                    DynamicUIManager.CreateModal<SelectPartModal>(args: new Action<GlobalPartData>(AddAdditionalPart));
                });

            _removeButton = remove.CreateButton("Remove").SetStretch<Button>().AddOnClickedEvent(
                _ => DynamicUIManager.CreateModal<RemovePartModal>(args: new Action(RemovePartCallback)));
            UpdateRemoveButton();

            parent.CreateButton("Parent").SetStretch<Button>().AddOnClickedEvent(
                _ => _selectingNode = !_selectingNode);

            void RemovePartCallback() {
                if (_selectedPart == null)
                    return;

                _parts.Remove(_selectedPart);
                Object.Destroy(_selectedPart.EditorPartInfo.GameObject);
                _selectedPart = null;

                PopulateScrollView();
                UpdateRemoveButton();
            }
        }

        /// <summary>Creates all of the part objects when the panel is first opened</summary>
        private void InstantiatePartGameObjects() {
            if (_robotData.PartTransformData == null)
                return;

            _robotData.PartTransformData.ForEachIndex((i, p) => {
                p.EditorPartInfo.GlobalPartData = _robotData.GlobalPartData[i];
                InstantiatePartGameObject(p);
            });
        }

        /// <summary>Instantiates a part object to position from a partData object</summary>
        private void InstantiatePartGameObject(LocalPartData localPartData) {
            var loadPartData = MixAndMatchSaveUtil.LoadPartData(localPartData.FileName, false);
            if (loadPartData == null) {
                Logger.Log($"Part file \"{localPartData.FileName}\" not found!", LogLevel.Error);
                _creationFailed = true;
                return;
            }

            MirabufLive miraLive = new MirabufLive(loadPartData.MirabufPartFile);

            GameObject partObj = new GameObject(loadPartData.Name);
            Transform partTrf  = partObj.transform;

            miraLive.GenerateDefinitionObjects(partObj, dynamicLayer: _robotLayer);

            partTrf.SetParent(_robotGameObject.transform);
            partTrf.localPosition = localPartData.LocalPosition;
            partTrf.localRotation = localPartData.LocalRotation;

            _parts.Add(localPartData);

            partTrf.GetComponentsInChildren<Rigidbody>().ForEach(x => {
                var rc     = x.gameObject.AddComponent<HighlightComponent>();
                rc.Color   = ColorManager.GetColor(ColorManager.SynthesisColor.HighlightHover);
                rc.enabled = false;

                x.isKinematic = true;
            });

            partObj.AddComponent<PartGuidHolder>().Guid = loadPartData.Guid;

            InstantiatePartConnectionPoints(partObj, loadPartData);

            localPartData.EditorPartInfo.GameObject = partObj;
        }

        /// <summary>Instantiates objects for all the connection points of a part</summary>
        private void InstantiatePartConnectionPoints(GameObject partGameObject, GlobalPartData globalPartData) {
            globalPartData.ConnectionPoints.ForEachIndex((_, connection) => {
                var connectionObj   = Object.Instantiate(SynthesisAssetCollection.Instance.MixAndMatchConnectionPrefab);
                connectionObj.layer = _connectionLayer;
                connectionObj.name  = "Connection Point";

                var connectionTrf = connectionObj.transform;

                connectionTrf.SetParent(partGameObject.transform);
                connectionTrf.localPosition = connection.LocalPosition;
                connectionTrf.localRotation = connection.LocalRotation;

                var color = ColorManager.GetColor(ColorManager.SynthesisColor.HighlightHover);
                color.a   = 0.5f;

                Material mat = new Material(Appearance.DefaultTransparentShader);
                mat.SetColor(Appearance.TRANSPARENT_COLOR, color);
                mat.SetFloat(Appearance.TRANSPARENT_SMOOTHNESS, 0);
                connectionTrf.Find("Sphere").GetComponent<MeshRenderer>().material = mat;
            });
        }

        /// <summary>Adds an addition part to the robot anytime during editing</summary>
        private void AddAdditionalPart(GlobalPartData globalPartData) {
            EnableConnectionColliders(_selectedPart?.EditorPartInfo.GameObject);

            var localPartData = new LocalPartData(globalPartData.Name, null, Vector3.zero, Quaternion.identity,
                new RobotEditorPartInfo()) { EditorPartInfo = { GlobalPartData = globalPartData } };

            InstantiatePartGameObject(localPartData);

            AddScrollViewEntry(localPartData);
            UpdateRemoveButton();
        }

        /// <summary>Clears the scroll view then repopulates it with all the current parts</summary>
        private void PopulateScrollView() {
            _scrollView.Content.DeleteAllChildren();

            _parts.ForEach(AddScrollViewEntry);
        }

        /// <summary>Adds an entry to the scroll view</summary>
        private void AddScrollViewEntry(LocalPartData localPart) {
            var toggle =
                _scrollView.Content.CreateToggle(label: localPart.EditorPartInfo.GameObject.name, radioSelect: true)
                    .SetSize<Toggle>(new Vector2(PANEL_WIDTH, 50f))
                    .ApplyTemplate(Toggle.RadioToggleLayout)
                    .StepIntoLabel(l => l.SetFontSize(16f))
                    .SetDisabledColor(ColorManager.SynthesisColor.Background);
            toggle.AddOnStateChangedEvent((t, s) => { SelectPart(localPart, t, s); });
        }

        /// <summary>Selects which part will be edited, and updates radio select buttons accordingly</summary>
        private void SelectPart(LocalPartData localPart, Toggle toggle, bool state) {
            if (state) {
                _scrollView.Content.ChildrenReadOnly.OfType<Toggle>().ForEach(x => { x.SetStateWithoutEvents(false); });
                toggle.SetStateWithoutEvents(true);

                _parts.ForEach(x => EnableConnectionColliders(x.EditorPartInfo.GameObject));
                DisableConnectionColliders(localPart.EditorPartInfo.GameObject);

                _selectedPart = localPart;
            } else
                _selectedPart = null;

            _hoveringNode = null;

            if (_selectedNode != null) {
                _selectedNode.enabled = false;
                _selectedNode         = null;
            }
            _connectedTransform = null;
            UpdateRemoveButton();
        }

        /// <summary>Enables the colliders of a part's connection points so other parts can snap to it</summary>
        private void EnableConnectionColliders(GameObject partObject) {
            if (partObject == null)
                return;

            partObject.GetComponentsInChildren<SphereCollider>(true).ForEach(c => c.enabled = true);
        }

        /// <summary>Disables the colliders of a part's connection points so other parts can't snap to it</summary>
        private void DisableConnectionColliders(GameObject partObject) {
            if (partObject == null)
                return;

            partObject.GetComponentsInChildren<SphereCollider>().ForEach(c => c.enabled = false);
        }

        /// Updates the remove button based on if a part is selected
        private void UpdateRemoveButton() {
            _removeButton.ApplyTemplate(
                (_parts.Count > 0 && _selectedPart != null) ? Button.EnableButton : Button.DisableButton);
        }

        private Transform _connectedTransform;

        /// <summary>Handles raycasts to find connection points and part rotation</summary>
        private void PartPlacement() {
            if (EventSystem.current.IsPointerOverGameObject() || _selectedPart == null)
                return;

            var selectedPartGlobalData = _selectedPart.EditorPartInfo.GlobalPartData;
            var selectedTrf            = _selectedPart.EditorPartInfo.GameObject.transform;

            // Handle rotation about the connection points normals
            if (Input.GetKey(KeyCode.R))
                _selectedPart.EditorPartInfo.Rotation +=
                    Time.deltaTime * PART_ROTATION_SPEED * (Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

            // Tab to cycle between connection points
            if (Input.GetKeyDown(KeyCode.Tab))
                _selectedPart.EditorPartInfo.ConnectedPoint =
                    (_selectedPart.EditorPartInfo.ConnectedPoint + 1) % selectedPartGlobalData.ConnectionPoints.Length;

            // Raycast to check if the mouse is pointing at a connection point
            Ray ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100, _connectionLayerMask)) {
                _connectedTransform = hit.transform;
            }

            if (_connectedTransform == null)
                return;

            int connectionIndex = _selectedPart.EditorPartInfo.ConnectedPoint;

            // Align the connection points normals
            var trf              = _connectedTransform.transform;
            selectedTrf.position = trf.position;
            selectedTrf.rotation = Quaternion.LookRotation(-trf.forward, Vector3.up);
            selectedTrf.Rotate(-selectedPartGlobalData.ConnectionPoints[connectionIndex].LocalRotation.eulerAngles);

            // Apply the user specified rotation
            Vector3 axis = selectedTrf.localToWorldMatrix.rotation *
                           (selectedPartGlobalData.ConnectionPoints[connectionIndex].LocalRotation * Vector3.forward);
            selectedTrf.RotateAround(axis, _selectedPart.EditorPartInfo.Rotation);

            // Offset so that the connection points are overlapping
            selectedTrf.Translate(-selectedPartGlobalData.ConnectionPoints[connectionIndex].LocalPosition);
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

                    int selectedPartIndex = _parts.FindIndex(x => x.Equals(_selectedPart));

                    var localPart            = _parts[selectedPartIndex];
                    localPart.ParentNodeData = new ParentNodeData(
                        _selectedNode.transform.parent.GetComponent<PartGuidHolder>().Guid, _selectedNode.name);
                    _parts[selectedPartIndex] = localPart;
                }
            } else {
                if (_hoveringNode == null)
                    return;

                _hoveringNode.enabled = false;
                _hoveringNode         = null;
            }
        }

        /// <summary>Saves all edits that have been made</summary>
        private void SaveRobotData() {
            List<LocalPartData> parts = new();
            _parts.ForEach(part => {
                parts.Add(new LocalPartData(part.EditorPartInfo.GameObject.name, part.ParentNodeData,
                    part.EditorPartInfo.GameObject.transform.localPosition,
                    part.EditorPartInfo.GameObject.transform.localRotation, part.EditorPartInfo));
            });

            _robotData.PartTransformData = parts.ToArray();

            MixAndMatchSaveUtil.SaveRobotData(_robotData);
        }

        public override void Update() {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (_selectingNode)
                NodeSelection();
            else
                PartPlacement();
        }

        public override void Delete() {
            Object.Destroy(_robotGameObject);
            SceneHider.IsHidden = false;
        }
    }

    /// <summary>Contains a parts GUID to reference during node selection</summary>
    public class PartGuidHolder : MonoBehaviour {
        [HideInInspector]
        public string Guid;
    }
}