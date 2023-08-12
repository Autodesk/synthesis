namespace SynthesisAPI.Utilities {
    public class Ref<T> where T : struct {
        private T _value;
        public T Value {
            get => _value;
            set => _value = value;
        }
        
        public Ref(T val) {
            _value = val;
        }

        public static Ref<T> Default => new Ref<T>(default);

        public static implicit operator Ref<T>(T val) => new Ref<T>(val);
    }
}