using MathNet.Spatial.Euclidean;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using SynthesisCore.Meshes;
using SynthesisCore.Simulation;
using SynthesisCore.Systems;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SynthesisCore.UI
{
    public class JointItem
    {
        private static List<JointItem> jointItems = new List<JointItem>();
        private bool IsHighlighted = false;
        private Button highlightButton;

        public VisualElement JointElement { get; }
        
        public JointItem(VisualElementAsset jointAsset, MotorAssembly assembly)
        {
            jointItems.Add(this);
            JointElement = jointAsset.GetElement("joint");
            RegisterJointButtons(assembly);
        }

        private void RegisterJointButtons(MotorAssembly assembly)
        {
            var j = assembly.Joint;
            var jointEntity = assembly.Entity;

            highlightButton = (Button)JointElement.Get("highlight-button");
            Dropdown motorTypeDropdown = new Dropdown("motor-type-dropdown-container", MotorTypes.AllTypeNames.IndexOf(assembly.Motor.Name), MotorTypes.AllTypeNames)
            {
                ItemHeight = 15
            };
            JointElement.Get("motor-type-dropdown-container").Add(motorTypeDropdown);
            TextField gearField = (TextField)JointElement.Get("motor-gear-field");
            TextField countField = (TextField)JointElement.Get("motor-count-field");
            
            gearField.SetValueWithoutNotify(assembly.GearReduction.ToString());
            countField.SetValueWithoutNotify(assembly.MotorCount.ToString());

            motorTypeDropdown.Subscribe(e =>
            {
                if (e is Dropdown.SelectionEvent selectionEvent)
                {
                    if (MotorTypes.Contains(selectionEvent.SelectionName))
                    {
                        assembly.Motor = MotorTypes.Get(selectionEvent.SelectionName);
                    }
                }
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
                    if (countField.Value != "")
                    {
                        if (uint.Parse(countField.Value) == 0)
                            countField.SetValueWithoutNotify(assembly.MotorCount.ToString());
                        else
                            assembly.MotorCount = uint.Parse(countField.Value);
                    }
                }
            });

            highlightButton.OnMouseEnter(() =>
            {
                HighlightButton();
            });

            highlightButton.OnMouseLeave(() =>
            {
                if(!IsHighlighted)
                    UnHighlightButton();
            });

            highlightButton.Subscribe(x =>
            {
                UnHighlightAllButtons();
                HighlightButton(true);

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

        private void HighlightButton(bool stick = false)
        {
            if (stick)
                IsHighlighted = true;
            highlightButton.SetStyleProperty("background-color", "rgba(255, 255, 0, 1)");
        }

        private void UnHighlightButton()
        {
            IsHighlighted = false;
            highlightButton.SetStyleProperty("background-color", "rgba(255, 255, 0, 0)");
        }

        public static void UnHighlightAllButtons()
        {
            foreach(var i in jointItems)
            {
                i.UnHighlightButton();
            }
        }
    }

}