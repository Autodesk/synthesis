using System.Threading;

namespace SynthesisAPI.Utilities {

    /// <summary>
    /// Multiple reads, one write allowed. Threadsafe value-type data. ReadOnly
    /// </summary>
    /// <typeparam name="T">Value Type</typeparam>
    public class AtomicReadOnly<T> where T : struct {
        protected readonly ReaderWriterLockSlim _lock;
        protected T _value;

        /// <summary>
        /// Threadsafe Data Accessor. Data is copied on get
        /// </summary>
        public T Value {
            get {
                _lock.EnterReadLock();
                var result = _value;
                _lock.ExitReadLock();
                return result;
            }
        }

        /// <summary>
        /// Constructs an Atomic with read only access to the data
        /// </summary>
        /// <param name="val"></param>
        public AtomicReadOnly(T val) {
            _value = val;
            _lock = new ReaderWriterLockSlim();
        }

        public static implicit operator T(AtomicReadOnly<T> atomic) => atomic.Value;
    }

    /// <summary>
    /// Multiple reads, one write allowed. Threadsafe value-type data
    /// </summary>
    /// <typeparam name="T">Value Type</typeparam>
    public class Atomic<T> : AtomicReadOnly<T> where T : struct {
        
        /// <summary>
        /// Threadsafe Data Accessor. Data is copied on get
        /// </summary>
        public new T Value {
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

        /// <summary>
        /// Constructs a new Atomic with read and write access to the stored data
        /// </summary>
        /// <param name="val">Data to store in Atomic</param>
        /// <returns></returns>
        public Atomic(T val) : base(val) { }

        /// <summary>
        /// Converts Atomic to a ReadOnlyAtomic
        /// </summary>
        /// <returns><see cref="SynthesisAPI.Utilities.AtomicReadOnly{T}" /></returns>
        public AtomicReadOnly<T> AsReadOnly() => this;
    }

}