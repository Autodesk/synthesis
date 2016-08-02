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
        public static void OrientRobot(List<BulletRigidNode> wheels, CollisionObject parent)
        {
            //Quaternion q = new Quaternion();
            //List<Vector3> wheelPosns = wheels.ConvertAll(w => w.BulletObject.WorldTransform.ExtractTranslation());

            //if (wheelPosns.Count < 3)
            //    return;

            //Vector3 a = wheelPosns[0] - wheelPosns[1];
            //Vector3 b = a;

            ////I have no idea how this works, I just copied it form the Unity version
            //for (int i = 2;Math.Abs(Vector3.Dot(a, b) / (a.Length * b.Length)) < 0.9 && i < wheels.Count; i++)
            //    b = wheelPosns[0] - wheelPosns[i];

            //Vector3 norm = Vector3.Cross(a, b).Normalized();

            parent.WorldTransform = Matrix4.CreateFromQuaternion(new Quaternion(Vector3.UnitY, 0)) * Matrix4.CreateTranslation(0, 0, 0);
        }

        public static bool RightRobot(List<CollisionObject> wheels, CollisionObject parent)
        {
            Quaternion q = new Quaternion();
            List<Vector3> wheelPosns = wheels.ConvertAll(w => w.WorldTransform.ExtractTranslation());

            return false;
        }
    }
}
