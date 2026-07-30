import * as THREE from "three"
import type WheelDriver from "@/systems/simulation/driver/WheelDriver.ts"
import { mecanumSuspensionTravel } from "@/systems/simulation/driver/WheelDriver.ts"
import JOLT from "@/util/loading/JoltSyncLoader"
import type { MecanumModule } from "./MecanumDriveBehavior.ts"
import { ROLLER_ANGLE } from "./MecanumDriveBehavior.ts"

/**
 * Wheels closer together than this along the chassis' forward axis count as the same row.
 *
 * Rows are what the roller handedness alternates over, so this only has to separate genuine axles.
 * Generous enough to absorb CAD asymmetry between the two ends of one axle, tight enough that the
 * middle wheels of a six-wheel chassis stay their own row.
 */
const ROW_TOLERANCE = 0.05

/**
 * The robot-local axes the module frame is expressed in.
 *
 * Which local axis is the nose depends on where the robot came from (see {@link resolveMecanumLayout}),
 * so anything that has to resolve chassis motion or heading against the frame the wheels were mixed
 * in — {@link MecanumDriveBehavior}'s correction loops and its field-oriented rotation — needs these
 * rather than a hardcoded +Z. Both are unit vectors, and `localNose x localLeft` is +Y up.
 */
export interface MecanumFrame {
    localNose: THREE.Vector3
    localLeft: THREE.Vector3
}

export interface MecanumLayout {
    modules: MecanumModule[]
    useLateralZ: boolean
    imbalanceX: number
    imbalanceZ: number
    frame: MecanumFrame
}

/**
 * Assigns each wheel its mecanum geometry from wheel positions alone.
 *
 * There is no detection step and no dependency on wheel geometry from CAD: mecanum is a mixing
 * scheme over whatever wheels the robot has. Which robot-local axis is lateral is decided with the
 * same X-vs-Z balance heuristic {@link SynthesisBrain.createSkidSteerDriveBehavior} uses, because
 * Fusion 360 and URDF imports disagree about it.
 *
 * ## Roller handedness
 *
 * A mecanum wheel's rollers run at 45 degrees to the wheel plane, and the handedness has to
 * alternate — mirrored left-to-right, and again front-to-back — or the wheels' push directions
 * don't span enough of the plane to produce lateral force. So each side's wheels are ordered
 * front-to-rear and the handedness flips at every row. For the usual four-wheel robot that is the
 * standard `FL/RR` versus `FR/RL` X-pattern; for six wheels it extends to the alternating pattern a
 * real six-wheel mecanum chassis uses, with the middle row opposite its neighbours.
 *
 * Every wheel drives. An earlier version drove only the four extreme corners and zeroed the tire
 * friction on the rest, which breaks badly on a drop-centre chassis: there the middle wheels are
 * the *lowest* ones, so the corners never reach the ground and the only tires with load are the
 * ones that just had their grip removed.
 *
 * @param wheelDrivers Every wheel on the robot.
 * @param chassisRotation The chassis body's world rotation, used to resolve its local axes.
 */
