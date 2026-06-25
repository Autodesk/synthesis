import * as THREE from "three"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"
import InputSystem from "@/systems/input/InputSystem.ts"
import { DriveBehavior } from "@/systems/simulation/behavior/synthesis/drive/DriveBehavior.ts"
import type WheelDriver from "@/systems/simulation/driver/WheelDriver.ts"
import type WheelRotationStimulus from "@/systems/simulation/stimulus/WheelStimulus.ts"
import World from "@/systems/World.ts"
import { convertJoltQuatToThreeQuaternion, convertJoltVec3ToThreeVector3 } from "@/util/TypeConversions.ts"
import type Driver from "@/systems/simulation/driver/Driver.ts"
import { DriverControlMode } from "@/systems/simulation/driver/Driver.ts"
import type HingeDriver from "@/systems/simulation/driver/HingeDriver.ts"
import type HingeStimulus from "@/systems/simulation/stimulus/HingeStimulus.ts"
import type Stimulus from "@/systems/simulation/stimulus/Stimulus.ts"

class SwerveDriveBehavior extends DriveBehavior {
    private _wheels: WheelDriver[]
    private _hinges: HingeDriver[]
    private _brainIndex: number
    private _assemblyName: string

    private _forwardSpeed = 30
    private _strafeSpeed = 30
    private _turnSpeed = 30

    private _fieldForward: THREE.Vector3 = new THREE.Vector3(1, 0, 0)

    constructor(
        wheels: WheelDriver[],
        hinges: HingeDriver[],
        wheelStimuli: WheelRotationStimulus[],
        hingeStimuli: HingeStimulus[],
        brainIndex: number,
        assemblyName: string
    ) {
        super((wheels as Driver[]).concat(hinges), (wheelStimuli as Stimulus[]).concat(hingeStimuli))

        this._wheels = wheels
        this._hinges = hinges
        this._brainIndex = brainIndex
        this._assemblyName = assemblyName

        hinges.forEach(h => {
            h.controlMode = DriverControlMode.POSITION
            // Swerve steering modules rotate continuously.
            h.setContinuousRotation()
        })

        // Zero field-oriented drive to the robot's spawn heading so "forward" starts aligned with
        // its nose. The reset input re-zeroes it later; falls back to world +X if the body isn't ready.
        const rootNodeId = this.resolveRootNodeId()
        if (rootNodeId) {
            const rotation = convertJoltQuatToThreeQuaternion(World.physicsSystem.getBody(rootNodeId)!.GetRotation())
            this._fieldForward = new THREE.Vector3(0, 0, 1).applyQuaternion(rotation)
        }
    }

    /** @returns the physics body id of this assembly's root node, or undefined if not found. */
    private resolveRootNodeId() {
        return [...World.sceneRenderer.sceneObjects.entries()]
            .filter(x => x[1] instanceof MirabufSceneObject)
            .map(x => x[1] as MirabufSceneObject)
            .find(o => o.assemblyName == this._assemblyName)
            ?.getRootNodeId()
    }

    private static deadband(x: number, threshold = 0.1): number {
        return Math.abs(x) < threshold ? 0 : x
    }

    private driveSpeeds(forward: number, strafe: number, turn: number) {
        const rootNodeId = this.resolveRootNodeId()
        if (rootNodeId == undefined) throw new Error("Robot root node should not be undefined")

        const rotation = convertJoltQuatToThreeQuaternion(World.physicsSystem.getBody(rootNodeId)!.GetRotation())
        const robotForward = new THREE.Vector3(0, 0, 1).applyQuaternion(rotation)
        const robotRight = new THREE.Vector3(1, 0, 0).applyQuaternion(rotation)
        const robotUp = new THREE.Vector3(0, 1, 0).applyQuaternion(rotation)

        if (InputSystem.getInput("swerveResetFieldForward", this._brainIndex)) this._fieldForward = robotForward.clone()

        forward = SwerveDriveBehavior.deadband(forward)
        strafe = SwerveDriveBehavior.deadband(strafe)
        turn = SwerveDriveBehavior.deadband(turn)

        if (forward === 0 && strafe === 0 && turn === 0) {
            this._wheels.forEach(w => {
                w.accelerationDirection = 0
            })
            World.physicsSystem.enablePhysicsForBody(rootNodeId)
            return
        }

        // Adjusts how much turning versus translation is favored.
        turn *= 1.5

        const chassisVelocity = robotForward
            .clone()
            .multiplyScalar(forward)
            .add(robotRight.clone().multiplyScalar(strafe))
        if (chassisVelocity.length() > 1) chassisVelocity.normalize()
        // Field-oriented drive: rotate commanded velocity by the chassis heading.
        chassisVelocity.applyAxisAngle(robotUp, this.fieldOrientedAngle(robotForward))

        const chassisAngularVelocity = robotUp.clone().multiplyScalar(turn)
        const com = convertJoltVec3ToThreeVector3(
            World.physicsSystem.getBody(rootNodeId)!.GetCenterOfMassPosition(),
            false
        )

        const velocities = this.computeModuleVelocities(chassisVelocity, chassisAngularVelocity, com)
        this.applyModuleTargets(velocities, robotForward, robotRight)
    }

