using UnityEngine;
using System.Collections;

[RequireComponent(typeof(IControllable))]
public abstract class BaseSynthBehaviour : MonoBehaviour
{
    protected bool driveNow = false; // A switch to turn on and off automatic robot steering towards next point.
                                     // Can be set to false if robot should not be controlling its driving.
    protected IControllable robot;
    protected UnityEngine.AI.NavMeshAgent agent;
    protected Vector3 LastPosition; // Used to calculate current velocity
    protected float curVelocity;
    // Initialization
    void Start()
    {
        this.robot = GetComponent<IControllable>();
        this.agent = new GameObject("AIPathfindingAgent").AddComponent<UnityEngine.AI.NavMeshAgent>();
        this.agent.transform.parent = this.transform;

        UnityEngine.AI.NavMeshHit hit;
        UnityEngine.AI.NavMesh.SamplePosition(robot.GetPosition(), out hit, 50, UnityEngine.AI.NavMesh.AllAreas);
        this.agent.Warp(hit.position);
        LastPosition = this.robot.GetPosition();
        this.agent.speed = 0f;
        StartCoroutine(this.UpdateAI());
    }

    // AI Logic updates happen every quarter second
    private IEnumerator UpdateAI()
    {
        while (true)
        {
            AILogic();
            yield return new WaitForSeconds(0.25f);
        }
    }

    // The AI Logic method is inherited and implemented in behaviour classes to control robot's pathfinding destination
    // Later, this may also include robot manipulators and other things
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

    // This is a default updating behaviour for simple behaviours. More complicated behaviours may override this method.
    protected virtual void FixedUpdate()
    {
        if (driveNow)
        {
            Vector3 robotPosition = robot.GetPosition();
            Vector3 targetPosition = this.agent.transform.position;
            curVelocity = Vector3.Dot((robotPosition - LastPosition)/ Time.fixedDeltaTime, robot.GetForward());
            // ######## NavAgent Logic ######## \\
            if (agent.hasPath)
            {
                if (Vector3.Distance(this.agent.transform.position, robotPosition) < SynthAIManager.Instance.AILookAhead + Mathf.Abs(curVelocity))
                {
                    // The navAgent "drives ahead" of the robot while the robot steers towards it
                    this.agent.speed = SynthAIManager.Instance.AIMaxSpeed * 1.2f;
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
            // Use cross product to find polarity of angle
            Vector3 crossProd = Vector3.Cross(robotForward, targetRotationV);
            if (crossProd.y < 0)
            {
                angle = -angle;
            }
            
            // Turn at full power until there are less than 10 degrees to turn. Then slow down turning
            robot.Turn(angle / 10f);            
            
            // ######## Velocity Logic ######## \\
            float targetVelocity = SynthAIManager.Instance.AIMaxSpeed * 15 / (Mathf.Max(angle, 15));
            float deltaVelocity = targetVelocity - curVelocity;
            robot.Accelerate(deltaVelocity / SynthAIManager.Instance.AIMaxSpeed);
            // Set LastPosition so that we can calculate velocity next frame

            LastPosition = robotPosition;
        }
    }
}
