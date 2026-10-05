import { Divider, Stack } from "@mui/material"
import { useCallback, useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MAX_UNSTICK_STRENGTH, MIN_UNSTICK_STRENGTH } from "@/systems/preferences/PreferenceTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import Checkbox from "@/ui/components/Checkbox"
import Label from "@/ui/components/Label"
import StatefulSlider from "@/ui/components/StatefulSlider"
import { applyDriverConfig, driverForce, type JointConfigControl, type JointConfigGroup } from "../jointConfigGroups"

type ControlSlidersProps = {
    robot: MirabufSceneObject
    control: JointConfigControl
}

/** Velocity (+ optional force) sliders for a single control, applied to all of its drivers. */
const ControlSliders: React.FC<ControlSlidersProps> = ({ robot, control }) => {
    const [velocity, setVelocity] = useState<number>(control.drivers[0].maxVelocity)
    const [force, setForce] = useState<number>(driverForce(control.drivers[0]))

    const showForce =
        control.force && (control.force.alwaysVisible || PreferencesSystem.getUserPreference("SubsystemGravity"))

    const apply = useCallback(
        (vel: number, f: number) => {
            control.drivers.forEach(d => applyDriverConfig(robot, d, vel, f))
            robot.savePreferences()
        },
        [robot, control]
    )

    return (
        <>
            <Label size="sm">{control.label}</Label>
            <StatefulSlider
                label="Max Velocity"
                min={control.velocityRange[0]}
                max={control.velocityRange[1]}
                defaultValue={velocity}
                onChange={value => {
                    setVelocity(value as number)
                    apply(value as number, force)
                }}
                step={0.01}
            />
            {showForce && control.force && (
                <StatefulSlider
                    label={control.force.label}
                    min={control.force.range[0]}
                    max={control.force.range[1]}
                    defaultValue={force}
                    onChange={value => {
                        setForce(value as number)
                        apply(velocity, value as number)
                    }}
                    step={0.01}
                />
            )}
        </>
    )
}

type SubsystemRowProps = {
    robot: MirabufSceneObject
    group: JointConfigGroup
    saveBehaviors?: () => void
}

const SubsystemRowInterface: React.FC<SubsystemRowProps> = ({ robot, group, saveBehaviors }) => {
    const [unstickStrength, setUnstickStrength] = useState<number>(robot.robotPreferences.unstickStrength)
    const [invertMotor, setInvertMotor] = useState<boolean>(group.sequential?.inverted ?? false)
    return (
        <>
            <Stack justifyContent={"space-between"} alignItems={"center"} gap={"1rem"}>
                <Stack direction="column">
                    {group.controls.map(control => (
                        <ControlSliders key={control.label} robot={robot} control={control} />
                    ))}
                    {group.sequential && (
                        <Checkbox
                            label="Invert Motor"
                            checked={invertMotor}
                            onClick={checked => {
                                group.sequential!.inverted = checked
                                setInvertMotor(checked)
                                saveBehaviors?.()
                            }}
                        />
                    )}
                    {group.id == "drivetrain" && (
                        <StatefulSlider
                            min={MIN_UNSTICK_STRENGTH}
                            max={MAX_UNSTICK_STRENGTH}
                            defaultValue={unstickStrength}
                            label="Unstick Strength"
                            onChange={(value: number | number[]) => {
                                setUnstickStrength(value as number)
                                robot.robotPreferences.unstickStrength = value as number
                                robot.savePreferences()
                            }}
                            step={0.1}
                        />
                    )}
                </Stack>
            </Stack>
            <Divider />
        </>
    )
}

export default SubsystemRowInterface
