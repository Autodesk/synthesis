import { FormControl, InputLabel, MenuItem } from "@mui/material"
import { Select } from "@/ui/components/StyledComponents"
import EventSystem from "@/systems/EventSystem.ts"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import { DriveType } from "@/systems/simulation/behavior/Behavior.ts"
import type SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"
import { useEffect } from "react"
import InputSystem from "@/systems/input/InputSystem.ts"

const DrivetrainSelectionInterface: ConfigurationSubpanelComponent = ({
    selectedAssembly,
    registerCleanupFunction,
}) => {
    useEffect(() => {
        const brain = selectedAssembly.brain
        if (!brain?.isSynthesis()) {
            return
        }
        const originalDriveBehavior = brain.driveType
        const originalScheme = InputSystem.getBrainIndexSchemeMapping(brain.brainIndex)
        registerCleanupFunction(undefined, () => {
            brain.configureDriveBehavior(originalDriveBehavior)
            if (originalScheme != null) {
                InputSystem.setBrainIndexSchemeMapping(brain.brainIndex, originalScheme)
            } else {
                InputSchemeManager.applyCompatibleScheme(brain.brainIndex)
            }
            EventSystem.dispatch("InputSchemeChanged", {})
        })
    }, [registerCleanupFunction, selectedAssembly])
    return (
        <>
            <FormControl fullWidth>
                <InputLabel id="drivetrain-type-label">Drivetrain Type</InputLabel>
                <Select // TODO: disable/hide when wpilib brain selected
                    labelId="drivetrain-type-label"
                    label="Drivetrain Type"
                    defaultValue={(selectedAssembly.brain as SynthesisBrain | undefined)?.driveType ?? DriveType.ARCADE}
                    onChange={e => {
                        if (selectedAssembly.brain?.isSynthesis()) {
                            selectedAssembly.brain.configureDriveBehavior(e.target.value as DriveType)
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
        </>
    )
}

export default DrivetrainSelectionInterface
