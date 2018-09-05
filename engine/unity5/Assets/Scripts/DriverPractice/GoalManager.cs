using BulletUnity;
using Synthesis.Field;
using Synthesis.FSM;
using Synthesis.Input;
using Synthesis.States;
using Synthesis.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.DriverPractice
{
    public class GoalManager : MonoBehaviour
    {

        public GameObject goalElementPrefab;    

        GameObject canvas;

        GameObject goalWindow;

        GameObject scoreboard;

        Transform goalDisplay;
        
        private List<GameObject> goalElements;

        public List<List<GameObject>> redGoals;
        public List<List<GameObject>> blueGoals;

        private GameObject redButton;
        private GameObject blueButton;

        Transform tabSystem;
        List<GameObject> tabElements;
        public GameObject tabButtonPrefab;

        string color = "Red";
        int gamepieceIndex = 0;

        private void Update()
        {
            if (canvas == null)
                FindElements();

        }

        /// <summary>
        /// Find and store the necessary UI elements related to the goal display.
        /// </summary>
        void FindElements()
        {
            canvas = GameObject.Find("Canvas");

            goalWindow = Auxiliary.FindObject(canvas, "GoalConfigPanel");

            goalDisplay = Auxiliary.FindObject(goalWindow, "GoalDisplay").transform;

            scoreboard = Auxiliary.FindObject(canvas, "ScorePanel");

            redButton = Auxiliary.FindObject(goalWindow, "Red");
            blueButton = Auxiliary.FindObject(goalWindow, "Blue");

            tabSystem = Auxiliary.FindObject(goalWindow, "TabLayout").transform;

            GetGoals();
        }
        public void OpenGoalManager()
        {
            InitializeDisplay();
            goalWindow.SetActive(true);
            Auxiliary.FindObject(goalWindow, "AddGoalButton").GetComponentInChildren<Text>().text = "New " + color + " Goal";
            if (color.Equals("Red"))
            {
                redButton.GetComponent<Button>().image.color = Color.red;
                redButton.GetComponent<Button>().interactable = false;
                blueButton.GetComponent<Button>().interactable = true;
                blueButton.GetComponent<Button>().image.color = Color.black;
            }
            else
            {
                blueButton.GetComponent<Button>().image.color = Color.blue;
                blueButton.GetComponent<Button>().interactable = false;
                redButton.GetComponent<Button>().interactable = true;
                redButton.GetComponent<Button>().image.color = Color.black;
            }
            CreateTabs();
            //OpenScoreboard();
        }
        public void CloseGoalManager()
        {
            goalWindow.SetActive(false);
        }
        public void ChangeColor(string color)
        {
            this.color = color;
            OpenGoalManager();
        }
        /// <summary>
        /// Prepares the goal display to show descriptions and buttons for a set of goals.
        /// </summary>
        /// <param name="descriptions">Descriptions of the goals.</param>
        /// <param name="points">Point value of the goals.</param>
        public void InitializeDisplay()
        {
            GameObject[] g = color.Equals("Red") ? redGoals[gamepieceIndex].ToArray() : blueGoals[gamepieceIndex].ToArray();
            if (goalDisplay != null)
            {
                if (goalElements == null)
                    goalElements = new List<GameObject>();
                
                while (goalElements.Count > 0)
                {
                    Destroy(goalElements[0]);
                    goalElements.RemoveAt(0);
                }

                for (int i = 0; i < g.Length; i++)
                {
                    int id = i;

                    GameObject newGoalElement = Instantiate(goalElementPrefab);
                    newGoalElement.transform.parent = goalDisplay;
                    newGoalElement.name = "Goal" + i.ToString();

                    InputField descText = Auxiliary.FindObject(newGoalElement, "DescriptionText").GetComponent<InputField>();
                    descText.text = g[i].GetComponent<Goal>().description;
                    descText.onValueChanged.AddListener(delegate { SetGoalDescription(id, descText.text); });

                    descText.onEndEdit.AddListener(delegate { InputControl.freeze = false; });

                    InputField pointValue = Auxiliary.FindObject(newGoalElement, "PointValue").GetComponent<InputField>();
                    pointValue.text = g[i].GetComponent<Goal>().pointValue.ToString();
                    pointValue.onValueChanged.AddListener(delegate { int value = int.TryParse(pointValue.text, out value) ? int.Parse(pointValue.text) : 0; pointValue.text = value.ToString();  SetGoalPoints(id, value); });

                    pointValue.onEndEdit.AddListener(delegate { InputControl.freeze = false; });

                    Button moveButton = Auxiliary.FindObject(newGoalElement, "MoveButton").GetComponent<Button>();
                    moveButton.onClick.AddListener(delegate { MoveGoal(id); });

                    Button scaleButton = Auxiliary.FindObject(newGoalElement, "ScaleButton").GetComponent<Button>();
                    scaleButton.onClick.AddListener(delegate { ScaleGoal(id); });

                    Button deleteButton = Auxiliary.FindObject(newGoalElement, "DeleteButton").GetComponent<Button>();
                    deleteButton.onClick.AddListener(delegate { DeleteGoal(id); });

                    goalElements.Add(newGoalElement);
                }
            }
            else Debug.Log("Could not initialize goal display, display not found!");
        }
        private void CreateTabs()
        {
            if (tabElements == null)
                tabElements = new List<GameObject>();

            while (tabElements.Count > 0)
            {
                Destroy(tabElements[0]);
                tabElements.RemoveAt(0);
            }
            if (FieldDataHandler.gamepieces.Count() > 0)
            {
                for (int i = 0; i < FieldDataHandler.gamepieces.Count; i++)
                {
                    int id = i;

                    GameObject tabButton = Instantiate(tabButtonPrefab);

                    tabButton.transform.parent = tabSystem;

                    Button change = Auxiliary.FindObject(tabButton, "Change").GetComponent<Button>();
                    change.onClick.AddListener(delegate { gamepieceIndex = id; OpenGoalManager(); });

                    Auxiliary.FindObject(change.gameObject, "Name").GetComponent<Text>().text = FieldDataHandler.gamepieces[id].name;

                    if (id == gamepieceIndex)
                    {
                        //change.image.color = new Color(241f, 133f, 24f;
                        change.image.color = new Color32(33, 43, 52, 120);
                        change.interactable = false;
                    }

                    tabElements.Add(tabButton);
                }
            }
        }
        void SetGoalDescription(int id, string description)
        {
            InputControl.freeze = true;
            if (color.Equals("Red"))
            {
                redGoals[gamepieceIndex][id].GetComponent<Goal>().description = description;
            }
            else
            {
                blueGoals[gamepieceIndex][id].GetComponent<Goal>().description = description;
            }
        }
        void SetGoalPoints(int id, int points)
        {
            InputControl.freeze = true;
            if (color.Equals("Red"))
            {
                redGoals[gamepieceIndex][id].GetComponent<Goal>().pointValue = points;
            }
            else
            {
                blueGoals[gamepieceIndex][id].GetComponent<Goal>().pointValue = points;
            }
            WriteGoals();
        }
        void DeleteGoal(int id)
        {
            if (color.Equals("Red"))
            {
                Destroy(redGoals[gamepieceIndex][id].GetComponent<Goal>());
                Destroy(redGoals[gamepieceIndex][id]);
                redGoals[gamepieceIndex].RemoveAt(id);
            }
            else
            {
                Destroy(blueGoals[gamepieceIndex][id].GetComponent<Goal>());
                Destroy(blueGoals[gamepieceIndex][id]);
                blueGoals[gamepieceIndex].RemoveAt(id);
            }
            WriteGoals();
            InitializeDisplay();
        }
        void GetGoals()
        {
            redGoals = FieldDataHandler.redGoals;
            blueGoals = FieldDataHandler.blueGoals;
        }
        void MoveGoal(int id)
        {
            StateMachine.SceneGlobal.PushState(new GoalState(color, gamepieceIndex, id, this, true), true);
        }
        void ScaleGoal(int id)
        {
            StateMachine.SceneGlobal.PushState(new GoalState(color, gamepieceIndex, id, this, false), true);
        }
        public void NewGoal()
        {
            int goalIndex = color.Equals("Red") ? redGoals[gamepieceIndex].Count() : blueGoals[gamepieceIndex].Count();
            GameObject gameobject = new GameObject("Gamepiece" + gamepieceIndex.ToString() + "Goal" + goalIndex.ToString());
            BBoxShape collider = gameobject.AddComponent<BBoxShape>();
            BRigidBody rigid = gameobject.AddComponent<BRigidBody>();
            rigid.collisionFlags = rigid.collisionFlags | BulletSharp.CollisionFlags.NoContactResponse | BulletSharp.CollisionFlags.StaticObject;
            Goal goal = gameobject.AddComponent<Goal>();
            collider.Extents = new UnityEngine.Vector3(0.5f, 0.5f, 0.5f);
            rigid.SetPosition(new UnityEngine.Vector3(0, 4, 0));
            goal.position = new UnityEngine.Vector3(0, 4, 0);
            goal.scale = Vector3.one;
            goal.SetKeyword(FieldDataHandler.gamepieces[gamepieceIndex].name);
            goal.description = "New Goal";
            goal.color = color;
            if (color.Equals("Red"))
            {
                redGoals[gamepieceIndex].Add(gameobject);
            }
            else
            {
                blueGoals[gamepieceIndex].Add(gameobject);
            }
            InitializeDisplay();
            WriteGoals();
        }
        public void WriteGoals()
        {
            FieldDataHandler.redGoals = redGoals;
            FieldDataHandler.blueGoals = blueGoals;
            FieldDataHandler.WriteField();
        }
        public void OpenScoreboard()
        {
            scoreboard.SetActive(true);
        }
        public void CloseScoreboard()
        {
            scoreboard.SetActive(false);
        }
        public void ResetScoreboard(string color)
        {
            GameObject score;
            if (color.Equals("Red")) score = Auxiliary.FindObject(Auxiliary.FindObject(Auxiliary.FindObject("Canvas"), "ScorePanel"), "RedScoreText");
            else score = Auxiliary.FindObject(Auxiliary.FindObject(Auxiliary.FindObject("Canvas"), "ScorePanel"), "BlueScoreText");
            score.GetComponent<Text>().text = "0";
        }
    }
}
