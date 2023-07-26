using System;
using System.Collections.Generic;
using System.Linq;
using SimObjects.MixAndMatch;
using Synthesis.Gizmo;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UI.Dynamic.Panels.MixAndMatch
{
    public class RobotEditorPanel : PanelDynamic
    {
        private const float PANEL_WIDTH = 400f;
        private const float PANEL_HEIGHT = 400f;

        private const float VERTICAL_PADDING = 7f;
        private const float HORIZONTAL_PADDING = 16f;

        private MixAndMatchRobotData _robotData;

        private GameObject _robotGameObject;
        private readonly List<GameObject> _partGameObjects = new();

        private float _scrollViewWidth;
        private float _entryWidth;

        private ScrollView _scrollView;

        private Button _removeButton;

        private GameObject _selectedPart = null;
        
        // TODO: Remove and replace with the vert layout in dynamic components after merge
        private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) =>
        {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0, rightPadding: 0);
            return u;
        };
        
        // TODO: After merge move this to dynamic components
        public Func<UIComponent, UIComponent> RadioToggleLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin);
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 15f, rightPadding: 15f); // used to be 15f
            return u;
        };

        // TODO: After merge move this to dynamic components
        private readonly Func<UIComponent, UIComponent> ListVerticalLayout = (u) =>
        {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(
                anchoredY: offset, leftPadding: HORIZONTAL_PADDING, rightPadding: HORIZONTAL_PADDING);
            return u;
        };
        
        // TODO: same
        public Func<Button, Button> EnableButton = b =>
            b.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE))
                .StepIntoLabel(l => l.SetColor(ColorManager.SYNTHESIS_ORANGE_CONTRAST_TEXT))
                .EnableEvents<Button>();

        // TODO: same
        public Func<Button, Button> DisableButton = b =>
            b.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK_ACCENT))
                .StepIntoLabel(l => l.SetColor(ColorManager.SYNTHESIS_ORANGE_CONTRAST_TEXT))
                .DisableEvents<Button>();

        public RobotEditorPanel(MixAndMatchRobotData robotData) : base(new Vector2(PANEL_WIDTH, PANEL_HEIGHT))
        {
            _robotData = robotData;
        }
        
        public override bool Create()
        {
            Title.SetText("Robot Editor");
            
            AcceptButton.StepIntoLabel(l => l.SetText("Save"))
                .AddOnClickedEvent(b => {
                    SaveRobotData();
                    GizmoManager.ExitGizmo();
                    DynamicUIManager.ClosePanel<RobotEditorPanel>();
                });
            CancelButton.RootGameObject.SetActive(false);
            
            _scrollView = MainContent.CreateScrollView().SetStretch<ScrollView>(bottomPadding: 60f);

            CreateAddRemoveButtons();

            _robotGameObject = InstantiateRobotGameObject();
            
            InstantiatePartGameObjects();
            PopulateScrollView();
            
            return true;
        }

        private void CreateAddRemoveButtons() {
            (Content left, Content right) = MainContent.CreateSubContent(new Vector2(400, 50))
                .SetBottomStretch<Content>()
                .SplitLeftRight((PANEL_WIDTH - 10f) / 2f, 10f);

            var addButton = left.CreateButton("Add").SetStretch<Button>().AddOnClickedEvent(_ => {
                // TODO: Prompt the user to pick a part from files
                AddScrollViewEntry(InstantiatePartGameObject(("abc", Vector3.zero, Quaternion.identity)));
                UpdateRemoveButton();
            });
            
            _removeButton = right.CreateButton("Remove").SetStretch<Button>().AddOnClickedEvent(_ => {
                if (_selectedPart != null) _partGameObjects.Remove(_selectedPart);
                PopulateScrollView();
                GizmoManager.ExitGizmo();
                _selectedPart = null;
                UpdateRemoveButton();
            });
            UpdateRemoveButton();
        }

        private void PopulateScrollView()
        {
            _scrollView.Content.DeleteAllChildren();
            
            _partGameObjects.ForEach(partGameObject => {
                AddScrollViewEntry(partGameObject);
            });
        }
        
        private void InstantiatePartGameObjects() {
            if (_robotData.Parts == null)
                return;
            
            _robotData.Parts.ForEach(part => {
                InstantiatePartGameObject(part);
            });
        }

        private GameObject InstantiateRobotGameObject() {
            return new GameObject("Mix and Match Robot Editor Parent");
        }

        private GameObject InstantiatePartGameObject((string fileName, Vector3 localPosition, Quaternion localRotation) part) {
            var partData = MixAndMatchSaveUtil.LoadPartData(part.fileName);
            
            // TODO: spawn part mirabuf instead of cube
            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.name = part.fileName;
            gameObject.transform.SetParent(_robotGameObject.transform);

            gameObject.transform.position = part.localPosition;
            gameObject.transform.rotation = part.localRotation;

            _partGameObjects.Add(gameObject);
            return gameObject;
        }

        private void AddScrollViewEntry(GameObject part)
        {
            var toggle = _scrollView.Content
                .CreateToggle(label: part.name)
                .SetSize<Toggle>(new Vector2(PANEL_WIDTH, 50f))
                .ApplyTemplate(RadioToggleLayout)
                .StepIntoLabel(l => l.SetFontSize(16f))
                .SetDisabledColor(ColorManager.SYNTHESIS_BLACK);
            toggle.AddOnStateChangedEvent((t, s) => {
                SelectPart(part, t, s);
            });
        }

        private void SelectPart(GameObject part, Toggle toggle, bool state) {
            if (state) {
                _selectedPart = part;
                GizmoManager.SpawnGizmo(part.transform,
                    t => {
                        part.transform.position = t.Position;
                        part.transform.rotation = t.Rotation;
                    },
                    _ => { });
                
                _scrollView.Content.ChildrenReadOnly.OfType<Toggle>().ForEach(x => {
                    x.SetStateWithoutEvents(false);
                });
                toggle.SetStateWithoutEvents(true);
            }
            else {
                _selectedPart = null;
                GizmoManager.ExitGizmo();
            }
            UpdateRemoveButton();
        }

        private void UpdateRemoveButton() {
            _removeButton.ApplyTemplate((_partGameObjects.Count > 0 && _selectedPart != null) ? EnableButton : DisableButton);
        }

        public override void Update() { }

        public override void Delete()
        {
            Object.Destroy(_robotGameObject);
        }

        private void SaveRobotData() {
            List<(string fileName, Vector3 localPosition, Quaternion localRotation)> parts = new();
            _partGameObjects.ForEach(part => {
                parts.Add((part.name, part.transform.position, part.transform.rotation));
            });

            _robotData.Parts = parts.ToArray();
            Debug.Log(_robotData.Parts.Length);
            
            MixAndMatchSaveUtil.SaveRobotData(_robotData);
        }
    }
}