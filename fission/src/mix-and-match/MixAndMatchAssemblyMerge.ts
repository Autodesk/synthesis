import * as THREE from "three"
import MirabufParser, { GROUNDED_JOINT_ID } from "@/mirabuf/MirabufParser"
import { mirabuf } from "@/proto/mirabuf"
import { convertArrayToThreeMatrix4, convertThreeMatrix4ToMirabufTransform } from "@/util/TypeConversions"
import { applyRelativeOffset, relativeOffsetBetween } from "./MixAndMatchPlacement"
import { subtreeOf, type TimelineState } from "./MixAndMatchTimeline"
import type { ComponentId } from "./MixAndMatchTypes"

/**
 * Combines the independent library-part assemblies of a finished build into one real
 * `mirabuf.Assembly` per weld tree - the shape the simulator loads like any other robot, not N
 * `MirabufSceneObject`s held together by runtime constraints.
 *
 * Pure data: every part instance's baked pose comes from the timeline's replayed `ComponentState`
 * transforms, not live Jolt bodies, so this has no dependency on physics or the scene and is
 * unit-testable against plain proto fixtures. The weld graph itself is only used for tree structure
 * (who's grafted onto whom) - not position, since a component's `transform` already reflects every
 * move applied after its weld.
 *
 * Assembly-level `physicalData` is left as whatever the tree's root already declared, and goes
 * stale the moment a second component is grafted in - a known gap. Per-part mass is unaffected:
 * `MirabufParser` sums `PartDefinition.physicalData`/`massOverride` fresh from whichever parts end
 * up in a RigidNode, so a merged assembly's bodies get correct mass the next time it's parsed.
 */

function namespaced(componentId: ComponentId, id: string): string {
    return `${componentId}:${id}`
}

function cloneAssembly(assembly: mirabuf.Assembly): mirabuf.Assembly {
    return mirabuf.Assembly.decode(mirabuf.Assembly.encode(assembly).finish())
}

/** The part instance every weld anchors to: the part `assembly`'s own grounded joint names. */
function groundedPartGuid(assembly: mirabuf.IAssembly): string {
    return assembly.data!.joints!.jointInstances![GROUNDED_JOINT_ID]!.parts!.nodes!.at(0)!.value!
}

/** Renames every node's `value` in a design/joint-hierarchy forest, recursively. */
function namespaceNodes(nodes: mirabuf.INode[] | null | undefined, componentId: ComponentId) {
    nodes?.forEach(node => {
        if (node.value) node.value = namespaced(componentId, node.value)
        namespaceNodes(node.children, componentId)
    })
}

/** Renames every key of a proto map field, and every value's own `info.GUID`, with `rewrite` fixing up any other outgoing references that value owns. */
function namespaceMap<T extends { info?: mirabuf.IInfo | null }>(
    map: { [key: string]: T } | null | undefined,
    componentId: ComponentId,
    rewrite?: (value: T) => void
): { [key: string]: T } {
    const renamed: { [key: string]: T } = {}
    Object.entries(map ?? {}).forEach(([key, value]) => {
        if (value.info?.GUID) value.info.GUID = namespaced(componentId, value.info.GUID)
        rewrite?.(value)
        renamed[namespaced(componentId, key)] = value
    })
    return renamed
}

/**
 * Rewrites every GUID a component's assembly owns so grafting it alongside other components -
 * including other spawns of the same library part - can never collide. Drops the component's own
 * "grounded" joint instance: once welded, its root is no longer a tree root in its own right.
 *
 * @returns The (now namespaced) GUID of the component's root part, and the occurrences its own
 *          root RigidGroup declared (or just itself, if it had none) - both needed to fold its root
 *          into the parent side's rigid group.
 */
