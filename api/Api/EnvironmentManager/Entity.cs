using System;

namespace SynthesisAPI.EnvironmentManager
{
    /// <summary>
    /// Entity is a 32 bit generational index
    /// 1st 16 bits represent index
    /// 2nd 16 bits represent generation
    /// </summary>
    public struct Entity
    {
        private UInt32 _value;

        public static implicit operator Entity(UInt32 value) => new Entity{ _value = value };

        public static implicit operator UInt32(Entity e) => e._value;

        public override string ToString() => _value.ToString();

        public override bool Equals(object obj) => Equals((Entity)obj);

        public bool Equals(Entity e) => _value == e._value;

        public bool Equals(UInt32 e) => _value == e;

        public bool Equals(int e) => _value == e;

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(Entity lhs, Entity rhs)
        {
            return lhs._value == rhs._value;
        }

        public static bool operator !=(Entity lhs, Entity rhs)
        {
            return !(lhs == rhs);
        }
    }

    public static class EntityExtensions
    {
        public static bool Equals(this UInt32 i, Entity e) => i == (UInt32)e;
        public static bool Equals(this int i, Entity e) => i == (UInt32)e;
    }
}
