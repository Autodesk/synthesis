using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Synthesis.FSM;
using Synthesis.GUI;

namespace Synthesis.MixAndMatch
{
    public class MixAndMatchMode : MonoBehaviour
    {
        #region Variables
        private GameObject mixAndMatchMode;
        private GameObject mixAndMatchModeScript;
        private GameObject infoText;
        private GameObject mecWheelPanel;

        //Wheel options
        private GameObject tractionWheel;
        private GameObject colsonWheel;
        private GameObject omniWheel;
        private GameObject pneumaticWheel;
        [HideInInspector] public List<GameObject> Wheels;
        public static int SelectedWheel; //This is public static so that it can be accessed by RNMesh


        //Drive Base options
        private GameObject defaultDrive;
        private GameObject mecanumDrive;
        private GameObject swerveDrive;
        private GameObject narrowDrive;
        [HideInInspector] public List<GameObject> Bases;
        int selectedDriveBase;
        public static bool IsMecanum = false;

        //Manipulator Options
        private GameObject noManipulator;
        private GameObject syntheClaw;
        private GameObject syntheShot;
        private GameObject lift;
        [HideInInspector] public List<GameObject> Manipulators;
        int selectedManipulator;
        public static bool HasManipulator = true;

        //Scroll buttons
        private GameObject wheelRightScroll;
        private GameObject wheelLeftScroll;
        private GameObject driveBaseRightScroll;
        private GameObject driveBaseLeftScroll;
        private GameObject manipulatorRightScroll;
        private GameObject manipulatorLeftScroll;

        private Color32 orange = new Color32(241, 133, 24, 255); //new Color(0.757f, 0.200f, 0.757f);
        #endregion
        // Use this for initialization
        private void Awake()
        {

        }
        void Start()
        {
            FindAllGameObjects();
            StartMixAndMatch();

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void FindAllGameObjects()
        {
            mixAndMatchMode = GameObject.Find("MixAndMatchMode");
            mixAndMatchModeScript = GameObject.Find("MixAndMatchModeScript");
            infoText = GameObject.Find("PartDescription");
            Text txt = infoText.GetComponent<Text>();
            mecWheelPanel = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("MecWheelLabel")).First();

            //Find wheel objects
            tractionWheel = GameObject.Find("TractionWheel");
            colsonWheel = GameObject.Find("ColsonWheel");
            omniWheel = GameObject.Find("OmniWheel");
            pneumaticWheel = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("PneumaticWheel")).First();
            //Put all the wheels in the wheels list
            Wheels = new List<GameObject> { tractionWheel, colsonWheel, omniWheel, pneumaticWheel };


            //Find drive base objects
            defaultDrive = GameObject.Find("DefaultBase");
            mecanumDrive = GameObject.Find("MecanumBase");
            swerveDrive = GameObject.Find("SwerveBase");
            narrowDrive = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("NarrowBase")).First();
            //Put all the drive bases in the bases list
            Bases = new List<GameObject> { defaultDrive, mecanumDrive, swerveDrive, narrowDrive };

