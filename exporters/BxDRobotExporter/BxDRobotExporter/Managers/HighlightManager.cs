using System.Collections.Generic;
using Inventor;
using Color = System.Drawing.Color;

namespace BxDRobotExporter.Managers
{
    public class HighlightManager
    {
        // Joint editor highlight
        private HighlightSet jointEditorHighlight;
        
        // Degrees of freedom highlight
        public bool DisplayDof { get; private set; }
        private HighlightSet blueHighlightSet;
        private HighlightSet greenHighlightSet;
        private HighlightSet redHighlightSet;


        public void EnvironmentOpening(Document asmDocument)
        {
            blueHighlightSet = asmDocument.CreateHighlightSet();
            blueHighlightSet.Color = InventorUtils.GetInventorColor(Color.DodgerBlue);
            greenHighlightSet = asmDocument.CreateHighlightSet();
            greenHighlightSet.Color = InventorUtils.GetInventorColor(Color.LawnGreen);
            redHighlightSet = asmDocument.CreateHighlightSet();
            redHighlightSet.Color = InventorUtils.GetInventorColor(Color.Red);

            jointEditorHighlight = asmDocument.CreateHighlightSet();
            jointEditorHighlight.Color = InventorUtils.GetInventorColor(RobotExporterAddInServer.Instance.AddInSettings.InventorChildColor);
        }

        public void EnableDofHighlight(RobotData robotData)
        {
            if (robotData.RobotBaseNode == null && !robotData.LoadRobotSkeleton())
                return;

            var rootNodes = new List<RigidNode_Base> {robotData.RobotBaseNode};
            var jointedNodes = new List<RigidNode_Base>();
            var problemNodes = new List<RigidNode_Base>();

            foreach (var node in robotData.RobotBaseNode.ListAllNodes())
            {
                if (node == robotData.RobotBaseNode) // Base node is already dealt with TODO: add ListChildren() to RigidNode_Base
                {
                    continue;
                }

                if (node.GetSkeletalJoint() == null || node.GetSkeletalJoint().cDriver == null) // TODO: Figure out how to identify nodes that aren't set up (highlight red)
                {
                    problemNodes.Add(node);
                }
                else
                {
                    jointedNodes.Add(node);
                }
            }

            jointEditorHighlight.Clear();
            InventorUtils.CreateHighlightSet(rootNodes, blueHighlightSet);
            InventorUtils.CreateHighlightSet(jointedNodes, greenHighlightSet);
            InventorUtils.CreateHighlightSet(problemNodes, redHighlightSet);
        }

        public void ClearDofHighlight()
        {
            blueHighlightSet.Clear();
            greenHighlightSet.Clear();
            redHighlightSet.Clear();
            DisplayDof = false;
        }

        public void ToggleDofHighlight(RobotData robotData)
        {
            DisplayDof = !DisplayDof;

            if (DisplayDof)
            {
                EnableDofHighlight(robotData);
            }
            else
            {
                ClearDofHighlight();
            }
        }

        public void ClearJointHighlight()
        {
            jointEditorHighlight.Clear();
        }

        public void HighlightJoint(ComponentOccurrence componentOccurrence)
        {
            jointEditorHighlight.AddItem(componentOccurrence);
        }

        public void ClearAllHighlight()
        {
            ClearDofHighlight();
            ClearJointHighlight();
        }

        public void SetJointHighlightColor(Color getInventorColor)
        {
            jointEditorHighlight.Color = InventorUtils.GetInventorColor(getInventorColor);
        }
    }
}