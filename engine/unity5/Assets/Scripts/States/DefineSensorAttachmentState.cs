using BulletSharp;
using BulletUnity;
using Synthesis.Camera;
using Synthesis.DriverPractice;
using Synthesis.FEA;
using Synthesis.Field;
using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Sensors;
using Synthesis.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Synthesis.States
{
    public class DefineSensorAttachmentState : State
    {
        private GameObject highlightedNode;
        private List<Color> originalColors = new List<Color>();
        private Color highlightColor = new Color(1, 1, 0, 0.1f);
        private int highlightTimer = -1;
        private GameObject hoveredNode;
        private List<Color> hoveredColors = new List<Color>();
        private Color hoverColor = new Color(1, 1, 0, 0.1f);
        private Button highlightButton;

        bool camera;
        SensorBase currentSensor;
        RobotCameraManager robotCameraManager;
        
        #region help ui variables
        GameObject ui;
        GameObject helpMenu;
        GameObject toolbar;
        GameObject overlay;
        #endregion

        public DefineSensorAttachmentState(SensorBase currentSensor)
        {
            camera = false;
            this.currentSensor = currentSensor;
        }
        public DefineSensorAttachmentState(RobotCameraManager robotCameraManager)
        {
            camera = true;
            this.robotCameraManager = robotCameraManager;
        }
        // Use this for initialization
        public override void Start()
        {
            #region init
            ui = GameObject.Find("DefineSensorAttachmentUI");
            helpMenu = Auxiliary.FindObject(ui, "Help");
            toolbar = Auxiliary.FindObject(ui, "NodeStateToolbar");
            overlay = Auxiliary.FindObject(ui, "Overlay");
            #endregion

            highlightButton = GameObject.Find("HighlightButton").GetComponent<Button>();
            highlightButton.onClick.RemoveAllListeners();
            highlightButton.onClick.AddListener(HighlightNode);
            Button helpButton = GameObject.Find("HelpButton").GetComponent<Button>();
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(HelpMenu);
            Button returnButton = GameObject.Find("ReturnButton").GetComponent<Button>();
            returnButton.onClick.RemoveAllListeners();
            returnButton.onClick.AddListener(ReturnToMainState);
            Button closeHelp = Auxiliary.FindObject(helpMenu, "CloseHelpButton").GetComponent<Button>();
            closeHelp.onClick.RemoveAllListeners();
            closeHelp.onClick.AddListener(CloseHelpMenu);
        }

        // Update is called once per frame
        public override void Update()
        {
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
            BulletSharp.Math.Vector3 start = ray.origin.ToBullet();
            BulletSharp.Math.Vector3 end = ray.GetPoint(200).ToBullet();

            //Creates a callback result that will be updated if we do a ray test with it
            ClosestRayResultCallback rayResult = new ClosestRayResultCallback(ref start, ref end);

            //Retrieves the bullet physics world and does a ray test with the given coordinates and updates the callback object
            BPhysicsWorld world = BPhysicsWorld.Get();
            world.world.RayTest(start, end, rayResult);

            //If there is a collision object and it is dynamic and not a robot part, change the gamepiece to that
            if (rayResult.CollisionObject != null)
            {
                GameObject collisionObject = (rayResult.CollisionObject.UserObject as BRigidBody).gameObject;
                if (rayResult.CollisionObject.CollisionFlags == BulletSharp.CollisionFlags.StaticObject)
                {
                    RevertNodeColors(hoveredNode, hoveredColors);
                }
                else if (collisionObject == null)
                {
                    RevertNodeColors(hoveredNode, hoveredColors);
                }
                else if (collisionObject.transform.parent == StateMachine.SceneGlobal.FindState<MainState>().ActiveRobot.transform)
                {
                    if (hoveredNode != collisionObject)
                    {
                        RevertNodeColors(hoveredNode, hoveredColors);
                    }

                    hoveredNode = collisionObject;

                    ChangeNodeColors(hoveredNode, hoverColor, hoveredColors);
                    if (UnityEngine.Input.GetMouseButtonUp(0))
                    {
                        if (camera) robotCameraManager.CurrentCamera.transform.parent = hoveredNode.transform;
                        else currentSensor.gameObject.transform.parent = hoveredNode.transform;
                        ReturnToMainState();
                    }

                }
                else RevertNodeColors(hoveredNode, hoveredColors);
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                ReturnToMainState();
            }
            if (EventSystem.current.currentSelectedGameObject == highlightButton.gameObject && UnityEngine.Input.GetMouseButton(0)) HighlightNode();
            else RevertHighlight();

            //if (highlightTimer > 0) highlightTimer--;
            //else if (highlightTimer == 0) RevertHighlight();
        }
        private void HighlightNode()
        {
            GameObject node;
            node = camera ? robotCameraManager.CurrentCamera.transform.parent.gameObject : currentSensor.gameObject.transform.parent.gameObject;  

            RevertHighlight();
            highlightedNode = node;
            ChangeNodeColors(highlightedNode, highlightColor, originalColors);
        }
        public void RevertHighlight()
        {
            RevertNodeColors(highlightedNode, originalColors);
            highlightedNode = null;
        }
        #region Highlighting Functions
        private void ChangeNodeColors(GameObject node, Color color, List<Color> storedColors)
        {
            foreach (Renderer renderers in node.GetComponentsInChildren<Renderer>())
            {
                foreach (Material m in renderers.materials)
                {
                    if (m.HasProperty("_Color")) { storedColors.Add(m.color); m.color = color; }
                }
            }
        }

        private void RevertNodeColors(GameObject node, List<Color> storedColors)
        {
            if (node != null && storedColors.Count != 0)
            {
                int counter = 0;
                foreach (Renderer renderers in node.GetComponentsInChildren<Renderer>())
                {
                    foreach (Material m in renderers.materials)
                    {
                        if (m.HasProperty("_Color")) { m.color = storedColors[counter]; counter++; }
                    }
                }
                storedColors.Clear();
            }
        }
        #endregion 
        private void ReturnToMainState()
        {
            RevertNodeColors(hoveredNode, hoveredColors);
            RevertHighlight();
            if (helpMenu.activeSelf) CloseHelpMenu();
            StateMachine.PopState();
        }
        private void HelpMenu()
        {
            helpMenu.SetActive(true);
            overlay.SetActive(true);
            toolbar.transform.Translate(new Vector3(100, 0, 0));
            foreach (Transform t in toolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(100, 0, 0));
                else t.gameObject.SetActive(false);
            }
        }
        private void CloseHelpMenu()
        {
            helpMenu.SetActive(false);
            overlay.SetActive(false);
            toolbar.transform.Translate(new Vector3(-100, 0, 0));
            foreach (Transform t in toolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(-100, 0, 0));
                else t.gameObject.SetActive(true);
            }
        }
    }
}
