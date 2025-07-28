import Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import { Mesh } from "three"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"
import InputSystem from "@/systems/input/InputSystem.ts"
import { DriveBehavior } from "@/systems/simulation/behavior/synthesis/drive/DriveBehavior.ts"
import WheelDriver from "@/systems/simulation/driver/WheelDriver.ts"
import WheelRotationStimulus from "@/systems/simulation/stimulus/WheelStimulus.ts"
import World from "@/systems/World.ts"
import { joltVec3ToString, threeQuaternionToString, threeVector3ToString } from "@/util/debug/DebugPrint.ts"
import JOLT from "@/util/loading/JoltSyncLoader.ts"
import {
    convertJoltQuatToThreeQuaternion,
    convertJoltVec3ToThreeVector3,
} from "@/util/TypeConversions.ts"
import Driver, { DriverControlMode } from "../../../driver/Driver.ts"
import HingeDriver from "../../../driver/HingeDriver.ts"
import HingeStimulus from "../../../stimulus/HingeStimulus.ts"
import Stimulus from "../../../stimulus/Stimulus.ts"

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
            // h.constraint.SetLimits(-Math.PI, Math.PI)
            // h.constraint.SetLimits(0, 0)
            h.controlMode = DriverControlMode.POSITION
        })
    }

    /** @returns true if the difference between a and b is within acceptanceDelta */
    private static withinTolerance(a: number, b: number, acceptableDelta: number) {
        return Math.abs(a - b) < acceptableDelta
    }

    // /**
    //  * Creates a quaternion that represents a rotation around a specified axis by a given angle.
    //  *
    //  * @param angle - The angle of rotation in degrees.
    //  * @param axis - The axis around which to rotate, represented as a Vector3.
    //  * @returns A Quaternion representing the rotation.
    //  *
    //  * The function converts the angle from degrees to radians, calculates the sine and cosine
    //  * of half the angle, and then constructs a quaternion from these values. The provided
    //  * axis is normalized to ensure the quaternion represents a valid rotation.
    //  */
    // private static angleAxis(angle: number, axis: THREE.Vector3): THREE.Quaternion {
    //     const rad = (angle * Math.PI) / 180 // Convert angle to radians
    //     const halfAngle = rad / 2
    //     const s = Math.sin(halfAngle)
    //     const normalizedAxis = axis.normalize()
    //
    //     return new THREE.Quaternion(
    //         Math.cos(halfAngle),
    //         normalizedAxis.x * s,
    //         normalizedAxis.y * s,
    //         normalizedAxis.z * s
    //     )
    // }
    //
    // /**
    //  * Applies the rotation represented by the quaternion to the given vector.
    //  *
    //  * @param quat - The quaternion representing the rotation.
    //  * @param vec - The vector to be rotated.
    //  * @returns The rotated vector as a new Vector3.
    //  */
    // private static multiplyQuaternionByVector3(quat: THREE.Quaternion, vec: THREE.Vector3): THREE.Vector3 {
    //     const qx = quat.x
    //     const qy = quat.y
    //     const qz = quat.z
    //     const qw = quat.w
    //     const vx = vec.x
    //     const vy = vec.y
    //     const vz = vec.z
    //
    //     // Compute quaternion-vector multiplication
    //     const ix = qw * vx + qy * vz - qz * vy
    //     const iy = qw * vy + qz * vx - qx * vz
    //     const iz = qw * vz + qx * vy - qy * vx
    //     const iw = -qx * vx - qy * vy - qz * vz
    //
    //     // Compute the result vector
    //     return new THREE.Vector3(
    //         ix * qw + iw * -qx + iy * -qz - iz * -qy,
    //         iy * qw + iw * -qy + iz * -qx - ix * -qz,
    //         iz * qw + iw * -qz + ix * -qy - iy * -qx
    //     )
    // }

    // Sets the drivetrains target linear and rotational velocity
    private DriveSpeeds(forward: number, strafe: number, turn: number) {
        const rootNodeId = [...World.sceneRenderer.sceneObjects.entries()]
            .filter(x => {
                const y = x[1] instanceof MirabufSceneObject
                return y
            })
            .map(x => x[1] as MirabufSceneObject)
            .find(o => o.assemblyName == this._assemblyName)
            ?.getRootNodeId()

        if (rootNodeId == undefined) throw new Error("Robot root node should not be undefined")

        const robotRotation = convertJoltQuatToThreeQuaternion(World.physicsSystem.getBody(rootNodeId).GetRotation())
        // const robotTransform = new THREE.Matrix4()
        // robotTransform.makeRotationFromQuaternion(robotRotation)

        // const robotLocalToWorldMatrix = new THREE.Matrix4()

        const robotForward: THREE.Vector3 = new THREE.Vector3(0, 0, 1).applyQuaternion(robotRotation)
        const robotRight: THREE.Vector3 = new THREE.Vector3(1, 0, 0).applyQuaternion(robotRotation)
        this._debugVector(
            "forward",
            0xff0000,
            robotForward,
            World.physicsSystem.getBody(rootNodeId).GetCenterOfMassPosition()
        )
        this._debugVector(
            "right",
            0x00ff00,
            robotRight,
            World.physicsSystem.getBody(rootNodeId).GetCenterOfMassPosition()
        )
        const robotUp: THREE.Vector3 = new THREE.Vector3(0, 1, 0).applyQuaternion(robotRotation)
        this._debugVector("up", 0xffff00, robotUp, World.physicsSystem.getBody(rootNodeId).GetCenterOfMassPosition())
        if (InputSystem.getInput("swerveResetFieldForward", this._brainIndex)) this._fieldForward = robotForward

        const headingVector: THREE.Vector3 = robotForward
            .clone()
            .sub(new THREE.Vector3(0, 1, 0).multiplyScalar(new THREE.Vector3(0, 1, 0).dot(robotForward)))

        const headingVectorY: number = this._fieldForward.dot(headingVector)
        const headingVectorX: number = this._fieldForward.cross(new THREE.Vector3(0, 1, 0)).dot(headingVector)
        const chassisAngle: number = Math.atan2(headingVectorX, headingVectorY) * (180.0 / Math.PI)

        forward = SwerveDriveBehavior.withinTolerance(forward, 0.0, 0.1) ? 0.0 : forward
        strafe = SwerveDriveBehavior.withinTolerance(strafe, 0.0, 0.1) ? 0.0 : strafe
        turn = SwerveDriveBehavior.withinTolerance(turn, 0.0, 0.1) ? 0.0 : turn

        // Are the inputs basically zero
        if (forward == 0.0 && turn == 0.0 && strafe == 0.0) {
            this._wheels.forEach(w => {
                w.accelerationDirection = 0
                // w.getWheel().SetAngularVelocity(0)
            })
            this._debugVector(
                "linearVelocity",
                0x00ffff,
                new THREE.Vector3(),
                World.physicsSystem.getBody(rootNodeId).GetCenterOfMassPosition()
            )
            this._wheels.forEach((wheel, i) => {
                this._debugVector(
                    "wheel" + i,
                    0x0000ff,
                    new THREE.Vector3(),
                    wheel.constraint
                        .GetWheelWorldTransform(0, new JOLT.Vec3(1, 0, 0), new JOLT.Vec3(0, 1, 0))
                        .GetTranslation()
                )
            })
            return
        } else {
            console.debug("==================")
            console.debug(`Input: ${forward.toFixed(1)}, ${strafe.toFixed(1)}, ${turn.toFixed(1)}`)
        }

        console.debug(`Robot Rotation: ${threeQuaternionToString(robotRotation, 2)}`)
        console.debug(`Robot Forward: ${threeVector3ToString(robotForward)}`)
        console.debug(`Robot Right: ${threeVector3ToString(robotRight)}`)
        console.debug(`Robot Up: ${threeVector3ToString(robotUp)}`)

        // Adjusts how much turning verse translation is favored
        turn *= 1.5

        const chassisVelocity: THREE.Vector3 = robotForward
            .clone()
            .multiplyScalar(forward)
            .add(robotRight.clone().multiplyScalar(strafe))
        const chassisAngularVelocity: THREE.Vector3 = robotUp.clone().multiplyScalar(turn)

        console.debug(`Lin Vel: ${threeVector3ToString(chassisVelocity)}`)
        console.debug(`Ang Vel: ${threeVector3ToString(chassisAngularVelocity)}`)
        console.debug(`Chassis Angle: ${chassisAngle.toFixed(2)}`)

        this._debugVector(
            "linearVelocity",
            0x00ffff,
            chassisVelocity,
            World.physicsSystem.getBody(rootNodeId).GetCenterOfMassPosition()
        )

        // Normalize velocity so its between 1 and 0. Should only max out at like 1 sqrt(2), but still
        if (chassisVelocity.length() > 1) chassisVelocity.normalize()

        // Rotate chassis velocity by chassis angle
        // chassisVelocity = SwerveDriveBehavior.multiplyQuaternionByVector3(
        //     SwerveDriveBehavior.angleAxis(chassisAngle, robotUp),
        //     chassisVelocity
        // )

        // SwerveDriveBehavior.angleAxis(chassisAngle, robotUp).setFromAxisAngle

        let maxVelocity = new THREE.Vector3()
        const com = convertJoltVec3ToThreeVector3(World.physicsSystem.getBody(rootNodeId).GetCenterOfMassPosition())

        const velocities: THREE.Vector3[] = []
        for (let i = 0; i < this._hinges.length; i++) {
            // TODO: We should do this only once for all azimuth drivers, but whatever for now
            const driver = this._hinges[i]

            const radius = convertJoltVec3ToThreeVector3(driver.worldAnchor).sub(com)

            // const driverAxis = convertJoltVec3ToThreeVector3(driver.worldAxis)

            // Remove axis component of radius
            // radius.sub(driverAxis.multiplyScalar(driverAxis.dot(radius)))

            velocities[i] = chassisAngularVelocity.clone().cross(radius).add(chassisVelocity)
            if (velocities[i].length() > maxVelocity.length()) maxVelocity = velocities[i]
        }

        // Normalize all if a velocity exceeds 1
        const maxVelocityLength = maxVelocity.length()
        if (maxVelocityLength > 1) {
            for (let i = 0; i < this._wheels.length; i++) {
                velocities[i].divideScalar(maxVelocityLength)
            }
        }

        // console.log(maxVelocity.length)

        // console.log("set speeds to " + this._hinges.length + " wheels")

        for (let i = 0; i < this._wheels.length; i++) {
            console.debug(`Velocity [${i}]: ${threeVector3ToString(velocities[i])}`)

            const speed: number = velocities[i].length()
            const yComponent: number = robotForward.dot(velocities[i])
            const xComponent: number = robotRight.dot(velocities[i])
            const angle: number = Math.atan2(xComponent, yComponent)

            console.debug(`Speed [${i}]: ${speed} (${xComponent.toFixed(3)}, ${yComponent.toFixed(3)})`)
            console.debug(`Angle [${i}]: ${angle.toFixed(3)}`)
            console.debug(`Forward [${i}]: ${joltVec3ToString(this._wheels[i].getWheel().GetSettings().mWheelForward)}`)

            //console.log(angle)
            const joltWheel = this._wheels[i].getWheel()
            this._hinges[i].targetAngle = angle
            if (SwerveDriveBehavior.withinTolerance(this._hinges[i].targetAngle, angle, 0.05)) {
                this._wheels[i].setFriction(0)
            } else {
                this._wheels[i].setFriction(1)
            }
            joltWheel.SetSteerAngle(angle)

            // convertThreeVector3ToJoltVec3(velocities[i].clone().normalize())
            const wheelVector = robotForward
                .clone()
                .applyAxisAngle(
                    convertJoltVec3ToThreeVector3(joltWheel.GetSettings().mWheelUp),
                    joltWheel.GetSteerAngle()
                )
            this._debugVector(
                "wheel" + i,
                joltWheel.get_mCombinedLateralFriction() > 0 ? 0x0000ff : 0x00ff00,
                wheelVector,
                this._wheels[i].constraint
                    .GetWheelWorldTransform(0, new JOLT.Vec3(1, 0, 0), new JOLT.Vec3(0, 1, 0))
                    .GetTranslation()
            )
            this._wheels[i].accelerationDirection = speed
        }
    }

    public update(_: number): void {
        const forwardInput = InputSystem.getInput("swerveForward", this._brainIndex)
        const strafeInput = InputSystem.getInput("swerveStrafe", this._brainIndex)
        const turnInput = InputSystem.getInput("swerveYaw", this._brainIndex)

        this.DriveSpeeds(
            forwardInput * this._forwardSpeed,
            strafeInput * this._strafeSpeed,
            turnInput * this._turnSpeed
        )
    }

    private _lines: Record<string, Mesh[]> = {}

    private _debugVector(id: string, color: THREE.ColorRepresentation, vec: THREE.Vector3, origin: Jolt.RVec3): void {
        const base = convertJoltVec3ToThreeVector3(origin)
        if (this._lines[id] == undefined) {
            const material = new THREE.MeshBasicMaterial({
                color: color,
                transparent: true,
                opacity: 0.1,
                wireframe: true,
            })
            material.depthTest = false
            this._lines[id] = []
            for (let i = 0; i < 10; i++) {
                const line = new THREE.Mesh(new THREE.SphereGeometry(0.005), material)
                World.sceneRenderer.scene.add(line)
                this._lines[id].push(line)
            }
        }
        const vec2 = vec.clone().setLength(0.01)
        let outVector = new THREE.Vector3().copy(base)
        for (let i = 0; i < 10; i++) {
            outVector = outVector.add(vec2)
            this._lines[id][i].position.copy(outVector)
        }
    }
}

export default SwerveDriveBehavior
