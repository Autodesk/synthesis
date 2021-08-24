using System;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = UnityEngine.Logger;
using Math = System.Math;

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

        public string Name { get; protected set; }
        public float Value
        {
            get
            {
                Update();
                return _value;
            }
            protected set => _value = value;
        }

        public bool UsePositiveSide { get; private set; }
        public float BaseValue { get; private set; }
        public bool Inverted { get; private set; }

        private float _value;

        public Analog(string name, bool usePositiveSide, bool inverted = false, float baseValue = 0)
        {
            Name = name;
            Inverted = inverted;
            UsePositiveSide = usePositiveSide;
            BaseValue = baseValue;
        }

        public virtual bool Update()
        {
            _value = UnityEngine.Input.GetAxis(Name);
            _value = Inverted ? _value *= -1 : _value;
            _value = Mathf.Clamp(_value, UsePositiveSide ? BaseValue : -999, UsePositiveSide ? 999 : BaseValue);
            return Math.Abs(_value - BaseValue) > 0.1;
        }
    }
    public class Digital : Analog
    {
        public DigitalState State { get; private set; }
        public Digital(string name) : base(name, true)
        {
            Name = name;
            Value = 0.0f;
            State = DigitalState.Up;
        }

        public override bool Update() {
            try
            {
                if (InputUtils.GetKeyOrButtonDown(Name))
                    State = DigitalState.Down;
                else if (InputUtils.GetKeyOrButton(Name))
                    State = DigitalState.Held;
                else if (InputUtils.GetKeyOrButtonUp(Name))
                    State = DigitalState.Up;
                else
                    State = DigitalState.None;
                Value = State > 0 ? 1 : 0;
                return State != DigitalState.None;
            }
            catch (ArgumentException e)
            {
                Utilities.Logger.Log($"Key {Name} is invalid." );
                return false;
            }
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
