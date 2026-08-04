import * as THREE from "three"
import { GROUNDED_JOINT_ID } from "@/mirabuf/MirabufParser"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import World from "../World"
import WorldSystem from "../WorldSystem"
import { type InteractionStart, PRIMARY_MOUSE_INTERACTION } from "./ScreenInteractionHandler"

interface PartPick {
    sceneObject: MirabufSceneObject
    guid: string
    object: THREE.Object3D
    instanceId: number
}

export interface PartDeletionSelection {
    sceneId: number
    guid: string
    name: string
    highlight: PartHighlight
}

interface PartHighlight {
    mesh: THREE.BatchedMesh
    instanceId: number
}

/** Tint for the part under the cursor. */
const HOVER_HIGHLIGHT_COLOR = new THREE.Color(1, 1, 0.1)
/** Tint for parts pending deletion. */
const PENDING_DELETE_HIGHLIGHT_COLOR = new THREE.Color(1, 0.15, 0.15)

/** Default BatchedMesh instance color; used to un-tint. */
const DEFAULT_INSTANCE_COLOR = new THREE.Color(1, 1, 1)

/** Reused across picks to avoid reallocating. */
const raycaster = new THREE.Raycaster()
const ndc = new THREE.Vector2()

interface PickIndexEntry {
    sceneObject: MirabufSceneObject
    guid: string
}

/** Interaction mode: click a part to mark it for deletion; click again (or Undo) to unmark. */
class PartDeletionMode extends WorldSystem {
    public pendingDeletions: HighlightMap = new HighlightMap(() => toKey(this._hover))

    private _enabled = false

    private _originalInteractionStart: ((i: InteractionStart) => void) | undefined
    private _pointerMoveListener: ((e: PointerEvent) => void) | undefined
    private _hover: PartHighlight | undefined
    private _latestMousePos: [number, number] | undefined
    private _lastProcessedMousePos: [number, number] | undefined

    // Rebuilt on enable and after apply() to avoid rescanning every mesh entry per raycast.
    private _candidateBatches: THREE.BatchedMesh[] = []
    private _pickIndex = new Map<THREE.BatchedMesh, Map<number, PickIndexEntry>>()

    public get enabled(): boolean {
        return this._enabled
    }

    public set enabled(enabled: boolean) {
        if (this._enabled === enabled) return
        this._enabled = enabled

        if (enabled) this.hookInteractionHandlers()
        else this.unhookInteractionHandlers()
    }

    public get pendingCount(): number {
        return this.pendingDeletions.size
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
        ndc.set((mousePos[0] / window.innerWidth) * 2 - 1, -(mousePos[1] / window.innerHeight) * 2 + 1)
        raycaster.setFromCamera(ndc, camera)

        const hits = raycaster.intersectObjects(this._candidateBatches, false)
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
        this.setHover({ mesh, instanceId: pick.instanceId })
    }

    public setHover(hover: PartHighlight) {
        if (toKey(this._hover) === toKey(hover)) return

        this.clearHover()

        hover.mesh.setColorAt(hover.instanceId, HOVER_HIGHLIGHT_COLOR)
        this._hover = hover
    }

    public clearHover(): void {
        if (!this._hover) return

        const isPending = this.pendingDeletions.hasHighlight(toKey(this._hover))

        this._hover.mesh.setColorAt(
            this._hover.instanceId,
            isPending ? PENDING_DELETE_HIGHLIGHT_COLOR : DEFAULT_INSTANCE_COLOR
        )
        this._hover = undefined
    }

    private onInteractionStart(interaction: InteractionStart): void {
        if (interaction.interactionType !== PRIMARY_MOUSE_INTERACTION) {
            this._originalInteractionStart?.(interaction)
            return
        }
        this.handlePartPick(interaction)
    }