function namespaceAssembly(
    assembly: mirabuf.Assembly,
    componentId: ComponentId
): { rootPartGuid: string; rootRigidGroupOccurrences: string[] } {
    const rootPartGuidOriginal = groundedPartGuid(assembly)
    const parts = assembly.data!.parts!
    const joints = assembly.data!.joints!
    const materials = assembly.data!.materials
    const signals = assembly.data!.signals

    delete joints.jointInstances![GROUNDED_JOINT_ID]

    const rigidGroups = joints.rigidGroups ?? []
    const rootGroupIndex = rigidGroups.findIndex(group => group.occurrences?.includes(rootPartGuidOriginal))
    const rootRigidGroupOccurrences = (
        rootGroupIndex >= 0 ? rigidGroups.splice(rootGroupIndex, 1)[0].occurrences! : [rootPartGuidOriginal]
    ).map(id => namespaced(componentId, id))
    rigidGroups.forEach(group => {
        group.occurrences = group.occurrences?.map(id => namespaced(componentId, id))
    })
    joints.rigidGroups = rigidGroups

    parts.partDefinitions = namespaceMap(parts.partDefinitions, componentId, def => {
        def.bodies?.forEach(body => {
            if (body.appearanceOverride) body.appearanceOverride = namespaced(componentId, body.appearanceOverride)
        })
    })
    parts.partInstances = namespaceMap(parts.partInstances, componentId, instance => {
        if (instance.partDefinitionReference)
            instance.partDefinitionReference = namespaced(componentId, instance.partDefinitionReference)
        if (instance.appearance) instance.appearance = namespaced(componentId, instance.appearance)
        if (instance.physicalMaterial) instance.physicalMaterial = namespaced(componentId, instance.physicalMaterial)
        instance.joints = instance.joints?.map(id => namespaced(componentId, id))
    })

    joints.motorDefinitions = namespaceMap(joints.motorDefinitions, componentId)
    joints.jointDefinitions = namespaceMap(joints.jointDefinitions, componentId, def => {
        if (def.motorReference) def.motorReference = namespaced(componentId, def.motorReference)
    })
    joints.jointInstances = namespaceMap(joints.jointInstances, componentId, instance => {
        if (instance.parentPart) instance.parentPart = namespaced(componentId, instance.parentPart)
        if (instance.childPart) instance.childPart = namespaced(componentId, instance.childPart)
        if (instance.jointReference) instance.jointReference = namespaced(componentId, instance.jointReference)
        if (instance.signalReference) instance.signalReference = namespaced(componentId, instance.signalReference)
        namespaceNodes(instance.parts?.nodes, componentId)
    })

    if (materials) {
        materials.physicalMaterials = namespaceMap(materials.physicalMaterials, componentId)
        materials.appearances = namespaceMap(materials.appearances, componentId)
    }
    if (signals) signals.signalMap = namespaceMap(signals.signalMap, componentId)

    namespaceNodes(assembly.designHierarchy?.nodes, componentId)

    return { rootPartGuid: namespaced(componentId, rootPartGuidOriginal), rootRigidGroupOccurrences }
}

/**
 * Rigidly repositions every part instance in `assembly` by `delta`, without touching internal joint
 * poses. Each top-level design-hierarchy entry's own transform already is that entry's effective
 * global transform (nothing premultiplies it), so patching just that one field propagates `delta` to
 * its whole subtree through the untouched, still-relative transforms underneath.
 */
function bakeTransforms(
    assembly: mirabuf.Assembly,
    globalTransforms: ReadonlyMap<string, THREE.Matrix4>,
    delta: THREE.Matrix4
) {
    assembly.designHierarchy?.nodes?.forEach(node => {
        const partInstance = node.value ? assembly.data!.parts!.partInstances![node.value] : undefined
        const currentGlobal = node.value ? globalTransforms.get(node.value) : undefined
        if (!partInstance || !currentGlobal) return

        partInstance.transform = convertThreeMatrix4ToMirabufTransform(delta.clone().multiply(currentGlobal))
    })
}

/** Adds `occurrences` to whichever RigidGroup already contains `anchorGuid`, or starts a new one. */
function foldIntoRigidGroup(rigidGroups: mirabuf.joint.IRigidGroup[], anchorGuid: string, occurrences: string[]) {
    const existing = rigidGroups.find(group => group.occurrences?.includes(anchorGuid))
    if (existing) {
        existing.occurrences = [...(existing.occurrences ?? []), ...occurrences]
        return
    }

    rigidGroups.push(
        mirabuf.joint.RigidGroup.create({ name: "mix-and-match-weld", occurrences: [anchorGuid, ...occurrences] })
    )
}

function mergeMap<T>(target: { [key: string]: T }, source: { [key: string]: T } | null | undefined) {
    Object.assign(target, source ?? {})
}

