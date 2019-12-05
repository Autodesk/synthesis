using BulletUnity;
using Synthesis.DriverPractice;
using System;
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
        public static BulletSharp.Math.Matrix robotSpawnOrientation = BulletSharp.Math.Matrix.Identity;
        public static int gamepieceIndex = 0;
        #endregion
        #region fileWriting
        /// <summary>
        /// Writes to field_data.xml file - does not append
        /// </summary>
        public static void WriteField()
        {
            string targetFile = PlayerPrefs.GetString("simSelectedField") + Path.DirectorySeparatorChar + "field_data.xml";

            CreateDefaultGridFiles(Path.GetDirectoryName(targetFile));

            if (!File.Exists(targetFile))
            {
                GUI.UserMessageManager.Dispatch("Changes are session-only - field meta file not found", 7);
                return;
            }
            //wrap all data
            XElement field = new XElement("FieldData", null); //parent
            field.Add(GoalData());
            field.Add(GeneralData());
            //save function
            field.Save(targetFile);
        }

        private static XElement convertGoals(List<List<GameObject>> goals, string goalsName, int gamepieceIndex)
        {
            if (goals.Count() <= 0)
            {
                throw new System.ArgumentOutOfRangeException();
            }
            return new XElement(goalsName + gamepieceIndex.ToString(), from g in goals[gamepieceIndex]
                                                                       select new XElement("Goal", new XAttribute("Color", g.GetComponent<Goal>().color),
                                                                               new XElement("Position",
                                                                                   new XAttribute("x", g.GetComponent<Goal>().position.x),
                                                                                   new XAttribute("y", g.GetComponent<Goal>().position.y),
                                                                                   new XAttribute("z", g.GetComponent<Goal>().position.z),
                                                                                   new XAttribute("i", g.GetComponent<Goal>().rotation.x),
                                                                                   new XAttribute("j", g.GetComponent<Goal>().rotation.y),
                                                                                   new XAttribute("k", g.GetComponent<Goal>().rotation.z)),
                                                                               new XElement("Scale",
                                                                               new XAttribute("x", g.GetComponent<Goal>().scale.x),
                                                                               new XAttribute("y", g.GetComponent<Goal>().scale.y),
                                                                               new XAttribute("z", g.GetComponent<Goal>().scale.z)),
                                                                               new XElement("Points", g.GetComponent<Goal>().pointValue),
                                                                               new XElement("KeepScored", g.GetComponent<Goal>().KeepScored),
                                                                               new XElement("Sticky", g.GetComponent<Goal>().Sticky),
                                                                               new XElement("Description", g.GetComponent<Goal>().description),
                                                                               new XElement("Keyword", g.GetComponent<Goal>().gamepieceKeyword)));
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
                    xRedGoals.Add(convertGoals(redGoals, "RedGoals", i));
                }
                if (blueGoals.Count() > 0)
                {
                    xBlueGoals.Add(convertGoals(blueGoals, "BlueGoals", i));
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
                                                                                          new XAttribute("z", g.spawnpoint.z),
                                                                                          new XAttribute("i", g.spawnorientation.x),
                                                                                          new XAttribute("j", g.spawnorientation.y),
                                                                                          new XAttribute("k", g.spawnorientation.z)
                                                                                          ));
            XElement robotSpawnPoint = new XElement("RobotSpawnPoint",
                                            new XAttribute("x", robotSpawn.x),
                                            new XAttribute("y", robotSpawn.y),
                                            new XAttribute("z", robotSpawn.z),
                                            new XAttribute("M11", robotSpawnOrientation.M11),
                                            new XAttribute("M12", robotSpawnOrientation.M12),
                                            new XAttribute("M13", robotSpawnOrientation.M13),
                                            new XAttribute("M14", robotSpawnOrientation.M14),
                                            new XAttribute("M21", robotSpawnOrientation.M21),
                                            new XAttribute("M22", robotSpawnOrientation.M22),
                                            new XAttribute("M23", robotSpawnOrientation.M23),
                                            new XAttribute("M24", robotSpawnOrientation.M24),
                                            new XAttribute("M31", robotSpawnOrientation.M31),
                                            new XAttribute("M32", robotSpawnOrientation.M32),
                                            new XAttribute("M33", robotSpawnOrientation.M33),
                                            new XAttribute("M34", robotSpawnOrientation.M34),
                                            new XAttribute("M41", robotSpawnOrientation.M41),
                                            new XAttribute("M42", robotSpawnOrientation.M42),
                                            new XAttribute("M43", robotSpawnOrientation.M43),
                                            new XAttribute("M44", robotSpawnOrientation.M44)
                                            );
            //wrap general data
            XElement gen = new XElement("General", null); //parent
            gen.Add(pieces);
            gen.Add(robotSpawnPoint);
            return gen;
        }

        /// <summary>
        /// Automatically create default grid files if necessary
        /// </summary>
        /// <param name="fieldPath"></param>
        /// <returns></returns>
        private static bool CreateDefaultGridFiles(string fieldPath)
        {
            string targetFile = fieldPath + Path.DirectorySeparatorChar + "field_data.xml";
            if (!File.Exists(targetFile))
            {
                if (!string.IsNullOrEmpty(fieldPath) && new DirectoryInfo(fieldPath).Name == UnityFieldDefinition.EmptyGridName)
                {
                    gamepieces = new List<Gamepiece>();
                    redGoals = new List<List<GameObject>>();
                    blueGoals = new List<List<GameObject>>();
                    if (fieldPath == PlayerPrefs.GetString("FieldDirectory") + Path.DirectorySeparatorChar + UnityFieldDefinition.EmptyGridName &&
                        fieldPath == PlayerPrefs.GetString("simSelectedField"))
                    {
                        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("FieldDirectory")) && Directory.Exists(PlayerPrefs.GetString("FieldDirectory")))
                        {
                            Directory.CreateDirectory(fieldPath);
                            File.Create(targetFile).Dispose();
                            WriteField();
                            return true;
                        }
                    }
                }
            }
            else
            {
                return true;
            }
            return false;
        }

        #endregion
        #region getData
        /// <summary>
        /// Assigns global variables values from field_data.xml
        /// </summary>
        /// <param name="fieldPath">location to field folder passed upon field load</param>
        public static void LoadFieldMetaData(string fieldPath)
        {
            string targetFile = fieldPath + Path.DirectorySeparatorChar + "field_data.xml";
            if (File.Exists(targetFile))
            {
                file = XDocument.Load(targetFile);
                gamepieces = getGamepieces();
                redGoals = getGoals("RedGoals");
                blueGoals = getGoals("BlueGoals");
                robotSpawn = getRobotSpawn();
                robotSpawnOrientation = getRobotSpawnRotation();
                gamepieceIndex = 0;
            }
            else
            {
                if (!string.IsNullOrEmpty(fieldPath) && new DirectoryInfo(fieldPath).Name == UnityFieldDefinition.EmptyGridName)
                {
                    gamepieces = new List<Gamepiece>();
                    if (!CreateDefaultGridFiles(fieldPath))
                    {
                        GUI.UserMessageManager.Dispatch("Features limited - field meta file not found", 7);
                    }
                }
                else
                {
                    GUI.UserMessageManager.Dispatch("Failed to load field meta data", 7);
                }
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
        /// Gets goals as list of list of Goal objects from field_data.xml, picking color from argument
        /// </summary>
        private static List<List<GameObject>> getGoals(string goalsName)
        {
            List<List<GameObject>> goals = new List<List<GameObject>>();
            foreach (XElement z in file.Root.Element("Goals").Element(goalsName).Elements())
            {
                List<GameObject> temp = new List<GameObject>();
                foreach (XElement e in z.Elements())
                {
                    GameObject g = new GameObject("Gamepiece" + goals.Count().ToString() + "Goal" + temp.Count().ToString());
                    g.transform.SetParent(GoalManager.GoalParent.transform);
                    BBoxShape collider = g.AddComponent<BBoxShape>();
                    BRigidBody rigid = g.AddComponent<BRigidBody>();
                    rigid.collisionFlags = rigid.collisionFlags | BulletSharp.CollisionFlags.NoContactResponse | BulletSharp.CollisionFlags.StaticObject;
                    Goal goal = g.AddComponent<Goal>();
                    collider.Extents = new UnityEngine.Vector3(0.5f, 0.5f, 0.5f);
                    goal.pointValue = int.Parse(e.Element("Points").Value);
                    goal.SetKeepScored(bool.Parse(e.Element("KeepScored").Value));
                    goal.position = new UnityEngine.Vector3(float.Parse(e.Element("Position").Attribute("x").Value), float.Parse(e.Element("Position").Attribute("y").Value), float.Parse(e.Element("Position").Attribute("z").Value));
                    rigid.SetPosition(goal.position);
                    try
                    {
                        goal.rotation = new UnityEngine.Vector3(float.Parse(e.Element("Position").Attribute("i").Value), float.Parse(e.Element("Position").Attribute("j").Value), float.Parse(e.Element("Position").Attribute("k").Value));
                    }
                    catch (System.Exception)
                    {
                        goal.rotation = Vector3.zero;
                    }
                    rigid.SetRotation(Quaternion.Euler(goal.rotation));
                    goal.scale = new UnityEngine.Vector3(float.Parse(e.Element("Scale").Attribute("x").Value), float.Parse(e.Element("Scale").Attribute("y").Value), float.Parse(e.Element("Scale").Attribute("z").Value));
                    collider.LocalScaling = goal.scale;
                    goal.gamepieceKeyword = e.Element("Keyword").Value;
                    goal.description = e.Element("Description").Value;
                    goal.color = e.Attribute("Color").Value;
                    try { goal.Sticky = bool.Parse(e.Element("Sticky").Value); } catch (Exception excep) { goal.Sticky = false; }
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

        private static BulletSharp.Math.Matrix getRobotSpawnRotation()
        {
            try
            {
                var spawn = file.Root.Element("General").Element("RobotSpawnPoint");
                //return new BulletSharp.Math.Matrix(float.Parse(spawn.Attribute("i").Value), float.Parse(spawn.Attribute("j").Value), float.Parse(spawn.Attribute("k").Value));
                return new BulletSharp.Math.Matrix
                {
                    M11 = float.Parse(spawn.Attribute("M11").Value),
                    M12 = float.Parse(spawn.Attribute("M12").Value),
                    M13 = float.Parse(spawn.Attribute("M12").Value),
                    M14 = float.Parse(spawn.Attribute("M13").Value),
                    M21 = float.Parse(spawn.Attribute("M21").Value),
                    M22 = float.Parse(spawn.Attribute("M22").Value),
                    M23 = float.Parse(spawn.Attribute("M23").Value),
                    M24 = float.Parse(spawn.Attribute("M24").Value),
                    M31 = float.Parse(spawn.Attribute("M31").Value),
                    M32 = float.Parse(spawn.Attribute("M32").Value),
                    M33 = float.Parse(spawn.Attribute("M33").Value),
                    M34 = float.Parse(spawn.Attribute("M34").Value),
                    M41 = float.Parse(spawn.Attribute("M41").Value),
                    M42 = float.Parse(spawn.Attribute("M42").Value),
                    M43 = float.Parse(spawn.Attribute("M43").Value),
                    M44 = float.Parse(spawn.Attribute("M44").Value),
                };
            }
            catch (System.Exception)
            {
                return BulletSharp.Math.Matrix.Identity;
            }
        }
        #endregion
    }
}