            //Find manipulator objects
            noManipulator = GameObject.Find("NoManipulator");
            syntheClaw = GameObject.Find("SyntheClaw");
            syntheShot = GameObject.Find("SyntheShot");
            lift = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("Lift")).First();
            //Put all the manipulators in the manipulators list
            Manipulators = new List<GameObject> { noManipulator, syntheClaw, syntheShot, lift };

            //Find all the scroll buttons
            wheelRightScroll = GameObject.Find("WheelRightScroll");
            wheelLeftScroll = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("WheelLeftScroll")).First();
            driveBaseRightScroll = GameObject.Find("BaseRightScroll");
            driveBaseLeftScroll = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("BaseLeftScroll")).First(); ;
            manipulatorRightScroll = GameObject.Find("ManipulatorRightScroll");
            manipulatorLeftScroll = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("ManipulatorLeftScroll")).First();

            if (this.gameObject.name == "MixAndMatchModeScript")
            {
                this.gameObject.GetComponent<MaMScroller>().FindAllGameObjects();
                this.gameObject.GetComponent<MaMInfoText>().FindAllGameObjects();
            }
        }

        /// <summary>
        /// Called when the Mix and Match Configuration tab is opened from the main menu. 
        /// </summary>
        public void StartMixAndMatch()
        {
            if (this.gameObject.name == "MixAndMatchModeScript")
            {
                wheelLeftScroll.SetActive(false);
                driveBaseLeftScroll.SetActive(false);
                manipulatorLeftScroll.SetActive(false);

                mecWheelPanel.SetActive(false);

                SelectWheel(0);
                SelectDriveBase(0);
                SelectManipulator(0);

                this.gameObject.GetComponent<MaMScroller>().ResetFirsts();
                this.gameObject.GetComponent<MaMScroller>().FindAllGameObjects();
                this.gameObject.GetComponent<MaMInfoText>().FindAllGameObjects();
                // Sets info panel to blank
                Text txt = infoText.GetComponent<Text>();
                txt.text = "";
            }
        }

        /// <summary>
        /// Sets the destination paths of the selected field, robot base and manipulator to be used by MainState. Starts the simulation in Quick Swap Mode. 
        /// </summary>
        public void StartMaMSim()
        {
            RobotTypeManager.IsMixAndMatch = true;
            RobotTypeManager.RobotPath = mixAndMatchModeScript.GetComponent<MaMGetters>().GetDriveBase(selectedDriveBase);
            RobotTypeManager.ManipulatorPath = mixAndMatchModeScript.GetComponent<MaMGetters>().GetManipulator(selectedManipulator);
            RobotTypeManager.WheelPath = mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheel(SelectedWheel);

            RobotTypeManager.SetWheelProperties(mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelMass(SelectedWheel),
                mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelRadius(SelectedWheel),
                mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelFriction(SelectedWheel),
                mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelLateralFriction(SelectedWheel));
            PlayerPrefs.SetString("simSelectedReplay", string.Empty);
            SceneManager.LoadScene("Scene");

            AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.MainSimulator,
                AnalyticsLedger.TimingVarible.Playing,
                AnalyticsLedger.TimingLabel.MixAndMatch);
        }

        #region Change or Add MaM Robot
        /// <summary>
        /// Called when the "next" button on the MaM panel is clicked within the simulator. 
        /// Determines if the user wants to change the active robot or add a robot for local multiplayer and calls the correct function.
        /// </summary>
        bool changeMaMRobot = true;
        public void ChangeOrAddMaMRobot()
        {
            if (changeMaMRobot)
            {
                ChangeMaMRobot();
            }
            else
            {
                AddMaMRobot();
            }
        }

        public void ChangeMaMClicked()
        {
            changeMaMRobot = true;
        }

        public void AddMaMClicked()
        {
            changeMaMRobot = false;
        }

        /// <summary>
        /// When the user changes wheels/drive bases/manipulators within the simulator, changes the robot.
        /// </summary>
        void ChangeMaMRobot()
        {
            string baseDirectory = mixAndMatchModeScript.GetComponent<MaMGetters>().GetDriveBase(selectedDriveBase);
            string manipulatorDirectory = mixAndMatchModeScript.GetComponent<MaMGetters>().GetManipulator(selectedManipulator);

            PlayerPrefs.SetString("simSelectedReplay", string.Empty);

            RobotTypeManager.IsMixAndMatch = true;
            RobotTypeManager.RobotPath = baseDirectory;
            if (selectedManipulator == 0)
            {
                RobotTypeManager.HasManipulator = false;
            }
            else
            {
                RobotTypeManager.HasManipulator = true;
                RobotTypeManager.ManipulatorPath = manipulatorDirectory;
            }

            RobotTypeManager.WheelPath = mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheel(SelectedWheel);
            RobotTypeManager.SetWheelProperties(mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelMass(SelectedWheel),
                mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelRadius(SelectedWheel),
                mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelFriction(SelectedWheel),
                mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelLateralFriction(SelectedWheel));

            StateMachine.SceneGlobal.gameObject.GetComponent<SimUI>().MaMChangeRobot(baseDirectory, manipulatorDirectory);

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.ChangeRobot,
                AnalyticsLedger.EventAction.Changed,
                "Robot - Mix and Match",
                AnalyticsLedger.getMilliseconds().ToString());
        }

        /// <summary>
        /// When the user adds a MaMRobot in  multiplayer mode, sets the player prefs to file paths of robot parts
        /// </summary>
        void AddMaMRobot()
        {
            string baseDirectory = mixAndMatchModeScript.GetComponent<MaMGetters>().GetDriveBase(selectedDriveBase);
            string manipulatorDirectory = mixAndMatchModeScript.GetComponent<MaMGetters>().GetManipulator(selectedManipulator);

            RobotTypeManager.IsMixAndMatch = true;
            RobotTypeManager.RobotPath = mixAndMatchModeScript.GetComponent<MaMGetters>().GetDriveBase(selectedDriveBase);
            if (selectedManipulator == 0)
            {
                RobotTypeManager.HasManipulator = false;
            }
            else
            {
                RobotTypeManager.HasManipulator = true;
                RobotTypeManager.ManipulatorPath = manipulatorDirectory;
            }
            RobotTypeManager.WheelPath = mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheel(SelectedWheel);

            RobotTypeManager.SetWheelProperties(mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelMass(SelectedWheel),
                mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelRadius(SelectedWheel),
                mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelFriction(SelectedWheel),
                mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelLateralFriction(SelectedWheel));


            PlayerPrefs.SetString("simSelectedReplay", string.Empty);
            GameObject simulatorObject = GameObject.Find("Simulator");

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.AddRobot,
                AnalyticsLedger.EventAction.Changed,
                "Local Multiplayer - Mix and Match Robot",
                AnalyticsLedger.getMilliseconds().ToString());

            simulatorObject.GetComponent<LocalMultiplayer>().AddMaMRobot(baseDirectory, manipulatorDirectory, RobotTypeManager.HasManipulator);
        }
        #endregion

        #region Selecters
        /// <summary>
        /// Selects a wheel, as referenced by its index in the wheels list.
        /// </summary>
        /// <param name="wheel"></param>
        public void SelectWheel(int wheel)
        {
            //unselects all wheels
            for (int i = 0; i < Wheels.Count; i++)
            {
                SetColor(Wheels[i], Color.white);
            }

            //selects the wheel that is clicked
            SetColor(Wheels[wheel], orange);
            this.gameObject.GetComponent<MaMInfoText>().SetWheelInfoText(wheel);
            SelectedWheel = wheel;
        }

        /// <summary>
        /// Selects a drive base, as referenced by its index in the bases list
        /// </summary>
        /// <param name="driveBase"></param>
        public void SelectDriveBase(int driveBase)
        {
            //unselects all drive bases
            for (int j = 0; j < Bases.Count; j++)
            {
                SetColor(Bases[j], Color.white);
            }

            //selects the wheel that is clicked
            SetColor(Bases[driveBase], orange);
            this.gameObject.GetComponent<MaMInfoText>().SetBaseInfoText(driveBase);
            selectedDriveBase = driveBase;
            mecWheelPanel = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("MecWheelLabel")).First();
            mecWheelPanel.SetActive(false);
            if (selectedDriveBase == 1)
            {
                IsMecanum = true;
                mecWheelPanel.SetActive(true);
            }
        }

        public static bool GetMecanum()
        {
            return IsMecanum;
        }

        /// <summary>
        /// Selects a manipulator, as referenced by its index in the manipualtors list.
        /// </summary>
        /// <param name="manipulator"></param>
        public void SelectManipulator(int manipulator)
        {
            //unselects all manipulators
            for (int k = 0; k < Manipulators.Count; k++)
            {
                SetColor(Manipulators[k], Color.white);
            }

            //selects the manipulator that is clicked
            SetColor(Manipulators[manipulator], orange);
            this.gameObject.GetComponent<MaMInfoText>().SetManipulatorInfoText(manipulator);
            selectedManipulator = manipulator;
        }

        /// <summary>
        /// Sets the color for selecting/unselecting parts.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="color"></param>
        public void SetColor(GameObject part, Color color)
        {
            part.GetComponent<Image>().color = color;
        }
        #endregion
    }
}