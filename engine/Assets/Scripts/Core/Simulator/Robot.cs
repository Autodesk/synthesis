using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.Simulator
{
    public class Robot
    {
        private DrivetrainType drivetrainType;
        private GameObject rootObject;

        public Robot(DrivetrainType type)
        {

        }

        public enum DrivetrainType {
            Tank, Swerve, NoDrive
        }
    }
}