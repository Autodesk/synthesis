import * as THREE from "three"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { applyWheelAssignments, type WheelAssignment } from "@/mirabuf/WheelJointBuilder"
import { mirabuf } from "@/proto/mirabuf"
import EventSystem from "@/systems/EventSystem.ts"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import { dumpAssemblyStructure } from "@/util/DebugAssemblyDump"
import {
    computeWheelAxisFromAABB,
    computeWheelAxisFromCircleFit,
    transformWheelAxis,
    type WheelAxis,
} from "@/util/geometry/WheelAxisFit"
import World from "../World"
import WorldSystem from "../WorldSystem"
import { type InteractionStart, PRIMARY_MOUSE_INTERACTION } from "./ScreenInteractionHandler"

/** Dumps an assembly's joints container (the part this feature actually mutates) as loggable JSON. */
function dumpAssemblyJoints(assembly: mirabuf.Assembly, label: string): void {
    const joints = mirabuf.joint.Joints.toObject(assembly.data!.joints as mirabuf.joint.Joints, {
        longs: String,
        enums: String,
        bytes: String,
    })
    console.log(`[WheelAssignmentMode] ${label} -- assembly.data.joints:`, joints)
    console.log(`[WheelAssignmentMode] ${label} -- assembly.data.joints (JSON):`, JSON.stringify(joints))
}

/**
 * Dumps the reconstructed assembly's structure under a label distinct from createMirabuf's generic
 * `[MirabufImport]` dump -- same assembly name as a plain import of the same robot, so a plain label
 * would be ambiguous about which log entry is the manually-wheel-jointed one.
 */
function dumpFullAssemblyAfterWheelAssignment(assembly: mirabuf.Assembly): void {
    dumpAssemblyStructure(assembly, "[WheelAssignmentMode] after reconstruction (with new wheel joints)")
}

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
            console.debug(
                `[WheelAssignmentMode] guid=${pick.guid} instanceId=${pick.instanceId} -- couldn't read geometry (${points?.length ?? 0} points)`
            )
            globalAddToast("warning", "Wheel Assignment", "Couldn't read this part's geometry.")
            return
        }

        const localAxisFit = computeWheelAxisFromCircleFit(points) ?? computeWheelAxisFromAABB(points)
        if (!localAxisFit) {
            globalAddToast("warning", "Wheel Assignment", "Couldn't derive a wheel axis from this part's geometry.")
            return
        }
        const baseline = computeWheelAxisFromAABB(points)
        console.debug(
            `[WheelAssignmentMode] guid=${pick.guid} -- axis=(${localAxisFit.axis.x.toFixed(3)}, ${localAxisFit.axis.y.toFixed(3)}, ${localAxisFit.axis.z.toFixed(3)}) ` +
                `center=(${localAxisFit.center.x.toFixed(4)}, ${localAxisFit.center.y.toFixed(4)}, ${localAxisFit.center.z.toFixed(4)}) ` +
                `[AABB-baseline center=(${baseline?.center.x.toFixed(4)}, ${baseline?.center.y.toFixed(4)}, ${baseline?.center.z.toFixed(4)})]`
        )

        const matrixWorld = getInstanceWorldMatrix(pick.object, pick.instanceId)
        const worldAxisFit = transformWheelAxis(localAxisFit, matrixWorld)

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
            dumpAssemblyJoints(assembly, "before applyWheelAssignments")

            applyWheelAssignments(assembly, assignments)
            dumpAssemblyJoints(assembly, "after applyWheelAssignments")

            const sceneId = sceneObject.id
            World.sceneRenderer.removeSceneObject(sceneId)

            const rebuilt = await createMirabuf(assembly)
            if (!rebuilt) {
                globalAddToast("error", "Wheel Assignment", "Failed to rebuild assembly after applying wheel joints.")
                continue
            }
            dumpFullAssemblyAfterWheelAssignment(rebuilt.mirabufInstance.parser.assembly)
            World.sceneRenderer.registerSceneObject(rebuilt, sceneId)

            const parser = rebuilt.mirabufInstance.parser
            if (parser.errors.length > 0) {
                console.warn(`[WheelAssignmentMode] Parser reported errors after rebuild:`, parser.errors)
            }
            for (const assignment of assignments) {
                const wheelNode = parser.partToNodeMap.get(assignment.wheelPartGuid)
                const parentNode = parser.partToNodeMap.get(assignment.parentPartGuid)
                console.log(
                    `[WheelAssignmentMode] wheel='${assignment.wheelPartGuid}' -> rigidNode=${wheelNode?.id ?? "MISSING"} (${wheelNode?.parts.size ?? 0} parts: ${wheelNode ? [...wheelNode.parts].join(", ") : "n/a"})`
                )
                console.log(
                    `[WheelAssignmentMode] parent='${assignment.parentPartGuid}' -> rigidNode=${parentNode?.id ?? "MISSING"} (${parentNode?.parts.size ?? 0} parts: ${parentNode ? [...parentNode.parts].join(", ") : "n/a"})`
                )
                if (!wheelNode || !parentNode) {
                    console.error(
                        `[WheelAssignmentMode] Couldn't find a rigid node for the wheel and/or parent part after rebuild -- ` +
                            `createJointsFromParser will skip this joint ("Couldn't find associated rigid nodes.").`
                    )
                } else if (wheelNode.id === parentNode.id) {
                    console.error(
                        `[WheelAssignmentMode] Wheel and parent ended up in the SAME rigid node (${wheelNode.id}) -- ` +
                            `PhysicsSystem.createJointsFromParser will silently skip this joint ("Jointing the same parts"), ` +
                            `so no wheel constraint gets created. This means a RigidGroup (or the default ancestral round-up) ` +
                            `is still bandaging the wheel occurrence to the chassis; check the RigidGroup[] logs above from ` +
                            `WheelJointBuilder for other occurrences that share a group with this wheel/parent pair.`
                    )
                }
            }
        }

        this._pending = []
        EventSystem.dispatch("WheelAssignmentPendingCountChanged", { count: 0 })
        globalAddToast("success", "Wheel Assignment", "Applied wheel joints and rebuilt the affected assembly.")
    }
}

function getInstanceWorldMatrix(object: THREE.Object3D, instanceId: number): THREE.Matrix4 {
    const batched = object as THREE.Object3D & {
        getMatrixAt?: (id: number, target: THREE.Matrix4) => THREE.Matrix4
    }
    if (typeof batched.getMatrixAt === "function") {
        const localMatrix = new THREE.Matrix4()
        batched.getMatrixAt(instanceId, localMatrix)
        return object.matrixWorld.clone().multiply(localMatrix)
    }
    return object.matrixWorld
}

export default WheelAssignmentMode
