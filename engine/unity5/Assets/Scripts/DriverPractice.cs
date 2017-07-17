using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletSharp;
using BulletSharp.Math;
using System.Collections;
using BulletUnity.Debugging;
using System.Linq;

namespace BulletUnity { 
    [AddComponentMenu("Physics Bullet/RigidBody")]
    /// <summary>
    /// This is a class that handles everything associated with the driver practice mode.
    /// It 'cheats physics' to overcome the limitations that our current simulation has to create a beter environment for drivers to practice and interact with game objects.
    /// 
    /// </summary>
    public class DriverPractice : MonoBehaviour {

        public UnityEngine.Vector3[] positionOffset; //position offset vectors for gamepiece while its being held
        public List<float[]> releaseVelocity; //release velocity vectors for gamepiece, defined not in x,y,z coordinates, but speed, hor angle, and ver angle.
        public float[] primaryVelocity = new float[3];
        public float[] secondaryVelocity = new float[3];

        public List<UnityEngine.Vector3> releaseVelocityVector;

        public List<GameObject> intakeNode; //node that is identified for intaking gamepieces
        public List<GameObject> releaseNode; //node that is identified for holding/releasing gamepieces
        private List<Interactor> intakeInteractor;

        public List<int> holdingLimit; //the maximum number of game objects that this robot can hold at any given time.

        public List<List<GameObject>> objectsHeld; //list of gamepieces this robot is currently holding
        public List<GameObject> primaryHeld;
        public List<GameObject> secondaryHeld; 

        public List<string> gamepieceNames; //list of the identifiers of gamepieces
        public List<GameObject> spawnedGamepieces;

        public bool displayTrajectories = false; //projects gamepiece trajectories if true
        private List<LineRenderer> drawnTrajectory;

        public bool modeEnabled = false;

        private int configuringIndex = 0;
        private int processingIndex = 0; //we use this to alternate which index is processed first.

        private bool addingGamepiece = false; //true when user is currently selecting a gamepiece to be added.
        private bool definingIntake = false; // true when user is currently selecting a robot part for the intake mechanism
        private bool definingRelease = false;

        //for highlight current mechanism features
        private GameObject highlightedNode;
        private List<Color> originalColors = new List<Color>();
        private Color highlightColor = new Color(1, 1, 0, 0.1f);
        private int highlightTimer = -1;

        //for defining mechanism features
        private GameObject hoveredNode;
        private List<Color> hoveredColors = new List<Color>();
        private Color hoverColor = new Color(1, 1, 0, 0.1f);


        /// <summary>
        /// If configuration file exists, loads information and auto-configures robot.
        /// If coniguration file doesn't exist, initializes variables for users to configure.
        /// 
        /// Also loads gamepiece list from MainState.cs.
        /// 
        /// *NOTE: Because gamepiece identification in the new field format doesn't exist yet, we are using a predetermined gamepiece list. This must be changed as soon as support for gamepieces is added in the field exporter.*
        /// </summary>
        private void Start()
        {
            //Initializes all the configurable values and assigns them a default value.
            positionOffset = new UnityEngine.Vector3[2];
            positionOffset[0] = UnityEngine.Vector3.zero;
            positionOffset[1] = UnityEngine.Vector3.zero;

            releaseVelocity = new List<float[]>();
            releaseVelocity.Add(primaryVelocity);
            releaseVelocity.Add(secondaryVelocity);

            releaseVelocityVector = new List<UnityEngine.Vector3>();
            releaseVelocityVector.Add(UnityEngine.Vector3.zero);
            releaseVelocityVector.Add(UnityEngine.Vector3.zero);

            intakeNode = new List<GameObject>();
            intakeNode.Add(GameObject.Find("node_1.bxda")); //We want these to be null so that the user must configure it to a node first.
            intakeNode.Add(GameObject.Find("node_1.bxda"));

            releaseNode = new List<GameObject>();
            releaseNode.Add(GameObject.Find("node_1.bxda"));
            releaseNode.Add(GameObject.Find("node_1.bxda"));

            intakeInteractor = new List<Interactor>();
            intakeInteractor.Add(null);
            intakeInteractor.Add(null);

            objectsHeld = new List<List<GameObject>>();
            primaryHeld = new List<GameObject>();
            secondaryHeld = new List<GameObject>();
            objectsHeld.Add(primaryHeld);
            objectsHeld.Add(secondaryHeld);


            gamepieceNames = new List<string>();
            gamepieceNames.Add("WOAH");
            gamepieceNames.Add("TEST");

            spawnedGamepieces = new List<GameObject>();

            holdingLimit = new List<int>();
            holdingLimit.Add(30);
            holdingLimit.Add(30);

            SetInteractor(intakeNode[0], 0);
            SetInteractor(intakeNode[1], 1);



            drawnTrajectory = new List<LineRenderer>();
            drawnTrajectory.Add(gameObject.AddComponent<LineRenderer>());
            GameObject secondLine = new GameObject();
            drawnTrajectory.Add(secondLine.AddComponent<LineRenderer>());
            foreach (LineRenderer line in drawnTrajectory)
            {
                line.startWidth = 0.2f;
                line.material = Resources.Load("Materials/Projection") as Material;
                line.enabled = false;
            }
            drawnTrajectory[0].startColor = Color.blue;
            drawnTrajectory[0].endColor = Color.cyan;
            drawnTrajectory[1].startColor = Color.red;
            drawnTrajectory[1].endColor = Color.magenta;
        }
	
