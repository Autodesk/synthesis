using System;
using MathNet.Spatial.Euclidean;
using Newtonsoft.Json;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = UnityEngine.Logger;
using Math = System.Math;

namespace SynthesisAPI.InputManager.Inputs
{
    // TODO: Should I add HashCodes?
    
    public interface Input {
        uint ContextBitmask { get; }
        string Name { get; }
        float Value { get; }
        int Modifier { get; }
        bool Update(bool ignoreMod = false, uint context = 0x00000001);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Analog : Input {
        [JsonProperty]
        public uint ContextBitmask { get; set; }
        [JsonProperty]
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

        [JsonProperty]
        public bool UsePositiveSide { get; private set; }
        [JsonProperty]
        public int Modifier { get; protected set; }
        [JsonProperty]
        public float BaseValue { get; private set; }
        [JsonProperty]
        public bool Inverted { get; private set; }

        private float _value;

        public Analog(string name, bool usePositiveSide, bool inverted = false, int modifier = 0, float baseValue = 0, uint context = 0xFFFFFFFF)
        {
            Name = name;
            Inverted = inverted;
            Modifier = modifier;
            UsePositiveSide = usePositiveSide;
            BaseValue = baseValue;
            ContextBitmask = context;
        }

        //public bool Update() {
        //    return Update(false);
        //}
        // TODO: Use modifier for analogs?
        public virtual bool Update(bool ignoreMod = false, uint context = 0x00000001) {
            if ((context & ContextBitmask) == 0)
                return false;

            _value = UnityEngine.Input.GetAxis(Name);
            _value = Inverted ? _value *= -1 : _value;
            _value = Mathf.Clamp(_value, UsePositiveSide ? BaseValue : -999, UsePositiveSide ? 999 : BaseValue);
            return Math.Abs(_value - BaseValue) > 0.1;
        }

        public virtual Analog WithModifier(int newMod)
            => new Analog(Name, UsePositiveSide, Inverted, newMod, BaseValue);

        public override bool Equals(object obj) {
            if (ReferenceEquals(obj, null) || !(obj is Analog))
                return false;
            return (obj as Analog).GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
            => 092847146 * Name.GetHashCode()
            + (UsePositiveSide ? 174829543 : 243563928)
            + 748273952 * Modifier
            + 374865392 * (int)(1000 * BaseValue)
            + (Inverted ? 345234768 : 980789437);
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class Digital : Analog
    {
        public DigitalState State { get; private set; }
        public Digital(string name, int modifier = 0, uint context = 0xFFFFFFFF) : base(name, true, context: context)
        {
            Name = name;
            Modifier = modifier;
            Value = 0.0f;
            State = DigitalState.Up;
        }

        public Digital(Analog analog) : base(analog.Name, true) {
            Name = analog.Name;
            Modifier = analog.Modifier;
            Value = 0.0f;
            State = DigitalState.Up;
        }

        [JsonConstructor]
        private Digital() : base(string.Empty, true) { }

        public override bool Update(bool ignoreMod = false, uint context = 0x00000001) {
            if ((context & ContextBitmask) == 0)
                return false;

            try
            {
                int activeMod = 0;
                if (!ignoreMod)
                    activeMod = InputManager.GetModifier();
                if (activeMod == Modifier) {
                    if (InputUtils.GetKeyOrButtonDown(Name))
                        State = DigitalState.Down;
                    else if (InputUtils.GetKeyOrButton(Name))
                        State = DigitalState.Held;
                    else if (InputUtils.GetKeyOrButtonUp(Name))
                        State = DigitalState.Up;
                    else
                        State = DigitalState.None;
                } else {
                    State = State > DigitalState.None ? DigitalState.Up : DigitalState.None;
                }
                Value = State > 0 ? 1 : 0;
                return State != DigitalState.None;
            }
            catch (ArgumentException)
            {
                Utilities.Logger.Log($"Key {Name} is invalid." );
                return false;
            }
        }

        // Make sure you do this for new input types if it matters
        public override Analog WithModifier(int newMod)
            => new Digital(Name, newMod);

        public override bool Equals(object obj) {
            if (ReferenceEquals(obj, null) || !(obj is Digital))
                return false;
            return (obj as Digital).GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
            => base.GetHashCode();
    }

    [Flags]
    public enum ModKey
    {
        LeftShift = 0x0001,
        LeftCommand = 0x0002,
        LeftApple = 0x0004,
        LeftAlt = 0x0008,
        RightShift = 0x0010,
        RightCommand = 0x0020,
        RightApple = 0x0040,
        RightAlt = 0x0080,
        LeftControl = 0x0100,
        RightControl = 0x0200
    }

    // TODO: Should this be MouseDown or just like Mouse/MouseInput
    public class MouseDown: Digital {
        public Vector2D MousePosition { get; private set; }
        public MouseDown(string name, int modifier = 0, uint context = 0xFFFFFFFF): base(name, modifier, context: context) { }
        public override bool Update(bool ignoreMod = false, uint context = 0x00000001) {
            if ((context & ContextBitmask) == 0)
                return false;

            var r = base.Update(ignoreMod);
            MousePosition = ((UnityEngine.Vector2)UnityEngine.Input.mousePosition).Map();
            return r;
        }

        public static readonly MouseDown LeftMouseButton = new MouseDown("Mouse 0");
        public static readonly MouseDown RightMouseButton = new MouseDown("Mouse 1");
        public static readonly MouseDown MiddleMouseButton = new MouseDown("Mouse 2");

        public override Analog WithModifier(int newMod)
            => new MouseDown(base.Name, newMod);
    }

    public enum DigitalState {
        Down = 1, Up = -1, Held = 2, None = 0
    }
}
