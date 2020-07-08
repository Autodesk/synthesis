using System;
using SynthesisAPI.EventBus;


namespace SynthesisAPI.InputManager.Events
{
    /// <summary>
    /// Event for updating subscribers on a registered axis
    /// </summary>
    public struct AxisValueEvent : IEvent
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

        public object[] GetArguments() => new object[] { Name, Value };
    }
}
