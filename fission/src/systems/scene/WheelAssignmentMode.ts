import * as THREE from "three"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { applyWheelAssignments, type WheelAssignment } from "@/mirabuf/WheelJointBuilder"
import EventSystem from "@/systems/EventSystem.ts"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import {
    computeWheelAxisFromAABB,
    computeWheelAxisFromCircleFit,
    transformWheelAxis,
    type WheelAxis,
} from "@/util/geometry/WheelAxisFit"
import World from "../World"
import WorldSystem from "../WorldSystem"
import { type InteractionStart, PRIMARY_MOUSE_INTERACTION } from "./ScreenInteractionHandler"

enum PickStage {
    WHEEL,
    PARENT,
}

interface PartPick {
    sceneObject: MirabufSceneObject
    guid: string
    object: THREE.Object3D
    instanceId: number
}

interface BatchedMeshRangeApi {
    getGeometryIdAt?: (instanceId: number) => number
    getGeometryRangeAt?: (geometryId: number, target?: object) => { vertexStart: number; vertexCount: number }
}

/**
 * Returns every local-space vertex belonging to just this one part's slice of a (possibly shared)
 * BatchedMesh buffer, using the public getGeometryIdAt/getGeometryRangeAt range API so we don't pull in
 * vertices from unrelated parts merged into the same batch. Falls back to the whole geometry for a
 * plain (non-batched) mesh.
 */
function getPartLocalVertices(object: THREE.Object3D, instanceId: number): THREE.Vector3[] | undefined {
    const mesh = object as THREE.Mesh
    const position = mesh.geometry?.getAttribute("position")
    if (!position) return undefined

    let start = 0
    let count = position.count

    const batched = object as unknown as BatchedMeshRangeApi
    if (typeof batched.getGeometryIdAt === "function" && typeof batched.getGeometryRangeAt === "function") {
        const geometryId = batched.getGeometryIdAt(instanceId)
        const range = batched.getGeometryRangeAt(geometryId)
        start = range.vertexStart
        count = range.vertexCount
    }

    const points: THREE.Vector3[] = []
    for (let i = start; i < start + count; i++) {
        points.push(new THREE.Vector3().fromBufferAttribute(position, i))
    }
    return points
}

interface WheelDraft {
    sceneObject: MirabufSceneObject
    wheelPartGuid: string
    axisFit: WheelAxis // world-space
}

interface PendingAssignment {
    sceneObject: MirabufSceneObject
    assignment: WheelAssignment
}

/** Raycasts the scene for the part-instance GUID under the mouse, resolved through MirabufInstance.meshes. */
function pickPart(mousePos: [number, number]): PartPick | undefined {
    const camera = World.sceneRenderer.mainCamera
    const ndc = new THREE.Vector2(
        (mousePos[0] / window.innerWidth) * 2 - 1,
        -(mousePos[1] / window.innerHeight) * 2 + 1
    )
    const raycaster = new THREE.Raycaster()
    raycaster.setFromCamera(ndc, camera)

    const sceneObjects = World.sceneRenderer.mirabufSceneObjects.getAll()
    const candidateBatches = sceneObjects.flatMap(obj => obj.mirabufInstance.batches)

    const hits = raycaster.intersectObjects(candidateBatches, false)
    if (hits.length === 0) return undefined

    const hit = hits[0]
    const object = hit.object
    const instanceId = (hit as unknown as { batchId?: number }).batchId ?? 0

    for (const sceneObject of sceneObjects) {
        if (!sceneObject.mirabufInstance.batches.includes(object as THREE.BatchedMesh)) continue

        for (const [guid, entries] of sceneObject.mirabufInstance.meshes) {
            if (entries.some(([mesh, id]) => mesh === object && id === instanceId)) {
                return { sceneObject, guid, object, instanceId }
            }
        }
    }

    return undefined
}

/**
 * Test-scope interaction mode for the "select a circular edge to place a wheel joint" mechanism.
 *
 * Flow per wheel: click the wheel's rim edge (fits a circle -> origin/axis/radius), then click a
 * second, different part on the same assembly as the mandatory parent/chassis. Repeats indefinitely,
 * accumulating pending assignments. apply() mutates the affected assembly/assemblies and fully rebuilds
 * their MirabufSceneObjects -- no partial/live patching of physics bodies, no persistence.
 */
class WheelAssignmentMode extends WorldSystem {
    private _enabled = false
    private _stage: PickStage = PickStage.WHEEL
    private _draft: WheelDraft | undefined
    private _pending: PendingAssignment[] = []

    private _originalInteractionStart: ((i: InteractionStart) => void) | undefined

    public get enabled(): boolean {
        return this._enabled
    }

    public set enabled(enabled: boolean) {
        if (this._enabled === enabled) return
        this._enabled = enabled

        if (enabled) {
            this._stage = PickStage.WHEEL
            this._draft = undefined
            this.hookInteractionHandlers()
        } else {
            this.unhookInteractionHandlers()
            this._draft = undefined
        }

        EventSystem.dispatch("WheelAssignmentModeToggled", { enabled })
    }

    public get pendingCount(): number {
        return this._pending.length
    }

    public get awaitingParentPick(): boolean {
        return this._enabled && this._stage === PickStage.PARENT
    }

    public update(_deltaT: number): void {}

    public destroy(): void {
        this.enabled = false
    }

    private hookInteractionHandlers(): void {
        const screenHandler = World.sceneRenderer.screenInteractionHandler
        this._originalInteractionStart = screenHandler.interactionStart
        screenHandler.interactionStart = (interaction: InteractionStart) => this.onInteractionStart(interaction)
    }

