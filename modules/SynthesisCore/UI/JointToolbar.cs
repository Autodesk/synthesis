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
    public class JointToolbar
    {
        private static bool toolbarCreated = false;

        public static void CreateToolbar()
        {
            if (toolbarCreated)
                return;

            UIManager.AddPanel(new JointsWindow().Panel);

            var jointsTab = new Tab("Joint Editor", Ui.ToolbarAsset, toolbarElement => {
                var designCategory = ToolbarTools.AddButtonCategory(toolbarElement, "JOINTS");
                ToolbarTools.AddButton(designCategory, "joints-button", "/modules/synthesis_core/UI/images/modules-icon.png",
                    _ => UIManager.TogglePanel("Joints"));
            });

            UIManager.AddTab(jointsTab);

            toolbarCreated = true;
        }
    }

}
