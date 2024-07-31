import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MechanismConstraint } from "@/systems/physics/Mechanism"
import Driver from "@/systems/simulation/driver/Driver"
import SliderDriver from "@/systems/simulation/driver/SliderDriver"
import World from "@/systems/World"
import Button from "@/ui/components/Button"
import Label, { LabelSize } from "@/ui/components/Label"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import ScrollView from "@/ui/components/ScrollView"
import Slider from "@/ui/components/Slider"
import Stack, { StackDirection } from "@/ui/components/Stack"
import { useTheme } from "@/ui/ThemeContext"
import { Box } from "@mui/material"
import { useEffect, useMemo, useState } from "react"
import { FaGear } from "react-icons/fa6"

type JointRowProps = {
    driver: Driver
}

const JointRow: React.FC<JointRowProps> = ({ driver }) => {
    const [velocity, setVelocity] = useState<number>((driver as SliderDriver).maxVelocity)
    const [force, setForce] = useState<number>((driver as SliderDriver).maxForceLimit)

    return (
        <Box component={"div"} display={"flex"} justifyContent={"space-between"} alignItems={"center"} gap={"1rem"}>
            <Stack direction={StackDirection.Vertical} spacing={8} justify="start">
                <Label size={LabelSize.Medium}>{driver.info?.name}</Label>
                <Slider
                    min={0.1}
                    max={80}
                    value={velocity}
                    label="Max Velocity"
                    format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                    onChange={(_, vel: number | number[]) => {
                        setVelocity(vel as number);
                        (driver as SliderDriver).maxVelocity = velocity
                    }}
                    step={0.01}
                />
                <Slider
                    min={150}
                    max={1000}
                    value={force}
                    label="Force"
                    format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                    onChange={(_, forc: number | number[]) => {
                        setForce(forc as number);
                        (driver as SliderDriver).maxForceLimit = force;
                        (driver as SliderDriver).minForceLimit = -force
                    }}
                    step={0.01}
                />
            </Stack>
        </Box>
    )
}


const ConfigureJointsPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding}) => {
    const { currentTheme, themes } = useTheme()
    const theme = useMemo(() => {
        return themes[currentTheme]
    }, [currentTheme, themes])

    const [selectedRobot, setSelectedRobot] = useState<MirabufSceneObject | undefined>(undefined)

    const robots = useMemo(() => {
        const assemblies = [...World.SceneRenderer.sceneObjects.values()].filter(x => {
            if (x instanceof MirabufSceneObject) {
                return x.miraType === MiraType.ROBOT
            }
            return false
        }) as MirabufSceneObject[]
        return assemblies
    }, [])

    const drivers = useMemo(() => {
        return selectedRobot?.mechanism ?
            World.SimulationSystem.GetSimulationLayer(selectedRobot.mechanism)?.drivers : undefined
    }, [selectedRobot])


    return (
        <Panel 
            name="Configure Joints"
            icon={<FaGear />}
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            onAccept={() => {}}       
            onCancel={() => {/* Back to original */ }}
            acceptEnabled={true}
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
                </>
            ) : (
                <>
                    {/* {selectedRobot.mechanism.constraints.filter(x=> x.constraint.GetSubType() == JOLT.EConstraintSubType_Slider).length > 0 ? (
                        <ScrollView className="flex flex-col gap-4">
                            {selectedRobot.mechanism.constraints.filter(x=> x.constraint.GetSubType() == JOLT.EConstraintSubType_Slider).map((mechanism: MechanismConstraint, i: number) => (

                                <JointRow
                                    key={i}
                                    driver={(() => {
                                        return driver
                                    })()}
                                />
                            
                            ))}
                        </ScrollView>
                    ) : (
                        <Label>No SliderDrivers</Label>
                    )} */}

                    {drivers ? (
                        <ScrollView className="flex flex-col gap-4">
                            {drivers.filter(x => x instanceof SliderDriver).map((driver: Driver, i: number) => (
                                <JointRow
                                    key={i}
                                    driver={(() => {
                                        return driver
                                    })()}
                                />
                            
                            ))}
                        </ScrollView>
                    ) : (
                        <Label>No SliderDrivers</Label>
                    )}
                </>
            )}


        </Panel>

    )
}

export default ConfigureJointsPanel