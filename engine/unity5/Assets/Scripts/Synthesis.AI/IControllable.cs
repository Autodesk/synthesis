using UnityEngine;
using System.Collections;

public interface IControllable
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns>Returns the forward vector of the robot</returns>
    Vector3 GetForward();

    /// <summary>
    /// Moves the robot's wheels to accelerate either forwards or backwards
    /// </summary>
    /// <param name="acceleration">A number from -1 to 1, to accelerate by</param>
    void Accelerate(float acceleration);

    /// <summary>
    /// Moves the robot's wheels to turn left or right. Magnitude matters. Higher numbers will turn sharper.
    /// </summary>
    /// <param name="direction">A number from -1 to 1, to turn by. -1 is Left, 1 is Right</param>
    void Turn(float direction);

    /// <summary>
    /// Calculates average center of robot based on node positions.
    /// </summary>
    /// <returns>Average center of robot</returns>
    Vector3 GetPosition();
}
