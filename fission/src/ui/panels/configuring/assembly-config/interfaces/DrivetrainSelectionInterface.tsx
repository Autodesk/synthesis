import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { DriveType } from "@/systems/simulation/behavior/Behavior.ts"
import { MenuItem, Select } from "@mui/material"

interface DrivetrainSelectionProps {
    selectedAssembly: MirabufSceneObject
}

const DrivetrainSelectionInterface: React.FC<DrivetrainSelectionProps> = ({ selectedAssembly }) => {
    return (
        <>
            <Select // TODO: disable/hide when wpilib brain selected
                label="Drivetrain Type"
                defaultValue={(selectedAssembly.brain as SynthesisBrain | undefined)?.driveType ?? DriveType.ARCADE}
                onChange={e => {
                    if (selectedAssembly.brain?.brainType == "synthesis") {
                        const brain = selectedAssembly.brain as SynthesisBrain
                        brain.configureDriveBehavior(e.target.value as DriveType)
                    }
                }}
            >
                {[DriveType.TANK, DriveType.ARCADE].map(dt => (
                    <MenuItem value={dt}>{dt}</MenuItem>
                ))}
            </Select>
        </>
    )
}

export default DrivetrainSelectionInterface
