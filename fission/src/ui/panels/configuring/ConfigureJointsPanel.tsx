import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import Driver from "@/systems/simulation/driver/Driver"
import HingeDriver from "@/systems/simulation/driver/HingeDriver"
import SliderDriver from "@/systems/simulation/driver/SliderDriver"
import World from "@/systems/World"
import Button from "@/ui/components/Button"
import Label, { LabelSize } from "@/ui/components/Label"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import ScrollView from "@/ui/components/ScrollView"
import Slider from "@/ui/components/Slider"
import Stack, { StackDirection } from "@/ui/components/Stack"
import { Box } from "@mui/material"
import { useMemo, useState } from "react"
import { FaGear } from "react-icons/fa6"

type JointRowProps = {
    robot: MirabufSceneObject
    driver: Driver
}

const JointRow: React.FC<JointRowProps> = ({ robot, driver }) => {
    const [velocity, setVelocity] = useState<number>(() => {
        switch (driver.constructor) {
            case SliderDriver:
                return (driver as SliderDriver).maxVelocity
            case HingeDriver:
                return (driver as HingeDriver).maxVelocity
            default:
                return 40
        }
    })
    const [force, setForce] = useState<number>(() => {
        switch (driver.constructor) {
            case SliderDriver:
                return (driver as SliderDriver).maxForce
            case HingeDriver:
                return (driver as HingeDriver).maxTorque
            default:
                return 40
        }
    })

    const onChange = (vel: number, force: number) => {
        (driver as SliderDriver || driver as HingeDriver).maxVelocity = vel
        switch (driver.constructor) {
            case SliderDriver:
                (driver as SliderDriver).maxForce = force
                break
            case HingeDriver:
                (driver as HingeDriver).maxTorque = force
                break
            default:
                return 40
        }

        // Preferences
        if (driver.info && driver.info.name) {
            const removedMotor = PreferencesSystem.getRobotPreferences(robot.assemblyName).motors ? PreferencesSystem.getRobotPreferences(robot.assemblyName).motors.filter(x => {
                if (x.name)
                    return x.name != driver.info?.name
                return false
            }) : []

            removedMotor.push({
                name: driver.info?.name ?? "",
                maxVelocity: vel,
                maxForce: force})

            PreferencesSystem.getRobotPreferences(robot.assemblyName).motors = removedMotor
            PreferencesSystem.savePreferences()
        }
    }


    return (
        <Box component={"div"} display={"flex"} justifyContent={"space-between"} alignItems={"center"} gap={"1rem"}>
            <Stack direction={StackDirection.Vertical} spacing={8} justify="start">
                <Label size={LabelSize.Medium}>{driver.info?.name ?? "UnnamedMotor"}</Label>
                <Slider
                    min={0.1}
                    max={80}
                    value={velocity}
                    label="Max Velocity"
                    format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                    onChange={(_, _velocity: number | number[]) => {
                        setVelocity(_velocity as number);
                        onChange(_velocity as number, force)
                    }}
                    step={0.01}
                />
                <Slider
                    min={(() => {return driver instanceof HingeDriver ? 20 : 100})()}
                    max={(() => {return driver instanceof HingeDriver ? 200 : 800})()}
                    value={force}
                    label={(() => {return driver instanceof HingeDriver ? "Max Torque" : "Max Force"})()}
                    format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                    onChange={(_, _force: number | number[]) => {
                        setForce(_force as number);
                        onChange(velocity, _force as number)
                    }}
                    step={0.01}
                />
            </Stack>
        </Box>
    )
}


const ConfigureJointsPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding}) => {

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
            onAccept={() => { PreferencesSystem.savePreferences()}}       
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

                    {drivers ? (
                        <ScrollView className="flex flex-col gap-4">
                            {drivers.filter(x => x instanceof SliderDriver || x instanceof HingeDriver).map((driver: Driver, i: number) => (
                                <JointRow
                                    key={i}
                                    robot={(() => {
                                        return selectedRobot
                                    })()}
                                    driver={(() => {
                                        return driver
                                    })()}
                                />
                            
                            ))}
                        </ScrollView>
                    ) : (
                        <Label>No Joints</Label>
                    )}
                </>
            )}


        </Panel>

    )
}

export default ConfigureJointsPanel