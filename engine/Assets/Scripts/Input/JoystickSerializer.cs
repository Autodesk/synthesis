using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthesis.Input
{
    public class JoystickSerializer
    {
        private const int NumAxes = 12;
        private const int NumButtons = 33;
        private const int NumPovs = 12;

        private readonly Dictionary<int, string> axisMap;
        private readonly Dictionary<int, string> buttonMap;

        /// <summary>
        /// The ID of the associated joystick.
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// The serialized axes of the associated joystick.
        /// </summary>
        public readonly double[] Axes;

        /// <summary>
        /// The serialized button values of the associated joystick.
        /// </summary>
        public readonly bool[] Buttons;

        /// <summary>
        /// The serialized POV values of the associated joystick.
        /// </summary>
        public readonly double[] Povs;

        /// <summary>
        /// Initializes a new <see cref="JoystickSerializer"/> with the given ID.
        /// </summary>
        /// <param name="id"></param>
        public JoystickSerializer(int id)
        {
            Id = id;
            Axes = new double[NumAxes];
            Buttons = new bool[NumButtons];
            Povs = new double[NumPovs];

            string joystickName = "Joystick " + (id + 1);

            axisMap = new Dictionary<int, string>()
            {
                { 0, joystickName + " Axis 1" },
                { 1, joystickName + " Axis 2" },
                { 2, joystickName + " Axis 3" },
                { 3, joystickName + " Axis 4" },
                { 4, joystickName + " Axis 5" },
                { 5, joystickName + " Axis 6" },
                { 6, joystickName + " Axis 7" },
                { 7, joystickName + " Axis 8" }
            };

            buttonMap = new Dictionary<int, string>()
            {
                { 0, joystickName + " Button 3" },
                { 1, joystickName + " Button 1" },
                { 2, joystickName + " Button 2" },
                { 3, joystickName + " Button 4" },
                { 4, joystickName + " Button 5" },
                { 5, joystickName + " Button 6" },
                { 6, joystickName + " Button 7" },
                { 7, joystickName + " Button 8" },
                { 8, joystickName + " Button 9" },
                { 9, joystickName + " Button 10" },
            };
        }

        /// <summary>
        /// Serializes current joystick inputs.
        /// </summary>
        public void SerializeInputs()
        {
            foreach (KeyValuePair<int, string> mapping in axisMap)
                Axes[mapping.Key] = UnityEngine.Input.GetAxis(mapping.Value);

            foreach (KeyValuePair<int, string> mapping in buttonMap)
                Buttons[mapping.Key] = UnityEngine.Input.GetButton(mapping.Value);
        }
    }
}
