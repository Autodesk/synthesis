import { describe, expect, test } from "vitest"
import { ConfigMode } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import {
    advanceConditionMet,
    CONDITIONS,
    reconcile,
    type TourResult,
    type TourRuntime,
    type TourSnapshot,
} from "@/ui/tour/TourConditions"
import { TOUR_STEPS, type TourStepId } from "@/ui/tour/TourSteps"

const stepOf = (id: TourStepId) => TOUR_STEPS.findIndex(step => step.id === id)

const snapshot = (overrides: Partial<TourSnapshot> = {}): TourSnapshot => ({
    panels: [],
    appMode: "Configure",
    fieldCount: 0,
    robotCount: 0,
    spawnPending: false,
    ...overrides,
})

const fresh: TourRuntime = { step: -1 }

function run(stepIndex: number, snapshots: TourSnapshot[], runtime: TourRuntime = fresh): TourResult {
    let result: TourResult = { stepIndex, runtime }
    for (const s of snapshots) {
        result = reconcile(result.stepIndex, s, result.runtime)
    }
    return result
}

function settle(stepIndex: number, s: TourSnapshot, runtime: TourRuntime = fresh): TourResult {
    let result: TourResult = { stepIndex, runtime }
    for (let i = 0; i < TOUR_STEPS.length + 1; i++) {
        const pass = reconcile(result.stepIndex, s, result.runtime)
        if (pass.stepIndex === result.stepIndex && i > 0) return pass
        result = pass
    }
    throw new Error("reconcile never settled")
}

describe("tour reconciler", () => {
    test("advances as soon as the step's condition holds", () => {
        const result = run(stepOf("add-field"), [snapshot(), snapshot({ modal: "LibraryModal" })])
        expect(result.stepIndex).toBe(stepOf("spawn-field"))
    })

    test("advances when the step's screen closes", () => {
        const open = snapshot({ fieldCount: 1, robotCount: 1, panels: [{ id: "InitialConfigPanel" }] })
        const result = run(stepOf("setup-assembly"), [open, { ...open, panels: [] }])
        expect(result.stepIndex).toBe(stepOf("select-assembly"))
    })

    test("does not re-ask for work the user already did", () => {
        const done = snapshot({ fieldCount: 1, robotCount: 1 })
        expect(run(stepOf("spawn-robot"), [{ ...done, modal: "LibraryModal" }]).stepIndex).toBe(
            stepOf("setup-assembly")
        )
        expect(run(stepOf("setup-assembly"), [done]).stepIndex).toBe(stepOf("select-assembly"))
    })

    test("skips every spawn pair whose asset already exists", () => {
        expect(settle(stepOf("add-field"), snapshot({ fieldCount: 1, robotCount: 1 })).stepIndex).toBe(
            stepOf("select-assembly")
        )
        expect(settle(stepOf("add-field"), snapshot({ fieldCount: 1 })).stepIndex).toBe(stepOf("add-robot"))
    })

    test("rewinds to the step that reopens a screen the user closed", () => {
        const result = run(stepOf("spawn-robot"), [snapshot({ fieldCount: 1 })])
        expect(result.stepIndex).toBe(stepOf("add-robot"))
        expect(result.toast).toBeDefined()
    })

    test("cascades back to the first step when everything is gone", () => {
        expect(settle(stepOf("spawn-robot"), snapshot()).stepIndex).toBe(stepOf("add-field"))
    })

    test("explains a cascade once, with the reason the tour moved", () => {
        const empty = snapshot()
        const toasts: string[] = []
        let result: TourResult = { stepIndex: stepOf("select-assembly"), runtime: fresh }
        for (let i = 0; i < TOUR_STEPS.length; i++) {
            result = reconcile(result.stepIndex, empty, result.runtime)
            if (result.toast) toasts.push(result.toast)
        }
        expect(toasts).toEqual([CONDITIONS.robot.hint])
    })

    test("holds the step, once, when nothing in the tour re-establishes the condition", () => {
        const gameplay = snapshot({ robotCount: 1, fieldCount: 1, appMode: "Gameplay" })
        const first = run(stepOf("select-assembly"), [gameplay])
        expect(first.stepIndex).toBe(stepOf("select-assembly"))
        expect(first.toast).toBeDefined()

        const second = reconcile(first.stepIndex, gameplay, first.runtime)
        expect(second.toast).toBeUndefined()
    })

    test("leaves requirements alone while a spawn is in flight", () => {
        const result = run(stepOf("spawn-robot"), [snapshot({ fieldCount: 1, spawnPending: true })])
        expect(result.stepIndex).toBe(stepOf("spawn-robot"))
        expect(result.toast).toBeUndefined()
    })

    test("completes a spawn step even though the library closed first", () => {
        const result = run(stepOf("spawn-robot"), [
            snapshot({ fieldCount: 1, modal: "LibraryModal" }),
            snapshot({ fieldCount: 1, spawnPending: true }),
            snapshot({ fieldCount: 1, robotCount: 1 }),
        ])
        expect(result.stepIndex).toBe(stepOf("setup-assembly"))
    })

    test("offers Next on a step whose screen is already closed", () => {
        const closed = snapshot({ fieldCount: 1, robotCount: 1 })
        expect(advanceConditionMet(TOUR_STEPS[stepOf("setup-assembly")], closed)).toBe(true)
        expect(advanceConditionMet(TOUR_STEPS[stepOf("save-config")], closed)).toBe(true)
    })

    test("withholds Next while a step is still waiting on its condition", () => {
        const empty = snapshot()
        expect(advanceConditionMet(TOUR_STEPS[stepOf("add-field")], empty)).toBe(false)
        expect(advanceConditionMet(TOUR_STEPS[stepOf("spawn-robot")], empty)).toBe(false)
    })

    test("recovers the intake panel by sending the user back to the step that opens it", () => {
        const configured = snapshot({ fieldCount: 1, robotCount: 1 })
        const result = run(stepOf("adjust-intake"), [
            { ...configured, panels: [{ id: "ConfigurePanel", configMode: ConfigMode.INTAKE }] },
            configured,
        ])
        expect(result.stepIndex).toBe(stepOf("pick-config"))
    })

    test("settles from every step, whatever the world already looks like", () => {
        for (let world = 0; world < 64; world++) {
            const s = snapshot({
                modal: world & 1 ? "LibraryModal" : undefined,
                fieldCount: world & 2 ? 1 : 0,
                robotCount: world & 4 ? 1 : 0,
                appMode: world & 8 ? "Gameplay" : "Configure",
                panels: [
                    ...(world & 16 ? [{ id: "InitialConfigPanel" as const }] : []),
                    ...(world & 32 ? [{ id: "ConfigurePanel" as const, configMode: ConfigMode.INTAKE }] : []),
                ],
            })
            TOUR_STEPS.forEach((_, index) => expect(() => settle(index, s)).not.toThrow())
        }
    })
})
