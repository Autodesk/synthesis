using UnityEngine;

public class RoamSynthBehaviour: BaseSynthBehaviour
{
    private Vector3 roamPosition;
    protected override void AILogic()
    {
        if(!agent.pathPending && !agent.hasPath)
        {
            UnityEngine.AI.NavMeshHit hit;
            Vector3 random = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f)); // Randomize destination

            // Places random position difference on NavMesh
            UnityEngine.AI.NavMesh.SamplePosition(robot.GetPosition() + random, out hit, 25f, UnityEngine.AI.NavMesh.AllAreas);

            roamPosition = hit.position;
        }
        this.agent.SetDestination(roamPosition);
        
    }

    public override bool Initialize(MainState main)
    {
        this.driveNow = true;
        maxSpeed = SynthAIManager.Instance.AIMaxSpeed / 2f; // Roaming is slower and less aggressive.
        Debug.Log("Roam behaviour initialized");
        return true;
    }

    public override string ToString()
    {
        return "Roam";
    }
}
