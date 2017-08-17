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

        /// <summary>
        /// The global instance of the BRobotManager.
        /// </summary>
        public static BRobotManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BRobotManager();

                return _instance;
            }
        }

        /// <summary>
        /// Initializes a new BRobotManager instance.
        /// </summary>
        private BRobotManager()
        {
            raycastRobots = new List<RaycastRobot>();
        }

        /// <summary>
        /// Registers a raycast robot with the BRobotManager.
        /// </summary>
        /// <param name="raycastRobot"></param>
        public void RegisterRaycastRobot(RaycastRobot raycastRobot)
        {
            if (!raycastRobots.Contains(raycastRobot))
                raycastRobots.Add(raycastRobot);
        }

        /// <summary>
        /// Deregisters a raycast robot with the BRobotManager.
        /// </summary>
        /// <param name="raycastRobot"></param>
        public void DeregisterRaycastRobot(RaycastRobot raycastRobot)
        {
            if (raycastRobots.Contains(raycastRobot))
                raycastRobots.Remove(raycastRobot);
        }

        /// <summary>
        /// Updates all registerd raycast robots.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="timeStep"></param>
        public void UpdateRaycastRobots(DynamicsWorld world, float timeStep, int framesPassed)
        {
            foreach (RaycastRobot r in raycastRobots)
                r.UpdateVehicle(timeStep);
        }
    }
}
