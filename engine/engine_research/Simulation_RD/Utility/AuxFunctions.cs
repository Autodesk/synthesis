using System.Collections.Generic;
using System.Linq;
using Simulation_RD.SimulationPhysics;
using BulletSharp;
using OpenTK;

namespace Simulation_RD.Utility
{
    /// <summary>
    /// Extra Functions for manipulating the robot
    /// </summary>
    class AuxFunctions
    {
        /// <summary>
        /// Resets the robot to its starting position
        /// </summary>
        /// <param name="wheels"></param>
        /// <param name="parent"></param>
        public static void OrientRobot(List<BulletRigidNode> wheels, CollisionObject parent)
        {
            List<CollisionObject> rbs = (from w in wheels select w.BulletObject).Concat(new[] { parent }).ToList();
            Vector3 delta = new Vector3(0, 75, 0) - parent.WorldTransform.ExtractTranslation();
            rbs.ForEach(w => 
            {
                w.WorldTransform *= Matrix4.CreateTranslation(delta);
                w.InterpolationWorldTransform = Matrix4.Zero;
            });
        }        

        /// <summary>
        /// Calcus a rotation from one orientation to another
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static Quaternion RotateTo(Vector3 from, Vector3 to)
        {
            Vector3 axis = Vector3.Cross(from, to);
            float angle = Vector3.CalculateAngle(from, to);
            return new Quaternion(axis, angle);
        }
    }
}
