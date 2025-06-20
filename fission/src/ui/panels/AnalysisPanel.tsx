import React, { useCallback, useEffect, useState } from "react"
import Panel, { PanelPropsImpl } from "@/components/Panel"

import { SynthesisIcons } from "../components/StyledComponents"
import Checkbox from "@/components/Checkbox.tsx"
import { Box } from "@mui/system"
import World from "@/systems/World.ts"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"
import { MiraType } from "@/mirabuf/MirabufLoader.ts"
import { MirabufObjectCreatedEvent } from "@/components/ObjectCreatedEvents.ts"

const AnalysisToolPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const [cogDisplayActive, setCogDisplayActive] = useState<boolean>(false)
    const updateIndicatorVisibility = useCallback((active: boolean) => {
        World.SceneRenderer.sceneObjects.forEach(obj => {
            if (obj instanceof MirabufSceneObject && obj.miraType == MiraType.ROBOT) {
                obj.SetCenterOfMassIndicatorVisible(active)
            }
        })
    }, [])
    useEffect(() => {
        updateIndicatorVisibility(cogDisplayActive)
        return () => {
            updateIndicatorVisibility(false)
        }
    }, [cogDisplayActive, updateIndicatorVisibility])
    MirabufObjectCreatedEvent.Listen(() => {
        updateIndicatorVisibility(cogDisplayActive)
    })

    return (
        <Panel
            name={"Analysis"}
            icon={SynthesisIcons.MagnifyingGlass}
            panelId={panelId}
            openLocation={"bottom-left"}
            acceptName={"Close"}
            cancelEnabled={false}
        >
            <Box display="flex" flexDirection={"column"}>
                <Checkbox
                    label="Display Centers of Gravity"
                    defaultState={cogDisplayActive}
                    onClick={checked => {
                        setCogDisplayActive(checked)
                    }}
                    tooltipText="Displays the center of gravity of all robots"
                />
            </Box>
        </Panel>
    )
}

export default AnalysisToolPanel
