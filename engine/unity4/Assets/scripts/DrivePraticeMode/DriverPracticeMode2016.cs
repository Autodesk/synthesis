using UnityEngine;
using System.Collections;

public partial class DriverPracticeMode : MonoBehaviour {

    /// <summary>
    /// Initializes non-robot specific interactor variables specific to the 2016 field.
    /// </summary>
    public void Init2016()
    {
        //Adding interactor scripts to run in each of the nodes to detect collisions.
        interactors = new Interactor[4];

        for (int i = 0; i < interactors.Length; i++)
            interactors[i] = interactorObjects[i].AddComponent<Interactor>();

        //The collision keyword is a set of characters that are used to identify if the interactor is colliding with the proper game object.
        interactors[0].collisionKeyword = "GE-16180"; //The keyword for the 2016 ball.
        interactors[1].collisionKeyword = "123";
        interactors[2].collisionKeyword = "123";
        interactors[3].collisionKeyword = "123";

        gameType = 2016;
        holdingLimit = 4; //Rules only allow one ball to be held at a time.
        
    }

    /// <summary>
    /// Initializes the 2016 SOTAbots robot for driver practice mode.
    /// To set up your own robot for driver practice mode, follow this format.
    /// </summary>
    /// <remarks>
    /// For the 2016 field:
    /// [0] = Ball Grabber
    /// [1] = Ball Shooter
    /// [2] = Defense Manipulator   
    /// [3] = Climber
    /// </remarks>
    public void InitSota2016()
    {
        interactorObjects = new GameObject[4];
        interactorObjects[0] = GameObject.Find("node_1.bxda");
        interactorObjects[1] = GameObject.Find("node_0.bxda"); //SOTAbots shoot from the center of their robot, so we just set the main rigidnode as the ball shooter interactor.
        interactorObjects[2] = GameObject.Find("node_0.bxda");
        interactorObjects[3] = GameObject.Find("node_0.bxda");

        Init2016();
    }

    public void InitSimbotics2012()
    {
        interactorObjects = new GameObject[4];
        interactorObjects[0] = GameObject.Find("node_1.bxda");
        interactorObjects[1] = GameObject.Find("node_0.bxda"); //SOTAbots shoot from the center of their robot, so we just set the main rigidnode as the ball shooter interactor.
        interactorObjects[2] = GameObject.Find("node_0.bxda");
        interactorObjects[3] = GameObject.Find("node_0.bxda");

        Init2016();
    }

    /// <summary>
    /// If there is a ball touching and the robot is not currently holding a ball,
    /// records the ball object as a held object and makes it ignore collisions with the robot and the ball as it is a projectile.
    /// </summary>
    private void IntakeBall2016()
    {
        if (ObjectsHeld.Count < holdingLimit && interactors[0].getDetected())
        {
            for (int i = 0; i < ObjectsHeld.Count; i++) if (ObjectsHeld[i].Equals(interactors[0].getObject())) return;
            //The code iterates through every single collider of the robot to ignore collisions with them and the ball.
            ObjectsHeld.Add(interactors[0].getObject());
            GameObject newObject = interactors[0].getObject();
            GameObject child;
            for (int i = 0; i < transform.childCount; i++)
            {
                for (int j = 0; j < transform.GetChild(i).childCount; j++)
                {
                    child = transform.GetChild(i).GetChild(j).gameObject;
                    if (child.name.Contains("ollider")) Physics.IgnoreCollision(child.collider, newObject.collider, true);
                }
            }
            for (int i = 0; i < ObjectsHeld.Count-1; i++)
            {
                Physics.IgnoreCollision(newObject.collider, ObjectsHeld[i].collider, true);
            }
        }
    }

    /// <summary>
    /// If the robot is currently holding a ball, then bind the ball's position to the node's position with an additional offset to have it be wherever it would be normally to shoot.
    /// </summary>
    private void HoldBall2016()
    {
        if (ObjectsHeld.Count > 0)
        {
            Vector3 offset = new Vector3(0f, 0.2f, 0.3f);
            for (int i = 0; i < ObjectsHeld.Count; i++)
            {
                ObjectsHeld[i].rigidbody.velocity = interactorObjects[1].rigidbody.velocity;
                ObjectsHeld[i].rigidbody.position = interactorObjects[1].rigidbody.position + (interactorObjects[1].rigidbody.rotation * offset);
            }
        }
    }

    /// <summary>
    /// Shoots a ball if the robot is curren tly holding one.
    /// </summary
    private void ShootBall2016()
    {
        if (ObjectsHeld.Count > 0)
        {
            ObjectsHeld[0].rigidbody.AddForce(interactorObjects[1].rigidbody.rotation * new Vector3(0, 0.22f, 0.15f));
            StartCoroutine(UnIgnoreCollision(ObjectsHeld[0]));
            ObjectsHeld.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// Runs every 'step' that the physics simulation runs in to 
    /// </summary>
    void FixedUpdate2016 () {

        


        HoldBall2016();
        //Rolling a ball in
        if (Input.GetKey(KeyCode.Space))
            IntakeBall2016();

        if (Input.GetKey(KeyCode.Q))
        {
            GameObject ball = (GameObject)Instantiate(GameObject.Find("GE-16180_1279"), Vector3.zero,Quaternion.identity);
            ball.AddComponent<Rainbow>();
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            GameObject.Find("node_0.bxda").AddComponent<Rainbow>();
            GameObject.Find("node_1.bxda").AddComponent<Rainbow>();
        }
    }

    void Update2016()
    {
        //Shooting a ball
        if (Input.GetKeyDown(KeyCode.E))
        {
            ShootBall2016();
        }
    }
}