	    // Update is called once per frame
	    void Update () {
            if (modeEnabled)
            {
                if (processingIndex == 0)
                {
                    if (Input.GetKey(Controls.ControlKey[(int)Controls.Control.Pickup]))
                    {
                        Intake(0);
                        Intake(1);
                    }
                    if (Input.GetKeyDown(Controls.ControlKey[(int)Controls.Control.Release]))
                    {
                        ReleaseGamepiece(0);
                        ReleaseGamepiece(1);
                    }
                    else
                    {
                        HoldGamepiece(0);
                        HoldGamepiece(1);
                    }
                    processingIndex = 1;
                }
                else
                {
                    if (Input.GetKey(Controls.ControlKey[(int)Controls.Control.Pickup]))
                    {
                        Intake(1);
                        Intake(0);
                    }
                    if (Input.GetKeyDown(Controls.ControlKey[(int)Controls.Control.Release]))
                    {
                        ReleaseGamepiece(1);
                        ReleaseGamepiece(0);
                    }
                    else
                    {
                        HoldGamepiece(1);
                        HoldGamepiece(0);
                    }
                    processingIndex = 0;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (addingGamepiece) SetGamepiece(configuringIndex);
                    else if (definingIntake || definingRelease) SetMechanism(configuringIndex);
                }

                if (definingIntake || definingRelease) SelectingNode();

                if (Input.GetKey(KeyCode.T)) SpawnGamepiece(0);

                if (displayTrajectories)
                {
                    for (int i = 0; i < 2; i++)
                    {

                        releaseVelocityVector[i] = VelocityToVector3(releaseVelocity[i][0], releaseVelocity[i][1], releaseVelocity[i][2]);
                        if (!drawnTrajectory[i].enabled) drawnTrajectory[i].enabled = true;
                        DrawTrajectory(releaseNode[i].transform.position + releaseNode[i].GetComponent<BRigidBody>().transform.rotation * positionOffset[i], releaseNode[i].GetComponent<BRigidBody>().velocity + releaseNode[i].transform.rotation * releaseVelocityVector[i], drawnTrajectory[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < 2; i++)
                    {

                        if (drawnTrajectory[i].enabled) drawnTrajectory[i].enabled = false;
                    }
                }

                if (highlightTimer > 0) highlightTimer--;
                else if (highlightTimer == 0) RevertHighlight();
            }
	    }

        private void OnGUI()
        {
        }

        #region Gamepiece Manipulation Functions
        /// <summary>
        /// If the robot's intake node is touching an gamepiece, make the robot 'intake' it by adding it to the list of held objects and cheats physics by disabling collisions on the gamepiece.
        /// </summary>
        void Intake(int index)
        {
            if (objectsHeld[index].Count < holdingLimit[index] && intakeInteractor[index].GetDetected(index))
            {
                for (int i = 0; i < objectsHeld[0].Count; i++)
                {
                    if (objectsHeld[0][i].Equals(intakeInteractor[0].GetObject(index))) return; //This makes sure the object the robot is touching isn't an object already being held.
                }
                for (int i = 0; i < objectsHeld[1].Count; i++)
                {
                    if (objectsHeld[1][i].Equals(intakeInteractor[1].GetObject(index))) return; //This makes sure the object the robot is touching isn't an object already being held.
                }
                GameObject newObject = intakeInteractor[index].GetObject(index);
                objectsHeld[index].Add(newObject);
                newObject.GetComponent<BRigidBody>().velocity = UnityEngine.Vector3.zero;
                newObject.GetComponent<BRigidBody>().angularVelocity = UnityEngine.Vector3.zero;
                newObject.GetComponent<BRigidBody>().collisionFlags = BulletSharp.CollisionFlags.NoContactResponse;

                intakeInteractor[index].heldGamepieces.Add(newObject);


                foreach (BRigidBody rb in this.GetComponentsInChildren<BRigidBody>())
                {
                    newObject.GetComponent<BRigidBody>().GetCollisionObject().SetIgnoreCollisionCheck(rb.GetCollisionObject(), true);
                }
            }
        }

        /// <summary>
        /// Binds every gamepiece the robot is holding to the proper node and its position.
        /// </summary>
        private void HoldGamepiece(int index)
        {

            BRigidBody nrb; //rigid body of the release node
            nrb = releaseNode[index].GetComponent<BRigidBody>();

            if (objectsHeld[index].Count > 0)
            {
                BRigidBody orb; //rigid body of the object


                for (int i = 0; i < objectsHeld[index].Count; i++)
                {
                  orb = objectsHeld[index][i].GetComponent<BRigidBody>();
                  orb.velocity = nrb.velocity;
                  orb.SetPosition(nrb.transform.position + nrb.transform.rotation * positionOffset[index]);
                    orb.angularVelocity = UnityEngine.Vector3.zero;
                    orb.angularFactor = UnityEngine.Vector3.zero;

                }
            }
        }

        /// <summary>
        /// Releases the gamepiece from the robot at a set velocity
        /// </summary>
        private void ReleaseGamepiece(int index)
        {
            if (objectsHeld[index].Count > 0)
            {
                BRigidBody orb = objectsHeld[index][0].GetComponent<BRigidBody>();
                orb.collisionFlags = BulletSharp.CollisionFlags.None;
                orb.velocity += releaseNode[index].transform.rotation * releaseVelocityVector[index];
                orb.angularFactor = UnityEngine.Vector3.one;
                StartCoroutine(UnIgnoreCollision(objectsHeld[index][0]));
                intakeInteractor[index].heldGamepieces.Remove(objectsHeld[index][0]);
                objectsHeld[index].RemoveAt(0);
            }
        }

        /// <summary>
        /// Waits .5 seconds before renabling collisions between the release gamepiece and the robot.
        /// </summary>
        IEnumerator UnIgnoreCollision(GameObject obj)
        {
            yield return new WaitForSeconds(0.5f);

            foreach (BRigidBody rb in this.GetComponentsInChildren<BRigidBody>())
            {
                obj.GetComponent<BRigidBody>().GetCollisionObject().SetIgnoreCollisionCheck(rb.GetCollisionObject(), false);
            }
        }



        private UnityEngine.Vector3 VelocityToVector3(float speed, float horAngle, float verAngle)
        {
            UnityEngine.Quaternion horVector;
            UnityEngine.Quaternion verVector;
            UnityEngine.Vector3 finalVector = UnityEngine.Vector3.zero;

            //finalVector.x = speed * Mathf.Cos(horAngle * Mathf.Deg2Rad);
            //finalVector.y = speed * Mathf.Sin(verAngle * Mathf.Deg2Rad);
            //finalVector.z = Mathf.Sqrt(speed * speed - finalVector.y * finalVector.y - finalVector.x * finalVector.x);
            horVector = UnityEngine.Quaternion.AngleAxis(horAngle, UnityEngine.Vector3.up);
            verVector = UnityEngine.Quaternion.AngleAxis(verAngle, UnityEngine.Vector3.right);

            UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(verAngle, horAngle, 0);

            finalVector = (UnityEngine.Quaternion.LookRotation(UnityEngine.Vector3.forward,UnityEngine.Vector3.up) * horVector * verVector) * UnityEngine.Vector3.forward * speed;

            return (finalVector);

        }

        /// <summary>
        /// Illustrates the trajectory a released gamepiece would follow.
        /// Does this by creating rendering lines bounded to several vertices positioned based on multiplying velocity, gravity, and time.
        /// </summary>
        /// <param name="position">starting position of the gamepiece</param>
        /// <param name="velocity">starting velocity of the gamepiece</param>
        void DrawTrajectory(UnityEngine.Vector3 position, UnityEngine.Vector3 velocity, LineRenderer line)
        {
            int verts = 100; //This determines how far along time the illustration goes.
            line.positionCount = verts;

            UnityEngine.Vector3 pos = position;
            UnityEngine.Vector3 vel = velocity;
            UnityEngine.Vector3 grav = GameObject.Find("BulletPhysicsWorld").GetComponent<BPhysicsWorld>().gravity;
            for (int i = 0; i < verts; i++)
            {
                line.SetPosition(i, pos);
                vel = vel + grav * Time.fixedDeltaTime;
                pos = pos + vel * Time.fixedDeltaTime;
            }
        }
        #endregion

        #region Configuring Gamepiece

        /// <summary>
        /// Allows the user to select a dynamic object with their mouse and add it to the list of gamepieces.
        /// </summary>
        public void SetGamepiece(int index)
        {
            //Casts a ray from the camera in the direction the mouse is in and returns the closest object hit
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
                string name = (rayResult.CollisionObject.UserObject.ToString().Replace(" (BulletUnity.BRigidBody)", ""));
                Debug.Log(name);
                if (rayResult.CollisionObject.CollisionFlags == BulletSharp.CollisionFlags.StaticObject)
                {
                    UserMessageManager.Dispatch("The gamepiece must be a dynamic object!", 3);
                }
                else if (GameObject.Find(name) == null)
                {
                    Debug.Log("DPM: Game object not found");
                    
                }
                else if (GameObject.Find(name).transform.parent != null && GameObject.Find(name).transform.parent.name == "Robot")
                {
                    UserMessageManager.Dispatch("You cannot select a robot part as a gamepiece!", 3);
                }
                else
                {   
                    gamepieceNames[index] = name.Replace("(Clone)",""); //gets rid of the clone tag given to spawned gamepieces 
                    intakeInteractor[index].SetKeyword(gamepieceNames[index],index);
                    GameObject gamepiece = GameObject.Find(name);

                    UserMessageManager.Dispatch(name + " has been selected as the gamepiece", 2);
                    addingGamepiece = false;
                }
            }
            else
            {
                
            }
        }

        public void DefineGamepiece(int index)
        {
            if (modeEnabled)
            {
                if (definingIntake) UserMessageManager.Dispatch("You must select a robot part first!", 5);
                else
                {
                    UserMessageManager.Dispatch("Click on a dynamic object to add it as a gamepiece", 5);
                    configuringIndex = index;
                    addingGamepiece = true;
                }
            }
        }

        /// <summary>
        /// Spawns a new gamepiece at its defined spawn location, or at the field's origin if one hasn't been defined.
        /// </summary>
        /// <param name="index">0 if primary gamepiece, 1 if secondary gamepiece</param>
        public void SpawnGamepiece(int index)
        {
            if (gamepieceNames[index] != null)
            {
                try //In case the game piece somehow doens't exist in the scene
                {
                    spawnedGamepieces.Add(Instantiate(AuxFunctions.FindObject(gamepieceNames[index]).GetComponentInParent<BRigidBody>().gameObject, new UnityEngine.Vector3(0, 3, 0), UnityEngine.Quaternion.identity));
                }
                catch
                {
                    UserMessageManager.Dispatch("Gamepiece not found!", 5);
                }
            }
            else UserMessageManager.Dispatch("You must define the gamepiece first!", 5);
        }

        /// <summary>
        /// Clears all the gamepieces sharing the same name as the ones that have been configured from the field.
        /// </summary>
        public void ClearGamepieces()
        {
            foreach (GameObject g in spawnedGamepieces)
            {
                Destroy(g);
            }
        }

        #endregion

        #region Configuring Mechanisms

        /// <summary>
        /// Allows the user to select a robot node with their mouse and change the intake/release node
        /// </summary>
        /// <param name="index">configuring index</param>
        public void SetMechanism(int index)
        {
            //Casts a ray from the camera in the direction the mouse is in and returns the closest object hit
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
                string name = (rayResult.CollisionObject.UserObject.ToString().Replace(" (BulletUnity.BRigidBody)", ""));
                Debug.Log(name);
                if (rayResult.CollisionObject.CollisionFlags == BulletSharp.CollisionFlags.StaticObject)
                {
                    UserMessageManager.Dispatch("Please click on a robot part", 3);
                }
                else if (GameObject.Find(name) == null)
                {
                    Debug.Log("DPM: Game object not found");

                }
                else if (GameObject.Find(name).transform.parent != null && GameObject.Find(name).transform.parent.name == "Robot")
                {
                    if (definingIntake)
                    {
                        intakeNode[index] = GameObject.Find(name);
                        SetInteractor(intakeNode[index], index);

                        UserMessageManager.Dispatch(name + " has been selected as intake node", 5);
                        
                        definingIntake = false;
                    }
                    else
                    {
                        releaseNode[index] = GameObject.Find(name);
                        SetInteractor(releaseNode[index], index);

                        UserMessageManager.Dispatch(name + " has been selected as release node", 5);

                        definingRelease = false;
                    }

                    RevertNodeColors(hoveredNode, hoveredColors);
                }
                else
                {
                    UserMessageManager.Dispatch("A gamepiece is NOT a robot part!", 3);
                }
            }
            else
            {

            }
        }

