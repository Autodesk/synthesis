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

interface HoverHighlight {
    mesh: THREE.BatchedMesh
    instanceId: number
}

/** Tint applied to the part currently under the cursor so the user can see what a click will select. */
const HOVER_HIGHLIGHT_COLOR = new THREE.Color(2.2, 1.6, 0.2)
/** BatchedMesh instance colors default to this (see BatchedMesh._initColorsTexture); used to un-tint on hover-out. */
const DEFAULT_INSTANCE_COLOR = new THREE.Color(1, 1, 1)

/** Reused across every pickPart() call instead of allocating a new Raycaster/Vector2 per mouse move. */
const _raycaster = new THREE.Raycaster()
const _ndc = new THREE.Vector2()

interface PickIndexEntry {
    sceneObject: MirabufSceneObject
    guid: string
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
    private _pointerMoveListener: ((e: PointerEvent) => void) | undefined
    private _hover: HoverHighlight | undefined
    private _latestMousePos: [number, number] | undefined
    private _lastProcessedMousePos: [number, number] | undefined

    // Rebuilt on enable and after apply(); avoids re-flattening batches and re-scanning every part's mesh
    // entries on every single mouse-move raycast (was O(total mesh entries) per pick).
    private _candidateBatches: THREE.BatchedMesh[] = []
    private _pickIndex = new Map<THREE.BatchedMesh, Map<number, PickIndexEntry>>()

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

    public update(_deltaT: number): void {
        if (!this._enabled || !this._latestMousePos) return

        const [x, y] = this._latestMousePos
        const last = this._lastProcessedMousePos
        if (last && last[0] === x && last[1] === y) return

        this._lastProcessedMousePos = this._latestMousePos
        this.updateHover(this._latestMousePos)
    }

    public destroy(): void {
        this.enabled = false
    }

    private hookInteractionHandlers(): void {
        const screenHandler = World.sceneRenderer.screenInteractionHandler
        this._originalInteractionStart = screenHandler.interactionStart
        screenHandler.interactionStart = (interaction: InteractionStart) => this.onInteractionStart(interaction)

        this._pointerMoveListener = (e: PointerEvent) => {
            this._latestMousePos = [e.clientX, e.clientY]
        }
        World.sceneRenderer.renderer.domElement.addEventListener("pointermove", this._pointerMoveListener)

        this.rebuildPickIndex()
    }

    private unhookInteractionHandlers(): void {
        const screenHandler = World.sceneRenderer.screenInteractionHandler
        if (this._originalInteractionStart) screenHandler.interactionStart = this._originalInteractionStart

        if (this._pointerMoveListener) {
            World.sceneRenderer.renderer.domElement.removeEventListener("pointermove", this._pointerMoveListener)
            this._pointerMoveListener = undefined
        }
        this._latestMousePos = undefined
        this._lastProcessedMousePos = undefined
        this.clearHover()

        this._candidateBatches = []
        this._pickIndex = new Map()
    }

    /**
     * Flattens all scene objects' batches and mesh-entry GUID maps into flat lookup structures once,
     * instead of re-deriving them on every raycast. Must be re-run whenever the set of registered scene
     * objects or their batches changes while this mode is active (currently: on enable, and after apply()
     * rebuilds the affected assemblies).
     */
    private rebuildPickIndex(): void {
        this._candidateBatches = []
        this._pickIndex = new Map()

        for (const sceneObject of World.sceneRenderer.mirabufSceneObjects.getAll()) {
            for (const batch of sceneObject.mirabufInstance.batches) {
                this._candidateBatches.push(batch)
            }

            for (const [guid, entries] of sceneObject.mirabufInstance.meshes) {
                for (const [mesh, instanceId] of entries) {
                    let byInstance = this._pickIndex.get(mesh)
                    if (!byInstance) {
                        byInstance = new Map()
                        this._pickIndex.set(mesh, byInstance)
                    }
                    byInstance.set(instanceId, { sceneObject, guid })
                }
            }
        }
    }

    /** Raycasts the cached candidate batches for the part-instance GUID under the mouse. */
    private pickPart(mousePos: [number, number]): PartPick | undefined {
        const camera = World.sceneRenderer.mainCamera
        _ndc.set((mousePos[0] / window.innerWidth) * 2 - 1, -(mousePos[1] / window.innerHeight) * 2 + 1)
        _raycaster.setFromCamera(_ndc, camera)

        const hits = _raycaster.intersectObjects(this._candidateBatches, false)
        if (hits.length === 0) return undefined

        const hit = hits[0]
        const object = hit.object as THREE.BatchedMesh
        const instanceId = (hit as unknown as { batchId?: number }).batchId ?? 0

        const resolved = this._pickIndex.get(object)?.get(instanceId)
        if (!resolved) return undefined

        return { sceneObject: resolved.sceneObject, guid: resolved.guid, object, instanceId }
    }

    /** Tints whatever part is currently under the cursor so the user can preview what a click will pick. */
    private updateHover(mousePos: [number, number]): void {
        const pick = this.pickPart(mousePos)
        if (!pick) {
            this.clearHover()
            return
        }

        const mesh = pick.object as THREE.BatchedMesh
        if (this._hover && this._hover.mesh === mesh && this._hover.instanceId === pick.instanceId) return

        this.clearHover()
        mesh.setColorAt(pick.instanceId, HOVER_HIGHLIGHT_COLOR)
        this._hover = { mesh, instanceId: pick.instanceId }
    }

    private clearHover(): void {
        if (!this._hover) return
        this._hover.mesh.setColorAt(this._hover.instanceId, DEFAULT_INSTANCE_COLOR)
        this._hover = undefined
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
        const pick = this.pickPart(mousePos)
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

        const pick = this.pickPart(mousePos)
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

        // The hovered mesh's owning scene object may be one of the ones rebuilt below, which destroys its
        // batches -- clear while the mesh is still valid so _hover can't end up pointing at a dead one.
        this.clearHover()

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

            const rebuilt = await createMirabuf(assembly.info!.GUID!, assembly)
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

        // Rebuilt assemblies got new batches/instance ids -- the cached pick index would point at stale,
        // now-destroyed meshes otherwise, and this mode may still be enabled for further picks.
        if (this._enabled) this.rebuildPickIndex()
    }
}

export default WheelAssignmentMode
