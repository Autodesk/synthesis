using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthesisAPI.InputManager.Axis
{
    /// <summary>
    /// Interface for axis input. These are intended to be a read in a single direction.
    /// Either positive or negative
    /// </summary>
    public interface IAxisInput
    {
        /// <summary>
        /// Get the value of the Axis
        /// </summary>
        /// <param name="positiveOnly">To help make the results more favorable for the application</param>
        /// <returns></returns>
        float GetValue(bool positiveOnly = false);
    }
}
