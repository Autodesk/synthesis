using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BulletUnity;
using Synthesis.FSM;
using System.IO;
using Synthesis.GUI;
using Synthesis.States;
using Synthesis.Utils;
using Synthesis.Input;
using System.Linq;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

namespace Synthesis.MixAndMatch
{
    public class MaMSimUI : LinkedMonoBehaviour<MainState>
    {
        #region Variables
        #endregion

        private struct DriveBase
        {
            public string path;
            public DriveBase(string path)
            {
                this.path = path;
            }
        }
        private struct Manipulator
        {
            public string path;
            public Manipulator(string path)
            {
                this.path = path;
            }
        }
        private struct Wheel
        {
            public string path;
            public float mass;
            public float radius;
            public float friction;
            public float lateralFriction;
            public Wheel(string path, float mass, float radius, float friction, float lateralFriction)
            {
                this.path = path;
                this.mass = mass;
                this.radius = radius;
                this.friction = friction;
                this.lateralFriction = lateralFriction;
            }
        }

        private Dictionary<string, DriveBase> driveBases = new Dictionary<string, DriveBase>();
        private Dictionary<string, Manipulator> manipulators = new Dictionary<string, Manipulator>();
        private Dictionary<string, Wheel> wheels = new Dictionary<string, Wheel>();

        private DriveBase selectedDriveBase;
        private Manipulator selectedManipulator;
        private Wheel selectedWheel;

        void Start()
        {
            addMixAndMatchObj();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void LoadRobot(bool change)
        {
            string baseDirectory = selectedDriveBase.path;
            string manipulatorDirectory = selectedManipulator.path;

            PlayerPrefs.SetString("simSelectedReplay", string.Empty);

            RobotTypeManager.IsMixAndMatch = true;
            RobotTypeManager.RobotPath = baseDirectory;
            if (manipulatorDirectory.Equals(""))
                RobotTypeManager.HasManipulator = false;
            else
            {
                RobotTypeManager.HasManipulator = true;
                RobotTypeManager.ManipulatorPath = manipulatorDirectory;
            }

            RobotTypeManager.WheelPath = selectedWheel.path;
            RobotTypeManager.SetWheelProperties(selectedWheel.mass, selectedWheel.radius, selectedWheel.friction, selectedWheel.lateralFriction);

            if (change) StateMachine.SceneGlobal.gameObject.GetComponent<SimUI>().MaMChangeRobot(baseDirectory, manipulatorDirectory);
            else StateMachine.SceneGlobal.gameObject.GetComponent<LocalMultiplayer>().AddMaMRobot(baseDirectory, manipulatorDirectory, RobotTypeManager.HasManipulator);
        }

        public void SetDriveBase(GameObject g)
        {
            selectedDriveBase = driveBases[g.name];
        }

        public void SetManipulator(GameObject g)
        {
            selectedManipulator = manipulators[g.name];
        }

        public void SetWheelType(GameObject g)
        {
            selectedWheel = wheels[g.name];
        }

        private void addMixAndMatchObj()
        {
            driveBases.Add("", new DriveBase(""));
            manipulators.Add("NONE", new Manipulator(""));
            wheels.Add("", new Wheel("", 0f, 0f, 0f, 0f));
        }
        public void StartMaMSim()
        {
            selectedDriveBase = driveBases["Default"];
            selectedManipulator = manipulators["None"];
            selectedWheel = wheels["Traction"];
        }
    }
}
