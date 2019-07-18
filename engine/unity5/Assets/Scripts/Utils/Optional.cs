using System;

namespace Synthesis
{
    public class Optional<T>
    {
        private T value;
        private bool isValid;

        public Optional()
        {
            isValid = false;
        }

        public Optional(T v)
        {
            value = v;
            isValid = true;
        }

        public bool IsValid()
        {
            return isValid;
        }

        public void Set(T v)
        {
            value = v;
            isValid = true;
        }

        public T Get()
        {
            if (isValid)
            {
                return value;
            }
            throw new Exception("Maybe type invalid");
        }
        public void Map(Func<T, T> f)
        {
            Set(f(value));
        }
        public static Optional<R> Map<R>(Func<T, R> f, Optional<T> t)
        {
            if (t.IsValid())
                return new Optional<R>(f(t.value));
            return new Optional<R>();
        }

        public static Func<Optional<T>, Optional<R>> Lift<R>(Func<T, R> f)
        {
            return (Optional<T> t) =>
            {
                if (t.IsValid())
                {
                    return new Optional<R>(f(t.value));
                }
                return new Optional<R>();
            };
        }
    }
}