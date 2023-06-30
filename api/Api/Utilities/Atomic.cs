using System.Threading;

namespace SynthesisAPI.Utilities {

    public class Atomic<T> where T: struct {
        private readonly ReaderWriterLockSlim _lock;
        private T _value;
        public T Value {
            get {
                _lock.EnterReadLock();
                var result = _value;
                _lock.ExitReadLock();
                return result;
            }
            set {
                _lock.EnterWriteLock();
                _value = value;
                _lock.ExitWriteLock();
            }
        }

        public Atomic(T val) {
            _value = val;
            _lock = new ReaderWriterLockSlim();
        }

        public static implicit operator Atomic<T>(T value) => new Atomic<T>(value);
        public static implicit operator T(Atomic<T> atomic) => atomic.Value;
    }

}