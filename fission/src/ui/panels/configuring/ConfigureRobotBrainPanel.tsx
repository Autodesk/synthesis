import { MiraType } from "@/mirabuf/MirabufLoader";
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject";
import World from "@/systems/World";
import Label from "@/ui/components/Label";
import Button from "@/ui/components/Button";
import Panel, { PanelPropsImpl } from "@/ui/components/Panel";
import { useMemo, useState } from "react";
import { FaGear } from "react-icons/fa6";
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup";

// eslint-disable-next-line react-refresh/only-export-components
export enum ConfigureRobotBrainTypes {
    SYNTHESIS = 0,
    WIPLIB = 1,
}

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
            onAccept={() => { }}
            onCancel={() => { }}
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
                            <Label>hi</Label>
                        ) : (
                            <Label>hi2</Label>
                        )}
                    </div>
                </>
            )}
        </Panel>
    );
}

export default ConfigureRobotBrainPanel;