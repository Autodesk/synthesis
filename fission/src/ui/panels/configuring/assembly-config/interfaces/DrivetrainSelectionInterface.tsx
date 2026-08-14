import { FormControl, InputLabel, MenuItem, Stack } from "@mui/material"
import { Select } from "@/ui/components/StyledComponents"
import Checkbox from "@/ui/components/Checkbox"
import EventSystem from "@/systems/EventSystem.ts"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import { DriveType } from "@/systems/simulation/behavior/Behavior.ts"
import type SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"
import { useEffect, useState } from "react"
import InputSystem from "@/systems/input/InputSystem.ts"
import World from "@/systems/World"

const DrivetrainSelectionInterface: ConfigurationSubpanelComponent = ({
    selectedAssembly,
    registerCleanupFunction,
}) => {
    const synthesisBrain = selectedAssembly.brain?.isSynthesis()
        ? (selectedAssembly.brain as SynthesisBrain)
        : undefined

    const [driveType, setDriveType] = useState<DriveType>(synthesisBrain?.driveType ?? DriveType.ARCADE)
    const [robotCentric, setRobotCentric] = useState<boolean>(synthesisBrain?.mecanumRobotCentric ?? false)

    useEffect(() => {
        const brain = selectedAssembly.brain
        if (!brain?.isSynthesis()) {
            return
        }
        const originalDriveBehavior = brain.driveType
        const originalRobotCentric = brain.mecanumRobotCentric
        const originalScheme = InputSystem.getBrainIndexSchemeMapping(brain.brainIndex)
        registerCleanupFunction(
            () => {
                if (brain.driveType === originalDriveBehavior && brain.mecanumRobotCentric === originalRobotCentric) {
                    return
                }
                World.analyticsSystem?.event("Drivetrain Configured", {
                    driveType: brain.driveType,
                    robotCentric: brain.mecanumRobotCentric,
                    source: "Drivetrain Config",
                })
            },
            () => {
                brain.configureDriveBehavior(originalDriveBehavior)
                brain.setMecanumRobotCentric(originalRobotCentric)
                if (originalScheme != null) {
                    InputSystem.setBrainIndexSchemeMapping(brain.brainIndex, originalScheme)
                } else {
                    InputSchemeManager.applyCompatibleScheme(brain.brainIndex)
                }
                EventSystem.dispatch("InputSchemeChanged", {})
            }
        )
    }, [registerCleanupFunction, selectedAssembly])
    return (
        <Stack direction="column" gap={2}>
            <FormControl fullWidth>
                <InputLabel id="drivetrain-type-label">Drivetrain Type</InputLabel>
                <Select // TODO: disable/hide when wpilib brain selected
                    labelId="drivetrain-type-label"
                    label="Drivetrain Type"
                    value={driveType}
                    onChange={e => {
                        if (selectedAssembly.brain?.isSynthesis()) {
                            const appliedDriveType = selectedAssembly.brain.configureDriveBehavior(
                                e.target.value as DriveType
                            )
                            setDriveType(appliedDriveType)

                            InputSchemeManager.applyCompatibleScheme(selectedAssembly.brain.brainIndex)
                            EventSystem.dispatch("InputSchemeChanged", {})
                        }
                    }}
                >
                    {[DriveType.TANK, DriveType.ARCADE, DriveType.SWERVE, DriveType.MECANUM].map(dt => (
                        <MenuItem key={`drivetrain-type-${dt}`} value={dt}>
                            {dt}
                        </MenuItem>
                    ))}
                </Select>
            </FormControl>
            {driveType === DriveType.MECANUM && (
                <Checkbox
                    label="Robot-Centric Drive"
                    tooltip="Drive relative to the robot's nose instead of a fixed field heading."
                    checked={robotCentric}
                    onClick={checked => {
                        synthesisBrain?.setMecanumRobotCentric(checked)
                        setRobotCentric(checked)
                    }}
                />
            )}
        </Stack>
    )
}

export default DrivetrainSelectionInterface
