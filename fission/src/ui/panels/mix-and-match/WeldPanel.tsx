import { Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import type * as THREE from "three"
import MixAndMatchMode from "@/mix-and-match/MixAndMatchMode"
import type { ComponentId } from "@/mix-and-match/MixAndMatchTypes"
import EventSystem from "@/systems/EventSystem"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import { Button } from "@/ui/components/StyledComponents"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { rayCastMesh } from "@/util/RaycastUtils"

const WeldPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const [, bumpRevision] = useState(0)
    const [armed, setArmed] = useState(false)
    const [target, setTarget] = useState<ComponentId | undefined>(undefined)

    useEffect(() => EventSystem.listen("MixAndMatchStateChangedEvent", () => bumpRevision(x => x + 1)), [])

    const selected = MixAndMatchMode.selected

    // The child being welded is whatever's selected in the assembly tree; a target picked against
    // the old part means nothing once that changes.
    useEffect(() => {
        setArmed(false)
        setTarget(undefined)
    }, [selected])

    useEffect(() => {
        configureScreen(
            panel!,
            { title: "Weld", disableAccept: !target },
            {
                onBeforeAccept: () => {
                    if (selected && target) MixAndMatchMode.weld(target, selected).catch(console.error)
                },
            }
        )
    }, [configureScreen, panel, selected, target])

    // A click while armed picks the part to weld onto: any component other than the one being
    // welded.
    useEffect(() => {
        if (!armed || !selected) return

        const onClick = (e: MouseEvent) => {
            const components = MixAndMatchMode.scene?.components
            if (!components) return

            const batchToComponent = new Map<THREE.Object3D, ComponentId>()
            const objects: THREE.Object3D[] = []
            components.forEach((component, componentId) => {
                component.mirabufInstance.batches.forEach(batch => {
                    batchToComponent.set(batch, componentId)
                    objects.push(batch)
                })
            })

            const hit = rayCastMesh([e.clientX, e.clientY], objects)
            const componentId = hit && batchToComponent.get(hit.object)
            if (!componentId || componentId === selected) return

            setTarget(componentId)
            setArmed(false)
        }

        World.sceneRenderer.renderer.domElement.addEventListener("click", onClick)
        return () => World.sceneRenderer.renderer.domElement.removeEventListener("click", onClick)
    }, [armed, selected])

    if (!selected) {
        return <Label size="sm">Select a part in the assembly tree first.</Label>
    }

    return (
        <Stack direction="column" gap={1.5} minWidth="18rem">
            <Label size="sm">A part has one weld at a time. Welding it again replaces the old one.</Label>
            <Stack direction="row" gap={1} alignItems="center" justifyContent="space-between">
                <Label size="sm">{target ? `Attach to ${target}` : "Pick the part to attach to"}</Label>
                <Button size="small" variant={armed ? "contained" : "outlined"} onClick={() => setArmed(prev => !prev)}>
                    {armed ? "Click a part…" : target ? "Re-pick" : "Select"}
                </Button>
            </Stack>
        </Stack>
    )
}

export default WeldPanel