        private void SelectingNode()
        {
            //Casts a ray from the camera in the direction the mouse is in and returns the closest object hit
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
                string name = (rayResult.CollisionObject.UserObject.ToString().Replace(" (BulletUnity.BRigidBody)", ""));
                Debug.Log(name);
                if (rayResult.CollisionObject.CollisionFlags == BulletSharp.CollisionFlags.StaticObject)
                {
                    RevertNodeColors(hoveredNode, hoveredColors);
                }
                else if (GameObject.Find(name) == null)
                {
                    Debug.Log("DPM: Game object not found");
                    RevertNodeColors(hoveredNode, hoveredColors);
                }
                else if (GameObject.Find(name).transform.parent != null && GameObject.Find(name).transform.parent.name == "Robot")
                {
                    if (hoveredNode != GameObject.Find(name))
                    {
                        RevertNodeColors(hoveredNode, hoveredColors);
                    }

                    hoveredNode = GameObject.Find(name);

                    ChangeNodeColors(hoveredNode, hoverColor, hoveredColors);

                }
                else RevertNodeColors(hoveredNode, hoveredColors);
            }
        }

        public void DefineIntake(int index)
        {
            if (modeEnabled)
            {
                if (addingGamepiece) UserMessageManager.Dispatch("You must select a gamepiece first!", 5);
                else if (definingRelease) UserMessageManager.Dispatch("You must define the release mechanism first!", 5);
                else
                {
                    UserMessageManager.Dispatch("Click on a robot part to define it as the intake mechanism", 5);
                    configuringIndex = index;
                    definingIntake = true;
                }
            }
        }

