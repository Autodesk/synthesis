import * as THREE from "three"
import { GROUNDED_JOINT_ID } from "@/mirabuf/MirabufParser"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { applyWheelAssignments, type WheelAssignment } from "@/mirabuf/WheelJointBuilder"
import EventSystem from "@/systems/EventSystem.ts"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import {
    computeWheelAxisFromAABB,
    computeWheelAxisFromCircleFit,
    transformWheelAxis,
} from "@/util/geometry/WheelAxisFit"
import World from "../World"
import WorldSystem from "../WorldSystem"
import { type InteractionStart, PRIMARY_MOUSE_INTERACTION } from "./ScreenInteractionHandler"

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

/** Local-space vertices for just this part's slice of a shared BatchedMesh buffer; whole geometry otherwise. */
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

interface PendingAssignment {
    sceneObject: MirabufSceneObject
    assignment: WheelAssignment
}

interface HoverHighlight {
    mesh: THREE.BatchedMesh
    instanceId: number
}

/** Tint for the part under the cursor. */
const HOVER_HIGHLIGHT_COLOR = new THREE.Color(2.2, 1.6, 0.2)
/** Default BatchedMesh instance color; used to un-tint. */
const DEFAULT_INSTANCE_COLOR = new THREE.Color(1, 1, 1)

/** Reused across picks to avoid reallocating. */
const _raycaster = new THREE.Raycaster()
const _ndc = new THREE.Vector2()

interface PickIndexEntry {
    sceneObject: MirabufSceneObject
    guid: string
}

/** Interaction mode: click a wheel's rim to fit a joint axis, using the assembly's grounded part as parent. */
class WheelAssignmentMode extends WorldSystem {
    private _enabled = false
    private _pending: PendingAssignment[] = []

    private _originalInteractionStart: ((i: InteractionStart) => void) | undefined
    private _pointerMoveListener: ((e: PointerEvent) => void) | undefined
    private _hover: HoverHighlight | undefined
    private _latestMousePos: [number, number] | undefined
    private _lastProcessedMousePos: [number, number] | undefined

    // Rebuilt on enable and after apply() to avoid rescanning every mesh entry per raycast.
    private _candidateBatches: THREE.BatchedMesh[] = []
    private _pickIndex = new Map<THREE.BatchedMesh, Map<number, PickIndexEntry>>()

    private _driveReversed = false

    public get enabled(): boolean {
        return this._enabled
    }

    public set enabled(enabled: boolean) {
        if (this._enabled === enabled) return
        this._enabled = enabled

        if (enabled) this.hookInteractionHandlers()
        else this.unhookInteractionHandlers()

        EventSystem.dispatch("WheelAssignmentModeToggled", { enabled })
    }

    public get pendingCount(): number {
        return this._pending.length
    }

    public get driveReversed(): boolean {
        return this._driveReversed
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

    public toggleReverseDrive(): void {
        this._driveReversed = !this._driveReversed

        for (const sceneObject of World.sceneRenderer.mirabufSceneObjects.getAll()) {
            if (!sceneObject.mechanism.urdfWheelForward) continue
            if (!(sceneObject.brain instanceof SynthesisBrain)) continue

            for (const driver of sceneObject.brain.getWheelDrivers()) {
                driver.reversed = this._driveReversed
            }
        }

        EventSystem.dispatch("WheelAssignmentDriveReversedChanged", { reversed: this._driveReversed })
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

    /** Flattens scene objects' batches and mesh-entry GUIDs into lookup structures for pickPart(). */
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

    /** Tints the part under the cursor. */
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

        const claimedInteraction = this.handleWheelPick(interaction.position)
        if (!claimedInteraction) {
            this._originalInteractionStart?.(interaction)
        }
    }

    private handleWheelPick(mousePos: [number, number]): boolean {
        const pick = this.pickPart(mousePos)
        if (!pick) {
            return false
        }

        const points = getPartLocalVertices(pick.object, pick.instanceId)
        if (!points || points.length === 0) {
            globalAddToast("warning", "Wheel Assignment", "Couldn't read this part's geometry.")
            return true
        }

        const localAxisFit = computeWheelAxisFromCircleFit(points) ?? computeWheelAxisFromAABB(points)
        if (!localAxisFit) {
            globalAddToast("warning", "Wheel Assignment", "Couldn't derive a wheel axis from this part's geometry.")
            return true
        }

        // Assembly-space transform, not the live scene matrix (which bakes in the physics body's world transform).
        const assemblySpaceTransform = pick.sceneObject.mirabufInstance.parser.globalTransforms.get(pick.guid)!
        const worldAxisFit = transformWheelAxis(localAxisFit, assemblySpaceTransform)

        // Grounded/root part doubles as the parent -- no second click needed.
        const groundedInstance =
            pick.sceneObject.mirabufInstance.parser.assembly.data!.joints!.jointInstances![GROUNDED_JOINT_ID]
        const parentPartGuid = groundedInstance.parts!.nodes!.at(0)!.value!
        if (parentPartGuid === pick.guid) {
            globalAddToast("warning", "Wheel Assignment", "This part is the assembly's grounded/root part.")
            return true
        }

        this._pending.push({
            sceneObject: pick.sceneObject,
            assignment: { wheelPartGuid: pick.guid, parentPartGuid, axisFit: worldAxisFit },
        })

        EventSystem.dispatch("WheelAssignmentPendingCountChanged", { count: this._pending.length })
        globalAddToast(
            "success",
            "Wheel Assignment",
            `Wheel staged (${this._pending.length} pending). Pick the next wheel, or Apply.`
        )
        return true
    }

    /** Mutates each affected assembly and fully rebuilds its MirabufSceneObject. */
    public async apply(): Promise<void> {
        if (this._pending.length === 0) return

        // Clear before rebuild destroys the hovered mesh's batches.
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

            let hadMismatch = false
            for (const assignment of assignments) {
                const wheelNode = parser.partToNodeMap.get(assignment.wheelPartGuid)
                const parentNode = parser.partToNodeMap.get(assignment.parentPartGuid)
                if (!wheelNode || !parentNode) continue
                if (wheelNode.id !== parentNode.id) continue
                hadMismatch = true
            }

            if (hadMismatch) {
                globalAddToast("warning", "Wheel Assignment", "Wheel and parent ended up in the same rigid node.")
            }
        }

        this._pending = []
        EventSystem.dispatch("WheelAssignmentPendingCountChanged", { count: 0 })
        globalAddToast("success", "Wheel Assignment", "Applied wheel joints and rebuilt the affected assembly.")

        // Rebuilt assemblies got new batches/instance ids; refresh the stale pick index.
        if (this._enabled) this.rebuildPickIndex()
    }
}

export default WheelAssignmentMode
