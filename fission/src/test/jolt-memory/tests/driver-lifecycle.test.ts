// Integration exercise for HingeDriver/SliderDriver/WheelDriver: per-frame Jolt manipulation
// during live control (`ArcadeDriveBehavior`/`SynthesisBrain` -> `Driver.update()`, every
// simulated tick for every controllable joint).
//
// Built the same way `SimulationLayer`'s constructor builds drivers (SimulationSystem.ts:84-104):
// dispatch off `MechanismConstraint.primaryConstraint.GetSubType()`, `JOLT.castObject` to the
// concrete constraint type, then hand the cast to the driver. Constructed directly rather than
// through `SimulationLayer`, since that class also builds Hinge/Slider/WheelRotation `Stimulus`
// objects in the same constructor pass, and pulling those in would make a failure here ambiguous
// about which side caused it.
//
// Exercises every Jolt-touching member: `update()` in both VELOCITY and POSITION control modes
// (the position branch is most likely to leak, since HingeDriver adds a shortest-path angle
// calculation and SliderDriver a rate-limited position ramp, neither present in the vehicle path),
// HingeDriver's `worldAnchor`/`worldAxis` getters (per-frame consumers exist in
// `SwerveDriveBehavior.ts`), `setContinuousRotation`, and the `maxAcceleration` get/set pair on all
// three (round-trips through `GetMotorSettings()`, an `INTERNAL_REF` per
// ownership-table.generated.json, never destroy()'d).
//
// Only `PreferencesSystem` is touched for real (HingeDriver/SliderDriver constructors read and
// subscribe to the "SubsystemGravity" preference), since it's inert w.r.t. Jolt. Everything else
// is a real `PhysicsSystem`/`MirabufParser`-built `Mechanism`, checked with the same leak/
// destroy-guard instrumentation as other real-lifecycle tests.
import { describe, expect, test } from "vitest"
import MirabufParser from "@/mirabuf/MirabufParser"
import HingeDriver from "@/systems/simulation/driver/HingeDriver"
import SliderDriver from "@/systems/simulation/driver/SliderDriver"
import WheelDriver from "@/systems/simulation/driver/WheelDriver"
import { DriverControlMode, makeDriverID } from "@/systems/simulation/driver/Driver"
import type Mechanism from "@/systems/physics/Mechanism"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import { diffLiveCountsFiltered, patchDestroyGuard, snapshotAllLiveCounts } from "../lib/instrumentation"
import { createFullDrivetrainAssembly } from "../lib/mirabuf-fixtures"

// Same dispatch as SimulationLayer's constructor (SimulationSystem.ts:84-104), minus the Stimulus
// construction it interleaves. This suite is scoped to Driver classes only.
function buildDrivers(mechanism: Mechanism): { hinge: HingeDriver; slider: SliderDriver; wheel: WheelDriver } {
    let hinge: HingeDriver | undefined
    let slider: SliderDriver | undefined
    let wheel: WheelDriver | undefined

    mechanism.constraints.forEach(x => {
        if (x.primaryConstraint.GetSubType() == JOLT.EConstraintSubType_Hinge) {
            const hingeConstraint = JOLT.castObject(x.primaryConstraint, JOLT.HingeConstraint)
            hinge = new HingeDriver(makeDriverID(x), hingeConstraint, x.maxVelocity, x.info)
        } else if (x.primaryConstraint.GetSubType() == JOLT.EConstraintSubType_Vehicle) {
            const vehicleConstraint = JOLT.castObject(x.primaryConstraint, JOLT.VehicleConstraint)
            wheel = new WheelDriver(makeDriverID(x), vehicleConstraint, x.maxVelocity, x.info)
        } else if (x.primaryConstraint.GetSubType() == JOLT.EConstraintSubType_Slider) {
            const sliderConstraint = JOLT.castObject(x.primaryConstraint, JOLT.SliderConstraint)
            slider = new SliderDriver(makeDriverID(x), sliderConstraint, x.maxVelocity, x.info)
        }
    })

    if (!hinge || !slider || !wheel) {
        throw new Error(
            `fixture did not produce all three constraint types (hinge=${!!hinge}, slider=${!!slider}, wheel=${!!wheel})`
        )
    }

    return { hinge, slider, wheel }
}

function driveDrivers(hinge: HingeDriver, slider: SliderDriver, wheel: WheelDriver, frames: number) {
    hinge.setContinuousRotation()
    hinge.maxAcceleration = 40
    slider.maxAcceleration = 300

    for (let i = 0; i < frames; i++) {
        hinge.controlMode = i % 2 === 0 ? DriverControlMode.VELOCITY : DriverControlMode.POSITION
        hinge.accelerationDirection = i % 2 === 0 ? 1 : -1
        hinge.targetAngle = (i / frames) * Math.PI * 3
        hinge.feedforwardVelocity = 0.1
        // Per-frame consumers (SwerveDriveBehavior.ts) read these every tick and never destroy()
        // them (STATIC_ALIAS per ownership-table.generated.json). Exercised here the same way.
        void hinge.worldAnchor
        void hinge.worldAxis
        hinge.update(1 / 60)

        slider.controlMode = i % 2 === 0 ? DriverControlMode.VELOCITY : DriverControlMode.POSITION
        slider.accelerationDirection = i % 2 === 0 ? -1 : 1
        slider.targetPosition = (i / frames - 0.5) * 5
        slider.update(1 / 60)

        wheel.accelerationDirection = i % 2 === 0 ? 1 : -1
        wheel.update(1 / 60)
    }
}

describe("real lifecycle: HingeDriver/SliderDriver/WheelDriver through a real Mechanism", () => {
    test("construct -> drive x30 frames (velocity + position modes) leaves no leak and no destroy-guard violation", () => {
        const guard = patchDestroyGuard(JOLT)
        const before = snapshotAllLiveCounts(JOLT)

        const system = new PhysicsSystem()
        const { assembly } = createFullDrivetrainAssembly()
        const parser = new MirabufParser(assembly)
        const mechanism = system.createMechanismFromParser(parser)

        const { hinge, slider, wheel } = buildDrivers(mechanism)
        driveDrivers(hinge, slider, wheel, 30)

        system.destroyMechanism(mechanism)
        mechanism.layerReserve?.release()
        system.destroy()

        const after = snapshotAllLiveCounts(JOLT)
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCountsFiltered(before, after)).toEqual([])
    })

    // Real usage: one long-lived PhysicsSystem, robots swapped in and out, drivers rebuilt each
    // time (SimulationLayer is reconstructed per Mechanism). A per-swap leak too small to see in
    // one cycle compounds linearly here.
    test("N drivetrain swaps (construct -> drive x10 frames -> destroy) x20 through one PhysicsSystem leaves no leak", () => {
        const guard = patchDestroyGuard(JOLT)
        const before = snapshotAllLiveCounts(JOLT)

        const system = new PhysicsSystem()

        for (let cycle = 0; cycle < 20; cycle++) {
            const { assembly } = createFullDrivetrainAssembly(cycle)
            const parser = new MirabufParser(assembly)
            const mechanism = system.createMechanismFromParser(parser)

            const { hinge, slider, wheel } = buildDrivers(mechanism)
            driveDrivers(hinge, slider, wheel, 10)

            system.destroyMechanism(mechanism)
            mechanism.layerReserve?.release()
        }

        system.destroy()

        const after = snapshotAllLiveCounts(JOLT)
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCountsFiltered(before, after)).toEqual([])
    })
})
