using MathNet.Spatial.Euclidean;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using SynthesisCore.Meshes;
using SynthesisCore.Systems;
using System.Collections.Generic;

namespace SynthesisCore.UI
{
    public class JointItem
    {
        public VisualElement JointElement { get; }
        public JointItem(VisualElementAsset jointAsset, Joints jointComponent, HingeJoint joint)
        {
            JointElement = jointAsset.GetElement("joint");
            RegisterJointButtons(jointComponent, joint);
        }

        private void RegisterJointButtons(Joints jointComponent, HingeJoint j)
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
                if (JointsWindow.jointHighlightEntity == null)
                {
                    JointsWindow.jointHighlightEntity = EnvironmentManager.AddEntity();
                    JointsWindow.jointHighlightEntity?.AddComponent<Transform>();
                    JointsWindow.jointHighlightEntity?.AddComponent<Mesh>();
                    
                    // Create mesh
                    Mesh m = JointsWindow.jointHighlightEntity?.GetComponent<Mesh>();
                    m.Color = (1f, 1f, 0f, 0.5f);
                    Cylinder.Make(m, 16, 0.01, 0.25);
                }

                // Set highlight entity parent
                var parentComponent = JointsWindow.jointHighlightEntity?.GetComponent<Parent>();
                parentComponent.ParentEntity = jointComponent.Entity.Value;

                // Set highlight entity transform
                var jointHighlightTransform = JointsWindow.jointHighlightEntity?.GetComponent<Transform>();
                jointHighlightTransform.Position = j.Anchor;
                
                var axis = j.Axis.Normalize();
                jointHighlightTransform.Rotation = MathUtil.LookAt(
                    axis.IsParallelTo(UnitVector3D.XAxis) ? axis.CrossProduct(UnitVector3D.YAxis) : axis.CrossProduct(UnitVector3D.XAxis), 
                    axis);

                // Move camera to look at the joint
                var parentPosition = jointComponent.Entity?.GetComponent<Parent>()?.ParentEntity.GetComponent<Transform>()?.GlobalPosition ?? new Vector3D();
                var cameraOffset = (jointHighlightTransform.GlobalPosition - parentPosition).Normalize();
                CameraController.Instance.cameraTransform.GlobalPosition = jointHighlightTransform.GlobalPosition + cameraOffset;
                
                CameraController.Instance.cameraTransform.LookAt(jointHighlightTransform.GlobalPosition);
                CameraController.Instance.SetNewFocus(jointHighlightTransform.GlobalPosition, false);
            });
        }
    }

}