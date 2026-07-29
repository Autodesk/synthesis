import { useEffect, useMemo, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import { defaultSequentialConfig, type SequentialBehaviorPreferences } from "@/systems/preferences/PreferenceTypes"
import GenericArmBehavior from "@/systems/simulation/behavior/synthesis/GenericArmBehavior"
import SequenceableBehavior from "@/systems/simulation/behavior/synthesis/SequenceableBehavior"
import type SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import { buildJointConfigGroups, type JointConfigGroup } from "../jointConfigGroups"
import SubsystemRowInterface from "./SubsystemRowInterface"
import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"

class JointGroupSelectionOption extends SelectMenuOption {
    group: JointConfigGroup

    constructor(group: JointConfigGroup) {
        super(group.id, group.name)
        this.group = group
    }
}

const ConfigureSubsystemsInterface: ConfigurationSubpanelComponent = ({
    selectedAssembly,
    registerCleanupFunction,
}) => {
    const [selectedGroup, setSelectedGroup] = useState<JointGroupSelectionOption | undefined>(undefined)

    const behaviors = useMemo<SequentialBehaviorPreferences[]>(
        () =>
            selectedAssembly.robotPreferences.sequentialConfig ??
            (selectedAssembly.brain as SynthesisBrain).behaviors
                .filter(b => b instanceof SequenceableBehavior)
                .map(b => defaultSequentialConfig(b.jointIndex, b instanceof GenericArmBehavior ? "Arm" : "Elevator")),
        [selectedAssembly]
    )

    useEffect(() => {
        const originalPrefs = structuredClone(selectedAssembly.robotPreferences.sequentialConfig)
        const originalUnstickForce = selectedAssembly.robotPreferences.unstickForce
        registerCleanupFunction(undefined, () => {
            selectedAssembly.robotPreferences.sequentialConfig = originalPrefs
            selectedAssembly.robotPreferences.unstickForce = originalUnstickForce
        })
    }, [registerCleanupFunction, selectedAssembly])

    const options = useMemo(
        () => buildJointConfigGroups(selectedAssembly, behaviors).map(g => new JointGroupSelectionOption(g)),
        [selectedAssembly, behaviors]
    )

    return (
        <>
            <SelectMenu
                options={options}
                onOptionSelected={val => {
                    if (val !== undefined) EventSystem.dispatch("ConfigurationSavedEvent")
                    setSelectedGroup(val)
                }}
                defaultHeaderText="Select a Subsystem"
            />
            {selectedGroup !== undefined && (
                <SubsystemRowInterface
                    robot={selectedAssembly}
                    group={selectedGroup.group}
                    saveBehaviors={() => {
                        selectedAssembly.robotPreferences.sequentialConfig = behaviors
                        selectedAssembly.savePreferences()
                    }}
                />
            )}
        </>
    )
}

export default ConfigureSubsystemsInterface
