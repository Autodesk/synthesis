import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { SequentialBehaviorPreferences } from "@/systems/preferences/PreferenceTypes"
import { DriveType } from "@/systems/simulation/behavior/Behavior"
import type Driver from "@/systems/simulation/driver/Driver"
import HingeDriver from "@/systems/simulation/driver/HingeDriver"
import SliderDriver from "@/systems/simulation/driver/SliderDriver"
import WheelDriver from "@/systems/simulation/driver/WheelDriver"
import type SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import World from "@/systems/World"

/** A driver whose velocity/force can be configured from the joints panel. */
export type ConfigurableDriver = WheelDriver | HingeDriver | SliderDriver

/** Metadata for an optional force/torque/acceleration slider on a control. */
export type ForceConfig = {
    label: string
    range: [number, number]
    /** Wheels always show a force slider; other joints only when subsystem gravity is enabled. */
    alwaysVisible: boolean
}

/** One velocity (+ optional force) slider that configures a set of joints together. */
export type JointConfigControl = {
    label: string
    drivers: ConfigurableDriver[]
    velocityRange: [number, number]
    force?: ForceConfig
}

/** A single entry in the joints menu, composed of one or more controls. */
export type JointConfigGroup = {
    id: string
    name: string
    controls: JointConfigControl[]
    sequential?: SequentialBehaviorPreferences
}

const isConfigurable = (d: Driver): d is ConfigurableDriver =>
    d instanceof WheelDriver || d instanceof HingeDriver || d instanceof SliderDriver

/** Per-driver-kind slider bounds and force-slider metadata. All members of a control share a kind. */
function controlBoundsFor(driver: ConfigurableDriver): Pick<JointConfigControl, "velocityRange" | "force"> {
    if (driver instanceof WheelDriver)
        return { velocityRange: [0.1, 80], force: { label: "Max Acceleration", range: [0.1, 15], alwaysVisible: true } }
    if (driver instanceof SliderDriver)
        return { velocityRange: [0.1, 80], force: { label: "Max Force", range: [100, 800], alwaysVisible: false } }
    return { velocityRange: [0.1, 40], force: { label: "Max Torque", range: [20, 150], alwaysVisible: false } }
}

function makeControl(label: string, drivers: ConfigurableDriver[]): JointConfigControl {
    return { label, drivers, ...controlBoundsFor(drivers[0]) }
}

/** Reads a driver's force/acceleration value. */
export function driverForce(driver: ConfigurableDriver): number {
    return driver.maxAcceleration
}

/**
 * Writes one driver's velocity/force live and into preferences. The only place that knows where
 * each driver kind persists: wheels use the shared drive prefs, others persist by name in the
 * motors list. Does not save; callers save once after a batch.
 */
export function applyDriverConfig(
    robot: MirabufSceneObject,
    driver: ConfigurableDriver,
    velocity: number,
    force: number
): void {
    driver.maxVelocity = velocity

    const prefs = robot.robotPreferences
    if (driver instanceof WheelDriver) {
        driver.maxAcceleration = force
        prefs.driveVelocity = velocity
        prefs.driveAcceleration = force
        return
    }

    driver.maxAcceleration = force
    const name = driver.info?.name
    if (!name) return
    const motors = (prefs.motors ?? []).filter(m => m.name !== name)
    motors.push({ name, maxVelocity: velocity, maxAcceleration: force })
    prefs.motors = motors
}

/**
 * Claims a subset of the robot's drivers into one named group; anything unclaimed falls through to
 * single-joint groups. New groupings plug in by appending another provider to {@link GROUP_PROVIDERS}.
 */
type GroupProvider = (
    drivers: ConfigurableDriver[],
    isSwerve: boolean
) => { group?: JointConfigGroup; claimed: Driver[] }

/** Drivetrain: all wheels ("Drive") plus, for swerve, the azimuth hinges ("Module Rotation"). */
const drivetrainGroupProvider: GroupProvider = (drivers, isSwerve) => {
    const wheels = drivers.filter((d): d is WheelDriver => d instanceof WheelDriver)
    const azimuth = isSwerve ? drivers.filter((d): d is HingeDriver => d instanceof HingeDriver && d.continuous) : []

    const controls: JointConfigControl[] = []
    if (wheels.length) controls.push(makeControl("Drive", wheels))
    if (azimuth.length) controls.push(makeControl("Module Rotation", azimuth))
    if (controls.length === 0) return { claimed: [] }

    return { group: { id: "drivetrain", name: "Drivetrain", controls }, claimed: [...wheels, ...azimuth] }
}

const GROUP_PROVIDERS: GroupProvider[] = [drivetrainGroupProvider]

/**
 * Builds the configurable joint groups for a robot: providers claim subsets into named groups, and
 * every remaining joint becomes its own single-joint group.
 */
export function buildJointConfigGroups(
    robot: MirabufSceneObject,
    behaviors: SequentialBehaviorPreferences[]
): JointConfigGroup[] {
    const drivers = (World.simulationSystem.getSimulationLayer(robot.mechanism)?.drivers ?? []).filter(isConfigurable)
    const isSwerve = (robot.brain as SynthesisBrain).driveType === DriveType.SWERVE

    const groups: JointConfigGroup[] = []
    const claimed = new Set<Driver>()
    for (const provider of GROUP_PROVIDERS) {
        const { group, claimed: providerClaimed } = provider(drivers, isSwerve)
        providerClaimed.forEach(d => claimed.add(d))
        if (group) groups.push(group)
    }

    // Remaining joints each become their own group. Sequential behaviors are assigned in the brain's
    // creation order (arm hinges first, then elevator sliders) so the invert toggle lines up.
    const leftoverHinges = drivers.filter((d): d is HingeDriver => d instanceof HingeDriver && !claimed.has(d))
    const leftoverSliders = drivers.filter((d): d is SliderDriver => d instanceof SliderDriver && !claimed.has(d))
    let behaviorIndex = 0
    for (const driver of [...leftoverHinges, ...leftoverSliders]) {
        const name = driver.info?.name ?? "UnnamedMotor"
        groups.push({
            id: `joint-${behaviorIndex}-${name}`,
            name,
            controls: [makeControl(name, [driver])],
            sequential: behaviors[behaviorIndex],
        })
        behaviorIndex++
    }

    return groups
}