export function resolveMecanumLayout(wheelDrivers: WheelDriver[], chassisRotation: THREE.Quaternion): MecanumLayout {
    // Synthesis' robot-local frame is right-handed with +Y up, so +Z is the nose and +X is left.
    const localLeft = new THREE.Vector3(1, 0, 0).applyQuaternion(chassisRotation)
    const localNose = new THREE.Vector3(0, 0, 1).applyQuaternion(chassisRotation)

    const wheelPositions = wheelDrivers.map(w => {
        const forward = new JOLT.Vec3(1, 0, 0)
        const up = new JOLT.Vec3(0, 1, 0)
        // GetWheelWorldTransform and GetTranslation hand back reused static temporaries;
        // only the two arguments are ours to free.
        const translation = w.constraint.GetWheelWorldTransform(0, forward, up).GetTranslation()
        const pos = new THREE.Vector3(translation.GetX(), translation.GetY(), translation.GetZ())
        JOLT.destroy(forward)
        JOLT.destroy(up)
        return pos
    })

    const centroid = wheelPositions
        .reduce((sum, p) => sum.add(p), new THREE.Vector3())
        .divideScalar(wheelPositions.length)
    const offsets = wheelPositions.map(p => p.clone().sub(centroid))
    const alongX = offsets.map(o => o.dot(localLeft))
    const alongZ = offsets.map(o => o.dot(localNose))

    const imbalance = (values: number[]) =>
        Math.abs(values.filter(v => v >= 0).length - values.filter(v => v < 0).length)
    const imbalanceX = imbalance(alongX)
    const imbalanceZ = imbalance(alongZ)

    // Use Z as the lateral axis when it gives the more balanced split (URDF robots); fall back
    // to X (Fusion 360 robots). URDF's usual +Y-left convention converts to -Z-left here, and
    // the remaining horizontal axis is the nose direction for that same handedness.
    const useLateralZ = imbalanceZ < imbalanceX
    const leftOffsets = useLateralZ ? alongZ.map(v => -v) : alongX
    const forwardOffsets = useLateralZ ? alongX : alongZ

    // Same choice as a pair of local axes, for everything downstream that has to know which way the
    // robot is actually facing. Both orderings are right-handed about +Y up, so a counter-clockwise
    // yaw means the same thing either way.
    const frame: MecanumFrame = useLateralZ
        ? { localNose: new THREE.Vector3(1, 0, 0), localLeft: new THREE.Vector3(0, 0, -1) }
        : { localNose: new THREE.Vector3(0, 0, 1), localLeft: new THREE.Vector3(1, 0, 0) }

    const rows = assignRows(forwardOffsets)

    const modules: MecanumModule[] = wheelDrivers.map((wheel, i) => {
        const leftSign = leftOffsets[i] >= 0 ? 1 : -1
        // Flip with the row so front and rear rollers mirror, and again with the side so the two
        // halves mirror. The leading negation puts the front-left wheel's rollers at -45 degrees,
        // pushing it forward-and-right, which is the standard build.
        const steerAngle = -(rows[i] % 2 === 0 ? 1 : -1) * leftSign * ROLLER_ANGLE
        return {
            wheel,
            x: forwardOffsets[i],
            y: leftOffsets[i],
            pushX: Math.cos(steerAngle),
            pushY: Math.sin(steerAngle),
            steerAngle,
            maxSurfaceSpeed: wheel.debugState().radius * wheel.maxVelocity,
            row: rows[i],
            leftSign,
        }
    })

    return { modules, useLateralZ, imbalanceX, imbalanceZ, frame }
}

/**
 * Groups wheels into axles by forward offset, numbering them 0 at the nose.
 *
 * @returns Row index per wheel, index-aligned with the offsets.
 */
function assignRows(forwardOffsets: number[]): number[] {
    const order = forwardOffsets.map((offset, i) => ({ offset, i })).sort((a, b) => b.offset - a.offset)

    const rows = new Array<number>(forwardOffsets.length)
    let row = 0
    order.forEach((entry, k) => {
        if (k > 0 && order[k - 1].offset - entry.offset > ROW_TOLERANCE) row++
        rows[entry.i] = row
    })

    return rows
}

/**
 * Points every tire along its roller axle.
 *
 * A `WheelWV` pushes along its own heading and its lateral friction resists sideways motion, which
 * is the exact opposite of a mecanum wheel. Without this the mix produces no lateral motion at all.
 */
export function applyMecanumTires(layout: MecanumLayout): void {
    const travel = mecanumSuspensionTravel(layout.modules.map(m => m.wheel.debugState().radius))
    layout.modules.forEach(m => m.wheel.configureMecanumRoller(m.steerAngle, travel))
}