    /** Returns the field-oriented angle (radians) for the current chassis heading. */
    private fieldOrientedAngle(robotForward: THREE.Vector3): number {
        const worldUp = new THREE.Vector3(0, 1, 0)
        // Project robotForward onto the horizontal plane.
        const heading = robotForward.clone().sub(worldUp.clone().multiplyScalar(worldUp.dot(robotForward)))
        const headingY = this._fieldForward.dot(heading)
        // cross() mutates the receiver, so clone _fieldForward before crossing.
        const headingX = this._fieldForward.clone().cross(worldUp).dot(heading)
        return Math.atan2(headingX, headingY)
    }

    /** Computes the target velocity vector for each swerve module. */
    private computeModuleVelocities(
        chassisVelocity: THREE.Vector3,
        chassisAngularVelocity: THREE.Vector3,
        com: THREE.Vector3
    ): THREE.Vector3[] {
        const velocities: THREE.Vector3[] = []
        let maxSpeed = 0

        for (let i = 0; i < this._hinges.length; i++) {
            const axis = convertJoltVec3ToThreeVector3(this._hinges[i].worldAxis).normalize()
            const radius = convertJoltVec3ToThreeVector3(this._hinges[i].worldAnchor).sub(com)
            // Remove the axis component so the moment arm is purely in the steering plane.
            radius.sub(axis.clone().multiplyScalar(axis.dot(radius)))

            velocities[i] = chassisAngularVelocity.clone().cross(radius).add(chassisVelocity)
            const speed = velocities[i].length()
            if (speed > maxSpeed) maxSpeed = speed
        }

        // Normalize all module velocities together if any exceeds 1, preserving their ratios.
        if (maxSpeed > 1) velocities.forEach(v => v.divideScalar(maxSpeed))

        return velocities
    }

    /** Sets each module's target angle and wheel speed, flipping 180° when it shortens the turn. */
    private applyModuleTargets(
        velocities: THREE.Vector3[],
        robotForward: THREE.Vector3,
        robotRight: THREE.Vector3
    ): void {
        for (let i = 0; i < this._wheels.length; i++) {
            const speed = velocities[i].length()
            const currentAngle = this._hinges[i].constraint.GetCurrentAngle()

            let angle = Math.atan2(robotRight.dot(velocities[i]), robotForward.dot(velocities[i]))
            let delta = angle - currentAngle
            while (delta > Math.PI) delta -= 2 * Math.PI
            while (delta < -Math.PI) delta += 2 * Math.PI

            const flip = Math.abs(delta) > Math.PI / 2
            // Steering comes from physically rotating the module via its azimuth hinge; the wheel
            // rides on the module and follows it. Don't also set the wheel's steer angle (double-steer).
            this._hinges[i].targetAngle = flip ? angle + (angle > 0 ? -Math.PI : Math.PI) : angle
            this._wheels[i].accelerationDirection = flip ? -speed : speed
        }
    }

    public update(_: number): void {
        const forwardInput = InputSystem.getInput("swerveForward", this._brainIndex)
        const strafeInput = InputSystem.getInput("swerveStrafe", this._brainIndex)
        const turnInput = InputSystem.getInput("swerveTurn", this._brainIndex)

        this.driveSpeeds(
            forwardInput * this._forwardSpeed,
            strafeInput * this._strafeSpeed,
            turnInput * this._turnSpeed
        )
    }
}

export default SwerveDriveBehavior
