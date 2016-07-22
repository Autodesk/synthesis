using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;

namespace Simulation_RD.GameFeatures
{
    /// <summary>
    /// Singleton class for controlling the robot within the environment
    /// </summary>
    class Controls
    {
        public static Controls GameControls
        {
            get;
        }

        static Controls()
        {
            GameControls = new Controls();
        }

        public enum Control { Forward, Backward, Right, Left, ResetRobot, RobotOrient, CameraToggle, Pwm1Pos, Pwm1Neg, Pwm2Pos, Pwm2Neg, Pwm3Pos, Pwm3Neg, Pwm4Pos, Pwm4Neg, Pwm5Pos, Pwm5Neg, Stats }
        private Dictionary<Control, Key> ControlKeys;

        private Controls()
        {
            ControlKeys = new Dictionary<Control, Key>();

            ControlKeys.Add(Control.Forward, Key.Up);
            ControlKeys.Add(Control.Left, Key.Left);
            ControlKeys.Add(Control.Right, Key.Right);
            ControlKeys.Add(Control.Backward, Key.Down);
            ControlKeys.Add(Control.ResetRobot, Key.R);
            ControlKeys.Add(Control.RobotOrient, Key.O);
            ControlKeys.Add(Control.CameraToggle, Key.C);
            ControlKeys.Add(Control.Pwm3Pos, Key.Number1);
            ControlKeys.Add(Control.Pwm3Neg, Key.Number2);
        }

        public bool SetControl(Control control, Key key)
        {
            ControlKeys[control] = key;

            foreach(var p in ControlKeys)
            {
                if (p.Key != control && p.Value == key)
                    return false;
            }

            return true;
        }

        public Key this[Control index]
        {
            get { return ControlKeys[index]; }
        }
    }
}