    private handlePartPick(interaction: InteractionStart): void {
        const pick = this.pickPart(interaction.position)
        if (!pick) {
            this._originalInteractionStart?.(interaction)
            return
        }

        if (this.pendingDeletions.hasPart(pick.guid)) {
            this.pendingDeletions.removePart(pick.guid)
            return
        }

        // Grounded/root part can't be removed -- nothing left to attach the assembly to.
        const groundedInstance =
            pick.sceneObject.mirabufInstance.parser.assembly.data!.joints!.jointInstances![GROUNDED_JOINT_ID]
        const rootPartGuid = groundedInstance.parts!.nodes!.at(0)!.value!
        if (rootPartGuid === pick.guid) {
            globalAddToast("warning", "Delete Parts", "Can't delete the assembly's grounded/root part.")
            return
        }

        const partInstances = pick.sceneObject.mirabufInstance.parser.assembly.data?.parts?.partInstances
        const name = partInstances?.[pick.guid]?.info?.name ?? pick.guid

        this.pendingDeletions.addPart(pick.guid, {
            sceneId: pick.sceneObject.id,
            guid: pick.guid,
            name,
            highlight: {
                instanceId: pick.instanceId,
                mesh: pick.object as THREE.BatchedMesh,
            },
        })
    }

    /** Pending deletions grouped by scene id, for a combined apply alongside other modes. */
    public collectPendingBySceneId(): Map<number, string[]> {
        const bySceneId = new Map<number, string[]>()
        for (const { sceneId, guid } of this.pendingDeletions.values()) {
            const list = bySceneId.get(sceneId)
            if (list) list.push(guid)
            else bySceneId.set(sceneId, [guid])
        }
        return bySceneId
    }

    /** Post-rebuild bookkeeping: the scene objects were rebuilt out from under the tracked
     *  highlights, so there's nothing left to un-tint -- clear silently and refresh the pick index. */
    public finishApply(): void {
        this.pendingDeletions.clearSilently()

        if (this._enabled) this.rebuildPickIndex()
    }
}

export default PartDeletionMode

type HighlightKey = `${number}-${number}`

function toKey(a: PartHighlight): HighlightKey
function toKey(a?: PartHighlight): HighlightKey | undefined
function toKey(a?: PartHighlight): HighlightKey | undefined {
    if (a == null) return undefined
    return `${a.mesh.id}-${a.instanceId}`
}

class HighlightMap {
    private _map: Map<string, PartDeletionSelection> = new Map()
    private _hoverMap: Set<HighlightKey> = new Set()
    public constructor(private _getHoverKey: () => HighlightKey | undefined) {}

    removePart(guid: string): void {
        const highlight = this._map.get(guid)?.highlight
        if (!highlight) return

        if (this._getHoverKey() !== toKey(highlight)) {
            highlight.mesh.setColorAt(highlight.instanceId, DEFAULT_INSTANCE_COLOR)
        }

        this._map.delete(guid)
        this._hoverMap.delete(toKey(highlight))
        this.dispatchUpdate()
    }

    hasPart(guid: string): boolean {
        return this._map.has(guid)
    }

    values() {
        return this._map.values()
    }

    hasHighlight(key: HighlightKey): boolean {
        return this._hoverMap.has(key)
    }

    addPart(guid: string, selection: PartDeletionSelection): void {
        this._map.set(guid, selection)

        const { highlight } = selection
        this._hoverMap.add(toKey(highlight))
        if (highlight && this._getHoverKey() !== toKey(highlight)) {
            highlight.mesh.setColorAt(highlight.instanceId, PENDING_DELETE_HIGHLIGHT_COLOR)
        }
        this.dispatchUpdate()
    }

    get size() {
        return this._map.size
    }

    clear() {
        this._map.forEach(({ highlight }) => {
            highlight.mesh.setColorAt(highlight.instanceId, DEFAULT_INSTANCE_COLOR)
        })
        this._map.clear()
        this.dispatchUpdate()
    }

    /** Drops tracking without touching mesh colors -- used after apply(), once the batches are already gone. */
    clearSilently() {
        this._map.clear()
        this._hoverMap.clear()
        this.dispatchUpdate()
    }

    dispatchUpdate() {
        EventSystem.dispatch("PartDeletionSelectionChanged", { parts: [...this._map.values()] })
    }
}
