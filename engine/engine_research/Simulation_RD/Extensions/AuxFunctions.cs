using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulation_RD.SimulationPhysics;
using BulletSharp;
using OpenTK;

namespace Simulation_RD.Extensions
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
            wheels.ForEach(w => w.BulletObject.WorldTransform *= Matrix4.CreateTranslation(new Vector3(0, 75, 0) - w.BulletObject.WorldTransform.ExtractTranslation()));
            parent.WorldTransform *= Matrix4.CreateTranslation(new Vector3(0, 75, 0) - parent.WorldTransform.ExtractTranslation());
        }        
    }
}
