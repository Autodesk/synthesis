using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Modes.MatchMode;
using SimObjects.MixAndMatch;
using Synthesis.Gizmo;
using Synthesis.Physics;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Dynamic.Panels.Spawning
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

        private float _scrollViewWidth;
        private float _entryWidth;

        private ScrollView _snapPointScrollView;

        private MixAndMatchPart _part;
        
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

        public PartConfigPanel(MixAndMatchPart part) : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT))
        {
            _part = part;
        }
        
        private static readonly int _snapPointLayerMask = 1 << LayerMask.NameToLayer("SnapPoint");
        
        public override bool Create()
        {
            Title.SetText("Part Config");
            
            _part.SnapPoints.ForEach(x => x.GetComponent<Collider>().enabled = false);

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
                        GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        point.GetComponent<Collider>().isTrigger = true;
                        point.transform.localScale = Vector3.one * .5f;
                        point.transform.SetParent(_part.Transform);
                        point.layer = LayerMask.NameToLayer("SnapPoint");
                        point.GetComponent<Collider>().enabled = false;
                        
                        _part.SnapPoints.Add(point);
                        AddAllPoints();
                    })
                .ApplyTemplate(VerticalLayout);

            AddAllPoints();
            
            return true;
        }

        private void AddAllPoints()
        {
            _snapPointScrollView.Content.DeleteAllChildren();
            foreach (var point in _part.SnapPoints)
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
                    _part.Transform.position = hit.transform.position;

                    _part.Transform.rotation = Quaternion.LookRotation(-hit.transform.forward, Vector3.up);
                    _part.Transform.Rotate(-_part.SnapPoints[0].transform.localRotation.eulerAngles);
                    //Debug.Log($"Rotation: {-hit.transform.forward} and {Quaternion.LookRotation(-hit.transform.forward, Vector3.up)}");
                    _part.Transform.Translate(-_part.SnapPoints[0].transform.localPosition);

                    _part.ConnectedPoint = hit.transform;
                }
            }
        }

        public override void Delete()
        {
            _part.SnapPoints.ForEach(x => x.GetComponent<Collider>().enabled = true);
        }
    }
}