    private unhookInteractionHandlers(): void {
        const screenHandler = World.sceneRenderer.screenInteractionHandler
        if (this._originalInteractionStart) screenHandler.interactionStart = this._originalInteractionStart
    }

    private onInteractionStart(interaction: InteractionStart): void {
        if (interaction.interactionType !== PRIMARY_MOUSE_INTERACTION) {
            this._originalInteractionStart?.(interaction)
            return
        }

        if (this._stage === PickStage.WHEEL) this.handleWheelPick(interaction.position)
        else this.handleParentPick(interaction.position)
    }

    private handleWheelPick(mousePos: [number, number]): void {
        const pick = pickPart(mousePos)
        if (!pick) {
            globalAddToast("warning", "Wheel Assignment", "Click directly on a part's mesh.")
            return
        }

        const points = getPartLocalVertices(pick.object, pick.instanceId)
        if (!points || points.length === 0) {
            globalAddToast("warning", "Wheel Assignment", "Couldn't read this part's geometry.")
            return
        }

        const localAxisFit = computeWheelAxisFromCircleFit(points) ?? computeWheelAxisFromAABB(points)
        if (!localAxisFit) {
            globalAddToast("warning", "Wheel Assignment", "Couldn't derive a wheel axis from this part's geometry.")
            return
        }

        // Use the part's assembly-space transform (the frame PhysicsSystem/WheelJointBuilder expect joint
        // origins in), not the live scene's rendered matrix -- that includes the mechanism's current
        // physics-body world transform (spawn placement + gravity settling of the single fused pre-split
        // body), which would bake an incidental vertical offset into the permanently-stored joint origin.
        const assemblySpaceTransform = pick.sceneObject.mirabufInstance.parser.globalTransforms.get(pick.guid)!
        const worldAxisFit = transformWheelAxis(localAxisFit, assemblySpaceTransform)

        this._draft = { sceneObject: pick.sceneObject, wheelPartGuid: pick.guid, axisFit: worldAxisFit }
        this._stage = PickStage.PARENT
        globalAddToast("info", "Wheel Assignment", "Wheel axis captured. Now click the wheel's parent/chassis part.")
    }

    private handleParentPick(mousePos: [number, number]): void {
        if (!this._draft) {
            this._stage = PickStage.WHEEL
            return
        }

        const pick = pickPart(mousePos)
        if (!pick) {
            globalAddToast("warning", "Wheel Assignment", "Click directly on a part's mesh.")
            return
        }

        if (pick.sceneObject !== this._draft.sceneObject) {
            globalAddToast("warning", "Wheel Assignment", "Parent must be part of the same assembly as the wheel.")
            return
        }

        if (pick.guid === this._draft.wheelPartGuid) {
            globalAddToast("warning", "Wheel Assignment", "Parent must be a different part than the wheel.")
            return
        }

        this._pending.push({
            sceneObject: this._draft.sceneObject,
            assignment: {
                wheelPartGuid: this._draft.wheelPartGuid,
                parentPartGuid: pick.guid,
                axisFit: this._draft.axisFit,
            },
        })

        this._draft = undefined
        this._stage = PickStage.WHEEL
        EventSystem.dispatch("WheelAssignmentPendingCountChanged", { count: this._pending.length })
        globalAddToast(
            "success",
            "Wheel Assignment",
            `Wheel staged (${this._pending.length} pending). Pick the next wheel, or Apply.`
        )
    }

    /** Mutates each affected assembly and fully rebuilds its MirabufSceneObject. */
    public async apply(): Promise<void> {
        if (this._pending.length === 0) return

        const bySceneObject = new Map<MirabufSceneObject, WheelAssignment[]>()
        for (const { sceneObject, assignment } of this._pending) {
            const list = bySceneObject.get(sceneObject)
            if (list) list.push(assignment)
            else bySceneObject.set(sceneObject, [assignment])
        }

        for (const [sceneObject, assignments] of bySceneObject) {
            const assembly = sceneObject.mirabufInstance.parser.assembly
            applyWheelAssignments(assembly, assignments)

            const sceneId = sceneObject.id
            World.sceneRenderer.removeSceneObject(sceneId)

            const rebuilt = await createMirabuf(assembly)
            if (!rebuilt) {
                globalAddToast("error", "Wheel Assignment", "Failed to rebuild assembly after applying wheel joints.")
                continue
            }
            World.sceneRenderer.registerSceneObject(rebuilt, sceneId)

            const parser = rebuilt.mirabufInstance.parser
            if (parser.errors.length > 0) {
                console.warn(`[WheelAssignmentMode] Parser reported errors after rebuild:`, parser.errors)
            }
            for (const assignment of assignments) {
                const wheelNode = parser.partToNodeMap.get(assignment.wheelPartGuid)
                const parentNode = parser.partToNodeMap.get(assignment.parentPartGuid)
                if (!wheelNode || !parentNode) {
                    console.error(`[WheelAssignmentMode] No rigid node found for the wheel and/or parent part.`)
                } else if (wheelNode.id === parentNode.id) {
                    console.error(`[WheelAssignmentMode] Wheel and parent ended up in the same rigid node.`)
                }
            }
        }

        this._pending = []
        EventSystem.dispatch("WheelAssignmentPendingCountChanged", { count: 0 })
        globalAddToast("success", "Wheel Assignment", "Applied wheel joints and rebuilt the affected assembly.")
    }
}

export default WheelAssignmentMode
