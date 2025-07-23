import { Checkbox, Divider, FormControlLabel, Slider, Stack, Typography } from "@mui/material"
import { useCallback, useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { SequentialBehaviorPreferences } from "@/systems/preferences/PreferenceTypes"
import type Driver from "@/systems/simulation/driver/Driver"
import HingeDriver from "@/systems/simulation/driver/HingeDriver"
import SliderDriver from "@/systems/simulation/driver/SliderDriver"
import WheelDriver from "@/systems/simulation/driver/WheelDriver"
import World from "@/systems/World"
import StatefulSlider from "@/ui/components/StatefulSlider"
import StatefulCheckbox from "@/ui/components/StatefulCheckbox"

type SubsystemRowProps = {
    robot: MirabufSceneObject
    driver: Driver
    sequentialBehavior?: SequentialBehaviorPreferences
    saveBehaviors?: () => void
}

const SubsystemRowInterface: React.FC<SubsystemRowProps> = ({ robot, driver, sequentialBehavior, saveBehaviors }) => {
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
                    ? World.simulationSystem
                          .getSimulationLayer(robot.mechanism)
                          ?.drivers.filter(x => x instanceof WheelDriver)
                    : undefined
                wheelDrivers?.forEach(x => {
                    x.maxVelocity = vel
                    x.maxForce = force
                })

                // Preferences
                PreferencesSystem.getRobotPreferences(robot.assemblyName).driveVelocity = vel
                PreferencesSystem.getRobotPreferences(robot.assemblyName).driveAcceleration = force
            } else {
                // Preferences
                if (driver.info?.name) {
                    const removedMotor = PreferencesSystem.getRobotPreferences(robot.assemblyName).motors
                        ? PreferencesSystem.getRobotPreferences(robot.assemblyName).motors.filter(x => {
                              if (x.name) return x.name !== driver.info?.name
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
                // eslint-disable-next-line no-extra-semi
                ;((driver as SliderDriver) || (driver as HingeDriver)).maxVelocity = vel
                ;((driver as SliderDriver) || (driver as HingeDriver)).maxForce = force
            }

            PreferencesSystem.savePreferences()
        },
        [driver, robot.mechanism, robot.assemblyName]
    )

    return (
        <>
            <Stack justifyContent={"space-between"} alignItems={"center"} gap={"1rem"}>
                <Stack direction="row" gap={8}>
                    <Typography variant="h5">
                        {driver instanceof WheelDriver ? "Drive" : (driver.info?.name ?? "UnnamedMotor")}
                    </Typography>
                    <StatefulSlider
                        label="Max Velocity"
                        min={0.1}
                        max={driverSwitch(driver, 80, 40, 80) as number}
                        defaultValue={velocity}
                        // TODO:
                        // format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                        onChange={velocity => {
                            setVelocity(velocity as number)
                            onChange(velocity as number, force)
                        }}
                        step={0.01}
                    />
                    {PreferencesSystem.getGlobalPreference("SubsystemGravity") ||
                        (driver instanceof WheelDriver && (
                            <StatefulSlider
                                label={driverSwitch(driver, "Max Force", "Max Torque", "Max Acceleration") as string}
                                min={driverSwitch(driver, 100, 20, 0.1) as number}
                                max={driverSwitch(driver, 800, 150, 15) as number}
                                defaultValue={force}
                                // TODO:
                                // format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                                onChange={force => {
                                    setForce(force as number)
                                    onChange(velocity, force as number)
                                }}
                                step={0.01}
                            />
                        ))}
                    {sequentialBehavior && (
                        <StatefulCheckbox
                            label="Invert Motor"
                            checked={sequentialBehavior.inverted}
                            onClick={checked => {
                                sequentialBehavior.inverted = checked
                                saveBehaviors?.()
                            }}
                        />
                    )}
                </Stack>
            </Stack>
            <Divider />
        </>
    )
}

export default SubsystemRowInterface
