using BulletSharp;
using BulletUnity;
using Synthesis.DriverPractice;
using Synthesis.FEA;
using Synthesis.Field;
using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Synthesis.States
{
    public class DefineNodeState : State
    {
        private DriverPractice.DriverPractice dp;
        private bool intake;
        private Transform t;
        private GameObject highlightedNode;
        private List<Color> originalColors = new List<Color>();
        private Color highlightColor = new Color(1, 1, 0, 0.1f);
        private int highlightTimer = -1;
        private GameObject hoveredNode;
        private List<Color> hoveredColors = new List<Color>();
        private Color hoverColor = new Color(1, 1, 0, 0.1f);
        private DriverPracticeRobot dpr;
        private Button highlightButton;

        public DefineNodeState(DriverPractice.DriverPractice dp, Transform t, bool intake, DriverPracticeRobot dpr)
        {
            this.dp = dp;
            this.intake = intake;
            this.t = t;
            this.dpr = dpr;
        }
        public DefineNodeState()
        {

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
                else if (collisionObject.transform.parent == t)
                {
                    if (hoveredNode != collisionObject)
                    {
                        RevertNodeColors(hoveredNode, hoveredColors);
                    }

                    hoveredNode = collisionObject;

                    ChangeNodeColors(hoveredNode, hoverColor, hoveredColors);
                    if (UnityEngine.Input.GetMouseButtonUp(0))
                    {
                        if (intake) DPMDataHandler.dpmodes.Find(d => d.Equals(dp)).intakeNode = hoveredNode.name;
                        else DPMDataHandler.dpmodes.Find(d => d.Equals(dp)).releaseNode = hoveredNode.name;
                        DPMDataHandler.WriteRobot();
                        dpr.SetAllInteractors();
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
        }
        private void HighlightNode()
        {
            GameObject node;
            node = intake ? GameObject.Find(DPMDataHandler.dpmodes.Find(d => d.Equals(dp)).intakeNode) : GameObject.Find(DPMDataHandler.dpmodes.Find(d => d.Equals(dp)).releaseNode);

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
                    storedColors.Add(m.color);
                    m.color = color;
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
                        m.color = storedColors[counter];
                        counter++;
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