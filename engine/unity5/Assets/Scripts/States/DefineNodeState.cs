using BulletSharp;
using BulletUnity;
using Synthesis.DriverPractice;
using Synthesis.FEA;
using Synthesis.Field;
using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        #region help ui variables
        GameObject ui;
        GameObject helpMenu;
        GameObject toolbar;
        GameObject overlay;
        #endregion

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
            #region init
            ui = GameObject.Find("DefineNodeUI");
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
