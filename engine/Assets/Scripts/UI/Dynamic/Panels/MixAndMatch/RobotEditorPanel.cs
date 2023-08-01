using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimObjects.MixAndMatch;
using Synthesis.Import;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Utilities;
using UI.Dynamic.Modals.MixAndMatch;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.ColorManager;
using Logger  = SynthesisAPI.Utilities.Logger;
using Object  = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;

namespace UI.Dynamic.Panels.MixAndMatch {
    public class RobotEditorPanel : PanelDynamic {
        private const float PANEL_WIDTH  = 400f;
        private const float PANEL_HEIGHT = 400f;

        private const float PART_ROTATION_SPEED = 10f;

        private static readonly int _connectionLayer     = LayerMask.NameToLayer("ConnectionPoint");
        private static readonly int _connectionLayerMask = 1 << _connectionLayer;

        private float _scrollViewWidth;
        private float _entryWidth;

        private ScrollView _scrollView;
        private Button _removeButton;

        private readonly MixAndMatchRobotData _robotData;

        private GameObject _robotGameObject;
        private readonly List<(GameObject gameObject, MixAndMatchPartData partData)> _partGameObjects = new();
        private (GameObject gameObject, MixAndMatchPartData partData)? _selectedPart;

        public RobotEditorPanel(MixAndMatchRobotData robotData) : base(new Vector2(PANEL_WIDTH, PANEL_HEIGHT)) {
            _robotData = robotData;
        }

        private bool _creationFailed;

        public override bool Create() {
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

            return true;
        }

        /// <summary>Creates the buttons to add and remove parts from the robot assembly</summary>
        private void CreateAddRemoveButtons() {
            (Content left, Content right) = MainContent.CreateSubContent(new Vector2(400, 50))
                                                .SetBottomStretch<Content>()
                                                .SplitLeftRight((PANEL_WIDTH - 10f) / 2f, 10f);

            left.CreateButton("Add").SetStretch<Button>().AddOnClickedEvent(
                _ => {
                    DynamicUIManager.CreateModal<SelectPartModal>(
                        args: new Action<MixAndMatchPartData>(AddAdditionalPart));
                });

            _removeButton = right.CreateButton("Remove").SetStretch<Button>().AddOnClickedEvent(
                _ => DynamicUIManager.CreateModal<RemovePartModal>(args: new Action(RemovePartCallback)));
            UpdateRemoveButton();

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
            if (_robotData.PartData == null)
                return;

            _robotData.PartData.ForEach(InstantiatePartGameObject);
        }

        /// <summary>Instantiates a part object to position from a part fileName</summary>
        private void InstantiatePartGameObject(
            (string fileName, Vector3 localPosition, Quaternion localRotation) partData) {
            InstantiatePartGameObject(
                partData.localPosition, partData.localRotation, MixAndMatchSaveUtil.LoadPartData(partData.fileName));
        }

        /// <summary>Instantiates a part object to position from a partData object</summary>
        private GameObject InstantiatePartGameObject(
            Vector3 localPosition, Quaternion localRotation, MixAndMatchPartData partData) {
            if (!File.Exists(partData.MirabufPartFile)) {
                Logger.Log($"Part file {partData.MirabufPartFile} not found!", LogLevel.Error);
                _creationFailed = true;
                return null;
            }

            MirabufLive miraLive = new MirabufLive(partData.MirabufPartFile);

            GameObject obj = new GameObject(partData.Name);
            Transform trf  = obj.transform;

            miraLive.GenerateDefinitionObjects(obj, false);

            trf.SetParent(_robotGameObject.transform);

            trf.position = localPosition;
            trf.rotation = localRotation;

            _partGameObjects.Add((obj, partData));

            InstantiatePartConnectionPoints(obj, partData);
            return obj;
        }

