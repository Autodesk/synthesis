using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.Simulator
{
    /**
     * This class is responsible for handling robots, fields, and of its integration to the controller and 
     */
    public class SimulatorHandler
    {
        public delegate void RobotLoaded();

        private SimulatorHandler() { }

        private static SimulatorHandler instance;
        public static SimulatorHandler Instance {
            get {
                if (instance == null) instance = new SimulatorHandler();
                return instance;
            }
        }
    }
}