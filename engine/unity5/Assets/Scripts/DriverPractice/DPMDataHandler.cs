using BulletUnity;
using Synthesis.Field;
using Synthesis.FSM;
using Synthesis.States;
using Synthesis.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
namespace Synthesis.DriverPractice
{
    public class DPMDataHandler
    {
        #region values
        private static XDocument file; //robot_data.xml in corresponding robot folder
        public static List<DriverPractice> dpmodes = new List<DriverPractice>(); //List of available driver practice "modes"
        public static List<Interactor> intakeInteractor = new List<Interactor>(); //List of interactors associated with an intake
        #endregion
        #region fileWriting
        /// <summary>
        /// Writes to robot_data.xml file - does not append
        /// </summary>
        public static void WriteRobot()
        {
            //wrap all data
            XElement robot = new XElement("RobotData", null); //parent
            robot.Add(DriverPracticeData());
            //save function - saves to active robot instead of SimSelectedRobot to support multiplayer
            robot.Save(StateMachine.SceneGlobal.FindState<MainState>().ActiveRobot.FilePath + Path.DirectorySeparatorChar + "robot_data.xml");
        }
        /// <summary>
        /// Get Driver Practice "Mode" Data as an XElement - Split into Red and Blue Goals
        /// </summary>
        private static XElement DriverPracticeData()
        {
            XElement dps = new XElement("DriverPractice", null); //parent wrapper
            for (int i = 0; i < dpmodes.Count(); i++)
            {
                XElement dp = new XElement("Mode" + i.ToString(), //increments to create distinction doesn't really matter
                                    new XElement("Gamepiece", //instead of incrementing could use gamepiece to define mode
                                        new XAttribute("id", dpmodes[i].gamepiece)),
                                    new XElement("IntakeNode",
                                        new XAttribute("id", dpmodes[i].intakeNode)),
                                    new XElement("ReleaseNode",
                                        new XAttribute("id", dpmodes[i].releaseNode)),
                                    new XElement("ReleasePosition",
                                        new XAttribute("x", dpmodes[i].releasePosition.x),
                                        new XAttribute("y", dpmodes[i].releasePosition.y),
                                        new XAttribute("z", dpmodes[i].releasePosition.z)),
                                    new XElement("ReleaseVelocity",
                                        new XAttribute("x", dpmodes[i].releaseVelocity.x),
                                        new XAttribute("y", dpmodes[i].releaseVelocity.y),
                                        new XAttribute("z", dpmodes[i].releaseVelocity.z)));
                dps.Add(dp);
            }
            return dps;
        }
        #endregion
        #region getData
        /// <summary>
        /// Assigns global variables values from robot_data.xml
        /// </summary>
        /// <param name="fieldPath">location to robot folder passed upon robot load</param>
        public static void Load(string filePath)
        {
            if (File.Exists(filePath + Path.DirectorySeparatorChar + "robot_data.xml"))
            {

                file = XDocument.Load(filePath + Path.DirectorySeparatorChar + "robot_data.xml");
                dpmodes = getDriverPractice();
            }
            else WriteRobot(); //creates dummy file w/ basic values ex: intake/release node = node_0.bxda
        }
        /// <summary>
        /// Gets driver practice modes as list of DriverPractice objects from robot_data.xml
        /// </summary>
        private static List<DriverPractice> getDriverPractice()
        {
            List<DriverPractice> dp = new List<DriverPractice>();
            foreach (XElement e in file.Root.Element("DriverPractice").Elements())
            {

                dp.Add(new DriverPractice(e));
            }
            return dp;
        }
        #endregion
    }
}
