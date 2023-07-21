using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimObjects.MixAndMatch;
using Synthesis.Physics;
using Synthesis.UI.Dynamic;
using UnityEngine;

namespace UI.Dynamic.Panels.Spawning.MixAndMatch {
    public class BuildRobotPanel : PanelDynamic {
        private const float MODAL_WIDTH = 500f;

        private const float MODAL_HEIGHT = 600f;

        private const float VERTICAL_PADDING = 16f;

        private const float HORIZONTAL_PADDING = 16f;

        private const float SCROLLBAR_WIDTH = 10f;

        private const float BUTTON_WIDTH = 64f;

        private const float ROW_HEIGHT = 64f;

        private float _scrollViewWidth;

        private float _entryWidth;

        private ScrollView _partsScrollView;

        private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0, rightPadding: 0);
            return u;
        };

        private readonly Func<UIComponent, UIComponent> ListVerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(
                anchoredY: offset, leftPadding: HORIZONTAL_PADDING, rightPadding: HORIZONTAL_PADDING);
            return u;
        };

        public BuildRobotPanel() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) { }

        public List<PartEditorPart> _parts = new();

        public override bool Create() {
            PhysicsManager.IsFrozen = true;

            Title.SetText("Mix & Match");

            AcceptButton.StepIntoLabel(l => l.SetText("Close"))
                .AddOnClickedEvent(b => DynamicUIManager.ClosePanel<BuildRobotPanel>());
            CancelButton.RootGameObject.SetActive(false);

            _partsScrollView = MainContent.CreateScrollView()
                .SetRightStretch<ScrollView>()
                .ApplyTemplate(VerticalLayout)
                .SetHeight<ScrollView>(MODAL_HEIGHT - VERTICAL_PADDING * 2 - 50);
            _scrollViewWidth = _partsScrollView.Parent!.RectOfChildren().width - SCROLLBAR_WIDTH;
            _entryWidth = _scrollViewWidth - HORIZONTAL_PADDING * 2;

            var addPartButton = MainContent.CreateButton()
                .SetTopStretch<Button>()
                .StepIntoLabel(l => l.SetText("Add Part"))
                .AddOnClickedEvent(
                    _ => {
                        if (_parts.Count > 0)
                            _parts.Add(_parts[0].Duplicate());
                        else _parts.Add(new PartEditorPart());
                        AddPartEntries();
                    })
                .ApplyTemplate(VerticalLayout);

            AddPartEntries();

            return true;
        }

        private void AddPartEntries() {
            _partsScrollView.Content.DeleteAllChildren();
            foreach (var part in _parts) {
                AddPartEntry(part, true);
            }
        }

        private void AddPartEntry(PartEditorPart partEditorPart, bool isNew) {
            if (!isNew) {
                AddPartEntries();
                return;
            }

            (Content leftContent, Content rightContent) =
                _partsScrollView.Content.CreateSubContent(new Vector2(_entryWidth, ROW_HEIGHT))
                    .ApplyTemplate(ListVerticalLayout)
                    .SplitLeftRight(BUTTON_WIDTH, HORIZONTAL_PADDING);
            leftContent.SetBackgroundColor<Content>(Color.blue);

            (Content labelsContent, Content buttonsContent) =
                rightContent.SplitLeftRight(_entryWidth - (HORIZONTAL_PADDING + BUTTON_WIDTH) * 3, HORIZONTAL_PADDING);
            (Content topContent, Content bottomContent) = labelsContent.SplitTopBottom(ROW_HEIGHT / 2, 0);
            topContent.CreateLabel()
                .SetText(partEditorPart.Name)
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
                .StepIntoLabel(l => l.SetText("Edit"))
                .AddOnClickedEvent(b => {
                    DynamicUIManager.CreatePanel<PartConfigPanel>(persistent: false, partEditorPart);
                })
                .ApplyTemplate(VerticalLayout)
                .SetSize<Button>(new Vector2(BUTTON_WIDTH, ROW_HEIGHT))
                .SetStretch<Button>();
            deleteButtonContent.CreateButton()
                .StepIntoLabel(l => l.SetText("Delete"))
                .AddOnClickedEvent(b => { })
                .ApplyTemplate(VerticalLayout)
                .SetSize<Button>(new Vector2(BUTTON_WIDTH, ROW_HEIGHT))
                .SetStretch<Button>();
        }

        public override void Update() {
            _parts.ForEach(p => {
                p.ConnectionPoints.ForEach(point => {
                    if (point != null)
                        Debug.DrawRay(point.transform.position, point.transform.forward, Color.blue);
                });
            });
        }

        public override void Delete() {
            var partData = new List<MixAndMatchPartData>();

            _parts.ForEachIndex((i, part) => {
                partData.Add(part.ToPartData());
                part.Index = i;
            });
 
            _parts.ForEachIndex((i, part) => {
                if (part.ConnectedPartEditorPart != null) {
                    partData[i].ConnectedPart = partData[part.ConnectedPartEditorPart.Index];
                }
            });
            
            var trfData = new MixAndMatchTransformData(partData.ToArray());

            _parts.ForEach(p => { GameObject.Destroy(p.UnityObject); });

            string Folder = "Mira";

            var root =
                AddRobotModal
                    .ParsePath(
                        Path
                            .Combine(
                                "$appdata/Autodesk/Synthesis",
                                Folder),
                        '/');
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            var files = Directory.GetFiles(root).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();

            var robotsToSpawn = Enumerable.Range(0, _parts.Count).Select(i => files[1]).ToArray();
            RobotSimObject.SpawnRobot(trfData, robotsToSpawn);
            //Transform grounded = new GameObject("grounded").transform;
            //grounded.gameObject.AddComponent<Rigidbody>();
            //grounded.parent = parent;

            /*_parts.ForEach(p => {
                p.Transform.SetParent(parent);

                if (p.ConnectedPoint == null)
                    return;

                Vector3 jointPosition = p.ConnectedPoint.position;

                FixedJoint thisJoint = p.Transform.Find("grounded").gameObject.AddComponent<FixedJoint>();
                FixedJoint otherJoint = p.ConnectedPoint.parent.transform.Find("grounded").gameObject.AddComponent<FixedJoint>();

                thisJoint.anchor = jointPosition;
                otherJoint.anchor = jointPosition;

                thisJoint.connectedBody = otherJoint.GetComponent<Rigidbody>();
                otherJoint.connectedBody = thisJoint.GetComponent<Rigidbody>();
            });
*/
            PhysicsManager.IsFrozen = false;
        }
    }
}