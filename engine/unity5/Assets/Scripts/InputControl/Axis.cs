using Synthesis.InputControl.Enums;
using UnityEngine;

namespace Synthesis.InputControl
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

        public string MName
        {
            get
            {
                return MName1;
            }

            set
            {
                MName1 = value;
            }
        }

        public string MName1 { get; set; }

        /// <summary>
        /// Create a new instance of <see cref="Axis"/> with specified negative <see cref="KeyMapping"/> and positive <see cref="KeyMapping"/>.
        /// </summary>
        /// <param name="name">Axis name.</param>
        /// <param name="negativeKeyMapping">Negative KeyMapping.</param>
        /// <param name="positiveKeyMapping">Positive KeyMapping.</param>
        public Axis(string name, KeyMapping negativeKeyMapping, KeyMapping positiveKeyMapping)
        {
            MName = name;
            Inverted = false;

            set(negativeKeyMapping, positiveKeyMapping);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Axis"/> class based on another instance.
        /// </summary>
        /// <param name="another">Another Axis instance.</param>
        public Axis(Axis another)
        {
            MName = another.MName;

            set(another);
        }

        /// <summary>
        /// Set the same negative <see cref="KeyMapping"/> and positive <see cref="KeyMapping"/> as in another instance.
        /// </summary>
        /// <param name="another">Another Axis instance.</param>
        public void set(Axis another)
        {
            mNegative = another.mNegative;
            mPositive = another.mPositive;
            Inverted = another.Inverted;
        }

        /// <summary>
        /// Set negative <see cref="KeyMapping"/> and positive <see cref="KeyMapping"/>.
        /// </summary>
        /// <param name="negativeKeyMapping">Negative KeyMapping.</param>
        /// <param name="positiveKeyMapping">Positive KeyMapping.</param>
        public void set(KeyMapping negativeKeyMapping, KeyMapping positiveKeyMapping)
        {
            Negative = negativeKeyMapping;
            Positive = positiveKeyMapping;
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
                return mNegative.getValue(exactKeyModifiers, MName, device: device) - mPositive.getValue(exactKeyModifiers, MName, device);
            }
            else
            {
                return mPositive.getValue(exactKeyModifiers, MName, device) - mNegative.getValue(exactKeyModifiers, MName, device);
            }
        }
    }
}
