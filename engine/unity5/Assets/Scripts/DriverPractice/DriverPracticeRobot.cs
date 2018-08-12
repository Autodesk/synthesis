using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletSharp;
using BulletSharp.Math;
using BulletUnity;
using System.IO;
using System.Text;
using BulletUnity.Debugging;
using System.Linq;
using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Input;
using Synthesis.States;
using Synthesis.Utils;
using Synthesis.Robot;
using Synthesis.Configuration;
using Synthesis.FEA;
using Synthesis.BUExtensions;
using Synthesis.Field;

namespace Synthesis.DriverPractice
{
    /// <summary>
    /// To be added to all robots, this class 'cheats physics' to overcome the limitations that our current simulation has to create a beter environment for drivers to practice and interact with game objects.
    /// 
    /// </summary>
    public class DriverPracticeRobot : LinkedMonoBehaviour<MainState>
    {
        private int controlIndex;
        public int controlType = 0;
        public GameObject[] moveArrows;
        public bool drawing = false;
        private void Start()
        {
            controlIndex = GetComponent<SimulatorRobot>().ControlIndex;
            SetAllInteractors();
        }
        private void Update()
        {
            controlIndex = GetComponent<SimulatorRobot>().ControlIndex;
            ProcessControls();
        }
        private void ProcessControls()
        {
            for (int i = 0; i < Input.Controls.buttons[controlIndex].pickup.Count; i++)
            {
                if (DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[i].name)).ToArray().Count() > 0)
                {
                    if (InputControl.GetButton(Controls.buttons[controlIndex].pickup[i])) Intake(i);
                    else HoldGamepiece(i);
                }
            }
            for (int i = 0; i < Input.Controls.buttons[controlIndex].release.Count; i++)
            {
                if (DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[i].name)).ToArray().Count() > 0)
                {
                    if (InputControl.GetButton(Controls.buttons[controlIndex].release[i])) Release(i);
                    else HoldGamepiece(i);
                }
            }
            for (int i = 0; i < Input.Controls.buttons[controlIndex].spawnPieces.Count; i++)
            {
                if (DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[i].name)).ToArray().Count() > 0)
                {
                    if (InputControl.GetButtonDown(Controls.buttons[controlIndex].spawnPieces[i])) Spawn(FieldDataHandler.gamepieces[i]);
                    else HoldGamepiece(i);
                }
            }
            if (InputControl.GetButtonDown(Controls.buttons[controlIndex].trajectory)) drawing = drawing ? false : true;
        }
        #region DriverPractice Creation Stuff
        public DriverPractice GetDriverPractice(Gamepiece g)
        {
            DriverPractice dp = new DriverPractice(g.name, "node_0.bxda", "node_0.bxda", UnityEngine.Vector3.zero, UnityEngine.Vector3.zero);
            if (DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(g.name)).Count() > 0) dp = DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(g.name)).ToArray()[0];
            else
            {
                DPMDataHandler.dpmodes.Add(dp);
                DPMDataHandler.WriteRobot();
                SetAllInteractors();
            }
            return dp;
        }
        #endregion
        #region Intake Stuff
        private List<Interactor> intakeInteractor = new List<Interactor>();
        private List<List<GameObject>> objectsHeld = new List<List<GameObject>>();
        private void Intake(int id)
        {
            while (objectsHeld.Count <= id) objectsHeld.Add(new List<GameObject>());
            if (objectsHeld[id].Count() < FieldDataHandler.gamepieces[id].holdingLimit && intakeInteractor[id].GetDetected(id))
            {
                #region disables intake functionality for already held gamepieces
                for (int i = 0; i < objectsHeld[id].Count; i++)
                    if (objectsHeld[id][i].Equals(intakeInteractor[id].GetObject(id)))
                        return;
                #endregion
                GameObject collisionObject = intakeInteractor[id].GetObject(id);
                #region move gamepiece to release node location
                GameObject releaseNode = Auxiliary.FindObject(gameObject, DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[id].name)).ToArray()[0].releaseNode);
                UnityEngine.Vector3 releasePosition = DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[id].name)).ToArray()[0].releasePosition;
                collisionObject.GetComponent<BRigidBody>().SetPosition(releaseNode.transform.position + releaseNode.transform.rotation * releasePosition);
                #endregion
                #region changes colliders for gamepiece
                BFixedConstraintEx fc = collisionObject.AddComponent<BFixedConstraintEx>();
                fc.otherRigidBody = releaseNode.GetComponent<BRigidBody>();  
                fc.localConstraintPoint = releasePosition;
                fc.localRotationOffset = UnityEngine.Quaternion.Inverse(releaseNode.transform.rotation) * collisionObject.transform.rotation;
                foreach (List<GameObject> l in objectsHeld)
                    foreach (GameObject g in l)
                        collisionObject.GetComponent<BRigidBody>().GetCollisionObject().SetIgnoreCollisionCheck(g.GetComponent<BRigidBody>().GetCollisionObject(), true);
                foreach (BRigidBody rb in GetComponentsInChildren<BRigidBody>())
                    collisionObject.GetComponent<BRigidBody>().GetCollisionObject().SetIgnoreCollisionCheck(rb.GetCollisionObject(), true);
                objectsHeld[id].Add(collisionObject);
                #endregion

            }
        }
        #endregion
        #region keep gamepiece with robot
        private void HoldGamepiece(int id)
        {
            while (objectsHeld.Count <= id) objectsHeld.Add(new List<GameObject>());
            if (objectsHeld[id].Count == 0)
                return;
            foreach (GameObject g in objectsHeld[id])
            {
                BRigidBody orb = g.GetComponent<BRigidBody>();
                if (UnityEngine.Input.GetKey(KeyCode.Backslash))
                    ((RigidBody)orb.GetCollisionObject()).ClearForces();
                UnityEngine.Vector3 releasePosition = DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[id].name)).ToArray()[0].releasePosition;
                if (orb.GetComponent<BFixedConstraintEx>().localConstraintPoint != releasePosition)
                {
                    orb.GetCollisionObject().Activate();
                    orb.GetComponent<BFixedConstraintEx>().localConstraintPoint = releasePosition;
                }
            }
        }

        public void SetAllInteractors()
        {
            for (int i = 0; i < FieldDataHandler.gamepieces.Count; i++)
            {
                if (DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[i].name)).ToArray().Count() > 0)
                    SetInteractor(DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[i].name)).ToArray()[0].intakeNode, i);
            }
        }
        private void SetInteractor(string n, int index)
        {
            GameObject node = Auxiliary.FindObject(gameObject, n);
            if (node.GetComponent<Interactor>() == null) intakeInteractor.Insert(index, node.AddComponent<Interactor>());
            else intakeInteractor.Insert(index, node.GetComponent<Interactor>());

            intakeInteractor[index].AddGamepiece(FieldDataHandler.gamepieces[index].name, index);
        }
        #endregion
        #region Release Functionality
        private void Release(int id)
        {
            if (objectsHeld[id].Count > 0)
            {
                GameObject heldObject = objectsHeld[id][0];
                objectsHeld[id].RemoveAt(0);
                StartCoroutine(UnIgnoreCollision(heldObject));
                BRigidBody intakeRigidBody = intakeInteractor[id].GetComponent<BRigidBody>();
                if (intakeRigidBody != null && !intakeRigidBody.GetCollisionObject().IsActive)
                    intakeRigidBody.GetCollisionObject().Activate();
                BRigidBody orb = heldObject.GetComponent<BRigidBody>();
                Destroy(orb.GetComponent<BFixedConstraintEx>());
                GameObject releaseNode = Auxiliary.FindObject(gameObject, DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[id].name)).ToArray()[0].releaseNode);
                UnityEngine.Vector3 releaseVelocity = VelocityToVector3(DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[id].name)).ToArray()[0].releaseVelocity);
                orb.velocity += releaseNode.transform.rotation * releaseVelocity;
                orb.angularFactor = UnityEngine.Vector3.one;
            }
        }
        #region renable collision on held gameobjects after .5 seconds
        IEnumerator UnIgnoreCollision(GameObject obj)
        {
            List<GameObject>[] cachedObjectsHeld = new List<GameObject>[objectsHeld.Count];

            for (int i = 0; i < cachedObjectsHeld.Length; i++)
                cachedObjectsHeld[i] = objectsHeld[i].ToList();

            yield return new WaitForSeconds(0.5f);

            foreach (List<GameObject> l in cachedObjectsHeld)
                foreach (GameObject g in l)
                    g.GetComponent<BRigidBody>().GetCollisionObject().SetIgnoreCollisionCheck(obj.GetComponent<BRigidBody>().GetCollisionObject(), false);

            foreach (BRigidBody rb in GetComponentsInChildren<BRigidBody>())
                obj.GetComponent<BRigidBody>().GetCollisionObject().SetIgnoreCollisionCheck(rb.GetCollisionObject(), false);
        }
        #endregion
        #region converts vector data with 2 scalars and angle to velocity vector
        private UnityEngine.Vector3 VelocityToVector3(UnityEngine.Vector3 release)
        {
            UnityEngine.Quaternion horVector;
            UnityEngine.Quaternion verVector;
            UnityEngine.Vector3 finalVector = UnityEngine.Vector3.zero;

            horVector = UnityEngine.Quaternion.AngleAxis(release.z, UnityEngine.Vector3.up);
            verVector = UnityEngine.Quaternion.AngleAxis(release.y, UnityEngine.Vector3.right);

            UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(release.y, release.z, 0);

            finalVector = (UnityEngine.Quaternion.LookRotation(UnityEngine.Vector3.forward, UnityEngine.Vector3.up) * horVector * verVector) * UnityEngine.Vector3.forward * release.x;

            return (finalVector);
        }
        #endregion
        #endregion
        #region Gamepiece Spawn
        private void Spawn(Gamepiece g)
        {
            GameObject gamepieceClone = Instantiate(GameObject.Find(g.name).GetComponentInParent<BRigidBody>().gameObject, g.spawnpoint, UnityEngine.Quaternion.identity);
            gamepieceClone.name = g.name + "(Clone)";
            gamepieceClone.GetComponent<BRigidBody>().collisionFlags = BulletSharp.CollisionFlags.None;
            gamepieceClone.GetComponent<BRigidBody>().velocity = UnityEngine.Vector3.zero;
        }
        #endregion
        /// <summary>
        /// Refreshes the position of the move arrows with the position offsets.
        /// </summary>
        public void RefreshMoveArrows()
        {
            for (int i = 0; i < moveArrows.Length; i++)
            {
                moveArrows[i].transform.parent = Auxiliary.FindObject(gameObject, DPMDataHandler.dpmodes[i].releaseNode).transform;
                moveArrows[i].transform.localPosition = DPMDataHandler.dpmodes[i].releasePosition;
            }
        }
        
        /// <summary>
        /// Creates a <see cref="GameObject"/> instantiated from the MoveArrows prefab.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private GameObject CreateMoveArrows(int index)
        {
            GameObject arrows = Instantiate(Resources.Load<GameObject>("Prefabs\\MoveArrows"));
            arrows.name = "ReleasePositionMoveArrows";
            arrows.transform.parent = Auxiliary.FindObject(gameObject, DPMDataHandler.dpmodes[index].releaseNode).transform;
            arrows.transform.localPosition = DPMDataHandler.dpmodes[index].releasePosition;

            arrows.GetComponent<MoveArrows>().Translate = (translation) =>
            {
                arrows.transform.position += translation;
                DPMDataHandler.dpmodes[index].releasePosition = arrows.transform.localPosition;
            };
            
            arrows.GetComponent<MoveArrows>().OnClick = () => GetComponent<SimulatorRobot>().LockRobot();
            arrows.GetComponent<MoveArrows>().OnRelease = () => GetComponent<SimulatorRobot>().UnlockRobot();

            StateMachine.SceneGlobal.Link<MainState>(arrows, false);

            return arrows;
        }
        public void DestroyAllHeld(bool clone = false, string name = "")
        {
            foreach(List<GameObject> gList in objectsHeld)
            {
                for(int i = 0; i < gList.Count; i++)
                {
                    if (!clone || gList[i].name.Equals(name + "(Clone)"))
                    {
                        Destroy(gList[i]);
                        gList.RemoveAt(i);
                    }
                }
            }
        }

    }
}