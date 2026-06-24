import type Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import type { Mesh } from "three"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"
import InputSystem from "@/systems/input/InputSystem.ts"
import { DriveBehavior } from "@/systems/simulation/behavior/synthesis/drive/DriveBehavior.ts"
import { shortestAngleDelta } from "@/systems/simulation/driver/AngleUtil.ts"
import type WheelDriver from "@/systems/simulation/driver/WheelDriver.ts"
import type WheelRotationStimulus from "@/systems/simulation/stimulus/WheelStimulus.ts"
import World from "@/systems/World.ts"
import { threeQuaternionToString, threeVector3ToString } from "@/util/debug/DebugPrint.ts"
import JOLT from "@/util/loading/JoltSyncLoader.ts"
import { convertJoltQuatToThreeQuaternion, convertJoltVec3ToThreeVector3 } from "@/util/TypeConversions.ts"
import type Driver from "../../../driver/Driver.ts"
import { DriverControlMode } from "../../../driver/Driver.ts"
import type HingeDriver from "../../../driver/HingeDriver.ts"
import type HingeStimulus from "../../../stimulus/HingeStimulus.ts"
import type Stimulus from "../../../stimulus/Stimulus.ts"

class SwerveDriveBehavior extends DriveBehavior {
    private _wheels: WheelDriver[]
    private _hinges: HingeDriver[]
    private _brainIndex: number
    private _assemblyName: string

    private _forwardSpeed = 30
    private _strafeSpeed = 30
    private _turnSpeed = 30

    private _fieldForward: THREE.Vector3 = new THREE.Vector3(1, 0, 0)

