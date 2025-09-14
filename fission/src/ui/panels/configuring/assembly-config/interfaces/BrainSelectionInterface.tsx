import { useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { BrainType } from "@/systems/simulation/Brain"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import WPILibBrain from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/StyledComponents"

type BrainSelectionInterfaceProps = {
    selectedAssembly: MirabufSceneObject
}

export default function BrainSelectionInterface({ selectedAssembly }: BrainSelectionInterfaceProps) {
    const [robotBrainType, setRobotBrainType] = useState<BrainType | undefined>(selectedAssembly.brain?.brainType)
    return (
        <ToggleButtonGroup
            value={robotBrainType}
            exclusive
            onChange={(_, v) => {
                const brainType = v as BrainType
                if (v === undefined) return

                switch (brainType) {
                    case "synthesis":
                        selectedAssembly.brain = new SynthesisBrain(selectedAssembly, selectedAssembly.assemblyName)
                        break
                    case "wpilib":
                        selectedAssembly.brain = new WPILibBrain(selectedAssembly)
                        break
                    default:
                        return
                }
                setRobotBrainType(brainType)
            }}
            sx={{
                alignSelf: "center",
            }}
        >
            <ToggleButton value={"synthesis"}>Synthesis Brain</ToggleButton>
            <ToggleButton value={"wpilib"}>WPILib Brain</ToggleButton>
        </ToggleButtonGroup>
    )
}
