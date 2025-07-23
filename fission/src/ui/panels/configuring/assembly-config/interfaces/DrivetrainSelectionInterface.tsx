import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import Dropdown from "@/components/Dropdown.tsx"
import { DriveType } from "@/systems/simulation/behavior/Behavior.ts"

interface DrivetrainSelectionProps {
    selectedAssembly: MirabufSceneObject
}

const DrivetrainSelectionInterface: React.FC<DrivetrainSelectionProps> = ({ selectedAssembly }) => {
    return (
        <>
            <Dropdown // TODO: disable/hide when wpilib brain selected
                label="Drivetrain Type"
                options={[DriveType.TANK, DriveType.ARCADE, DriveType.SWERVE]}
                defaultValue={(selectedAssembly.brain as SynthesisBrain | undefined)?.driveType ?? DriveType.ARCADE}
                onSelect={val => {
                    if (selectedAssembly.brain?.brainType == "synthesis") {
                        const brain = selectedAssembly.brain as SynthesisBrain
                        brain.configure(val)
                    }
                }}
            />
        </>
    )
}

export default DrivetrainSelectionInterface
