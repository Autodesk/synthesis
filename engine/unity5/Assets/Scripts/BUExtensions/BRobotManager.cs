using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.BUExtensions
{
    public class BRobotManager
    {
        List<RaycastRobot> raycastRobots;

        private static BRobotManager _instance;
        public static BRobotManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BRobotManager();

                return _instance;
            }
        }

        private BRobotManager()
        {
            raycastRobots = new List<RaycastRobot>();
        }

        public void RegisterRaycastRobot(RaycastRobot raycastRobot)
        {
            if (!raycastRobots.Contains(raycastRobot))
                raycastRobots.Add(raycastRobot);
        }

        public void DeregisterRaycastRobot(RaycastRobot raycastRobot)
        {
            if (raycastRobots.Contains(raycastRobot))
                raycastRobots.Remove(raycastRobot);
        }

        public void UpdateRaycastRobots(DynamicsWorld world, float timeStep)
        {
            foreach (RaycastRobot r in raycastRobots)
                r.UpdateVehicle(timeStep);
        }
    }
}
