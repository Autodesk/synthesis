import { useEffect, useState } from "react"
import type Brain from "@/systems/simulation/Brain"
import type { BrainType } from "@/systems/simulation/Brain"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import WPILibBrain from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/StyledComponents"
import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"
import InputSystem from "@/systems/input/InputSystem.ts"

function createBrain(assembly: MirabufSceneObject, brainType: BrainType): Brain | undefined {
    switch (brainType) {
        case "synthesis":
            return new SynthesisBrain(assembly)
        case "wpilib":
            return new WPILibBrain(assembly, "wpilib")
        case "ftc":
            return new WPILibBrain(assembly, "ftc")
        default:
            return
    }
}

const BrainSelectionInterface: ConfigurationSubpanelComponent = ({ selectedAssembly, registerCleanupFunction }) => {
    const [robotBrainType, setRobotBrainType] = useState<BrainType | undefined>(selectedAssembly.brain?.brainType)

    useEffect(() => {
        const originalBrainType = selectedAssembly.brain!.brainType
        const originalScheme = selectedAssembly.brain?.isSynthesis()
            ? InputSystem.getBrainIndexSchemeMapping(selectedAssembly.brain.brainIndex)
            : null
        registerCleanupFunction(undefined, () => {
            if (selectedAssembly.brain?.brainType != originalBrainType) {
                selectedAssembly.brain = createBrain(selectedAssembly, originalBrainType)!
            }

            if (
                selectedAssembly.brain.isSynthesis() &&
                originalScheme != null &&
                InputSystem.getBrainIndexSchemeMapping(selectedAssembly.brain.brainIndex) == null
            ) {
                InputSystem.setBrainIndexSchemeMapping(selectedAssembly.brain.brainIndex, originalScheme)
            }
        })
    }, [registerCleanupFunction, selectedAssembly])

    return (
        <ToggleButtonGroup
            value={robotBrainType}
            exclusive
            onChange={(_, v) => {
                const brainType = v as BrainType
                if (v === undefined) return
                selectedAssembly.brain = createBrain(selectedAssembly, brainType)
                setRobotBrainType(brainType)
            }}
            sx={{
                alignSelf: "center",
            }}
        >
            <ToggleButton value={"synthesis"}>Synthesis Brain</ToggleButton>
            <ToggleButton value={"wpilib"}>WPILib Brain</ToggleButton>
            <ToggleButton value={"ftc"}>FTC Brain</ToggleButton>
        </ToggleButtonGroup>
    )
}

export default BrainSelectionInterface
