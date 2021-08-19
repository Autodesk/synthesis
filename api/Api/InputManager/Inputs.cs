using System;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace SynthesisAPI.InputManager.Inputs
{
    // TODO: Should I add HashCodes?
    
    public interface Input
    {
        string Name { get; }
        float Value { get; }
        bool Update();
    }

    public class Analog : Input
    {
        public string Name { get; private set; }
        public float Value { get; private set; }
        public bool UsePositiveSide { get; private set; }
        public float BaseValue { get; private set; }
        public bool Inverted { get; private set; }

        public Analog(string name, bool usePositiveSide, bool inverted = false, float baseValue = 0)
        {
            Name = name;
            Inverted = inverted;
            UsePositiveSide = usePositiveSide;
            BaseValue = baseValue;
        }

        public virtual bool Update()
        {
            Value = UnityEngine.Input.GetAxis(Name);
            Value = Inverted ? Value *= -1 : Value;
            Value = Mathf.Clamp(Value, UsePositiveSide ? BaseValue : -999, UsePositiveSide ? 999 : BaseValue);
            return Value != BaseValue;
        }
    }
    public class Digital : Analog
    {
        public string Name { get; private set; }
        public float Value { get; private set; }
        public DigitalState State { get; private set; }
        public Digital(string name) : base(name, true)
        {
            Name = name;
            Value = 0.0f;
            State = DigitalState.Up;
        }

        public override bool Update() {
            if (UnityEngine.Input.GetKeyDown(Name))
                State = DigitalState.Down;
            else if (UnityEngine.Input.GetKey(Name))
                State = DigitalState.Held;
            else if (UnityEngine.Input.GetKeyUp(Name))
                State = DigitalState.Up;
            else
                State = DigitalState.None;
            Value = State > 0 ? 1 : 0;
            return State != DigitalState.None;
        }
    }

    public class MouseDown: Digital
    {
        public Vector2D MousePosition { get; private set; }
        public MouseDown(string name): base(name) { }
        public override bool Update()
        {
            var r = base.Update();
            MousePosition = ((UnityEngine.Vector2)UnityEngine.Input.mousePosition).Map();
            return r;
        }

        public static readonly MouseDown LeftMouseButton = new MouseDown("Mouse 0");
        public static readonly MouseDown RightMouseButton = new MouseDown("Mouse 1");
        public static readonly MouseDown MiddleMouseButton = new MouseDown("Mouse 2");
    }

    public enum DigitalState {
        Down = 1, Up = -1, Held = 2, None = 0
    }
}