    // Throttle for the always-on diagnostic logs (~ every half second at 60 fps). _verbose is
    // recomputed each update() and read throughout driveSpeeds so a single frame logs as a unit.
    private static readonly LOG_EVERY = 30
    private _logTick = 0
    private _verbose = false

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
            // Swerve steering modules rotate continuously (coaxial), so remove the ±π hinge limit;
            // otherwise a module gets stuck taking the long way around when the shortest steering
            // path crosses the ±π seam.
            h.setContinuousRotation()
        })

        // Field-oriented drive is zeroed to the robot's heading at the moment swerve is
        // configured (spawn, or when the drivetrain is switched to swerve). This makes "forward"
        // line up with the robot's nose initially; R re-zeroes it later. Without this the field
        // frame would default to world +X and the controls would feel decoupled from the robot.
        const rootNodeId = this.resolveRootNodeId()
        if (rootNodeId != undefined) {
            const rotation = convertJoltQuatToThreeQuaternion(World.physicsSystem.getBody(rootNodeId).GetRotation())
            this._fieldForward = new THREE.Vector3(0, 0, 1).applyQuaternion(rotation)
        } else {
            console.warn(
                "[Swerve][init] could not resolve root node at construction; field-forward defaults to world +X"
            )
        }

        console.debug(
            `[Swerve][init] brain=${this._brainIndex} ` +
                `scheme=${InputSystem.brainIndexSchemeMap.get(this._brainIndex)?.schemeName ?? "NONE"} ` +
                `wheels=${this._wheels.length} hinges=${this._hinges.length} ` +
                `wheelMaxVel=${this._wheels.map(w => w.maxVelocity).join(",")} ` +
                `fieldForward=${threeVector3ToString(this._fieldForward)}`
        )

        // Per-wheel mounting parameters. If steerAxis / wheelForward are NOT uniform across the
        // four modules, then commanding the same steer angle produces different world rolling
        // directions → driving "straight" makes the robot spin. This is the key thing to compare.
        const v = (j: Jolt.Vec3) => `(${j.GetX().toFixed(2)},${j.GetY().toFixed(2)},${j.GetZ().toFixed(2)})`
        this._wheels.forEach((w, i) => {
            const s = w.getWheel().GetSettings()
            console.debug(
                `[Swerve][mount ${i}] pos=${v(s.mPosition)} steerAxis=${v(s.mSteeringAxis)} ` +
                    `wheelFwd=${v(s.mWheelForward)} wheelUp=${v(s.mWheelUp)} reversed=${w.reversed}`
            )
        })
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
        this.debugVector(
            "forward",
            0xff0000,
            robotForward,
            World.physicsSystem.getBody(rootNodeId).GetCenterOfMassPosition()
        )
        this.debugVector(
            "right",
            0x00ff00,
            robotRight,
            World.physicsSystem.getBody(rootNodeId).GetCenterOfMassPosition()
        )
        const robotUp: THREE.Vector3 = new THREE.Vector3(0, 1, 0).applyQuaternion(robotRotation)
        this.debugVector("up", 0xffff00, robotUp, World.physicsSystem.getBody(rootNodeId).GetCenterOfMassPosition())
        if (InputSystem.getInput("swerveResetFieldForward", this._brainIndex)) this._fieldForward = robotForward

        const headingVector: THREE.Vector3 = robotForward
            .clone()
            .sub(new THREE.Vector3(0, 1, 0).multiplyScalar(new THREE.Vector3(0, 1, 0).dot(robotForward)))

        const headingVectorY: number = this._fieldForward.dot(headingVector)
        // BUGFIX: THREE.Vector3.cross() mutates the receiver. The original code called
        // `this._fieldForward.cross(...)` which corrupted _fieldForward every frame. Clone first.
        const headingVectorX: number = this._fieldForward
            .clone()
            .cross(new THREE.Vector3(0, 1, 0))
            .dot(headingVector)
        const chassisAngleRad: number = Math.atan2(headingVectorX, headingVectorY)
        const chassisAngle: number = chassisAngleRad * (180.0 / Math.PI)

        forward = SwerveDriveBehavior.withinTolerance(forward, 0.0, 0.1) ? 0.0 : forward
        strafe = SwerveDriveBehavior.withinTolerance(strafe, 0.0, 0.1) ? 0.0 : strafe
        turn = SwerveDriveBehavior.withinTolerance(turn, 0.0, 0.1) ? 0.0 : turn

        // Are the inputs basically zero
        if (forward == 0.0 && turn == 0.0 && strafe == 0.0) {
            this._wheels.forEach(w => {
                w.accelerationDirection = 0
            })
            this.debugVector(
                "linearVelocity",
                0x00ffff,
                new THREE.Vector3(),
                World.physicsSystem.getBody(rootNodeId).GetCenterOfMassPosition()
            )
            this._wheels.forEach((wheel, i) => {
                this.debugVector(
                    "wheel" + i,
                    0x0000ff,
                    new THREE.Vector3(),
                    wheel.constraint
                        .GetWheelWorldTransform(0, new JOLT.Vec3(1, 0, 0), new JOLT.Vec3(0, 1, 0))
                        .GetTranslation()
                )
            })
            World.physicsSystem.enablePhysicsForBody(rootNodeId)
            if (this._verbose) {
                console.debug(
                    `[Swerve][drive] IDLE — scaled inputs ~0 after deadband ` +
                        `(fwd=${forward.toFixed(2)} str=${strafe.toFixed(2)} turn=${turn.toFixed(2)}); wheels zeroed`
                )
            }
            return
        } else if (this._verbose) {
            console.debug("==================")
            console.debug(
                `[Swerve][drive] ACTIVE scaled inputs: ${forward.toFixed(1)}, ${strafe.toFixed(1)}, ${turn.toFixed(1)}`
            )
            console.debug(`Robot Rotation: ${threeQuaternionToString(robotRotation, 2)}`)
            console.debug(`Robot Forward: ${threeVector3ToString(robotForward)}`)
            console.debug(`Robot Right: ${threeVector3ToString(robotRight)}`)
            console.debug(`Robot Up: ${threeVector3ToString(robotUp)}`)

            // ACTUAL robot motion straight from the physics body. If linear velocity is ~0 but
            // angular velocity is large while every module is commanded straight, the physics is
            // spinning the robot despite a translate command — the actuation, not the math.
            const lv = World.physicsSystem.getBody(rootNodeId).GetLinearVelocity()
            const av = World.physicsSystem.getBody(rootNodeId).GetAngularVelocity()
            console.debug(
                `[Swerve][actual] bodyLinVel=(${lv.GetX().toFixed(2)},${lv.GetY().toFixed(2)},${lv.GetZ().toFixed(2)}) ` +
                    `bodyAngVel=(${av.GetX().toFixed(2)},${av.GetY().toFixed(2)},${av.GetZ().toFixed(2)})`
            )
        }

        // Adjusts how much turning verse translation is favored
        turn *= 1.5

        const chassisVelocity: THREE.Vector3 = robotForward
            .clone()
            .multiplyScalar(forward)
            .add(robotRight.clone().multiplyScalar(strafe))
        const chassisAngularVelocity: THREE.Vector3 = robotUp.clone().multiplyScalar(turn)

        if (this._verbose) {
            console.debug(`Lin Vel: ${threeVector3ToString(chassisVelocity)}`)
            console.debug(`Ang Vel: ${threeVector3ToString(chassisAngularVelocity)}`)
            console.debug(`Chassis Angle: ${chassisAngle.toFixed(2)}`)
        }

        this.debugVector(
            "linearVelocity",
            0x00ffff,
            chassisVelocity,
            World.physicsSystem.getBody(rootNodeId).GetCenterOfMassPosition()
        )

        // Normalize velocity so its between 1 and 0. Should only max out at like 1 sqrt(2), but still
        if (chassisVelocity.length() > 1) chassisVelocity.normalize()

        // Field-oriented drive: rotate the commanded chassis velocity into the field frame by the
        // chassis heading. Ported from the original `Quaternion.AngleAxis(chassisAngle, up) * v`,
        // which the prior port computed but never applied (field-oriented drive was dead).
        const chassisVelocityRobotFrame = chassisVelocity.clone()
        chassisVelocity.applyAxisAngle(robotUp, chassisAngleRad)
        if (this._verbose) {
            console.debug(
                `[Swerve][drive] chassisAngle=${chassisAngle.toFixed(2)}° ` +
                    `vRobotFrame=${threeVector3ToString(chassisVelocityRobotFrame)} ` +
                    `vFieldFrame=${threeVector3ToString(chassisVelocity)}`
            )
        }

        let maxVelocity = new THREE.Vector3()
        const com = convertJoltVec3ToThreeVector3(World.physicsSystem.getBody(rootNodeId).GetCenterOfMassPosition())

        const velocities: THREE.Vector3[] = []
        for (let i = 0; i < this._hinges.length; i++) {
            // TODO: We should do this only once for all azimuth drivers, but whatever for now
            const driver = this._hinges[i]

            const axis = convertJoltVec3ToThreeVector3(driver.worldAxis).normalize()
            const radius = convertJoltVec3ToThreeVector3(driver.worldAnchor).sub(com)
            // Remove the component of the radius along the hinge axis so the moment arm is purely
            // in the steering plane (matches original `radius -= dot(axis, radius) * axis`).
            radius.sub(axis.clone().multiplyScalar(axis.dot(radius)))

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

        for (let i = 0; i < this._wheels.length; i++) {
            const speed: number = velocities[i].length()
            const yComponent: number = robotForward.dot(velocities[i])
            const xComponent: number = robotRight.dot(velocities[i])

            const joltWheel = this._wheels[i].getWheel()
            const currentAngle = this._hinges[i].constraint.GetCurrentAngle()

            // Steering angle optimization: never rotate a module more than 90°. If the desired
            // heading is more than a quarter-turn from where the module currently points, steer to
            // the opposite heading (±180°) and drive the wheel in reverse instead. For a plain
            // direction reversal this means the module barely rotates (it just reverses roll),
            // avoiding the large synchronized pivot whose reaction torque jerks the chassis.
            let angle: number = Math.atan2(xComponent, yComponent)
            let driveSpeed: number = speed
            let delta: number = angle - currentAngle
            while (delta > Math.PI) delta -= 2 * Math.PI
            while (delta < -Math.PI) delta += 2 * Math.PI
            if (Math.abs(delta) > Math.PI / 2) {
                angle += angle > 0 ? -Math.PI : Math.PI
                driveSpeed = -speed
            }

            this._hinges[i].targetAngle = angle

            // NOTE: friction is intentionally left ON at all times (matching the original Unity
            // swerve, which never toggled it). The previous code disabled both lateral and
            // longitudinal friction whenever a module was mid-rotation. That was proven to cause an
            // uncontrollable spin: reversing/strafing requires the modules to pivot, during which
            // the azimuth motors' reaction torque spun the chassis with no ground friction to
            // resist it; field-oriented tracking then kept the targets moving so the modules never
            // realigned and friction never re-engaged — a runaway. Logs showed wheels rolling on
            // the ground (contact=true, wheelAngVel=cmdVel) yet bodyLinVel≈0, bodyAngVel large.
            const aligned = Math.abs(shortestAngleDelta(currentAngle, this._hinges[i].targetAngle)) < 0.05

            this._wheels[i].setSteeringAngle(angle)

            // One consolidated line per module so behaviour can be confirmed at a glance:
            // commanded vs. actual steering angle, hinge travel limits (a locked/clamped azimuth
            // can never reach target → never grips → robot can't move), grip state, the steer
            // angle the vehicle wheel actually holds, and the resulting commanded wheel velocity.
            if (this._verbose) {
                const clampedTarget = this._hinges[i].targetAngle
                const limMin = this._hinges[i].constraint.GetLimitsMin()
                const limMax = this._hinges[i].constraint.GetLimitsMax()
                const clamped = SwerveDriveBehavior.withinTolerance(clampedTarget, angle, 1e-4) ? "" : " CLAMPED!"
                // Actual world-space orientation of the wheel (includes its mounting + steer). The
                // transform's X/Z axes let us check whether the four wheels actually point the same
                // way in the world when commanded the same angle.
                const wt = this._wheels[i].constraint.GetWheelWorldTransform(
                    0,
                    new JOLT.Vec3(1, 0, 0),
                    new JOLT.Vec3(0, 1, 0)
                )
                const wx = wt.GetAxisX()
                const wz = wt.GetAxisZ()
                // Whether the drive force actually reaches the ground: is the wheel touching the
                // floor, is it physically rolling at the commanded rate, and is the suspension sane.
                // Parallel gripping wheels that spin the body imply the force is NOT translating it.
                const contact = joltWheel.HasContact()
                const wheelAngVel = joltWheel.GetAngularVelocity()
                const suspLen = joltWheel.GetSuspensionLength()
                console.debug(
                    `[Swerve][module ${i}] desiredAngle=${angle.toFixed(3)} target=${clampedTarget.toFixed(3)}${clamped} ` +
                        `current=${currentAngle.toFixed(3)} limits=[${limMin.toFixed(2)},${limMax.toFixed(2)}] ` +
                        `${aligned ? "ALIGNED" : "rotating"} ` +
                        `steerHeld=${joltWheel.GetSteerAngle().toFixed(3)} ` +
                        `driveSpeed=${driveSpeed.toFixed(3)} cmdVel=${(driveSpeed * this._wheels[i].maxVelocity).toFixed(2)} ` +
                        `contact=${contact} wheelAngVel=${wheelAngVel.toFixed(2)} suspLen=${suspLen.toFixed(3)} ` +
                        `worldX=(${wx.GetX().toFixed(2)},${wx.GetY().toFixed(2)},${wx.GetZ().toFixed(2)}) ` +
                        `worldZ=(${wz.GetX().toFixed(2)},${wz.GetY().toFixed(2)},${wz.GetZ().toFixed(2)})`
                )
            }

            const wheelVector = robotForward
                .clone()
                .applyAxisAngle(
                    convertJoltVec3ToThreeVector3(joltWheel.GetSettings().mWheelUp),
                    joltWheel.GetSteerAngle()
                )
            this.debugVector(
                "wheel" + i,
                joltWheel.get_mCombinedLateralFriction() > 0 ? 0x0000ff : 0x00ff00,
                wheelVector,
                this._wheels[i].constraint
                    .GetWheelWorldTransform(0, new JOLT.Vec3(1, 0, 0), new JOLT.Vec3(0, 1, 0))
                    .GetTranslation()
            )
            this._wheels[i].accelerationDirection = driveSpeed
        }
    }

    public update(_: number): void {
        // Recompute the throttle gate once per frame so update() and driveSpeeds() log together.
        this._verbose = this._logTick % SwerveDriveBehavior.LOG_EVERY === 0
        this._logTick++

        const forwardInput = InputSystem.getInput("swerveForward", this._brainIndex)
        const strafeInput = InputSystem.getInput("swerveStrafe", this._brainIndex)
        // BUGFIX: the original read "swerveYaw", which no input scheme ever defined (schemes
        // bind "swerveTurn"), so turning was always dead. Read the bound input name.
        const turnInput = InputSystem.getInput("swerveTurn", this._brainIndex)
        const resetInput = InputSystem.getInput("swerveResetFieldForward", this._brainIndex)

        // Always-on (throttled): confirms update() is actually being called and shows the raw
        // axis values the input system resolves for this brain. If these stay 0 while keys are
        // pressed, the problem is input binding/scheme, not the drive math.
        if (this._verbose) {
            console.debug(
                `[Swerve][update] tick=${this._logTick} brain=${this._brainIndex} ` +
                    `raw forward=${forwardInput.toFixed(3)} strafe=${strafeInput.toFixed(3)} ` +
                    `turn=${turnInput.toFixed(3)} reset=${resetInput.toFixed(3)}`
            )
        }

        this.driveSpeeds(
            forwardInput * this._forwardSpeed,
            strafeInput * this._strafeSpeed,
            turnInput * this._turnSpeed
        )
    }

    private _lines: Record<string, Mesh[]> = {}

    private debugVector(id: string, color: THREE.ColorRepresentation, vec: THREE.Vector3, origin: Jolt.RVec3): void {
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
            ;(this._lines[id][i].material as THREE.MeshBasicMaterial).color.set(color)
        }
    }
}

export default SwerveDriveBehavior
