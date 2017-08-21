using UnityEngine;
using System.Collections;
using BulletUnity;
using System;

[RequireComponent(typeof(IControllable))]
// We make this an IComparable so we can sort them by name in our ChangeBehaviourScrollable class
public abstract class BaseSynthBehaviour : MonoBehaviour, IComparable<BaseSynthBehaviour>
{
    private static readonly float STUCK_TIME = 2.25f; // How long the robot must be stuck for before backing up, in seconds.
    private static readonly float BACKUP_TIME = 1.25f; // How long the robot should back up for, in seconds.

    protected bool driveNow = false; // A switch to turn on and off automatic robot steering towards next point.
                                     // Can be set to false if robot should not be controlling its driving.

    protected IControllable robot; // Reference to robot. Used to manipulate robot.
    protected UnityEngine.AI.NavMeshAgent agent; // The NavMesh agent pathfinds around obstacles.
    protected Vector3 LastPosition; // Used to calculate current velocity
    protected float curVelocity;
    protected float maxSpeed; // Max speed belongs to behaviour so that custom behaviours can use their own Max Speed
                              // It is recommended that custom behaviours set this max speed 
                              // to less than AIManager.Instance.AIMaxSpeed

    // Initialization
    void Start()
    {
        this.robot = GetComponent<IControllable>();

        // Setup NavMesh agent to pathfind in front of robot, avoiding obstacles.
        this.agent = new GameObject("AIPathfindingAgent").AddComponent<UnityEngine.AI.NavMeshAgent>();
        this.agent.transform.parent = this.transform;

        // Place NavMesh agent on the NavMesh. NavMesh.SamplePosition will always return a point on the NavMesh.
        UnityEngine.AI.NavMeshHit hit;
        UnityEngine.AI.NavMesh.SamplePosition(robot.GetPosition(), out hit, 50, UnityEngine.AI.NavMesh.AllAreas);
        this.agent.Warp(hit.position);

        // LastPosition is used to calculate curVelocity of robot.
        LastPosition = this.robot.GetPosition();
        this.agent.speed = 0f;

        maxSpeed = SynthAIManager.Instance.AIMaxSpeed;
        StartCoroutine(this.UpdateAI());
    }

    // AI Logic updates happen every quarter second -- this keeps our process lightweight.
    private IEnumerator UpdateAI()
    {
        while (enabled)
        {
            AILogic();
            yield return new WaitForSeconds(0.25f);
        }
    }

    // The AI Logic method is inherited and implemented in behaviour classes to control robot's pathfinding destination
    protected abstract void AILogic();

    /// <summary>
    /// Initializes and starts the robot.
    /// </summary>
    /// <param name="main">The main state -- provides AI robot access to variables for processing</param>
    /// <returns></returns>
    public abstract bool Initialize(MainState main);

    /// <summary>
    /// Returns the name of this behaviour.
    /// </summary>
    /// <returns>The name of this behaviour.</returns>
    public override abstract string ToString();

    // This is a default steering behaviour for simple behaviours. More complicated behaviours may override this method.
    // Uses FixedUpdate to get accurate velocity readings
    protected virtual void FixedUpdate()
    {
        if (driveNow)
        {
            Vector3 robotPosition = robot.GetPosition();
            Vector3 targetPosition = this.agent.transform.position;
            // Time.deltaTime returns correct fixed update time when used in FixedUpdate()
            curVelocity = Vector3.Dot((robotPosition - LastPosition) / Time.deltaTime, robot.GetForward());

            // ######## NavAgent Logic ######## \\
            if (agent.hasPath)
            {
                float steeringDistance = Vector3.Distance(targetPosition, robotPosition);
                if (steeringDistance < SynthAIManager.Instance.AILookAhead)
                {
                    // The navAgent "drives ahead" of the robot while the robot steers towards it
                    this.agent.speed = maxSpeed * 1.1f;
                }
                else if (steeringDistance > SynthAIManager.Instance.AILookAhead * 1.75f)
                {
                    // If robot gets too far away from steering agent, reset steering agent's position in front
                    // of AI robot.
                    this.agent.transform.position = robotPosition + robot.GetForward() * Mathf.Abs(curVelocity);
                }
                else
                {
                    this.agent.speed = 0f;
                }

            }

            // ######## Rotation Logic ######## \\
            Vector3 robotForward = robot.GetForward();

            Vector3 targetRotationV = targetPosition - robotPosition;
            targetRotationV.y = 0;
            float angle = Vector3.Angle(robotForward, targetRotationV);

            // Use cross product to find polarity of angle (left or right?)
            Vector3 crossProd = Vector3.Cross(robotForward, targetRotationV);
            if (crossProd.y < 0)
            {
                angle = -angle;
            }

            // Turn at full power until there are less than 5 degrees to turn. Then slow down turning.
            robot.Turn(angle / 5f);

            // ######## Velocity Logic ######## \\

            // Compare current rotation to NavMesh rotation. This will give us an angle difference if the robot
            // is turning a sharp corner. cornerValue is a number between 0 and 1. A value of 0.5 should require a 
            // lots of slowing down to turn, while a value of 0.9 should only slow down a little bit.
            float angleDiff = Vector3.Angle(robotForward, agent.transform.forward);
            float cornerValue = 1 - Mathf.Min((angleDiff / 180f), 1f);

            // Target velocity is lower if we need to make a sharp turn and if NavMesh agent shows there is a corner
            float targetVelocity = (maxSpeed * 5 / (Mathf.Max(angle, 5))) * cornerValue;
            float deltaVelocity = targetVelocity - curVelocity;
            robot.Accelerate(deltaVelocity * 2);
            // Set LastPosition so that we can calculate velocity next frame

            LastPosition = robotPosition;
        }
    }

    private float timeStuck;
    // In update loop, check if robot is stuck, and back up if it is.
    protected virtual void Update()
    {
        // If robot is not moving and is in drive mode, robot is stuck
        if (Mathf.Abs(curVelocity) < 0.015f && driveNow && timeStuck < STUCK_TIME)
        {
            timeStuck += Time.deltaTime;
        }
        else if (timeStuck > STUCK_TIME && driveNow) // When stuck for a long time, stop driving, and back up.
        {
            // Reuse time stuck float as timer for backing up (back up for BACKUP_TIME seconds).
            timeStuck = STUCK_TIME + BACKUP_TIME;
            driveNow = false;
        }
        else if (timeStuck > STUCK_TIME)
        {
            robot.Accelerate(-1f);
            timeStuck -= Time.deltaTime;
        }
        else // If the robot is not stuck and is not driving, start driving.
        {
            driveNow = true;
            timeStuck = 0f;
        }
    }

    /// <summary>
    /// Used to sort behaviours in behaviour list by alphabetical order.
    /// </summary>
    /// <param name="other">Another BaseSynthBehaviour to compare.</param>
    /// <returns>An integer that indicates the strings' relative position in the sort order.</returns>
    public int CompareTo(BaseSynthBehaviour other)
    {
        return String.Compare(this.ToString(), other.ToString());
    }
}
