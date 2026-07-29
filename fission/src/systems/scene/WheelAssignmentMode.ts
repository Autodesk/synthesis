import * as THREE from "three"
import { GROUNDED_JOINT_ID } from "@/mirabuf/MirabufParser"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import {
    applyPodAssignments,
    applyWheelAssignments,
    type PodAssignment,
    type WheelAssignment,
} from "@/mirabuf/WheelJointBuilder"
import EventSystem from "@/systems/EventSystem.ts"
import { DriveType } from "@/systems/simulation/behavior/Behavior"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { pairNearestHinges } from "@/systems/simulation/synthesis_brain/SwervePairing"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import {
    computeWheelAxisFromAABB,
    computeWheelAxisFromCircleFit,
    transformWheelAxis,
} from "@/util/geometry/WheelAxisFit"
import World from "../World"
import WorldSystem from "../WorldSystem"
import { type InteractionStart, PRIMARY_MOUSE_INTERACTION } from "./ScreenInteractionHandler"

/** What kind of joint the next click will stage: a wheel (drive) or a swerve module pod (steer). */
export type WheelAssignmentPickTarget = "wheel" | "pod"

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

interface PendingWheelAssignment {
    sceneObject: MirabufSceneObject
    assignment: WheelAssignment
}

interface PendingPodAssignment {
    sceneObject: MirabufSceneObject
    assignment: PodAssignment
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

/**
 * Interaction mode: click a wheel's rim to fit a joint axis, using the assembly's grounded part as
 * parent, or (in "pod" mode) click a swerve module's rotating housing to stage a steering hinge.
 * At Apply, any staged wheels are paired to staged pods by nearest-neighbor (mirroring the runtime
 * `SwervePairing.pairNearestHinges`); wheels with no pods staged keep the plain arcade/tank parent.
 */
class WheelAssignmentMode extends WorldSystem {
    private _enabled = false
    private _pickTarget: WheelAssignmentPickTarget = "wheel"
    private _pendingWheels: PendingWheelAssignment[] = []
    private _pendingPods: PendingPodAssignment[] = []

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

    public get pickTarget(): WheelAssignmentPickTarget {
        return this._pickTarget
    }

    public set pickTarget(target: WheelAssignmentPickTarget) {
        this._pickTarget = target
    }

    public get wheelPendingCount(): number {
        return this._pendingWheels.length
    }

    public get podPendingCount(): number {
        return this._pendingPods.length
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

        if (this._pickTarget === "pod") this.handlePodPick(interaction.position)
        else this.handleWheelPick(interaction.position)
    }

    /** Grounded/root part's GUID -- doubles as the default parent for both wheel and pod picks. */
    private getGroundedPartGuid(sceneObject: MirabufSceneObject): string {
        const groundedInstance =
            sceneObject.mirabufInstance.parser.assembly.data!.joints!.jointInstances![GROUNDED_JOINT_ID]
        return groundedInstance.parts!.nodes!.at(0)!.value!
    }

    private dispatchPendingCount(): void {
        EventSystem.dispatch("WheelAssignmentPendingCountChanged", {
            wheelCount: this._pendingWheels.length,
            podCount: this._pendingPods.length,
        })
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

        // Assembly-space transform, not the live scene matrix (which bakes in the physics body's world transform).
        const assemblySpaceTransform = pick.sceneObject.mirabufInstance.parser.globalTransforms.get(pick.guid)!
        const worldAxisFit = transformWheelAxis(localAxisFit, assemblySpaceTransform)

        // Grounded/root part doubles as the parent by default -- reassigned to a pod at Apply time
        // if this scene object has any pod picks staged.
        const parentPartGuid = this.getGroundedPartGuid(pick.sceneObject)
        if (parentPartGuid === pick.guid) {
            globalAddToast("warning", "Wheel Assignment", "This part is the assembly's grounded/root part.")
            return
        }

        this._pendingWheels.push({
            sceneObject: pick.sceneObject,
            assignment: { wheelPartGuid: pick.guid, parentPartGuid, axisFit: worldAxisFit },
        })

        this.dispatchPendingCount()
        globalAddToast(
            "success",
            "Wheel Assignment",
            `Wheel staged (${this._pendingWheels.length} pending). Pick the next wheel, or Apply.`
        )
    }

