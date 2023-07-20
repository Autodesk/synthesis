using System;
using Org.BouncyCastle.Asn1.Esf;
using SimObjects.MixAndMatch;
using Synthesis.Gizmo;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Dynamic.Panels.Spawning.MixAndMatch
{
    public class PartConfigPanel : PanelDynamic
    {
        private const float MODAL_WIDTH = 500f;
        private const float MODAL_HEIGHT = 600f;

        private const float VERTICAL_PADDING = 16f;
        private const float HORIZONTAL_PADDING = 16f;
        private const float SCROLLBAR_WIDTH = 10f;
        private const float BUTTON_WIDTH = 64f;
        private const float ROW_HEIGHT = 64f;
        
        private static readonly int _snapPointLayerMask = 1 << LayerMask.NameToLayer("ConnectionPoint");

        private float _scrollViewWidth;
        private float _entryWidth;

        private ScrollView _snapPointScrollView;

        private PartEditorPart _partEditorPart;
        
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

        public PartConfigPanel(PartEditorPart partEditorPart) : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT))
        {
            _partEditorPart = partEditorPart;
        }
        
        public override bool Create()
        {
            Title.SetText("Part Config");
            
            _partEditorPart.ConnectionPoints.ForEach(x => x.GetComponent<Collider>().enabled = false);

            AcceptButton.StepIntoLabel(l => l.SetText("Close"))
                .AddOnClickedEvent(b =>
                {
                    DynamicUIManager.ClosePanel<PartConfigPanel>();
                    GizmoManager.ExitGizmo();
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
                .AddOnClickedEvent(
                    _ =>
                    {
                        _partEditorPart.AddConnectionPoint();
                        AddAllPoints();
                    })
                .ApplyTemplate(VerticalLayout);

            AddAllPoints();
            
            return true;
        }

        private void AddAllPoints()
        {
            _snapPointScrollView.Content.DeleteAllChildren();
            foreach (var point in _partEditorPart.ConnectionPoints)
            {
                AddPoint(point);
            }
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
            if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButton(1)) {
                // Raycast out from camera to see where the mouse is pointing
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100, _snapPointLayerMask))
                {
                    _partEditorPart.Transform.position = hit.transform.position;

                    _partEditorPart.Transform.rotation = Quaternion.LookRotation(-hit.transform.forward, Vector3.up);
                    _partEditorPart.Transform.Rotate(-_partEditorPart.ConnectionPoints[0].transform.localRotation.eulerAngles);
                    //Debug.Log($"Rotation: {-hit.transform.forward} and {Quaternion.LookRotation(-hit.transform.forward, Vector3.up)}");
                    _partEditorPart.Transform.Translate(-_partEditorPart.ConnectionPoints[0].transform.localPosition);

                    _partEditorPart.ConnectedPartEditorPart = hit.transform.GetComponent<ConnectionPointReference>().part;
                }
            }
        }

        public override void Delete()
        {
            _partEditorPart.ConnectionPoints.ForEach(x => x.GetComponent<Collider>().enabled = true);
        }
    }
}