using BulletUnity;
using Synthesis.DriverPractice;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace Synthesis.Field
{
    public class FieldDataHandler
    {
        #region values
        private static XDocument file;
        public static List<Gamepiece> gamepieces;
        public static List<List<GameObject>> redGoals;
        public static List<List<GameObject>> blueGoals;
        public static Vector3 robotSpawn;
        #endregion
        #region fileWriting
        public static void WriteField()
        {
            XElement field = new XElement("FieldData", null);
            field.Add(GoalData());
            field.Add(GeneralData());
            field.Save(PlayerPrefs.GetString("simSelectedField") + "\\" + "field_data.xml");
        }
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
                                                                                      new XElement("Points", g.GetComponent<Goal>().pointValue),
                                                                                      new XElement("Description", g.GetComponent<Goal>().description),
                                                                                      new XElement("Keyword", g.GetComponent<Goal>().gamepieceKeyword)));
                    xBlueGoals.Add(bGoals);
                }
            }

            //wrap goal elements
            XElement goals = new XElement("Goals", null);
            goals.Add(xRedGoals);
            goals.Add(xBlueGoals);
            return goals;
        }
        private static XElement GeneralData()
        {
            XElement gen = new XElement("General", null);
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
            gen.Add(pieces);
            gen.Add(robotSpawnPoint);
            return gen;
        }
        #endregion
        #region getData
        public static void Load()
        {
            file = XDocument.Load(PlayerPrefs.GetString("simSelectedField") + "\\" + "field_data.xml");
            gamepieces = getGamepieces();
            redGoals = getRedGoals();
            blueGoals = getBlueGoals();
            robotSpawn = getRobotSpawn();
        }
        private static List<Gamepiece> getGamepieces()
        {
            List<Gamepiece> pieces = new List<Gamepiece>();
            foreach (XElement g in file.Root.Element("General").Element("Gamepieces").Elements())
            {
                pieces.Add(new Gamepiece(g));
            }
            return pieces;
        }
        private static List<List<GameObject>> getRedGoals()
        {
            List<List<GameObject>> goals = new List<List<GameObject>>();
            foreach(XElement z in file.Root.Element("Goals").Element("RedGoals").Elements())
            {
                List<GameObject> temp = new List<GameObject>();
                foreach(XElement e in z.Elements())
                {
                    GameObject g = new GameObject("Gamepiece" + goals.Count().ToString() + "Goal" + temp.Count().ToString());
                    BBoxShape collider = g.AddComponent<BBoxShape>();
                    BRigidBody rigid = g.AddComponent<BRigidBody>();
                    rigid.collisionFlags = rigid.collisionFlags | BulletSharp.CollisionFlags.NoContactResponse | BulletSharp.CollisionFlags.StaticObject;
                    Goal goal = g.AddComponent<Goal>();
                    collider.Extents = new UnityEngine.Vector3(0.5f, 0.5f, 0.5f);
                    rigid.SetPosition(new UnityEngine.Vector3(float.Parse(e.Element("Position").Attribute("x").Value), float.Parse(e.Element("Position").Attribute("y").Value), float.Parse(e.Element("Position").Attribute("z").Value)));
                    goal.pointValue = int.Parse(e.Element("Points").Value);
                    goal.position = new UnityEngine.Vector3(float.Parse(e.Element("Position").Attribute("x").Value), float.Parse(e.Element("Position").Attribute("y").Value), float.Parse(e.Element("Position").Attribute("z").Value));
                    goal.SetKeyword(e.Element("Keyword").Value);
                    goal.description = e.Element("Description").Value;
                    goal.color = e.Attribute("Color").Value;
                    temp.Add(g);
                }
                goals.Add(temp);
            }
            if(file.Root.Element("Goals").Element("RedGoals").Elements().Count() == 0)
            {
                foreach(Gamepiece g in gamepieces)
                {
                    goals.Add(new List<GameObject>());
                }
            }
            return goals;
        }
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
                    rigid.SetPosition(new UnityEngine.Vector3(float.Parse(e.Element("Position").Attribute("x").Value), float.Parse(e.Element("Position").Attribute("y").Value), float.Parse(e.Element("Position").Attribute("z").Value)));
                    goal.pointValue = int.Parse(e.Element("Points").Value);
                    goal.position = new UnityEngine.Vector3(float.Parse(e.Element("Position").Attribute("x").Value), float.Parse(e.Element("Position").Attribute("y").Value), float.Parse(e.Element("Position").Attribute("z").Value));
                    goal.SetKeyword(e.Element("Keyword").Value);
                    goal.description = e.Element("Description").Value;
                    goal.color = e.Attribute("Color").Value;
                    temp.Add(g);
                }
                goals.Add(temp);
            }
            if (file.Root.Element("Goals").Element("BlueGoals").Elements().Count() == 0)
            {
                foreach(Gamepiece g in gamepieces)
                {
                    goals.Add(new List<GameObject>());
                }
            }
            return goals;
        }
        private static Vector3 getRobotSpawn()
        {
            return new Vector3(float.Parse(file.Root.Element("General").Element("RobotSpawnPoint").Attribute("x").Value), float.Parse(file.Root.Element("General").Element("RobotSpawnPoint").Attribute("y").Value), float.Parse(file.Root.Element("General").Element("RobotSpawnPoint").Attribute("z").Value)); ;
        }
        #endregion
    }
}
