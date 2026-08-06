import * as THREE from "three"
import { GROUNDED_JOINT_ID } from "@/mirabuf/MirabufParser"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "../World"
import WorldSystem from "../WorldSystem"
import { type InteractionEnd, type InteractionStart, PRIMARY_MOUSE_INTERACTION } from "./ScreenInteractionHandler"

export interface PartPick {
    guid: string
    object: THREE.BatchedMesh
    instanceId: number
}

export interface PartHighlight {
    mesh: THREE.BatchedMesh
    instanceId: number
}

/** Common shape a mode's pending-selection value must have to be trackable by HighlightMap/PartPickingMode. */
export interface PartSelection {
    guid: string
    highlight: PartHighlight
}

/** Tint for the part under the cursor. */
export const HOVER_HIGHLIGHT_COLOR = new THREE.Color(1, 1, 0.1)
/** Default BatchedMesh instance color; used to un-tint. */
export const DEFAULT_INSTANCE_COLOR = new THREE.Color(1, 1, 1)

type HighlightKey = `${number}-${number}`

export function toKey(a: PartHighlight): HighlightKey
export function toKey(a?: PartHighlight): HighlightKey | undefined
export function toKey(a?: PartHighlight): HighlightKey | undefined {
    if (a == null) return undefined
    return `${a.mesh.id}-${a.instanceId}`
}

/** Reused across picks to avoid reallocating. */
const raycaster = new THREE.Raycaster()
const ndc = new THREE.Vector2()

type HighlightStyler = (isSelected: boolean, highlight: PartHighlight) => void

/** Tracks a set of picked parts and tints their meshes; a hovered pending part keeps `selectedColor`. */
export class HighlightMap<T extends PartSelection> {
    private _map = new Map<string, T>()
    private _highlightKeys = new Set<HighlightKey>()

    public constructor(
        public readonly styler: HighlightStyler,
        private _getHoverKey: () => HighlightKey | undefined,
        private _dispatchUpdate: (values: T[]) => void
    ) {}

    removePart(guid: string): void {
        const highlight = this._map.get(guid)?.highlight
        if (!highlight) return

        if (this._getHoverKey() !== toKey(highlight)) {
            this.styler(false, highlight)
        }

        this._map.delete(guid)
        this._highlightKeys.delete(toKey(highlight))
        this.dispatchUpdate()
    }

    hasPart(guid: string): boolean {
        return this._map.has(guid)
    }

    values() {
        return this._map.values()
    }

    hasHighlight(key: HighlightKey): boolean {
        return this._highlightKeys.has(key)
    }

