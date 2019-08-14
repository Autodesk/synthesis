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
            highlightButton = GameObject.Find("HighlightButton").GetComponent<Button>();
            highlightButton.onClick.RemoveAllListeners();
            highlightButton.onClick.AddListener(HighlightNode);
            Button returnButton = GameObject.Find("ReturnButton").GetComponent<Button>();
            returnButton.onClick.RemoveAllListeners();
            returnButton.onClick.AddListener(ReturnToMainState);
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
            StateMachine.PopState();
        }
    }
}
