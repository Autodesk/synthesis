import * as THREE from "three"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { WheelAssignment } from "@/mirabuf/WheelJointBuilder"
import EventSystem from "@/systems/EventSystem.ts"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import {
    computeWheelAxisFromAABB,
    computeWheelAxisFromCircleFit,
    transformWheelAxis,
} from "@/util/geometry/WheelAxisFit"
import World from "../World"
import PartPickingMode, { type HighlightMap, type PartPick, type PartSelection } from "./PartPickingMode"

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

export interface WheelSelection extends PartSelection {
    assignment: WheelAssignment
}

/** Tint for selected parts */
const SELECTED_HIGHLIGHT_COLOR = new THREE.Color(0, 1, 0)

/** Interaction mode: click a wheel's rim to fit a joint axis, using the assembly's grounded part as parent. */
class WheelAssignmentMode extends PartPickingMode<WheelSelection> {
    private _driveReversed = false

    public constructor() {
        super(SELECTED_HIGHLIGHT_COLOR, wheels => EventSystem.dispatch("WheelAssignmentSelectionChanged", { wheels }))
    }

    public get pendingWheels(): HighlightMap<WheelSelection> {
        return this.pending
    }

    public get driveReversed(): boolean {
        return this._driveReversed
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

    protected handlePick(pick: PartPick): void {
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

        // Grounded/root part doubles as the parent -- no second click needed.
        const parentPartGuid = this.getGroundedRootPartGuid(pick.sceneObject)
        if (parentPartGuid === pick.guid) {
            globalAddToast("warning", "Wheel Assignment", "This part is the assembly's grounded/root part.")
            return
        }

        this.pending.addPart(pick.guid, {
            sceneId: pick.sceneObject.id,
            guid: pick.guid,
            highlight: { instanceId: pick.instanceId, mesh: pick.object as THREE.BatchedMesh },
            assignment: { wheelPartGuid: pick.guid, parentPartGuid, axisFit: worldAxisFit },
        })
    }

    /** Warns if any pending assignment's wheel and parent ended up in the same rigid node post-rebuild. */
    public warnIfMismatched(
        assignmentsBySceneId: Map<number, WheelAssignment[]>,
        rebuiltBySceneId: Map<number, MirabufSceneObject>
    ): void {
        let hadMismatch = false
        for (const [sceneId, assignments] of assignmentsBySceneId) {
            const parser = rebuiltBySceneId.get(sceneId)?.mirabufInstance.parser
            if (!parser) continue

            for (const assignment of assignments) {
                const wheelNode = parser.partToNodeMap.get(assignment.wheelPartGuid)
                const parentNode = parser.partToNodeMap.get(assignment.parentPartGuid)
                if (!wheelNode || !parentNode) continue
                if (wheelNode.id !== parentNode.id) continue
                hadMismatch = true
            }
        }

        if (hadMismatch) {
            globalAddToast("warning", "Wheel Assignment", "Wheel and parent ended up in the same rigid node.")
        }
    }
}

export default WheelAssignmentMode
