import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import Label, { LabelSize } from "@/ui/components/Label"
import Button from "@/ui/components/Button"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import { useMemo, useState } from "react"
import { FaGear } from "react-icons/fa6"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import { Divider, styled } from "@mui/material"

// eslint-disable-next-line react-refresh/only-export-components
export enum ConfigureRobotBrainTypes {
    SYNTHESIS = 0,
    WIPLIB = 1,
}

const LabelStyled = styled(Label)({
    fontWeight: 700,
    margin: "0pt",
})

const DividerStyled = styled(Divider)({
    borderColor: "white",
})

const ConfigureRobotBrainPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const [selectedRobot, setSelectedRobot] = useState<MirabufSceneObject | undefined>(undefined)
    const [viewType, setViewType] = useState<ConfigureRobotBrainTypes>(ConfigureRobotBrainTypes.SYNTHESIS)
    const robots = useMemo(() => {
        const assemblies = [...World.SceneRenderer.sceneObjects.values()].filter(x => {
            if (x instanceof MirabufSceneObject) {
                return x.miraType === MiraType.ROBOT
            }
            return false
        }) as MirabufSceneObject[]
        return assemblies
    }, [])

    return (
        <Panel
            name="Configure Robot Brain"
            icon={<FaGear />}
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            onAccept={() => {}}
            onCancel={() => {}}
        >
            {selectedRobot?.ejectorPreferences == undefined ? (
                <>
                    <Label>Select a robot</Label>
                    {/** Scroll view for selecting a robot to configure */}
                    <div className="flex overflow-y-auto flex-col gap-2 min-w-[20vw] max-h-[40vh] bg-background-secondary rounded-md p-2">
                        {robots.map(mirabufSceneObject => {
                            return (
                                <Button
                                    value={mirabufSceneObject.assemblyName}
                                    onClick={() => {
                                        setSelectedRobot(mirabufSceneObject)
                                    }}
                                    key={mirabufSceneObject.id}
                                ></Button>
                            )
                        })}
                    </div>
                    {/* TODO: remove the accept button on this version */}
                </>
            ) : (
                <>
                    <div className="flex flex-col gap-2 bg-background-secondary rounded-md p-2">
                        <ToggleButtonGroup
                            value={viewType}
                            exclusive
                            onChange={(_, v) => v != null && setViewType(v)}
                            sx={{
                                alignSelf: "center",
                            }}
                        >
                            <ToggleButton value={ConfigureRobotBrainTypes.SYNTHESIS}>SynthesisBrain</ToggleButton>
                            <ToggleButton value={ConfigureRobotBrainTypes.WIPLIB}>WIPLIBBrain</ToggleButton>
                        </ToggleButtonGroup>
                        {viewType === ConfigureRobotBrainTypes.SYNTHESIS ? (
                            <>
                                <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                    Behaviors  
                                </LabelStyled>
                                <DividerStyled />

                                <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                    thing2
                                </LabelStyled>
                                <DividerStyled />

                                <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                    thing3
                                </LabelStyled>
                                <DividerStyled />

                                <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                    thing4
                                </LabelStyled>
                                <DividerStyled /> 
                            </>
                        ) : (
                            <>
                                <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                    Example WIPLIB Brain 
                                </LabelStyled>
                                <DividerStyled />

                                <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                    Example 2
                                </LabelStyled>
                                <DividerStyled />

                                <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                    Example 3
                                </LabelStyled>
                                <DividerStyled />

                                <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                    Example 4
                                </LabelStyled>
                                <DividerStyled /> 
                            </>
                        )}
                    </div>
                </>
            )}
        </Panel>
    )
}

export default ConfigureRobotBrainPanel
