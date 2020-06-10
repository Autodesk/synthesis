using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.Simulator.Interaction;

namespace Synthesis.Simulator
{
    public class Robot : MonoBehaviour, ISelectable
    {
        Vector3 ISelectable.Position => transform.position;

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