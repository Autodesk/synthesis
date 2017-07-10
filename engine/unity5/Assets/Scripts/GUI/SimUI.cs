using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.FSM
{
    /// <summary>
    /// SimUI serves as an interface between the Unity button UI and the various functions within the simulator.
    /// It acomplishes this by having a public function for each button that interacts with the Main State to complete various tasks.
    /// </summary>
    public class SimUI : MonoBehaviour
    {

        MainState main;
        DynamicCamera camera;

        /// <summary>
        /// Retreives the Main State instance which controls everything in the simulator.
        /// </summary>
        void Start()
        {
        }

        private void Update()
        {
            if (main == null)
            {
                main = transform.GetComponent<StateMachine>().GetMainState();
                camera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();
            }
        }

        /// <summary>
        /// Resets the robot
        /// </summary>
        public void PressReset()
        {
            main.ResetRobot();
        }

        //Camera Functions
        public void SwitchCameraFreeroam()
        {
            camera.SwitchCameraState(0);
        }

        public void SwitchCameraOrbit()
        {
            camera.SwitchCameraState(1);
        }

        public void SwitchCameraDriverStation()
        {
            camera.SwitchCameraState(2);
        }

        //Orient Robot Functions
        public void OrientStart()
        {
            main.StartOrient();
        }

        public void OrientLeft()
        {
            main.RotateRobot(new Vector3(Mathf.PI * 0.25f, 0f, 0f));
        }

        public void OrientRight()
        {
            main.RotateRobot(new Vector3(-Mathf.PI * 0.25f, 0f, 0f));
        }

        public void OrientForward()
        {
            main.RotateRobot(new Vector3(0f, 0f, Mathf.PI * 0.25f));
        }

        public void OrientBackward()
        {
            main.RotateRobot(new Vector3(0f, 0f, -Mathf.PI * 0.25f));
        }

        public void OrientSave()
        {
            main.SaveOrientation();
        }

        public void OrientEnd()
        {
            //To be filled in later when UI work has been done
        }

        public void OrientDefault()
        {
            main.ResetOrientation();
        }
        
    }
}
