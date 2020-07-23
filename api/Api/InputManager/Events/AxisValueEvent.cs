using System;


namespace SynthesisAPI.InputManager.Events
{
    /// <summary>
    /// Event for updating subscribers on a registered axis
    /// TODO: Inherit yet to be made Event interface
    /// </summary>
    public struct AxisValueEvent
    {
        /// <summary>
        /// Customized name used for identification of the value
        /// </summary>
        public string Name { get; private set; }
        public float Value { get; private set; }

        public AxisValueEvent(string name, float value)
        {
            Name = name;
            Value = value;
        }
    }
}
