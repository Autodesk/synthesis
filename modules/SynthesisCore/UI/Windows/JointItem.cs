﻿using MathNet.Spatial.Euclidean;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using SynthesisCore.Meshes;
using SynthesisCore.Simulation;
using SynthesisCore.Systems;
using System.Text.RegularExpressions;

namespace SynthesisCore.UI
{
    public class JointItem
    {
        public VisualElement JointElement { get; }
        public JointItem(VisualElementAsset jointAsset, MotorAssembly assembly)
        {
            JointElement = jointAsset.GetElement("joint");
            RegisterJointButtons(assembly);
        }

        private void RegisterJointButtons(MotorAssembly assembly)
        {
            var j = assembly.Joint;
            var jointEntity = assembly.Entity;

            Button highlightButton = (Button)JointElement.Get("highlight-button");
            Button motorTypeButton = (Button)JointElement.Get("motor-type-button");
            TextField gearField = (TextField)JointElement.Get("motor-gear-field");
            TextField countField = (TextField)JointElement.Get("motor-count-field");
            
            gearField.IsReadOnly = false;
            gearField.SetValueWithoutNotify(assembly.GearReduction.ToString());

            countField.SetValueWithoutNotify("1"); // TODO
            countField.IsReadOnly = true; // TODO

            motorTypeButton.Subscribe(x =>
            {
                Logger.Log("To Do: Motor Type Dropdown", LogLevel.Debug);
            });

            gearField.SubscribeOnChange(e =>
            {
                if(e is TextField.TextFieldChangeEvent changeEvent)
                {
                    var value = Regex.Replace(changeEvent.NewValue, @"[^0-9.]", ""); // Only allow integers and .
                    var i = value.IndexOf('.');
                    if(i != -1)
                    {
                        value = value.Substring(0, i + 1) + value.Substring(i).Replace(".", ""); // Only allow one .
                    }
                    gearField.SetValueWithoutNotify(value);
                }
            });

            gearField.SubscribeOnFocusLeave(e =>
            {
                if (e is TextField.TextFieldFocusLeaveEvent)
                {
                    if (gearField.Value != "")
                    {
                        assembly.GearReduction = double.Parse(gearField.Value);
                    }
                }
            });

            countField.SubscribeOnChange(e =>
            {
                if (e is TextField.TextFieldChangeEvent changeEvent)
                {
                    var value = Regex.Replace(changeEvent.NewValue, @"[^0-9]", ""); // Only allow integers
                    countField.SetValueWithoutNotify(value);
                }
            });

            countField.SubscribeOnFocusLeave(e =>
            {
                if (e is TextField.TextFieldFocusLeaveEvent leaveEvent)
                {
                    // TODO
                }
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
                parentComponent.ParentEntity = jointEntity;

                // Set highlight entity transform
                var jointHighlightTransform = JointsWindow.jointHighlightEntity?.GetComponent<Transform>();
                jointHighlightTransform.Position = j.Anchor;
                
                var axis = j.Axis.Normalize();
                jointHighlightTransform.Rotation = MathUtil.LookAt(
                    axis.IsParallelTo(UnitVector3D.XAxis) ? axis.CrossProduct(UnitVector3D.YAxis) : axis.CrossProduct(UnitVector3D.XAxis), 
                    axis);

                // Move camera to look at the joint
                var parentPosition = jointEntity.GetComponent<Parent>()?.ParentEntity.GetComponent<Transform>()?.GlobalPosition ?? new Vector3D();
                var cameraOffset = (jointHighlightTransform.GlobalPosition - parentPosition).Normalize().ScaleBy(3);
                CameraController.Instance.cameraTransform.GlobalPosition = jointHighlightTransform.GlobalPosition + cameraOffset;
                
                CameraController.Instance.cameraTransform.LookAt(jointHighlightTransform.GlobalPosition);
                CameraController.Instance.SetNewFocus(jointHighlightTransform.GlobalPosition, false);
            });
        }
    }

}