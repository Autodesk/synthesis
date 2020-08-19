using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using SynthesisCore.Systems;
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

            var engineTab = new Tab("Engine", Ui.ToolbarAsset, toolbarElement => {
                var designCategory = ToolbarTools.AddButtonCategory(toolbarElement, "ENTITIES");
                ToolbarTools.AddButton(designCategory, "add-entity-button", "/modules/synthesis_core/UI/images/add-icon.png",
                    _ => Logger.Log("TODO"));
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

            var environmentsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Environments.uxml");
            Panel environmentsWindow = new Panel("Environments", environmentsAsset,
                element => Utilities.RegisterOKCloseButtons(element, "Environments"));

            UIManager.AddPanel(environmentsWindow);

            // ===========================================================================================================
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

            foreach (var j in Joints.GlobalAllJoints)
            {
                VisualElement jointElement = jointAsset?.GetElement("joint");
                ListView jointList = (ListView)visualElement.Get("joint-list");
                jointList.Add(jointElement);

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
    }
}
