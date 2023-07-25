using System;
using System.Collections.Generic;
using System.Linq;
using SimObjects.MixAndMatch;
using Synthesis.Gizmo;
using Synthesis.UI.Dynamic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UI.Dynamic.Panels.MixAndMatch
{
    public class PartEditorPanel : PanelDynamic
    {
        private const float MODAL_WIDTH = 500f;
        private const float MODAL_HEIGHT = 600f;

        private const float VERTICAL_PADDING = 16f;
        private const float HORIZONTAL_PADDING = 16f;
        private const float SCROLLBAR_WIDTH = 10f;
        private const float BUTTON_WIDTH = 64f;
        private const float ROW_HEIGHT = 64f;

        private MixAndMatchPartData _partData;

        private GameObject _partGameObject;
        private List<GameObject> _connectionGameObjects = new();

        private float _scrollViewWidth;
        private float _entryWidth;

        private ScrollView _snapPointScrollView;
        
        private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) =>
        {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0, rightPadding: 0);
            return u;
        };

        private readonly Func<UIComponent, UIComponent> ListVerticalLayout = (u) =>
        {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(
                anchoredY: offset, leftPadding: HORIZONTAL_PADDING, rightPadding: HORIZONTAL_PADDING);
            return u;
        };

        public PartEditorPanel(MixAndMatchPartData partData) : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT))
        {
            _partData = partData;
        }
        
        public override bool Create()
        {
            Title.SetText("Part Editor");
            
            AcceptButton.StepIntoLabel(l => l.SetText("Close"))
                .AddOnClickedEvent(b => {
                    SavePartData();
                    DynamicUIManager.ClosePanel<PartEditorPanel>();
                });
            CancelButton.RootGameObject.SetActive(false);

            _snapPointScrollView = MainContent.CreateScrollView()
                .SetRightStretch<ScrollView>()
                .ApplyTemplate(VerticalLayout)
                .SetHeight<ScrollView>(MODAL_HEIGHT - VERTICAL_PADDING * 2 - 50);
            _scrollViewWidth = _snapPointScrollView.Parent!.RectOfChildren().width - SCROLLBAR_WIDTH;
            _entryWidth = _scrollViewWidth - HORIZONTAL_PADDING * 2;

            var addPointButton = MainContent.CreateButton()
                .SetTopStretch<Button>()
                .StepIntoLabel(l => l.SetText("Add Snap Point"))
                .AddOnClickedEvent(_ => {
                    throw new NotImplementedException();
                })
                .ApplyTemplate(VerticalLayout);

            _partGameObject = InstantiatePartGameObject();
            CreateConnectionPoints();
            
            return true;
        }

        private void CreateConnectionPoints()
        {
            _snapPointScrollView.Content.DeleteAllChildren();
            
            _partData.ConnectionPoints.ForEach(point => {
                var pointGameObject = InstantiatePointGameObject(point);
                
                _connectionGameObjects.Add(pointGameObject);
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

            return gameObject;
        }

        private void AddPoint(GameObject point)
        {
            (Content leftContent, Content rightContent) =
                _snapPointScrollView.Content.CreateSubContent(new Vector2(_entryWidth, ROW_HEIGHT))
                    .ApplyTemplate(ListVerticalLayout)
                    .SplitLeftRight(BUTTON_WIDTH, HORIZONTAL_PADDING);
            
            leftContent.SetBackgroundColor<Content>(Color.blue);

            (Content labelsContent, Content buttonsContent) =
                rightContent.SplitLeftRight(_entryWidth - (HORIZONTAL_PADDING + BUTTON_WIDTH) * 3, HORIZONTAL_PADDING);
            
            (Content topContent, Content bottomContent) = labelsContent.SplitTopBottom(ROW_HEIGHT / 2, 0);
            topContent.CreateLabel()
                .SetText("Name")
                .ApplyTemplate(VerticalLayout)
                .SetAnchorLeft<Label>()
                .SetAnchoredPosition<Label>(new Vector2(0, -ROW_HEIGHT / 8));

            bottomContent.CreateLabel()
                .SetText($"Points")
                .ApplyTemplate(VerticalLayout)
                .SetAnchorLeft<Label>()
                .SetAnchoredPosition<Label>(new Vector2(0, -ROW_HEIGHT / 8));

            (Content editButtonContent, Content deleteButtonContent) =
                buttonsContent.SplitLeftRight(BUTTON_WIDTH, HORIZONTAL_PADDING);
            editButtonContent.CreateButton()
                .StepIntoLabel(l => l.SetText("Move"))
                .AddOnClickedEvent(b => { 
                    GizmoManager.ExitGizmo();
                    GizmoManager.SpawnGizmo(point.transform,
                        t => {
                            point.transform.position = t.Position;
                            point.transform.rotation = t.Rotation;
                        },
                        t => {});
                })
                .ApplyTemplate(VerticalLayout)
                .SetSize<Button>(new Vector2(BUTTON_WIDTH, ROW_HEIGHT))
                .SetStretch<Button>();
            deleteButtonContent.CreateButton()
                .StepIntoLabel(l => l.SetText("Delete"))
                .AddOnClickedEvent(b =>
                { })
                .ApplyTemplate(VerticalLayout)
                .SetSize<Button>(new Vector2(BUTTON_WIDTH, ROW_HEIGHT))
                .SetStretch<Button>();
        }

        public override void Update()
        {

        }

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