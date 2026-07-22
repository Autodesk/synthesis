// Integration exercise: `SimulationLayer` (SimulationSystem.ts) is the entry point that builds
// Hinge/Slider/WheelRotation `Stimulus` objects alongside HingeDriver/SliderDriver/WheelDriver.
// driver-lifecycle.test.ts audits the drivers directly but bypasses `SimulationLayer` itself so
// its failures stay unambiguous about which side caused them (see its header comment). This
// covers the other half: the constructor (`JOLT.castObject` + `new HingeStimulus`/
// `WheelRotationStimulus`/`SliderStimulus`/`ChassisStimulus`, SimulationSystem.ts:84-111) and
// `update()` (drivers.forEach + stimuli.forEach, every tick), run through a real
// `Mechanism`/`PhysicsSystem`.
//
// Every Stimulus class (Stimulus.ts/ChassisStimulus.ts/HingeStimulus.ts/WheelStimulus.ts/
// SliderStimulus.ts/EncoderStimulus.ts) holds no Jolt ownership per ownership-table.generated.json:
// every getter it reads (GetAccumulatedForce, GetRotation, GetEulerAngles, GetShape,
// GetMassProperties, GetCurrentAngle, GetCurrentPosition, GetRotationAngle,
// GetTargetAngularVelocity, GetAngularVelocity) is STATIC_ALIAS/INTERNAL_REF/NONE, and none of
// these classes call `new JOLT.___()` or `JOLT.destroy()`. `SimulationLayer`'s own
// `JOLT.castObject` calls hit the same classified cache-alias entries driver-lifecycle.test.ts's
// audit added (HingeConstraint/SliderConstraint/VehicleConstraint/Wheel/WheelWV/MotorSettings/
// SpringSettings/VehicleEngine/VehicleController/WheeledVehicleController). A new classification
// entry here would signal this code path differs from the driver path.
//
// Only `World.sceneRenderer.mirabufSceneObjects.findWhere` is mocked: SimulationLayer's
// constructor looks up the owning MirabufSceneObject to decide whether to build
// Intake/EjectorDriver, and returning undefined matches the "no assembly found" path, which only
// skips those two Jolt-free driver classes. `World.physicsSystem` is a real `PhysicsSystem`.
import { describe, expect, test, vi } from "vitest"
import MirabufParser from "@/mirabuf/MirabufParser"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import { SimulationLayer } from "@/systems/simulation/SimulationSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import { diffLiveCountsFiltered, patchDestroyGuard, snapshotAllLiveCounts } from "../lib/instrumentation"
import { createFullDrivetrainAssembly } from "../lib/mirabuf-fixtures"

let activePhysicsSystem: PhysicsSystem

const mockSceneRenderer = {
    mirabufSceneObjects: {
        findWhere: vi.fn(() => undefined),
    },
}

vi.mock("@/systems/World", () => ({
    default: {
        get physicsSystem() {
            return activePhysicsSystem
        },
        get sceneRenderer() {
            return mockSceneRenderer
        },
    },
}))

describe("real lifecycle: SimulationLayer (drivers + stimuli) through a real Mechanism", () => {
    test("construct -> update x10 leaves no leak and no destroy-guard violation", () => {
        const guard = patchDestroyGuard(JOLT)
        const before = snapshotAllLiveCounts(JOLT)

        activePhysicsSystem = new PhysicsSystem()
        const { assembly } = createFullDrivetrainAssembly()
        const parser = new MirabufParser(assembly)
        const mechanism = activePhysicsSystem.createMechanismFromParser(parser)

        const layer = new SimulationLayer(mechanism)
        expect(layer.drivers.length).toBe(3)
        expect(layer.stimuli.length).toBe(4)

        for (let i = 0; i < 10; i++) {
            layer.update(1 / 60)
        }

        activePhysicsSystem.destroyMechanism(mechanism)
        mechanism.layerReserve?.release()
        activePhysicsSystem.destroy()

        const after = snapshotAllLiveCounts(JOLT)
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCountsFiltered(before, after)).toEqual([])
    })
})
