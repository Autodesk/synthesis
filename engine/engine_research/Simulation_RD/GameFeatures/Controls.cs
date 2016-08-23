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
        /// <summary>
        /// Get the <see cref="Controls"/> instance
        /// </summary>
        public static Controls GameControls
        {
            get;
        }

        /// <summary>
        /// Sets <see cref="GameControls"/> to a new instance
        /// </summary>
        static Controls()
        {
            GameControls = new Controls();
        }

        /// <summary>
        /// All possible controls, Eumerated
        /// </summary>
        public enum Control { Forward, Backward, Right, Left, ResetRobot, RobotOrient, CameraToggle, Pwm1Pos, Pwm1Neg, Pwm2Pos, Pwm2Neg, Pwm3Pos, Pwm3Neg, Pwm4Pos, Pwm4Neg, Pwm5Pos, Pwm5Neg, Stats }

        /// <summary>
        /// Data
        /// </summary>
        private Dictionary<Control, Key> ControlKeys;

        /// <summary>
        /// Instantiates dictionary and sets default keys
        /// </summary>
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

        /// <summary>
        /// Sets the key provided to the given control
        /// </summary>
        /// <param name="control">Control to bind to <see cref="Key"/></param>
        /// <param name="key">Key to bind to <see cref="Control"/></param>
        /// <returns>True if a key has been set to more than one control</returns>
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

        /// <summary>
        /// Gets the key associated with a control
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Key this[Control index]
        {
            get { return ControlKeys[index]; }
        }
    }
}
