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

        // Field-oriented drive is zeroed to the robot's heading at the moment swerve is configured
        // (spawn, or when the drivetrain is switched to swerve), so "forward" lines up with the
        // robot's nose initially; the reset input re-zeroes it later. Falls back to world +X if the
        // body can't be resolved yet.
        const rootNodeId = this.resolveRootNodeId()
        if (rootNodeId != undefined) {
            const rotation = convertJoltQuatToThreeQuaternion(World.physicsSystem.getBody(rootNodeId).GetRotation())
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

    /** @returns true if the difference between a and b is within acceptableDelta */
    private static withinTolerance(a: number, b: number, acceptableDelta: number) {
        return Math.abs(a - b) < acceptableDelta
    }

    // Sets the drivetrain's target linear and rotational velocity
    private driveSpeeds(forward: number, strafe: number, turn: number) {
        const rootNodeId = this.resolveRootNodeId()

        if (rootNodeId == undefined) throw new Error("Robot root node should not be undefined")

        const robotRotation = convertJoltQuatToThreeQuaternion(World.physicsSystem.getBody(rootNodeId).GetRotation())

        const robotForward: THREE.Vector3 = new THREE.Vector3(0, 0, 1).applyQuaternion(robotRotation)
        const robotRight: THREE.Vector3 = new THREE.Vector3(1, 0, 0).applyQuaternion(robotRotation)
        const robotUp: THREE.Vector3 = new THREE.Vector3(0, 1, 0).applyQuaternion(robotRotation)

        if (InputSystem.getInput("swerveResetFieldForward", this._brainIndex)) this._fieldForward = robotForward

        const headingVector: THREE.Vector3 = robotForward
            .clone()
            .sub(new THREE.Vector3(0, 1, 0).multiplyScalar(new THREE.Vector3(0, 1, 0).dot(robotForward)))

        const headingVectorY: number = this._fieldForward.dot(headingVector)
        // NOTE: THREE.Vector3.cross() mutates the receiver, so clone _fieldForward before crossing.
        const headingVectorX: number = this._fieldForward
            .clone()
            .cross(new THREE.Vector3(0, 1, 0))
            .dot(headingVector)
        const chassisAngleRad: number = Math.atan2(headingVectorX, headingVectorY)

        forward = SwerveDriveBehavior.withinTolerance(forward, 0.0, 0.1) ? 0.0 : forward
        strafe = SwerveDriveBehavior.withinTolerance(strafe, 0.0, 0.1) ? 0.0 : strafe
        turn = SwerveDriveBehavior.withinTolerance(turn, 0.0, 0.1) ? 0.0 : turn

        // Inputs are basically zero: stop the wheels and leave the modules where they are.
        if (forward == 0.0 && turn == 0.0 && strafe == 0.0) {
            this._wheels.forEach(w => {
                w.accelerationDirection = 0
            })
            World.physicsSystem.enablePhysicsForBody(rootNodeId)
            return
        }

        // Adjusts how much turning versus translation is favored
        turn *= 1.5

        const chassisVelocity: THREE.Vector3 = robotForward
            .clone()
            .multiplyScalar(forward)
            .add(robotRight.clone().multiplyScalar(strafe))
        const chassisAngularVelocity: THREE.Vector3 = robotUp.clone().multiplyScalar(turn)

        // Normalize translation so its magnitude is at most 1.
        if (chassisVelocity.length() > 1) chassisVelocity.normalize()

        // Field-oriented drive: rotate the commanded chassis velocity into the field frame by the
        // chassis heading (original `Quaternion.AngleAxis(chassisAngle, up) * v`).
        chassisVelocity.applyAxisAngle(robotUp, chassisAngleRad)

        let maxVelocity = new THREE.Vector3()
        const com = convertJoltVec3ToThreeVector3(World.physicsSystem.getBody(rootNodeId).GetCenterOfMassPosition())

        const velocities: THREE.Vector3[] = []
        for (let i = 0; i < this._hinges.length; i++) {
            const driver = this._hinges[i]

            const axis = convertJoltVec3ToThreeVector3(driver.worldAxis).normalize()
            const radius = convertJoltVec3ToThreeVector3(driver.worldAnchor).sub(com)
            // Remove the component of the radius along the hinge axis so the moment arm is purely
            // in the steering plane (matches original `radius -= dot(axis, radius) * axis`).
            radius.sub(axis.clone().multiplyScalar(axis.dot(radius)))

            velocities[i] = chassisAngularVelocity.clone().cross(radius).add(chassisVelocity)
            if (velocities[i].length() > maxVelocity.length()) maxVelocity = velocities[i]
        }

        // Normalize all module velocities together if any exceeds 1, preserving their ratios.
        const maxVelocityLength = maxVelocity.length()
        if (maxVelocityLength > 1) {
            for (let i = 0; i < this._wheels.length; i++) {
                velocities[i].divideScalar(maxVelocityLength)
            }
        }

        for (let i = 0; i < this._wheels.length; i++) {
            const speed: number = velocities[i].length()
            const yComponent: number = robotForward.dot(velocities[i])
            const xComponent: number = robotRight.dot(velocities[i])

            const currentAngle = this._hinges[i].constraint.GetCurrentAngle()

            let angle: number = Math.atan2(xComponent, yComponent)
            let driveSpeed: number = speed
            let delta: number = angle - currentAngle
            while (delta > Math.PI) delta -= 2 * Math.PI
            while (delta < -Math.PI) delta += 2 * Math.PI
            if (Math.abs(delta) > Math.PI / 2) {
                angle += angle > 0 ? -Math.PI : Math.PI
                driveSpeed = -speed
            }

            // Steering is applied by physically rotating the module via its azimuth hinge (the wheel
            // rides on the module body and follows it). The wheel's own steer angle is intentionally
            // left alone, setting it would double-steer on top of the hinge rotation.
            this._hinges[i].targetAngle = angle
            this._wheels[i].accelerationDirection = driveSpeed
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