        /// <summary>Instantiates objects for all the connection points of a part</summary>
        private void InstantiatePartConnectionPoints(GameObject partGameObject, MixAndMatchPartData partData) {
            partData.ConnectionPoints.ForEachIndex((_, connection) => {
                var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                var trf = obj.transform;

                trf.SetParent(partGameObject.transform);
                trf.localPosition = connection.LocalPosition;
                trf.localRotation = connection.LocalRotation;
                trf.localScale    = Vector3.one * 0.25f;

                obj.layer = _connectionLayer;
                obj.name  = "Connection Point";

                trf.GetComponent<MeshRenderer>().material.color =
                    ColorManager.GetColor(ColorManager.SynthesisColor.HighlightHover);

                if (trf.TryGetComponent<SphereCollider>(out var collider)) {
                    collider.isTrigger = true;
                    collider.radius    = 1f;
                }
            });
        }

        /// <summary>Adds an addition part to the robot anytime during editing</summary>
        private void AddAdditionalPart(MixAndMatchPartData part) {
            EnableConnectionColliders(_selectedPart?.gameObject);
            AddScrollViewEntry((InstantiatePartGameObject(Vector3.zero, Quaternion.identity, part), part));
            UpdateRemoveButton();
        }

        /// <summary>Clears the scroll view then repopulates it with all the current parts</summary>
        private void PopulateScrollView() {
            _scrollView.Content.DeleteAllChildren();

            _partGameObjects.ForEach(AddScrollViewEntry);
        }

        /// <summary>Adds an entry to the scroll view</summary>
        private void AddScrollViewEntry((GameObject gameObject, MixAndMatchPartData partData) part) {
            var toggle = _scrollView.Content.CreateToggle(label: part.gameObject.name, radioSelect: true)
                             .SetSize<Toggle>(new Vector2(PANEL_WIDTH, 50f))
                             .ApplyTemplate(Toggle.RadioToggleLayout)
                             .StepIntoLabel(l => l.SetFontSize(16f))
                             .SetDisabledColor(ColorManager.SynthesisColor.Background);
            toggle.AddOnStateChangedEvent((t, s) => { SelectPart(part, t, s); });
        }

        /// <summary>Selects which part will be edited, and updates radio select buttons accordingly</summary>
        private void SelectPart((GameObject gameObject, MixAndMatchPartData partData) part, Toggle toggle, bool state) {
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

        /// <summary>Deselects all radio select buttons</summary>
        private void DeselectSelectedPart() {
            _scrollView.Content.ChildrenReadOnly.OfType<Toggle>().ForEach(x => { x.SetStateWithoutEvents(false); });
            _selectedPart = null;
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

        // TODO: store separately for each part not globally
        private float _axisRotation;

        /// <summary>Handles raycasts to find connection points and part rotation</summary>
        private void PartPlacement() {
            if (EventSystem.current.IsPointerOverGameObject() || _selectedPart == null)
                return;

            if (Input.GetMouseButtonDown(0)) {
                DeselectSelectedPart();
                return;
            }

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
                
                // It says rotate around is obsolete but .Rotate didn't seem to work the same way
                selectedTrf.RotateAround(axis, _axisRotation);

                // Offset so that the connection points are overlapping
                selectedTrf.Translate(-selectedPartData.ConnectionPoints[0].LocalPosition);
            }
        }

        /// <summary>Saves all edits that have been made</summary>
        private void SaveRobotData() {
            List<(string fileName, Vector3 localPosition, Quaternion localRotation)> parts = new();
            _partGameObjects.ForEach(part => {
                parts.Add(
                    (part.gameObject.name, part.gameObject.transform.position, part.gameObject.transform.rotation));
            });

            _robotData.PartData = parts.ToArray();

            MixAndMatchSaveUtil.SaveRobotData(_robotData);

            if (parts.Count > 0)
                RobotSimObject.SpawnRobot(_robotData);
        }

        public override void Update() {
            PartPlacement();
        }

        public override void Delete() {
            Object.Destroy(_robotGameObject);
        }
    }
}