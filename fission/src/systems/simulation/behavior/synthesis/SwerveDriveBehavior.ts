import WheelDriver from "@/systems/simulation/driver/WheelDriver"
import WheelRotationStimulus from "@/systems/simulation/stimulus/WheelStimulus"
import Behavior from "@/systems/simulation/behavior/Behavior"
import InputSystem from "@/systems/input/InputSystem"
import HingeDriver from "../../driver/HingeDriver"
import Driver, { DriverControlMode } from "../../driver/Driver"
import HingeStimulus from "../../stimulus/HingeStimulus"
import Stimulus from "../../stimulus/Stimulus"
import { Matrix4, Quaternion, Vector3 } from "three"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import { JoltVec3_ThreeVector3 } from "@/util/TypeConversions"

class SwerveDriveBehavior extends Behavior {
    private _wheels: WheelDriver[]
    private _hinges: HingeDriver[]
    private _brainIndex: number
    private _assemblyName: string

    private _forwardSpeed = 30
    private _strafeSpeed = 30
    private _turnSpeed = 30

    private _fieldForward: Vector3 = new Vector3(1, 0, 0)

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
            h.constraint.SetLimits(-Infinity, Infinity)
            h.controlMode = DriverControlMode.Velocity
        })
    }

    /** @returns true if the difference between a and b is within acceptanceDelta */
    private static withinTolerance(a: number, b: number, acceptableDelta: number) {
        return Math.abs(a - b) < acceptableDelta
    }

    /**
     * Creates a quaternion that represents a rotation around a specified axis by a given angle.
     *
     * @param angle - The angle of rotation in degrees.
     * @param axis - The axis around which to rotate, represented as a Vector3.
     * @returns A Quaternion representing the rotation.
     *
     * The function converts the angle from degrees to radians, calculates the sine and cosine
     * of half the angle, and then constructs a quaternion from these values. The provided
     * axis is normalized to ensure the quaternion represents a valid rotation.
     */
    private static angleAxis(angle: number, axis: Vector3): Quaternion {
        const rad = (angle * Math.PI) / 180 // Convert angle to radians
        const halfAngle = rad / 2
        const s = Math.sin(halfAngle)
        const normalizedAxis = axis.normalize()

        return new Quaternion(Math.cos(halfAngle), normalizedAxis.x * s, normalizedAxis.y * s, normalizedAxis.z * s)
    }

    /**
     * Applies the rotation represented by the quaternion to the given vector.
     *
     * @param quat - The quaternion representing the rotation.
     * @param vec - The vector to be rotated.
     * @returns The rotated vector as a new Vector3.
     */
    private static multiplyQuaternionByVector3(quat: Quaternion, vec: Vector3): Vector3 {
        const qx = quat.x
        const qy = quat.y
        const qz = quat.z
        const qw = quat.w
        const vx = vec.x
        const vy = vec.y
        const vz = vec.z

        // Compute quaternion-vector multiplication
        const ix = qw * vx + qy * vz - qz * vy
        const iy = qw * vy + qz * vx - qx * vz
        const iz = qw * vz + qx * vy - qy * vx
        const iw = -qx * vx - qy * vy - qz * vz

        // Compute the result vector
        return new Vector3(
            ix * qw + iw * -qx + iy * -qz - iz * -qy,
            iy * qw + iw * -qy + iz * -qx - ix * -qz,
            iz * qw + iw * -qz + ix * -qy - iy * -qx
        )
    }

    // Sets the drivetrains target linear and rotational velocity
    private DriveSpeeds(forward: number, strafe: number, turn: number) {
        const rootNodeId = [...World.SceneRenderer.sceneObjects.entries()]
            .filter(x => {
                const y = x[1] instanceof MirabufSceneObject
                return y
            })
            .map(x => x[1] as MirabufSceneObject)
            .find(o => o.assemblyName == this._assemblyName)
            ?.GetRootNodeId()

        if (rootNodeId == undefined) throw new Error("Robot root node should not be undefined")

        const robotTransform = JoltVec3_ThreeVector3(World.PhysicsSystem.GetBody(rootNodeId).GetPosition())

        const robotLocalToWorldMatrix = new Matrix4()

        const robotForward: Vector3 = robotTransform.normalize()
        const robotRight: Vector3 = robotTransform.cross(new Vector3(0, 1, 0)).normalize()
        const robotUp: Vector3 = robotRight.cross(robotForward).normalize()

        if (InputSystem.getInput("resetFieldForward", this._brainIndex)) this._fieldForward = robotForward

        const headingVector: Vector3 = robotForward.min(
            new Vector3(0, 1, 0).multiplyScalar(new Vector3(0, 1, 0).dot(robotForward))
        )

        const headingVectorY: number = this._fieldForward.dot(headingVector)
        const headingVectorX: number = this._fieldForward.cross(new Vector3(0, 1, 0)).dot(headingVector)
        const chassisAngle: number = Math.atan2(headingVectorX, headingVectorY) * (180.0 / Math.PI)

        forward = SwerveDriveBehavior.withinTolerance(forward, 0.0, 0.1) ? 0.0 : forward
        strafe = SwerveDriveBehavior.withinTolerance(strafe, 0.0, 0.1) ? 0.0 : strafe
        turn = SwerveDriveBehavior.withinTolerance(turn, 0.0, 0.1) ? 0.0 : turn

        // Are the inputs basically zero
        if (forward == 0.0 && turn == 0.0 && strafe == 0.0) {
            this._wheels.forEach(w => (w.accelerationDirection = 0.0))
            return
        }

        // Adjusts how much turning verse translation is favored
        turn *= 1.5

        let chassisVelocity: Vector3 = robotForward.multiplyScalar(forward).add(robotRight).multiplyScalar(strafe)
        const chassisAngularVelocity: Vector3 = robotUp.multiplyScalar(turn)

        // Normalize velocity so its between 1 and 0. Should only max out at like 1 sqrt(2), but still
        if (chassisVelocity.length() > 1) chassisVelocity = chassisVelocity.normalize()

        // Rotate chassis velocity by chassis angle
        chassisVelocity = SwerveDriveBehavior.multiplyQuaternionByVector3(
            SwerveDriveBehavior.angleAxis(chassisAngle, robotUp),
            chassisVelocity
        )

        SwerveDriveBehavior.angleAxis(chassisAngle, robotUp).setFromAxisAngle

        var maxVelocity = new Vector3()

        const velocities: Vector3[] = []
        for (let i = 0; i < this._hinges.length; i++) {
            // TODO: We should do this only once for all azimuth drivers, but whatever for now
            const driver = this._hinges[i]

            // TODO: center of mass of rigidbody calculation
            const com = robotTransform /* .GetComponent<Rigidbody>().centerOfMass) */
                .applyMatrix4(robotLocalToWorldMatrix)

            // TODO: driver anchor
            const driverAnchor = new Vector3()

            let radius = driverAnchor.min(com)

            // TODO: get axis from driver
            const driverAxis = new Vector3()

            // Remove axis component of radius
            radius = radius.min(driverAxis.multiplyScalar(driverAxis.dot(radius)))

            velocities[i] = chassisAngularVelocity.cross(radius).add(chassisVelocity)
            if (velocities[i].length() > maxVelocity.length()) maxVelocity = velocities[i]
        }

        // Normalize all if a velocity exceeds 1
        if (maxVelocity.length() > 1) {
            for (let i = 0; i < this._wheels.length; i++) {
                velocities[i] = velocities[i].divideScalar(maxVelocity.length())
            }
        }

        console.log(maxVelocity.length)

        console.log("set speeds to " + this._hinges.length + " wheels")

        for (let i = 0; i < this._wheels.length; i++) {
            const speed: number = velocities[i].length()
            const yComponent: number = robotForward.dot(velocities[i])
            const xComponent: number = robotRight.dot(velocities[i])
            const angle: number = Math.atan2(xComponent, yComponent) * (180.0 / Math.PI)

            //console.log(angle)
            this._hinges[i].targetAngle = angle
            this._wheels[i].accelerationDirection = speed
        }
    }

    public Update(_: number): void {
        const forwardInput = InputSystem.getInput("swerveForward", this._brainIndex)
        const strafeInput = InputSystem.getInput("swerveStrafe", this._brainIndex)
        const turnInput = InputSystem.getInput("swerveTurn", this._brainIndex)

        this.DriveSpeeds(
            forwardInput * this._forwardSpeed,
            strafeInput * this._strafeSpeed,
            turnInput * this._turnSpeed
        )
    }
}

export default SwerveDriveBehavior
