import { beforeEach, describe, expect, test, vi } from "vitest"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { applyPartDeletions } from "@/mirabuf/PartDeletionBuilder"
import { applyWheelAssignments } from "@/mirabuf/WheelJointBuilder"
import World from "@/systems/World"
import { applyModelConfigChanges } from "@/systems/scene/ApplyModelConfig"

vi.mock("@/mirabuf/MirabufSceneObject", () => ({
    createMirabuf: vi.fn(),
}))
vi.mock("@/mirabuf/PartDeletionBuilder", () => ({
    applyPartDeletions: vi.fn(),
}))
vi.mock("@/mirabuf/WheelJointBuilder", () => ({
    applyWheelAssignments: vi.fn(),
}))
vi.mock("@/systems/World", () => ({
    default: {
        wheelAssignmentMode: {
            pendingList: [],
            sceneObject: undefined,
            clearHover: vi.fn(),
            finishApply: vi.fn(),
            cancel: vi.fn(),
            isMismatched: vi.fn(),
        },
        partDeletionMode: {
            pendingList: [],
            sceneObject: undefined,
            clearHover: vi.fn(),
            finishApply: vi.fn(),
            cancel: vi.fn(),
        },
        sceneRenderer: {
            sceneObjects: new Map(),
            removeSceneObject: vi.fn(),
            registerSceneObject: vi.fn(),
        },
    },
}))

const sceneObject = {
    id: "scene",
    mirabufInstance: {
        parser: { assembly: { info: { GUID: "assembly" } } },
    },
}

const wheelAssignment = { wheelPartGuid: "wheel", parentPartGuid: "root" }

type WorldMock = {
    wheelAssignmentMode: {
        pendingList: Array<{ assignment: typeof wheelAssignment }>
        sceneObject: typeof sceneObject | undefined
        clearHover: ReturnType<typeof vi.fn>
        finishApply: ReturnType<typeof vi.fn>
        cancel: ReturnType<typeof vi.fn>
        isMismatched: ReturnType<typeof vi.fn>
    }
    partDeletionMode: {
        pendingList: Array<{ guid: string }>
        sceneObject: typeof sceneObject | undefined
        clearHover: ReturnType<typeof vi.fn>
        finishApply: ReturnType<typeof vi.fn>
        cancel: ReturnType<typeof vi.fn>
    }
    sceneRenderer: {
        sceneObjects: Map<string, typeof sceneObject>
        removeSceneObject: ReturnType<typeof vi.fn>
        registerSceneObject: ReturnType<typeof vi.fn>
    }
}

describe("applyModelConfigChanges", () => {
    const world = World as unknown as WorldMock

    beforeEach(() => {
        vi.clearAllMocks()
        world.wheelAssignmentMode.pendingList = [{ assignment: wheelAssignment }]
        world.wheelAssignmentMode.sceneObject = sceneObject
        world.partDeletionMode.pendingList = []
        world.partDeletionMode.sceneObject = undefined
        world.sceneRenderer.sceneObjects = new Map([[sceneObject.id, sceneObject]])
        vi.mocked(applyWheelAssignments).mockImplementation(() => {})
        vi.mocked(applyPartDeletions).mockImplementation(() => {})
        vi.mocked(createMirabuf).mockResolvedValue(sceneObject as never)
    })

    test("returns a failure and clears lifecycle state when applying throws", async () => {
        vi.mocked(applyWheelAssignments).mockImplementation(() => {
            throw new Error("invalid assembly")
        })

        await expect(applyModelConfigChanges()).resolves.toBe(false)

        expect(world.wheelAssignmentMode.finishApply).toHaveBeenCalledWith(sceneObject)
        expect(world.partDeletionMode.finishApply).toHaveBeenCalledWith(sceneObject)
        expect(createMirabuf).not.toHaveBeenCalled()
    })

    test("cancels modes if rebuilding removes the old scene without a replacement", async () => {
        world.sceneRenderer.removeSceneObject.mockImplementation((id: string) => {
            world.sceneRenderer.sceneObjects.delete(id)
        })
        vi.mocked(createMirabuf).mockResolvedValue(undefined)

        await expect(applyModelConfigChanges()).resolves.toBe(false)

        expect(world.wheelAssignmentMode.cancel).toHaveBeenCalledOnce()
        expect(world.partDeletionMode.cancel).toHaveBeenCalledOnce()
        expect(world.wheelAssignmentMode.finishApply).not.toHaveBeenCalled()
    })
})