        public void DefineRelease(int index)
        {
            if (modeEnabled)
            {
                if (addingGamepiece) UserMessageManager.Dispatch("You must select a gamepiece first!", 5);
                else if (definingIntake) UserMessageManager.Dispatch("You must define the intake mechanism first!", 5);
                else
                {
                    UserMessageManager.Dispatch("Click on a robot part to define it as the release mechanism", 5);
                    configuringIndex = index;
                    definingRelease = true;
                }
            }
        }

        private void SetInteractor(GameObject node, int index)
        {
            if (node.GetComponent<Interactor>() == null) intakeInteractor[index] = node.AddComponent<Interactor>();
            else intakeInteractor[index] = node.GetComponent<Interactor>();

            intakeInteractor[index].SetKeyword(gamepieceNames[index], index);
        }

        public void HighlightNode(string node)
        {
            RevertHighlight();
            highlightedNode = GameObject.Find(node);
            ChangeNodeColors(highlightedNode, highlightColor, originalColors);
            highlightTimer = 120;
 

        }
        public void RevertHighlight()
        {
            RevertNodeColors(highlightedNode, originalColors);
            highlightedNode = null;
            highlightTimer = -1;
        }

        #endregion

        #region Configuring Vector Values


        public void ChangeOffsetX(float amount, int index)
        {
            positionOffset[index].x += amount;
        }
        public void ChangeOffsetY(float amount, int index)
        {
            positionOffset[index].y += amount;
        }
        public void ChangeOffsetZ(float amount, int index)
        {
            positionOffset[index].z += amount;
        }
        public void ChangeReleaseSpeed(float amount, int index)
        {
            releaseVelocity[index][0] += amount;
        }
        public void ChangeReleaseHorizontalAngle(float amount, int index)
        {
            releaseVelocity[index][1] += amount;
        }
        public void ChangeReleaseVerticalAngle(float amount, int index)
        {
            releaseVelocity[index][2] += amount;
        }
        #endregion

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
    }
}