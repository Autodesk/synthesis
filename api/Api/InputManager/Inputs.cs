using System;
using SynthesisAPI.Runtime;

namespace SynthesisAPI.InputManager.Inputs
{
    public interface Input
    {
        string Name { get; }
        bool Update();
    }

    public class Analog : Input
    {
        public string Name { get; private set; }
        public float Value { get; private set; }
        public float BaseValue { get; private set; }
        public bool Inverted { get; private set; }

        public Analog(string name, bool inverted = false, float baseValue = 0)
        {
            Name = name;
            Inverted = inverted;
            BaseValue = baseValue;
        }

        public bool Update()
        {
            Value = UnityEngine.Input.GetAxis(Name);
            Value = Inverted ? Value *= -1 : Value;
            return Value != BaseValue;
        }
    }
    public class Digital : Input
    {
        public string Name { get; private set; }
        public DigitalState State { get; private set; }
        public Digital(string name)
        {
            Name = name;
            State = DigitalState.None;
        }

        public bool Update()
        {
            if (UnityEngine.Input.GetKey(Name))
            {
                if (State == DigitalState.None)
                    State = DigitalState.Down;
                else State = DigitalState.Held;
            }
            else
            {
                if (State == DigitalState.Down || State == DigitalState.Held)
                    State = DigitalState.Up;
                else State = DigitalState.None;
            }
            return State != DigitalState.None;
        }
    }
    public enum DigitalState
    {
        None = 0, Down = 1, Held = 2, Up = 3
    }
}
