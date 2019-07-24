using Synthesis.Input.Enums;
using UnityEngine;

namespace Synthesis.Input
{
    /// <summary>
    /// <see cref="Axis"/> is a named handler for negative <see cref="KeyMapping"/> and positive <see cref="KeyMapping"/>.
    /// Source: https://github.com/Gris87/InputControl
    /// </summary>
    public class Axis
    {
        private KeyMapping mNegative;
        private KeyMapping mPositive;

        /// <summary>
        /// Gets the axis name.
        /// </summary>
        /// <value>Axis name.</value>
        public string Name
        {
            get
            {
                return MName;
            }
        }

        /// <summary>
        /// Gets or sets the negative KeyMapping. Please note that null value is prohibited.
        /// </summary>
        /// <value>Negative KeyMapping.</value>
        public KeyMapping Negative
        {
            get
            {
                return mNegative;
            }

            set
            {
                if (value == null)
                {
                    Debug.LogError("value can't be null");
                }

                mNegative = value;
            }
        }

        /// <summary>
        /// Gets or sets the positive KeyMapping. Please note that null value is prohibited.
        /// </summary>
        /// <value>Positive KeyMapping.</value>
        public KeyMapping Positive
        {
            get
            {
                return mPositive;
            }

            set
            {
                if (value == null)
                {
                    Debug.LogError("value can't be null");
                }

                mPositive = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Axis"/> is inverted.
        /// </summary>
        /// <value><c>true</c> if inverted; otherwise, <c>false</c>.</value>
        public bool Inverted { get; set; }

        public string MName { get; set; }

        /// <summary>
        /// Create a new instance of <see cref="Axis"/> with specified negative <see cref="KeyMapping"/> and positive <see cref="KeyMapping"/>.
        /// </summary>
        /// <param name="name">Axis name.</param>
        /// <param name="negativeKeyMapping">Negative KeyMapping.</param>
        /// <param name="positiveKeyMapping">Positive KeyMapping.</param>
        [Newtonsoft.Json.JsonConstructor]
        public Axis(string name, KeyMapping negativeKeyMapping, KeyMapping positiveKeyMapping, bool inverted = false)
        {
            set(name, negativeKeyMapping, positiveKeyMapping, inverted);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Axis"/> class based on another instance.
        /// </summary>
        /// <param name="another">Another Axis instance.</param>
        public Axis(Axis another)
        {
            set(another);
        }

        /// <summary>
        /// Set the same negative <see cref="KeyMapping"/> and positive <see cref="KeyMapping"/> as in another instance.
        /// </summary>
        /// <param name="another">Another Axis instance.</param>
        public void set(Axis another)
        {
            set(another.MName, another.mNegative, another.mPositive, another.Inverted);
        }

        /// <summary>
        /// Set negative <see cref="KeyMapping"/> and positive <see cref="KeyMapping"/>.
        /// </summary>
        /// <param name="negativeKeyMapping">Negative KeyMapping.</param>
        /// <param name="positiveKeyMapping">Positive KeyMapping.</param>
        public void set(KeyMapping negativeKeyMapping, KeyMapping positiveKeyMapping, bool inverted = false)
        {
            Inverted = inverted;
            Negative = negativeKeyMapping;
            Positive = positiveKeyMapping;
        }

        /// <summary>
        /// Set negative <see cref="KeyMapping"/> and positive <see cref="KeyMapping"/>.
        /// </summary>
        /// <param name="negativeKeyMapping">Negative KeyMapping.</param>
        /// <param name="positiveKeyMapping">Positive KeyMapping.</param>
        public void set(string name, KeyMapping negativeKeyMapping, KeyMapping positiveKeyMapping, bool inverted = false)
        {
            MName = name;
            set(negativeKeyMapping, positiveKeyMapping, inverted);
        }

        /// <summary>
        /// Returns axis value by using negative <see cref="KeyMapping"/> and positive <see cref="KeyMapping"/>.
        /// </summary>
        /// <returns>Axis value.</returns>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        /// <param name="device">Preferred input device.</param>
        public float getValue(bool exactKeyModifiers = false, InputDevice device = InputDevice.Any)
        {
            if (Inverted)
            {
                return mNegative.getValue(exactKeyModifiers, MName, device) - mPositive.getValue(exactKeyModifiers, MName, device);
            }
            else
            {
                return mPositive.getValue(exactKeyModifiers, MName, device) - mNegative.getValue(exactKeyModifiers, MName, device);
            }
        }
    }
}
