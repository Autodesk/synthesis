import { FormControl, InputLabel, MenuItem } from "@mui/material"
import { Select } from "@/ui/components/StyledComponents"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import { DriveType } from "@/systems/simulation/behavior/Behavior.ts"
import type SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"

interface DrivetrainSelectionProps {
    selectedAssembly: MirabufSceneObject
}

const DrivetrainSelectionInterface: React.FC<DrivetrainSelectionProps> = ({ selectedAssembly }) => {
    return (
        <>
            <FormControl fullWidth>
                <InputLabel id="drivetrain-type-label">Drivetrain Type</InputLabel>
                <Select // TODO: disable/hide when wpilib brain selected
                    labelId="drivetrain-type-label"
                    label="Drivetrain Type"
                    defaultValue={(selectedAssembly.brain as SynthesisBrain | undefined)?.driveType ?? DriveType.ARCADE}
                    onChange={e => {
                        if (selectedAssembly.brain?.brainType == "synthesis") {
                            const brain = selectedAssembly.brain as SynthesisBrain
                            brain.configureDriveBehavior(e.target.value as DriveType)

                            InputSchemeManager.applyCompatibleScheme(brain.brainIndex)
                            EventSystem.dispatch("InputSchemeChanged", { panelId: undefined })
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
