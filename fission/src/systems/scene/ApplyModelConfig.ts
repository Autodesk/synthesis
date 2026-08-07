import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
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

    const sceneObject = World.wheelAssignmentMode.sceneObject ?? World.partDeletionMode.sceneObject
    if (sceneObject == null) {
        console.warn("Missing part handler", World.wheelAssignmentMode.sceneObject, World.partDeletionMode.sceneObject)
        return false
    }
    const sceneId = sceneObject.id

    const assembly = sceneObject.mirabufInstance.parser.assembly

    if (assignments) applyWheelAssignments(assembly, assignments)
    if (deletions) applyPartDeletions(assembly, deletions)

    World.sceneRenderer.removeSceneObject(sceneId)

    const rebuilt = await createMirabuf(assembly.info!.GUID!, assembly)
    if (!rebuilt) {
        console.warn("Create mirabuf failed")
        return false
    }
    World.sceneRenderer.registerSceneObject(rebuilt, sceneId)

    wheelMode.isMismatched(assignments, rebuilt)
    wheelMode.finishApply()
    deleteMode.finishApply()

    return true
}
