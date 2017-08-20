using UnityEngine;

public class DummySynthBehaviour : BaseSynthBehaviour
{
    protected override void AILogic()
    {
        // Dummy does nothing.
    }

    public override bool Initialize(MainState main)
    {
        this.driveNow = false; // Easy way to disable driving. We could also simply override FixedUpdate(), but that isn't necessary
        Debug.Log("Dummy behaviour initialized");
        return true;
    }

    public override string ToString()
    {
        return "Dummy";
    }

    protected override void Update()
    {
        // Do nothing
    }
}