namespace SynthesisAPI.Utilities {
    /// <summary>
    /// Turns a String reference type into a immutable String value type. Created for use in SynthesisAPI.Utlities.Atomic
    /// </summary>
    public struct StringValue {
        private string _str;

        public int Length => _str.Length;

        public char this[int i] => _str[i];

        public StringValue(string str) {
            _str = str.Substring(0);
        }

        public static implicit operator string(StringValue s) => s._str.Substring(0);
        public static implicit operator StringValue(string s) => new StringValue(s);

        public override string ToString() => this;
        public override int GetHashCode() => _str.GetHashCode();
        public override bool Equals(object obj) => _str.Equals(obj);
    }
}