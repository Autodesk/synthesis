using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using Simulation_RD.SimulationPhysics;

namespace Simulation_RD.GameFeatures
{
    /// <summary>
    /// Provides utility functions for moving robot joints
    /// </summary>
    public static class DriveJoints
    {
        /// <summary>
        /// Drives all motors associated with a PWM port
        /// </summary>
        /// <param name="skeleton">robot</param>
        /// <param name="e">keyboard args</param>
        public static void UpdateAllMotors(RigidNode_Base skeleton /*spooky*/, KeyboardKeyEventArgs e)
        {
            float[] pwm = new float[5];

            pwm[0] =
                e.Key == Controls.GameControls[Controls.Control.Forward] ? 1 : 0 +
                e.Key == Controls.GameControls[Controls.Control.Backward] ? -1 : 0 +
                e.Key == Controls.GameControls[Controls.Control.Left] ? -1 : 0 +
                e.Key == Controls.GameControls[Controls.Control.Right] ? 1 : 0;

            pwm[1] =
                e.Key == Controls.GameControls[Controls.Control.Forward] ? -1 : 0 +
                e.Key == Controls.GameControls[Controls.Control.Backward] ? 1 : 0 +
                e.Key == Controls.GameControls[Controls.Control.Left] ? -1 : 0 +
                e.Key == Controls.GameControls[Controls.Control.Right] ? 1 : 0;

            pwm[2] = pwm[3] = pwm[4] = 0;

            List<RigidNode_Base> subNodes = skeleton.ListAllNodes();

            for(int i = 0; i < pwm.Length; i++)
            {
                foreach(RigidNode_Base node in subNodes)
                {
                    BulletRigidNode bNode = (BulletRigidNode)node;
                    if (bNode?.GetSkeletalJoint()?.cDriver?.GetDriveType().IsMotor() ?? false)
                    {
                        if (bNode.GetSkeletalJoint().cDriver.portA == i + 1)
                            bNode.Update?.Invoke(pwm[i] * 10);
                    }
                }
            }
        }
    }
}
