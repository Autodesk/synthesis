using MathNet.Spatial.Euclidean;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using SynthesisCore.Systems;
using System.Collections.Generic;

namespace SynthesisCore.UI
{
    public class JointItem
    {
        public VisualElement JointElement { get; }
        public JointItem(VisualElementAsset jointAsset, Joints jointComponent, IJoint joint)
        {
            JointElement = jointAsset.GetElement("joint");
            RegisterJointButtons(jointComponent, joint);
        }

        private void RegisterJointButtons(Joints jointComponent, IJoint j)
        {
            Button highlightButton = (Button)JointElement.Get("highlight-button");
            Button motorButton = (Button)JointElement.Get("motor-type-button");
            Button gearButton = (Button)JointElement.Get("motor-gear-button");
            Button countButton = (Button)JointElement.Get("motor-count-button");

            motorButton.Subscribe(x =>
            {
                Logger.Log("To Do: Motor Type Dropdown", LogLevel.Debug);
            });

            gearButton.Subscribe(x =>
            {
                Logger.Log("To Do: Motor Gear Dropdown", LogLevel.Debug);
            });

            countButton.Subscribe(x =>
            {
                Logger.Log("To Do: Motor Count Dropdown", LogLevel.Debug);
            });

            highlightButton.Subscribe(x =>
            {
                Logger.Log("Highlight Joint", LogLevel.Debug);

                if (JointsWindow.jointHighlightEntity == null)
                {
                    JointsWindow.jointHighlightEntity = EnvironmentManager.AddEntity();
                    JointsWindow.jointHighlightEntity?.AddComponent<Transform>();
                    JointsWindow.jointHighlightEntity?.AddComponent<Mesh>();
                }

                var jointPosition = jointComponent.Entity?.GetComponent<Transform>()?.GlobalPosition ?? new Vector3D();
                jointPosition += j.Anchor;
                var parentPosition = jointComponent.Entity?.GetComponent<Parent>()?.ParentEntity.GetComponent<Transform>()?.Position ?? new Vector3D();
                var dir = (jointPosition - parentPosition).Normalize();

                var t = JointsWindow.jointHighlightEntity?.GetComponent<Transform>();
                t.Position = jointPosition; //joint anchor position

                Mesh m = JointsWindow.jointHighlightEntity?.GetComponent<Mesh>();
                cube(m);
                
                CameraController.Instance.cameraTransform.Position = t.Position + dir;
                CameraController.Instance.cameraTransform.LookAt(t.Position);

                if (JointsWindow.jointHighlightEntity?.RemoveEntity() ?? false) // TODO
                    JointsWindow.jointHighlightEntity = null;
            });
        }

        private Mesh cube(Mesh m)
        {
            if (m == null)
                m = new Mesh();

            m.Vertices = new List<Vector3D>()
            {
                new Vector3D(-0.5,-0.5,-0.5),
                new Vector3D(0.5,-0.5,-0.5),
                new Vector3D(0.5,0.5,-0.5),
                new Vector3D(-0.5,0.5,-0.5),
                new Vector3D(-0.5,0.5,0.5),
                new Vector3D(0.5,0.5,0.5),
                new Vector3D(0.5,-0.5,0.5),
                new Vector3D(-0.5,-0.5,0.5)
            };
            m.Triangles = new List<int>()
            {
                0, 2, 1, //face front
			    0, 3, 2,
                2, 3, 4, //face top
			    2, 4, 5,
                1, 2, 5, //face right
			    1, 5, 6,
                0, 7, 4, //face left
			    0, 4, 3,
                5, 4, 7, //face back
			    5, 7, 6,
                0, 6, 7, //face bottom
			    0, 1, 6
            };
            return m;
        }
    }

}