    addPart(guid: string, selection: T): void {
        this._map.set(guid, selection)

        const { highlight } = selection
        this._highlightKeys.add(toKey(highlight))
        if (highlight) {
            this.styler(true, highlight)
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

    /** Drops tracking without touching mesh colors -- used once the batches are already gone (e.g. post-rebuild). */
    clearSilently() {
        this._map.clear()
        this._highlightKeys.clear()
        this.dispatchUpdate()
    }

    dispatchUpdate() {
        this._dispatchUpdate([...this._map.values()])
    }
}

/** Interaction mode that raycasts scene parts under the cursor and tracks a pending pick set. */
abstract class PartPickingMode<T extends PartSelection> extends WorldSystem {
    public readonly pending: HighlightMap<T>

    protected _object: MirabufSceneObject | null = null

    private _originalInteractionStart: ((i: InteractionStart) => void) | undefined
    private _originalInteractionEnd: ((i: InteractionEnd) => void) | undefined
    private _interactionStartTarget: PartPick | undefined

    private _pointerMoveListener: ((e: PointerEvent) => void) | undefined
    private _hover: PartHighlight | undefined
    private _latestMousePos: [number, number] | undefined
    private _lastProcessedMousePos: [number, number] | undefined

    // Rebuilt on enable and after finishApply() to avoid rescanning every mesh entry per raycast.
    private _candidateBatches: THREE.BatchedMesh[] = []
    private _pickIndex = new Map<THREE.BatchedMesh, Map<number, string>>()

    protected constructor(styler: HighlightStyler, dispatchUpdate: (values: T[]) => void) {
        super()
        this.pending = new HighlightMap<T>(styler, () => toKey(this._hover), dispatchUpdate)
    }

    public get enabled() {
        return this._object != null
    }

    public enable(object: MirabufSceneObject) {
        if (this.enabled) return
        this._object = object
        this.hookInteractionHandlers()
    }

    public disable() {
        if (!this.enabled) return
        this._object = null
        this.unhookInteractionHandlers()
    }

    public get pendingCount(): number {
        return this.pending.size
    }

    public update(_deltaT: number): void {
        if (!this.enabled || !this._latestMousePos) return

        const [x, y] = this._latestMousePos
        const last = this._lastProcessedMousePos
        if (last && last[0] === x && last[1] === y) return

        this._lastProcessedMousePos = this._latestMousePos
        this.updateHover(this._latestMousePos)
    }

    public destroy(): void {
        this.disable()
    }

    private hookInteractionHandlers(): void {
        const screenHandler = World.sceneRenderer.screenInteractionHandler
        this._originalInteractionEnd = screenHandler.interactionEnd
        this._originalInteractionStart = screenHandler.interactionStart
        screenHandler.interactionStart = (interaction: InteractionStart) => this.onInteractionStart(interaction)
        screenHandler.interactionEnd = (interaction: InteractionEnd) => this.onInteractionEnd(interaction)

        this._pointerMoveListener = (e: PointerEvent) => {
            this._latestMousePos = [e.clientX, e.clientY]
        }
        World.sceneRenderer.renderer.domElement.addEventListener("pointermove", this._pointerMoveListener)

        this.rebuildPickIndex()
    }

    private unhookInteractionHandlers(): void {
        const screenHandler = World.sceneRenderer.screenInteractionHandler
        if (this._originalInteractionStart) screenHandler.interactionStart = this._originalInteractionStart
        if (this._originalInteractionEnd) screenHandler.interactionEnd = this._originalInteractionEnd

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
        if (this._object == null) return
        for (const batch of this._object.mirabufInstance.batches) {
            this._candidateBatches.push(batch)
        }

        for (const [guid, entries] of this._object.mirabufInstance.meshes) {
            for (const [mesh, instanceId] of entries) {
                let byInstance = this._pickIndex.get(mesh)
                if (!byInstance) {
                    byInstance = new Map()
                    this._pickIndex.set(mesh, byInstance)
                }
                byInstance.set(instanceId, guid)
            }
        }
    }

    /** Raycasts the cached candidate batches for the part-instance GUID under the mouse. */
    protected pickPart(mousePos: [number, number]): PartPick | undefined {
        const camera = World.sceneRenderer.mainCamera
        ndc.set((mousePos[0] / window.innerWidth) * 2 - 1, -(mousePos[1] / window.innerHeight) * 2 + 1)
        raycaster.setFromCamera(ndc, camera)

        const hits = raycaster.intersectObjects<THREE.BatchedMesh>(this._candidateBatches, false)
        if (hits.length === 0) return undefined

        const hit = hits[0]
        const object = hit.object
        const instanceId = hit.batchId ?? 0

        const guid = this._pickIndex.get(object)?.get(instanceId)
        if (!guid) return undefined

        return { guid, object, instanceId }
    }

    /** Tints the part under the cursor. */
    private updateHover(mousePos: [number, number]): void {
        const pick = this.pickPart(mousePos)
        if (!pick) {
            this.clearHover()
            return
        }

        const mesh = pick.object
        this.setHover({ mesh, instanceId: pick.instanceId })
    }

    public setHover(hover: PartHighlight) {
        if (toKey(this._hover) === toKey(hover)) return

        this.clearHover()
        this.pending.styler(false, hover)
        hover.mesh.setColorAt(hover.instanceId, HOVER_HIGHLIGHT_COLOR)
        this._hover = hover
    }

    public clearHover(): void {
        if (!this._hover) return
        this._hover.mesh.setColorAt(this._hover.instanceId, DEFAULT_INSTANCE_COLOR)

        const isPending = this.pending.hasHighlight(toKey(this._hover))
        this.pending.styler(isPending, this._hover)

        this._hover = undefined
    }

    private onInteractionStart(interaction: InteractionStart): void {
        if (interaction.interactionType !== PRIMARY_MOUSE_INTERACTION) {
            this._originalInteractionStart?.(interaction)
            return
        }

        const pick = this.pickPart(interaction.position)
        if (!pick) {
            this._originalInteractionStart?.(interaction)
            return
        }

        this._interactionStartTarget = pick
    }

    private onInteractionEnd(interaction: InteractionEnd): void {
        if (interaction.interactionType !== PRIMARY_MOUSE_INTERACTION) {
            this._originalInteractionEnd?.(interaction)
            return
        }

        if (!this._interactionStartTarget) {
            this._originalInteractionEnd?.(interaction)
            return
        }

        const pick = this.pickPart(interaction.position)
        if (!pick || pick.guid != this._interactionStartTarget.guid) {
            this._originalInteractionEnd?.(interaction)
            return
        }

        if (this.pending.hasPart(pick.guid)) {
            this.pending.removePart(pick.guid)
            return
        }

        this.handlePick(pick)
        this.updateHover(interaction.position)
    }

    protected abstract handlePick(pick: PartPick): void

    protected getGroundedRootPartGuid(sceneObject: MirabufSceneObject): string {
        const groundedInstance =
            sceneObject.mirabufInstance.parser.assembly.data!.joints!.jointInstances![GROUNDED_JOINT_ID]
        return groundedInstance.parts!.nodes!.at(0)!.value!
    }

    public get pendingList(): T[] {
        return [...this.pending.values()]
    }

    public finishApply(): void {
        this.pending.clearSilently()
        if (this.enabled) this.rebuildPickIndex()
    }

    public get sceneObject() {
        return this._object
    }

    protected getName(guid: string) {
        const partInstances = this._object?.mirabufInstance.parser.assembly.data?.parts?.partInstances
        return partInstances?.[guid]?.info?.name ?? guid
    }
}

export default PartPickingMode
