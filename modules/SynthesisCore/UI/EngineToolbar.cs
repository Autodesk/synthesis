using MathNet.Spatial.Euclidean;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using SynthesisCore.Systems;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;

namespace SynthesisCore.UI
{
    public class EngineToolbar
    {
        private static bool toolbarCreated = false;

        public static void CreateToolbar()
        {
            if (toolbarCreated)
                return;

            UIManager.AddPanel(new EntitiesWindow().Panel);
            UIManager.AddPanel(new EnvironmentsWindow().Panel);

            var engineTab = new Tab("Engine", Ui.ToolbarAsset, toolbarElement => {
                var designCategory = ToolbarTools.AddButtonCategory(toolbarElement, "ENTITIES");
                ToolbarTools.AddButton(designCategory, "add-entity-button", "/modules/synthesis_core/UI/images/add-icon.png",
                    _ => UIManager.TogglePanel("Entities"));
                ToolbarTools.AddButton(designCategory, "change-environment-button", "/modules/synthesis_core/UI/images/environments-icon.png",
                    _ => UIManager.TogglePanel("Environments"));
            });

            var jointsTab = new Tab("Joint Editor", Ui.ToolbarAsset, toolbarElement => {
                var designCategory = ToolbarTools.AddButtonCategory(toolbarElement, "JOINTS");
                ToolbarTools.AddButton(designCategory, "joints-button", "/modules/synthesis_core/UI/images/modules-icon.png",
                    _ => UIManager.TogglePanel("Joints"));
            });

            UIManager.AddTab(engineTab);
            UIManager.AddTab(jointsTab);
            UIManager.SetDefaultTab(engineTab.Name);
           
            var jointsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Joints.uxml");
            Panel jointsWindow = new Panel("Joints", jointsAsset,
            element =>
            {
                Utilities.RegisterOKCloseButtons(element, "Joints");
                LoadJointsWindowContent(element);
            });

            UIManager.AddPanel(jointsWindow);

            toolbarCreated = true;
        }

        private static void LoadJointsWindowContent(VisualElement visualElement)
        {
            var jointAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Joint.uxml");

            foreach (var entity in SynthesisCoreData.ModelsDict.Values)
            {
                if (entity.GetComponent<Joints>() != null)
                {
                    foreach (var joint in entity.GetComponent<Joints>().AllJoints)
                    {
                        VisualElement jointElement = jointAsset?.GetElement("joint");
                        ListView jointList = (ListView)visualElement.Get("joint-list");

                        //HighlightJoint();

                        jointList.Add(jointElement);
                    }
                }
            }
            //    //if (j.GetType() == typeof(HingeJoint))
            //    //{

            //        //HingeJoint hinge = new HingeJoint();


            //       // what joint, where (e.g. joint 0 is front-left wheel)
            //       // highlight meshes
            //       // motor configuration (motor type, count, gearing)
            //       // what key controls it
            //       //foreach (var motor in MotorManager.AllMotorControllers)
            //       //{
            //       //     motorType = motor.MotorType.MotorName;
            //       //     gear = motor.Gearing;
            //       //     motorCount = motor.MotorCount;
            //       //}

            //        // wheel type
            //        // motor type
            //        // advanced: torque, force, friction, etc.



            //        //titleText.Text = titleText.Text
            //        //    .Replace("%name%", ((HingeJoint)j).ConnectedParent.ExportedJointUuid);

            //        ListView jointList = (ListView)visualElement.Get("joint-list");
            //        jointList.Add(jointElement);
            //    //}
        }

        private void HighlightJoint()
        {
            Entity jointTest = EnvironmentManager.AddEntity();
            Bundle b = new Bundle();

            Selectable selectable = new Selectable();
            b.Components.Add(selectable);

            Transform t = new Transform();
            t.Position = new Vector3D(10, 10, 10); //joint anchor position
            b.Components.Add(t);
            b.Components.Add(cube()); //replace the cube function with your mesh creation
            jointTest.AddBundle(b);
            //when done
            jointTest.RemoveEntity();
        }

        private Mesh cube()
        {
            Mesh m = new Mesh();
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
