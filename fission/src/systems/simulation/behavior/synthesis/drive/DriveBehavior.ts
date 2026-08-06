import Behavior from "@/systems/simulation/behavior/Behavior.ts"

/**
 * Common base class for all drivetrain behaviors (skid-steer, swerve, etc.).
 *
 * Allows {@link SynthesisBrain} to locate and swap the active drive behavior via
 * a single `instanceof DriveBehavior` check regardless of the concrete drivetrain.
 */
export abstract class DriveBehavior extends Behavior {
    protected _testingForwardSpeed: number|null = null

    runTestingForward(speed: number): void {
        this._testingForwardSpeed = speed
    }
    releaseTesting(): void {
        this._testingForwardSpeed = null
    }
}

export default DriveBehavior
