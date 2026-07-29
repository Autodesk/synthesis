import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"
import TransformGizmoControl from "@/components/TransformGizmoControl.tsx"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers.ts"
import { useEffect } from "react"
import World from "@/systems/World.ts"
import JOLT from "@/util/loading/JoltSyncLoader.ts"
import { PAUSE_REF_ASSEMBLY_MOVE } from "@/systems/physics/PhysicsTypes.ts"

const MoveInterface: ConfigurationSubpanelComponent = ({ selectedAssembly, panel, registerCleanupFunction }) => {
    const { closePanel } = useUIContext()

    // Keep physics paused while the gizmo is up, otherwise the assembly drifts
    // out from under the handles as it is being positioned.
    useEffect(() => {
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_MOVE)
        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_MOVE)
        }
    }, [])

    useEffect(() => {
        const scaleVec = new JOLT.Vec3()
        const initialTransform = selectedAssembly.getRootBody().GetWorldTransform().Decompose(scaleVec)
        JOLT.destroy(scaleVec)
        registerCleanupFunction(
            () => {
                JOLT.destroy(initialTransform)
            },
            () => {
                World.physicsSystem.setBodyPositionAndRotation(
                    selectedAssembly.getRootNodeId(),
                    initialTransform.GetTranslation(),
                    initialTransform.GetQuaternion(),
                    undefined,
                    false
                )
                JOLT.destroy(initialTransform)
            }
        )
    }, [registerCleanupFunction, selectedAssembly])
    return (
        <TransformGizmoControl
            key="config-move-gizmo"
            defaultMode="translate"
            scaleDisabled={true}
            size={3.0}
            parent={selectedAssembly}
            onAccept={() => closePanel(panel.id, CloseType.ACCEPT)}
            onCancel={() => closePanel(panel.id, CloseType.CANCEL)}
        />
    )
}

export default MoveInterface
