using System;
using System.Collections.Generic;
using System.Linq;
using Inventor;

namespace BxDRobotExporter.Wizard
{
    public delegate void WheelTypeChangedEventHandler(object sender, WheelTypeChangedEventArgs e);
    public class WheelTypeChangedEventArgs : EventArgs
    {
        public WizardData.WizardWheelType NewWheelType;
    }

    public class WizardUtilities
    {
        private struct GUIDDoublePair { public Guid guid; public double d; }

        /// <summary>
        /// Gets an array of all the names at and below a node.
        /// </summary>
        /// <param name="baseNode"></param>
        /// <returns></returns>
        public static string[] GetExportedComponentNames(RigidNode_Base baseNode)
        {
            List<string> names = new List<string>();
            foreach(RigidNode_Base node in baseNode.ListAllNodes())
            {
                names.AddRange(node.ModelFullID.Split(new string[] { "-_-" }, StringSplitOptions.RemoveEmptyEntries));
            }
            return names.ToArray();
        }

        /// <summary>
        /// Gets an array of all the exported <see cref="ComponentOccurrence"/>s at and below a node.
        /// </summary>
        /// <param name="baseNode"></param>
        /// <returns></returns>
        public static ComponentOccurrence[] GetExportedComponents(RigidNode_Base baseNode)
        {
            List<ComponentOccurrence> names = new List<ComponentOccurrence>();
            foreach (RigidNode_Base node in baseNode.ListAllNodes())
            {
                foreach (string s in node.ModelFullID.Split(new string[] { "-_-" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    names.Add(InventorUtils.GetOccurrence(s));
                }
            }
            return names.ToArray();
        }

        /// <summary>
        /// NOT FUNCTIONAL: Gets the volume of a list of <see cref="ComponentOccurrence"/>s
        /// </summary>
        /// <param name="occurrences"></param>
        /// <returns></returns>
        public static double GetVolume(ComponentOccurrence[] occurrences)
        {
            double volume = 0.0d;

            Inventor.Application app = StandardAddInServer.Instance.MainApplication;
            
            foreach(ComponentOccurrence component in occurrences)
            {
                foreach (ComponentOccurrence occurrence in component.SubOccurrences)
                {
                    foreach (SurfaceBody body in occurrence.SurfaceBodies)
                    {
                        volume += body.Volume[0.001];
                    } 
                }
            }

            return volume;
        }

        /// <summary>
        /// Detects all the wheel nodes in a robot. Needs improvement
        /// </summary>
        /// <param name="baseNode">Base node of the robot.</param>
        /// <param name="leftWheels">Detected wheels on the left side.</param>
        /// <param name="rightWheels">Detected wheels on the right side.</param>
        /// <param name="threshold">Threshold distance for wheels to be detected.</param>
        /// <returns>True if successful</returns>
        public static bool DetectWheels(RigidNode_Base baseNode, out List<RigidNode_Base> leftWheels, out List<RigidNode_Base> rightWheels, double threshold = 2d)
        {
            List<RigidNode_Base> potentialWheels = new List<RigidNode_Base>();

            foreach(RigidNode_Base node in baseNode.ListAllNodes())
            {
                //For the first filter, we take out any nodes that do not have parents and rotational joints.
                if (node.GetParent() != null && node.GetSkeletalJoint() != null && node.GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL)
                {
                    potentialWheels.Add(node);
                }
            }

            //Find node with the lowest y value to filter out all nodes above bottom of robot
            RigidNode_Base lowestNode = null; double lowestY = 0;
            foreach(RigidNode_Base node in potentialWheels)
            {
                float nodeY = node.GetSkeletalJoint().GetAngularDOF().First().basePoint.y;

                if (lowestNode == null)
                {
                    lowestNode = node;
                    lowestY = nodeY;
                }
                else
                {
                    if (nodeY < lowestY)
                    {
                        lowestNode = node;
                        lowestY = nodeY;
                    }
                }
            }

            //Find all nodes with y values within 0.1 of the lowest node
            List<RigidNode_Base> nodesToFilter = potentialWheels;
            potentialWheels = new List<RigidNode_Base>();
            foreach (RigidNode_Base node in nodesToFilter)
            {
                float nodeY = node.GetSkeletalJoint().GetAngularDOF().First().basePoint.y;

                if (lowestY - nodeY <= threshold && lowestY - nodeY >= -threshold)
                    potentialWheels.Add(node);
            }

            //Find the nodes with the highest and lowest x values
            RigidNode_Base leftMostNode = null; double highestX = 0;
            RigidNode_Base rightMostNode = null; double lowestX = 0;
            foreach(RigidNode_Base node in potentialWheels)
            {
                float nodeX = node.GetSkeletalJoint().GetAngularDOF().First().basePoint.x;

                if (leftMostNode == null)
                {
                    leftMostNode = node;
                    highestX = nodeX;
                }
                else if (nodeX > highestX)
                {
                    leftMostNode = node;
                    highestX = nodeX;
                }

                if (rightMostNode == null)
                {
                    rightMostNode = node;
                    lowestX = nodeX;
                }
                else if (node.GetSkeletalJoint().GetAngularDOF().First().basePoint.x < lowestX)
                {
                    leftMostNode = node;
                    lowestX = nodeX;
                }
            }

            //Find all the nodes with x values within 0.1 of both.
            rightWheels = new List<RigidNode_Base>();
            leftWheels = new List<RigidNode_Base>();

            foreach (RigidNode_Base node in potentialWheels)
            {
                float nodeX = node.GetSkeletalJoint().GetAngularDOF().First().basePoint.x;

                if (nodeX - lowestX <= threshold && nodeX - lowestX >= -threshold)
                    rightWheels.Add(node);
                else if (nodeX - highestX <= threshold && nodeX - highestX >= -threshold)
                    leftWheels.Add(node);
            }

            return true;
        }
        public static bool DetectWheels(RigidNode_Base baseNode, out List<RigidNode_Base> leftWheels, out List<RigidNode_Base> rightWheels, 
            out List<RigidNode_Base> middleWheels, double threshold = 2d)
        {
            List<RigidNode_Base> potentialWheels = new List<RigidNode_Base>();

            foreach (RigidNode_Base node in baseNode.ListAllNodes())
            {
                //For the first filter, we take out any nodes that do not have parents and rotational joints.
                if (node.GetParent() != null && node.GetSkeletalJoint() != null && node.GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL)
                {
                    potentialWheels.Add(node);
                }
            }

            //Find node with the lowest y value to filter out all nodes above bottom of robot
            RigidNode_Base lowestNode = null; double lowestY = 0;
            foreach (RigidNode_Base node in potentialWheels)
            {
                float nodeY = node.GetSkeletalJoint().GetAngularDOF().First().basePoint.y;

                if (lowestNode == null)
                {
                    lowestNode = node;
                    lowestY = nodeY;
                }
                else
                {
                    if (nodeY < lowestY)
                    {
                        lowestNode = node;
                        lowestY = nodeY;
                    }
                }
            }

            //Find all nodes with y values within 0.1 of the lowest node
            List<RigidNode_Base> nodesToFilter = potentialWheels;
            potentialWheels = new List<RigidNode_Base>();
            foreach (RigidNode_Base node in nodesToFilter)
            {
                float nodeY = node.GetSkeletalJoint().GetAngularDOF().First().basePoint.y;

                if (lowestY - nodeY <= threshold && lowestY - nodeY >= -threshold)
                    potentialWheels.Add(node);
            }

            //Find the nodes with the highest and lowest x values
            RigidNode_Base leftMostNode = null; double highestX = 0;
            RigidNode_Base middleMostNode = null; double middlestX = 0;
            RigidNode_Base rightMostNode = null; double lowestX = 0;
            foreach (RigidNode_Base node in potentialWheels)
            {
                float nodeX = node.GetSkeletalJoint().GetAngularDOF().First().basePoint.x;

                if (leftMostNode == null)
                {
                    leftMostNode = node;
                    highestX = nodeX;
                }
                else if (nodeX > highestX)
                {
                    leftMostNode = node;
                    highestX = nodeX;
                }

                if (rightMostNode == null)
                {
                    rightMostNode = node;
                    lowestX = nodeX;
                }
                else if (node.GetSkeletalJoint().GetAngularDOF().First().basePoint.x < lowestX)
                {
                    leftMostNode = node;
                    lowestX = nodeX;
                }
            }

            foreach (RigidNode_Base node in potentialWheels)
            {
                float nodeX = node.GetSkeletalJoint().GetAngularDOF().First().basePoint.x;
                double centerX = ((highestX - lowestX) / 2) + lowestX;
                if (middleMostNode == null)
                {
                    middleMostNode = node;
                    middlestX = nodeX;
                }
                else if (nodeX < highestX && nodeX > lowestX)
                {
                    if (Math.Abs(centerX - middlestX) > Math.Abs(centerX - nodeX))
                    {
                        middlestX = nodeX;
                        middleMostNode = node;
                    }
                }
            }
            //Find all the nodes with x values within 0.1 of both.
            rightWheels = new List<RigidNode_Base>();
            middleWheels = new List<RigidNode_Base>();
            leftWheels = new List<RigidNode_Base>();

            foreach (RigidNode_Base node in potentialWheels)
            {
                float nodeX = node.GetSkeletalJoint().GetAngularDOF().First().basePoint.x;

                if (nodeX - lowestX <= threshold && nodeX - lowestX >= -threshold)
                    rightWheels.Add(node);
                else if (nodeX - highestX <= threshold && nodeX - highestX >= -threshold)
                {
                    leftWheels.Add(node);
                }
                else if (nodeX - middlestX <= threshold && nodeX - middlestX >= -threshold)
                {
                    middleWheels.Add(node);
                }
            }

            return true;
        }
    }
}