/** Grafts `child`'s data (already namespaced and pose-baked) onto `root`. */
function graft(root: mirabuf.Assembly, child: mirabuf.Assembly) {
    const rootData = root.data!
    const childData = child.data!

    rootData.parts!.partDefinitions ??= {}
    rootData.parts!.partInstances ??= {}
    mergeMap(rootData.parts!.partDefinitions, childData.parts?.partDefinitions)
    mergeMap(rootData.parts!.partInstances, childData.parts?.partInstances)

    rootData.joints!.jointDefinitions ??= {}
    rootData.joints!.jointInstances ??= {}
    rootData.joints!.motorDefinitions ??= {}
    mergeMap(rootData.joints!.jointDefinitions, childData.joints?.jointDefinitions)
    mergeMap(rootData.joints!.jointInstances, childData.joints?.jointInstances)
    mergeMap(rootData.joints!.motorDefinitions, childData.joints?.motorDefinitions)
    rootData.joints!.rigidGroups = [...(rootData.joints!.rigidGroups ?? []), ...(childData.joints?.rigidGroups ?? [])]

    if (childData.materials) {
        rootData.materials ??= mirabuf.material.Materials.create({})
        rootData.materials.physicalMaterials ??= {}
        rootData.materials.appearances ??= {}
        mergeMap(rootData.materials.physicalMaterials, childData.materials.physicalMaterials)
        mergeMap(rootData.materials.appearances, childData.materials.appearances)
    }
    if (childData.signals) {
        rootData.signals ??= mirabuf.signal.Signals.create({})
        rootData.signals.signalMap ??= {}
        mergeMap(rootData.signals.signalMap, childData.signals.signalMap)
    }

    root.designHierarchy ??= mirabuf.GraphContainer.create({})
    root.designHierarchy.nodes = [...(root.designHierarchy.nodes ?? []), ...(child.designHierarchy?.nodes ?? [])]
}

/**
 * Combines every component in one weld tree into a single `mirabuf.Assembly`. The tree's root keeps
 * its own coordinate frame unchanged - it becomes the merged assembly's frame - while every welded
 * component is namespaced and repositioned to its live transform relative to the root before being
 * grafted in.
 */
function mergeTree(
    rootId: ComponentId,
    state: TimelineState,
    assembliesByComponent: ReadonlyMap<ComponentId, mirabuf.Assembly>
): mirabuf.Assembly | undefined {
    const rootAssemblyOriginal = assembliesByComponent.get(rootId)
    if (!rootAssemblyOriginal) return undefined

    const merged = cloneAssembly(rootAssemblyOriginal)
    const rootAnchorGuid = groundedPartGuid(merged)
    const rootAnchorGlobal = new MirabufParser(cloneAssembly(rootAssemblyOriginal)).globalTransforms.get(
        rootAnchorGuid
    )!

    // Each component's live world transform (kept current by every "move" replay, including
    // snap-to-face) relative to the root's live world transform - not the weld's `relativeOffset`,
    // which is frozen at weld time and goes stale the moment the component is moved again.
    const rootTransform = convertArrayToThreeMatrix4(state.components.get(rootId)!.transform)
    const anchorGuidByComponent = new Map<ComponentId, string>([[rootId, rootAnchorGuid]])

    for (const componentId of subtreeOf(state.components, rootId)) {
        if (componentId === rootId) continue

        const componentState = state.components.get(componentId)
        const weld = componentState?.weld
        const childAssemblyOriginal = assembliesByComponent.get(componentId)
        const parentAnchorGuid = weld ? anchorGuidByComponent.get(weld.parentId) : undefined
        if (!weld || !componentState || !childAssemblyOriginal || !parentAnchorGuid) continue

        const childParser = new MirabufParser(cloneAssembly(childAssemblyOriginal))
        const childAnchorGuidOriginal = groundedPartGuid(childAssemblyOriginal)
        const childAnchorGlobalOriginal = childParser.globalTransforms.get(childAnchorGuidOriginal)!

        const childTransform = convertArrayToThreeMatrix4(componentState.transform)
        const relativeOffset = relativeOffsetBetween(rootTransform, childTransform)

        const newAnchorGlobal = applyRelativeOffset(rootAnchorGlobal, relativeOffset)
        const worldDelta = newAnchorGlobal.clone().multiply(childAnchorGlobalOriginal.clone().invert())

        const child = cloneAssembly(childAssemblyOriginal)
        bakeTransforms(child, childParser.globalTransforms, worldDelta)
        const { rootPartGuid, rootRigidGroupOccurrences } = namespaceAssembly(child, componentId)

        merged.data!.joints!.rigidGroups ??= []
        foldIntoRigidGroup(merged.data!.joints!.rigidGroups, parentAnchorGuid, rootRigidGroupOccurrences)
        graft(merged, child)

        anchorGuidByComponent.set(componentId, rootPartGuid)
    }

    return merged
}

/**
 * @returns One `mirabuf.Assembly` per weld tree in `state` - a build with no welds at all produces
 *          one assembly per placed component, same as today.
 */
export function mergeAssemblies(
    state: TimelineState,
    assembliesByComponent: ReadonlyMap<ComponentId, mirabuf.Assembly>
): mirabuf.Assembly[] {
    const rootIds = [...state.components.values()].filter(component => !component.weld).map(component => component.id)

    return rootIds
        .map(rootId => mergeTree(rootId, state, assembliesByComponent))
        .filter((assembly): assembly is mirabuf.Assembly => assembly != null)
}
