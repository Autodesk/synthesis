using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Synthesis.Robot
{
    /// <summary>
    /// An interface used to get access to a robot contained by some other instance.
    /// </summary>
    public interface IRobotProvider
    {
        /// <summary>
        /// Used for accessing the robot provided by this instance.
        /// </summary>
        /// <returns>The robot provided by this instance.</returns>
        GameObject Robot { get; }

        /// <summary>
        /// If true, the active robot is in its active state.
        /// </summary>
        bool RobotActive { get; }
    }
}
