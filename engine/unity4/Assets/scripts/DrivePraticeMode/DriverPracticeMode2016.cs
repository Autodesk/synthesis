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
        holdingLimit = 1; //Rules only allow one ball to be held at a time.
        
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
        interactorObjects[0] = GameObject.Find("node_7.bxda");
        interactorObjects[1] = GameObject.Find("node_7.bxda"); //SOTAbots shoot from the center of their robot, so we just set the main rigidnode as the ball shooter interactor.
        interactorObjects[2] = GameObject.Find("node_7.bxda");
        interactorObjects[3] = GameObject.Find("node_8.bxda");

        Init2016();
    }

    /// <summary>
    /// If there is a ball touching and the robot is not currently holding a ball,
    /// records the ball object as a held object and makes it ignore collisions with the robot and the ball as it is a projectile.
    /// </summary>
    private void IntakeBall2016()
    {
        if (ObjectsHeld.Count == 0 && interactors[0].getDetected())
        {
            //The code iterates through every single collider of the robot to ignore collisions with them and the ball.
            ObjectsHeld.Add(interactors[0].getObject());
            GameObject child;
            for (int i = 0; i < transform.childCount; i++)
            {
                for (int j = 0; j < transform.GetChild(i).childCount; j++)
                {
                    child = transform.GetChild(i).GetChild(j).gameObject;
                    if (child.name.Contains("ollider")) Physics.IgnoreCollision(child.collider, (ObjectsHeld[0]).collider, true);
                }
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
            Vector3 offset = new Vector3(-0.28f, 0.1f, -0.5f);
            ObjectsHeld[0].rigidbody.velocity = interactorObjects[1].rigidbody.velocity;
            ObjectsHeld[0].rigidbody.position = interactorObjects[1].rigidbody.position + (interactorObjects[1].rigidbody.rotation * offset);
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
            ObjectsHeld.RemoveAt(0);
            StartCoroutine("UnIgnoreCollision");
        }
    }
    
    /// <summary>
    /// Runs every 'step' that the physics simulation runs in to 
    /// </summary>
    void Update2016 () {

        //Rolling a ball in
        if (Input.GetKey(KeyCode.Space))
            IntakeBall2016();

        HoldBall2016();

        //Shooting a ball
        if (Input.GetKey(KeyCode.RightShift))
        {
            ShootBall2016();
        }
        Debug.Log(ObjectsHeld.Count);
    }
}