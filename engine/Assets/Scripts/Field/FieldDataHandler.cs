using BulletUnity;
using Synthesis.DriverPractice;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace Synthesis.Field
{
    public class FieldDataHandler
    {
        #region values
        private static XDocument file; //file_data.xml in corresponding field folder
        public static List<Gamepiece> gamepieces = new List<Gamepiece>(); //List of available gamepieces
        public static List<List<GameObject>> redGoals = new List<List<GameObject>>(); //List of available red goals
        public static List<List<GameObject>> blueGoals = new List<List<GameObject>>(); //List of available blue goals
        public static Vector3 robotSpawn = new Vector3(0f, 3f, 0f); //robot spawn defaults to 0,3,0 if nothing defined
        public static int gamepieceIndex = 0;
        #endregion
        #region fileWriting
        /// <summary>
        /// Writes to field_data.xml file - does not append
        /// </summary>
        public static void WriteField()
        {
            //wrap all data
            XElement field = new XElement("FieldData", null); //parent
            field.Add(GoalData());
            field.Add(GeneralData());
            //save function
            field.Save(PlayerPrefs.GetString("simSelectedField") + Path.DirectorySeparatorChar + "field_data.xml");
        }
        /// <summary>
        /// Get Goal Data as an XElement - Split into Red and Blue Goals
        /// </summary>
        private static XElement GoalData()
        {
            //create goal elements
            XElement xRedGoals = new XElement("RedGoals");
            XElement xBlueGoals = new XElement("BlueGoals");
            for (int i = 0; i < gamepieces.Count(); i++)
            {
                if (redGoals.Count() > 0)
                {
                    XElement rGoals = new XElement("RedGoals" + i.ToString(), from g in redGoals[i]
                                                                              select new XElement("Goal", new XAttribute("Color", g.GetComponent<Goal>().color),
                                                                                     new XElement("Position",
                                                                                          new XAttribute("x", g.GetComponent<Goal>().position.x),
                                                                                          new XAttribute("y", g.GetComponent<Goal>().position.y),
                                                                                          new XAttribute("z", g.GetComponent<Goal>().position.z)),
                                                                                     new XElement("Scale",
                                                                                        new XAttribute("x", g.GetComponent<Goal>().scale.x),
                                                                                        new XAttribute("y", g.GetComponent<Goal>().scale.y),
                                                                                        new XAttribute("z", g.GetComponent<Goal>().scale.z)),
                                                                                     new XElement("Points", g.GetComponent<Goal>().pointValue),
                                                                                     new XElement("Description", g.GetComponent<Goal>().description),
                                                                                     new XElement("Keyword", g.GetComponent<Goal>().gamepieceKeyword)));
                    xRedGoals.Add(rGoals);
                }
                if (blueGoals.Count() > 0)
                {
                    XElement bGoals = new XElement("BlueGoals" + i.ToString(), from g in blueGoals[i]
                                                                               select new XElement("Goal", new XAttribute("Color", g.GetComponent<Goal>().color),
                                                                                      new XElement("Position",
                                                                                          new XAttribute("x", g.GetComponent<Goal>().position.x),
                                                                                          new XAttribute("y", g.GetComponent<Goal>().position.y),
                                                                                          new XAttribute("z", g.GetComponent<Goal>().position.z)),
                                                                                      new XElement("Scale",
                                                                                        new XAttribute("x", g.GetComponent<Goal>().scale.x),
                                                                                        new XAttribute("y", g.GetComponent<Goal>().scale.y),
                                                                                        new XAttribute("z", g.GetComponent<Goal>().scale.z)),
                                                                                      new XElement("Points", g.GetComponent<Goal>().pointValue),
                                                                                      new XElement("Description", g.GetComponent<Goal>().description),
                                                                                      new XElement("Keyword", g.GetComponent<Goal>().gamepieceKeyword)));
                    xBlueGoals.Add(bGoals);
                }
            }

            //wrap goal elements
            XElement goals = new XElement("Goals", null); //Goal parent
            goals.Add(xRedGoals); //red goal child
            goals.Add(xBlueGoals); //blue goal child
            return goals;
        }
        /// <summary>
        /// Get General Data as an XElement - Contains Gamepieces and RobotSpawnPoint
        /// </summary>
        private static XElement GeneralData()
        {
            XElement pieces = new XElement("Gamepieces", from g in gamepieces
                                                         select new XElement("gamepiece", new XAttribute("id", g.name),
                                                                                          new XAttribute("holdinglimit", g.holdingLimit),
                                                                                          new XAttribute("x", g.spawnpoint.x),
                                                                                          new XAttribute("y", g.spawnpoint.y),
                                                                                          new XAttribute("z", g.spawnpoint.z)));
            XElement robotSpawnPoint = new XElement("RobotSpawnPoint",
                                            new XAttribute("x", robotSpawn.x),
                                            new XAttribute("y", robotSpawn.y),
                                            new XAttribute("z", robotSpawn.z));
            //wrap general data
            XElement gen = new XElement("General", null); //parent
            gen.Add(pieces);
            gen.Add(robotSpawnPoint);
            return gen;
        }
        #endregion
        #region getData
        /// <summary>
        /// Assigns global variables values from field_data.xml
        /// </summary>
        /// <param name="fieldPath">location to field folder passed upon field load</param>
        public static void Load(string fieldPath)
        {
            if (File.Exists(fieldPath + Path.DirectorySeparatorChar + "field_data.xml"))
            {
                file = XDocument.Load(fieldPath + Path.DirectorySeparatorChar + "field_data.xml");
                gamepieces = getGamepieces();
                redGoals = getRedGoals();
                blueGoals = getBlueGoals();
                robotSpawn = getRobotSpawn();
                gamepieceIndex = 0;
            }
            else
            {
                gamepieces = new List<Gamepiece>();
                WriteField(); //creates dummy file - allows robot spawn point functionality (No gamepieces)
            }
        }
        /// <summary>
        /// Gets gamepiece as list of Gamepiece objects from field_data.xml
        /// </summary>
        private static List<Gamepiece> getGamepieces()
        {
            List<Gamepiece> pieces = new List<Gamepiece>();
            foreach (XElement g in file.Root.Element("General").Element("Gamepieces").Elements())
                pieces.Add(new Gamepiece(g));
            return pieces;
        }
        /// <summary>
        /// Gets red goals as list of list of Goal objects from field_data.xml
        /// </summary>
        private static List<List<GameObject>> getRedGoals()
        {
            List<List<GameObject>> goals = new List<List<GameObject>>();
            foreach (XElement z in file.Root.Element("Goals").Element("RedGoals").Elements())
            {
                List<GameObject> temp = new List<GameObject>();
                foreach (XElement e in z.Elements())
                {
                    GameObject g = new GameObject("Gamepiece" + goals.Count().ToString() + "Goal" + temp.Count().ToString());
                    BBoxShape collider = g.AddComponent<BBoxShape>();
                    BRigidBody rigid = g.AddComponent<BRigidBody>();
                    rigid.collisionFlags = rigid.collisionFlags | BulletSharp.CollisionFlags.NoContactResponse | BulletSharp.CollisionFlags.StaticObject;
                    Goal goal = g.AddComponent<Goal>();
                    collider.Extents = new UnityEngine.Vector3(0.5f, 0.5f, 0.5f);
                    goal.pointValue = int.Parse(e.Element("Points").Value);
                    goal.position = new UnityEngine.Vector3(float.Parse(e.Element("Position").Attribute("x").Value), float.Parse(e.Element("Position").Attribute("y").Value), float.Parse(e.Element("Position").Attribute("z").Value));
                    rigid.SetPosition(goal.position);
                    goal.scale = new UnityEngine.Vector3(float.Parse(e.Element("Scale").Attribute("x").Value), float.Parse(e.Element("Scale").Attribute("y").Value), float.Parse(e.Element("Scale").Attribute("z").Value));
                    collider.LocalScaling = goal.scale;
                    goal.gamepieceKeyword = e.Element("Keyword").Value;
                    goal.description = e.Element("Description").Value;
                    goal.color = e.Attribute("Color").Value;
                    temp.Add(g);
                }
                goals.Add(temp);
            }
            //increases depth of list to number of gamepieces for future goal writing
            while (goals.Count != gamepieces.Count)
                goals.Add(new List<GameObject>());
            return goals;
        }
        /// <summary>
        /// Gets blue goals as list of list of Goal objects from field_data.xml
        /// </summary>
        private static List<List<GameObject>> getBlueGoals()
        {
            List<List<GameObject>> goals = new List<List<GameObject>>();
            foreach (XElement z in file.Root.Element("Goals").Element("BlueGoals").Elements())
            {
                List<GameObject> temp = new List<GameObject>();
                foreach (XElement e in z.Elements())
                {
                    GameObject g = new GameObject("Gamepiece" + goals.Count().ToString() + "Goal" + temp.Count().ToString());
                    BBoxShape collider = g.AddComponent<BBoxShape>();
                    BRigidBody rigid = g.AddComponent<BRigidBody>();
                    rigid.collisionFlags = rigid.collisionFlags | BulletSharp.CollisionFlags.NoContactResponse | BulletSharp.CollisionFlags.StaticObject;
                    Goal goal = g.AddComponent<Goal>();
                    collider.Extents = new UnityEngine.Vector3(0.5f, 0.5f, 0.5f);
                    goal.pointValue = int.Parse(e.Element("Points").Value);
                    goal.position = new UnityEngine.Vector3(float.Parse(e.Element("Position").Attribute("x").Value), float.Parse(e.Element("Position").Attribute("y").Value), float.Parse(e.Element("Position").Attribute("z").Value));
                    rigid.SetPosition(goal.position);
                    goal.scale = new UnityEngine.Vector3(float.Parse(e.Element("Scale").Attribute("x").Value), float.Parse(e.Element("Scale").Attribute("y").Value), float.Parse(e.Element("Scale").Attribute("z").Value));
                    collider.LocalScaling = goal.scale;
                    goal.gamepieceKeyword = e.Element("Keyword").Value;
                    goal.description = e.Element("Description").Value;
                    goal.color = e.Attribute("Color").Value;
                    temp.Add(g);
                }
                goals.Add(temp);
            }
            //increases depth of list to number of gamepieces for future goal writing
            while (goals.Count != gamepieces.Count)
                goals.Add(new List<GameObject>());
            return goals;
        }
        /// <summary>
        /// Gets robot spawn point as Vector from field_data.xml
        /// </summary>
        private static Vector3 getRobotSpawn()
        {
            return new Vector3(float.Parse(file.Root.Element("General").Element("RobotSpawnPoint").Attribute("x").Value), float.Parse(file.Root.Element("General").Element("RobotSpawnPoint").Attribute("y").Value), float.Parse(file.Root.Element("General").Element("RobotSpawnPoint").Attribute("z").Value)); ;
        }
        #endregion
    }
}
