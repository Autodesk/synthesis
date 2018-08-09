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
        private static XDocument file;
        public static List<DriverPractice> dpmodes = new List<DriverPractice>();
        public static List<Interactor> intakeInteractor = new List<Interactor>();
        #endregion
        #region fileWriting
        public static void WriteRobot()
        {
            XElement robot = new XElement("RobotData", null);
            robot.Add(DriverStationData());
            robot.Save(StateMachine.SceneGlobal.FindState<MainState>().ActiveRobot.FilePath + "\\" + "robot_data.xml");
        }
        private static XElement DriverStationData()
        {
            XElement dps = new XElement("DriverPractice", null);
            for (int i = 0; i < dpmodes.Count(); i++)
            {
                XElement dp = new XElement("Mode" + i.ToString(),
                                    new XElement("Gamepiece",
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
        public static void Load()
        {
            if (File.Exists(PlayerPrefs.GetString("simSelectedRobot") + "\\" + "robot_data.xml"))
            {

                file = XDocument.Load(PlayerPrefs.GetString("simSelectedRobot") + "\\" + "robot_data.xml");
                dpmodes = getDriverPractice();
            }
            else WriteRobot();
        }
        public static void Load(string filePath)
        {
            if (File.Exists(PlayerPrefs.GetString("simSelectedRobot") + "\\" + "robot_data.xml"))
            {

                file = XDocument.Load(filePath + "\\" + "robot_data.xml");
                dpmodes = getDriverPractice();
            }
            else WriteRobot();
        }
        private static List<DriverPractice> getDriverPractice()
        {
            List<DriverPractice> dp = new List<DriverPractice>();
            foreach(XElement e in file.Root.Element("DriverPractice").Elements())
            {

                dp.Add(new DriverPractice(e));
            }
            return dp;
        }
        #endregion
    }
}
