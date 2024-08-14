import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { RobotPreferences } from "@/systems/preferences/PreferenceTypes"
import Driver from "@/systems/simulation/driver/Driver"
import HingeDriver from "@/systems/simulation/driver/HingeDriver"
import SliderDriver from "@/systems/simulation/driver/SliderDriver"
import WheelDriver from "@/systems/simulation/driver/WheelDriver"
import World from "@/systems/World"
import Button from "@/ui/components/Button"
import Label, { LabelSize } from "@/ui/components/Label"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import ScrollView from "@/ui/components/ScrollView"
import Slider from "@/ui/components/Slider"
import Stack, { StackDirection } from "@/ui/components/Stack"
import { SectionDivider } from "@/ui/components/StyledComponents"
import { Box } from "@mui/material"
import { useCallback, useMemo, useState } from "react"
import { FaGear } from "react-icons/fa6"

type SubsystemRowProps = {
    robot: MirabufSceneObject
    driver: Driver
}

const SubsystemRow: React.FC<SubsystemRowProps> = ({ robot, driver }) => {
    const driverSwitch = (driver: Driver, slider: unknown, hinge: unknown, drivetrain: unknown) => {
        switch (driver.constructor) {
            case SliderDriver:
                return slider
            case HingeDriver:
                return hinge
            case WheelDriver:
                return drivetrain
            default:
                return drivetrain
        }
    }

    const [velocity, setVelocity] = useState<number>(
        ((driver as SliderDriver) || (driver as HingeDriver) || (driver as WheelDriver)).maxVelocity
    )
    const [force, setForce] = useState<number>(
        ((driver as SliderDriver) || (driver as HingeDriver) || (driver as WheelDriver)).maxForce
    )

    const onChange = useCallback(
        (vel: number, force: number) => {
            if (driver instanceof WheelDriver) {
                const wheelDrivers = robot?.mechanism
                    ? World.SimulationSystem.GetSimulationLayer(robot.mechanism)?.drivers.filter(
                          x => x instanceof WheelDriver
                      )
                    : undefined
                wheelDrivers?.forEach(x => {
                    x.maxVelocity = vel
                    x.maxForce = force
                })

                // Preferences
                PreferencesSystem.getRobotPreferences(robot.assemblyName).driveVelocity = vel
                PreferencesSystem.getRobotPreferences(robot.assemblyName).driveAcceleration = force
            } else {
                ;((driver as SliderDriver) || (driver as HingeDriver)).maxVelocity = vel
                ;((driver as SliderDriver) || (driver as HingeDriver)).maxForce = force

                // Preferences
                if (driver.info && driver.info.name) {
                    const removedMotor = PreferencesSystem.getRobotPreferences(robot.assemblyName).motors
                        ? PreferencesSystem.getRobotPreferences(robot.assemblyName).motors.filter(x => {
                              if (x.name) return x.name != driver.info?.name
                              return false
                          })
                        : []

                    removedMotor.push({
                        name: driver.info?.name ?? "",
                        maxVelocity: vel,
                        maxForce: force,
                    })

                    PreferencesSystem.getRobotPreferences(robot.assemblyName).motors = removedMotor
                }
            }

            PreferencesSystem.savePreferences()
        },
        [driver, robot.mechanism, robot.assemblyName]
    )

    return (
        <>
            <Box component={"div"} display={"flex"} justifyContent={"space-between"} alignItems={"center"} gap={"1rem"}>
                <Stack direction={StackDirection.Vertical} spacing={8} justify="start">
                    <Label size={LabelSize.Medium}>
                        {driver instanceof WheelDriver ? "Drive" : driver.info?.name ?? "UnnamedMotor"}
                    </Label>
                    <Slider
                        min={0.1}
                        max={driverSwitch(driver, 80, 40, 80) as number}
                        value={velocity}
                        label="Max Velocity"
                        format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                        onChange={(_, _velocity: number | number[]) => {
                            setVelocity(_velocity as number)
                            onChange(_velocity as number, force)
                        }}
                        step={0.01}
                    />
                    {PreferencesSystem.getGlobalPreference("SubsystemGravity") || driver instanceof WheelDriver ? (
                        <Slider
                            min={driverSwitch(driver, 100, 20, 0.1) as number}
                            max={driverSwitch(driver, 800, 150, 15) as number}
                            value={force}
                            label={driverSwitch(driver, "Max Force", "Max Torque", "Max Acceleration") as string}
                            format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                            onChange={(_, _force: number | number[]) => {
                                setForce(_force as number)
                                onChange(velocity, _force as number)
                            }}
                            step={0.01}
                        />
                    ) : driver instanceof HingeDriver ? (
                        <Label>Select Realistic Gravity in Settings for torque config</Label>
                    ) : (
                        <Label>Select Realistic Gravity in Settings for force config</Label>
                    )}
                </Stack>
            </Box>
            <SectionDivider />
        </>
    )
}

const ConfigureSubsystemsPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const [selectedRobot, setSelectedRobot] = useState<MirabufSceneObject | undefined>(undefined)
    const [origPref, setOrigPref] = useState<RobotPreferences | undefined>(undefined)

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
        return selectedRobot?.mechanism
            ? World.SimulationSystem.GetSimulationLayer(selectedRobot.mechanism)?.drivers
            : undefined
    }, [selectedRobot])

    // Gets motors in preferences for ease of saving into origPrefs which can be used to revert on Cancel()
    function saveOrigMotors(robot: MirabufSceneObject) {
        drivers?.forEach(driver => {
            if (driver.info && driver.info.name && !(driver instanceof WheelDriver)) {
                const motors = PreferencesSystem.getRobotPreferences(robot.assemblyName).motors
                const removedMotor = motors.filter(x => {
                    if (x.name) return x.name != driver.info?.name
                    return false
                })

                if (removedMotor.length == drivers.length) {
                    removedMotor.push({
                        name: driver.info?.name ?? "",
                        maxVelocity: ((driver as SliderDriver) || (driver as HingeDriver)).maxVelocity,
                        maxForce: ((driver as SliderDriver) || (driver as HingeDriver)).maxForce,
                    })
                    PreferencesSystem.getRobotPreferences(robot.assemblyName).motors = removedMotor
                }
            }
        })
        PreferencesSystem.savePreferences()
        setOrigPref({ ...PreferencesSystem.getRobotPreferences(robot.assemblyName) }) // clone
    }

    function Cancel() {
        if (selectedRobot && origPref) {
            drivers?.forEach(driver => {
                if (driver instanceof WheelDriver) {
                    driver.maxVelocity = origPref.driveVelocity
                    driver.maxForce = origPref.driveAcceleration
                } else {
                    if (driver.info && driver.info.name) {
                        const motor = origPref.motors.filter(x => {
                            if (x.name) return x.name == driver.info?.name
                            return false
                        })[0]
                        if (motor) {
                            ((driver as SliderDriver) || (driver as HingeDriver)).maxVelocity = motor.maxVelocity
                            ;((driver as SliderDriver) || (driver as HingeDriver)).maxForce =
                                PreferencesSystem.getGlobalPreference("SubsystemGravity")
                                    ? motor.maxForce
                                    : driver instanceof SliderDriver
                                      ? 500
                                      : 100
                        }
                    }
                }
            })
            PreferencesSystem.setRobotPreferences(selectedRobot.assemblyName, origPref)
        }
        PreferencesSystem.savePreferences()
    }

    return (
        <Panel
            name="Configure Subsystems"
            icon={<FaGear />}
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            onAccept={() => {
                PreferencesSystem.savePreferences()
            }}
            onCancel={Cancel}
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
                                        saveOrigMotors(mirabufSceneObject)
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
                            {/** Drivetrain row. Then other SliderDrivers and HingeDrivers */}
                            <SubsystemRow
                                key={0}
                                robot={(() => {
                                    return selectedRobot
                                })()}
                                driver={(() => {
                                    return drivers.filter(x => x instanceof WheelDriver)[0]
                                })()}
                            />
                            {drivers
                                .filter(x => x instanceof SliderDriver || x instanceof HingeDriver)
                                .map((driver: Driver, i: number) => (
                                    <SubsystemRow
                                        key={i + 1}
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
                        <Label>No Subsystems</Label>
                    )}
                </>
            )}
        </Panel>
    )
}

export default ConfigureSubsystemsPanel
