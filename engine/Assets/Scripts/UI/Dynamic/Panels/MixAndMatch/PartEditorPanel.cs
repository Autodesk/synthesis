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
    public class PartEditorPanel : PanelDynamic
    {
        private const float PANEL_WIDTH = 400f;
        private const float PANEL_HEIGHT = 400f;

        private const float VERTICAL_PADDING = 7f;
        private const float HORIZONTAL_PADDING = 16f;
        private const float SCROLLBAR_WIDTH = 10f;
        private const float BUTTON_WIDTH = 64f;
        private const float ROW_HEIGHT = 64f;

        private MixAndMatchPartData _partData;

        private GameObject _partGameObject;
        private readonly List<GameObject> _connectionGameObjects = new();

        private float _scrollViewWidth;
        private float _entryWidth;

        private ScrollView _scrollView;

        private Button _addButton;
        private Button _removeButton;
        
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

        public PartEditorPanel(MixAndMatchPartData partData) : base(new Vector2(PANEL_WIDTH, PANEL_HEIGHT))
        {
            _partData = partData;
        }
        
        public override bool Create()
        {
            Title.SetText("Part Editor");
            
            AcceptButton.StepIntoLabel(l => l.SetText("Save"))
                .AddOnClickedEvent(b => {
                    SavePartData();
                    GizmoManager.ExitGizmo();
                    DynamicUIManager.ClosePanel<PartEditorPanel>();
                });
            CancelButton.RootGameObject.SetActive(false);
            
            _scrollView = MainContent.CreateScrollView().SetStretch<ScrollView>(bottomPadding: 60f);
            
            var addPointButton = MainContent.CreateButton()
                .SetTopStretch<Button>()
                .StepIntoLabel(l => l.SetText("Add Snap Point"))
                .AddOnClickedEvent(_ => {
                    AddPoint(InstantiatePointGameObject(new ConnectionPointData()));
                })
                .ApplyTemplate(VerticalLayout);

            _partGameObject = InstantiatePartGameObject();
            CreateConnectionPoints();
            
            return true;
        }

        private void CreateButtons() {
            (Content left, Content right) = MainContent.CreateSubContent(new Vector2(400, 50))
                .SetBottomStretch<Content>()
                .SplitLeftRight((PANEL_WIDTH - 10f) / 2f, 10f);

                _addButton = left.CreateButton("Add").SetStretch<Button>().AddOnClickedEvent(
                    b => { DynamicUIManager.CreateModal<AddRobotModal>(); });
                _removeButton = right.CreateButton("Remove").SetStretch<Button>().AddOnClickedEvent(b => {
                    RobotSimObject.RemoveRobot(RobotSimObject.CurrentlyPossessedRobot);
                    PopulateScrollView();
                    if (RobotSimObject.SpawnedRobots.Count < RobotSimObject.MAX_ROBOTS)
                        _addButton.ApplyTemplate<Button>(EnableButton);
                });
        }

        private void CreateConnectionPoints()
        {
            //_scrollView.Content.DeleteAllChildren();
            
            _partData.ConnectionPoints.ForEach(point => {
                var pointGameObject = InstantiatePointGameObject(point);
                
                AddPoint(pointGameObject);
            });
        }

        private GameObject InstantiatePartGameObject() {
            return GameObject.CreatePrimitive(PrimitiveType.Cube);
        }

        private GameObject InstantiatePointGameObject(ConnectionPointData point) {
            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.name = "ConnectionPoint";
            gameObject.transform.SetParent(_partGameObject.transform);

            gameObject.transform.position = point.Position;
            gameObject.transform.forward = point.Normal;
            gameObject.transform.localScale = Vector3.one / 2f;
            
            _connectionGameObjects.Add(gameObject);
            return gameObject;
        }

        private void AddPoint(GameObject point)
        {
            var toggle = _scrollView.Content
                .CreateToggle(label: "Connection Point")
                .SetSize<Toggle>(new Vector2(PANEL_WIDTH, 50f))
                .ApplyTemplate(RadioToggleLayout)
                .StepIntoLabel(l => l.SetFontSize(16f))
                .SetDisabledColor(ColorManager.SYNTHESIS_BLACK);
            toggle.AddOnStateChangedEvent((t, s) => {
                SelectConnectionPoint(point, t, s);
            });
        }

        private void SelectConnectionPoint(GameObject point, Toggle toggle, bool state) {
            if (state) {
                GizmoManager.SpawnGizmo(point.transform,
                    t => {
                        point.transform.position = t.Position;
                        point.transform.rotation = t.Rotation;
                    },
                    _ => { });
                
                _scrollView.Content.ChildrenReadOnly.OfType<Toggle>().ForEach(x => {
                    x.SetStateWithoutEvents(false);
                });
                toggle.SetStateWithoutEvents(true);
            }
            else {
                GizmoManager.ExitGizmo();
            }
        }

        public override void Update() { }

        public override void Delete()
        {
            Object.Destroy(_partGameObject);
        }

        private void SavePartData() {
            List<ConnectionPointData> connectionPoints = new();
            _connectionGameObjects.ForEach(point => {
                connectionPoints.Add(new ConnectionPointData(point.transform.position, point.transform.forward));
            });

            _partData.ConnectionPoints = connectionPoints.ToArray();
            
            MixAndMatchSaveUtil.SavePartData(_partData);
        }
    }
}