using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synthesis.FEA
{
    /// <summary>
    /// A simple queue that contains a fixed amounts of elements.
    /// Elements pushed out of range are overwritten by new elements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedQueue<T> : IEnumerable<T>
    {
        int marker;
        private T[] values;

        /// <summary>
        /// Returns the length of the FixedQueue.
        /// </summary>
        public int Length
        {
            get
            {
                return values.Length;
            }
        }

        /// <summary>
        /// Returns the newest element of the FixedQueue.
        /// </summary>
        public T Front
        {
            get
            {
                return marker == 0 ? values[values.Length - 1] : values[marker - 1];
            }
        }

        /// <summary>
        /// Returns the oldest element of the FixedQueue.
        /// </summary>
        public T Back
        {
            get
            {
                return values[marker];
            }
        }

        /// <summary>
        /// Returns an element from the given index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= values.Length)
                    throw new IndexOutOfRangeException(index + " is out of range for the FixedQueue with size "
                        + values.Length + ".");

                int realIndex = marker - 1 - index;
                return realIndex < 0 ? values[(values.Length) + realIndex] : values[realIndex];
            }
        }

        /// <summary>
        /// Initializes a new FixedQueue of the given size.
        /// </summary>
        /// <param name="size"></param>
        public FixedQueue(int size)
        {
            marker = 0;
            values = new T[size];
        }

        /// <summary>
        /// Initializes a new FixedQueue of the given size with the given initial value for each element.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="initValue"></param>
        public FixedQueue(int size, T initValue) : this(size)
        {
            for (int i = 0; i < values.Length; i++)
                values[i] = initValue;
        }

        /// <summary>
        /// Clears the FixedQueue with the given value.
        /// </summary>
        /// <param name="value"></param>
        public void Clear(T value)
        {
            marker = 0;

            for (int i = 0; i < values.Length; i++)
                values[i] = value;
        }

        /// <summary>
        /// Adds a new element to the front of the FixedQueue.
        /// </summary>
        /// <param name="value"></param>
        public void Add(T value)
        {
            values[marker] = value;

            if (++marker == values.Length)
                marker = 0;
        }

        /// <summary>
        /// Used for enumerating through each value in the FixedQueue.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = marker - 1; i >= 0; i--)
                yield return values[i];

            for (int i = values.Length - 1; i >= marker; i--)
                yield return values[i];
        }

        /// <summary>
        /// A generic IEnumerator used for enumerating through each value in the FixedQueue.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
