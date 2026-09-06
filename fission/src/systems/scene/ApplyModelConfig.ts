import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { applyPartDeletions } from "@/mirabuf/PartDeletionBuilder"
import { applyWheelAssignments } from "@/mirabuf/WheelJointBuilder"
import World from "../World"

/**
 * Applies pending wheel assignments and pending part deletions for every affected scene object in a
 * single pass, all within one teardown and build.
 */
export async function applyModelConfigChanges(): Promise<boolean> {
    const wheelMode = World.wheelAssignmentMode
    const deleteMode = World.partDeletionMode

    const assignments = wheelMode.pendingList.map(selection => selection.assignment)
    const deletions = deleteMode.pendingList.map(selection => selection.guid)
    if (assignments.length === 0 && deletions.length === 0) return false

    wheelMode.clearHover()
    deleteMode.clearHover()

    const sceneObject = wheelMode.sceneObject ?? deleteMode.sceneObject
    if (sceneObject == null) {
        console.warn("Missing part handler")
        return false
    }

    let rebuilt: MirabufSceneObject | undefined
    try {
        const sceneId = sceneObject.id
        const assembly = sceneObject.mirabufInstance.parser.assembly

        if (assignments.length > 0) applyWheelAssignments(assembly, assignments)
        if (deletions.length > 0) applyPartDeletions(assembly, deletions)

        World.sceneRenderer.removeSceneObject(sceneId)

        rebuilt = await createMirabuf(assembly.info!.GUID!, assembly)
        if (!rebuilt) {
            console.warn("Create mirabuf failed")
            return false
        }
        World.sceneRenderer.registerSceneObject(rebuilt, sceneId)

        wheelMode.isMismatched(assignments, rebuilt)
        return true
    } catch (error) {
        console.error("Applying model config failed", error)
        return false
    } finally {
        // Never leave selections pointing at batches that a rebuild disposed.
        const current = World.sceneRenderer.sceneObjects.get(sceneObject.id)
        const liveObject = rebuilt ?? (current === sceneObject ? sceneObject : undefined)
        if (liveObject) {
            wheelMode.finishApply(liveObject)
            deleteMode.finishApply(liveObject)
        } else {
            wheelMode.cancel()
            deleteMode.cancel()
        }
    }
}