    /** Stages a swerve module pod: an untagged vertical hinge will be created from chassis to this part. */
    private handlePodPick(mousePos: [number, number]): void {
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

        // Pivot origin only -- the steering axis is always assembly-space up, matching the
        // spawn-upright assumption SynthesisBrain.detectSwerve() already relies on.
        const localCenter = new THREE.Box3().setFromPoints(points).getCenter(new THREE.Vector3())
        const assemblySpaceTransform = pick.sceneObject.mirabufInstance.parser.globalTransforms.get(pick.guid)!
        const worldCenter = localCenter.clone().applyMatrix4(assemblySpaceTransform)

        const parentPartGuid = this.getGroundedPartGuid(pick.sceneObject)
        if (parentPartGuid === pick.guid) {
            globalAddToast("warning", "Wheel Assignment", "This part is the assembly's grounded/root part.")
            return
        }

        this._pendingPods.push({
            sceneObject: pick.sceneObject,
            assignment: {
                podPartGuid: pick.guid,
                parentPartGuid,
                origin: { x: worldCenter.x, y: worldCenter.y, z: worldCenter.z },
            },
        })

        this.dispatchPendingCount()
        globalAddToast(
            "success",
            "Wheel Assignment",
            `Pod staged (${this._pendingPods.length} pending). Pick the next pod, or a wheel.`
        )
    }

    /** Mutates each affected assembly and fully rebuilds its MirabufSceneObject. */
    public async apply(): Promise<void> {
        if (this._pendingWheels.length === 0 && this._pendingPods.length === 0) return

        // Clear before rebuild destroys the hovered mesh's batches.
        this.clearHover()

        const sceneObjects = new Set<MirabufSceneObject>([
            ...this._pendingWheels.map(p => p.sceneObject),
            ...this._pendingPods.map(p => p.sceneObject),
        ])

        for (const sceneObject of sceneObjects) {
            const wheelPicks = this._pendingWheels.filter(p => p.sceneObject === sceneObject)
            const podPicks = this._pendingPods.filter(p => p.sceneObject === sceneObject)

            // Pair staged wheels to staged pods by nearest-neighbor, same algorithm the runtime
            // uses to pair WheelDriver/HingeDriver pairs (SwervePairing.pairNearestHinges). Wheels
            // with no pods staged for this scene object keep their plain arcade/tank parent.
            const wheelAssignments: WheelAssignment[] = wheelPicks.map(p => ({ ...p.assignment }))
            if (podPicks.length > 0) {
                const wheelCenters = wheelPicks.map(p => p.assignment.axisFit.center)
                const podOrigins = podPicks.map(p => p.assignment.origin)
                const pairing = pairNearestHinges(wheelCenters, podOrigins)
                pairing.forEach((podIndex, wheelIndex) => {
                    if (podIndex === -1) return
                    wheelAssignments[wheelIndex].parentPartGuid = podPicks[podIndex].assignment.podPartGuid
                })
            }
            const podAssignments: PodAssignment[] = podPicks.map(p => p.assignment)

            const assembly = sceneObject.mirabufInstance.parser.assembly
            applyPodAssignments(assembly, podAssignments)
            applyWheelAssignments(assembly, wheelAssignments)

            const priorDriveType =
                sceneObject.brain instanceof SynthesisBrain ? sceneObject.brain.driveType : undefined

            const sceneId = sceneObject.id
            World.sceneRenderer.removeSceneObject(sceneId)

            const rebuilt = await createMirabuf(assembly.info!.GUID!, assembly)
            if (!rebuilt) {
                globalAddToast("error", "Wheel Assignment", "Failed to rebuild assembly after applying wheel joints.")
                continue
            }
            World.sceneRenderer.registerSceneObject(rebuilt, sceneId)

            // A fresh SynthesisBrain always constructs with DriveType.ARCADE; restore what the
            // robot had before, or switch it to Swerve outright if modules were just marked.
            if (rebuilt.brain instanceof SynthesisBrain) {
                rebuilt.brain.configureDriveBehavior(
                    podAssignments.length > 0 ? DriveType.SWERVE : (priorDriveType ?? DriveType.ARCADE)
                )
            }

            const parser = rebuilt.mirabufInstance.parser

            let hadMismatch = false
            const partGuidPairs = [
                ...wheelAssignments.map(a => [a.wheelPartGuid, a.parentPartGuid]),
                ...podAssignments.map(a => [a.podPartGuid, a.parentPartGuid]),
            ]
            for (const [childGuid, parentGuid] of partGuidPairs) {
                const childNode = parser.partToNodeMap.get(childGuid)
                const parentNode = parser.partToNodeMap.get(parentGuid)
                if (!childNode || !parentNode) continue
                if (childNode.id !== parentNode.id) continue
                hadMismatch = true
            }

            if (hadMismatch) {
                globalAddToast("warning", "Wheel Assignment", "A marked part ended up in the same rigid node as its parent.")
            }
        }

        this._pendingWheels = []
        this._pendingPods = []
        this.dispatchPendingCount()
        globalAddToast("success", "Wheel Assignment", "Applied joints and rebuilt the affected assembly.")

        // Rebuilt assemblies got new batches/instance ids; refresh the stale pick index.
        if (this._enabled) this.rebuildPickIndex()
    }
}

export default WheelAssignmentMode
