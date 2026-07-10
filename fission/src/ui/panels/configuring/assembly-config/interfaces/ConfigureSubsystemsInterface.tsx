import type React from "react"
import { useMemo, useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import { defaultSequentialConfig, type SequentialBehaviorPreferences } from "@/systems/preferences/PreferenceTypes"
import GenericArmBehavior from "@/systems/simulation/behavior/synthesis/GenericArmBehavior"
import SequenceableBehavior from "@/systems/simulation/behavior/synthesis/SequenceableBehavior"
import type SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import { buildJointConfigGroups, type JointConfigGroup } from "../jointConfigGroups"
import SubsystemRowInterface from "./SubsystemRowInterface"

class JointGroupSelectionOption extends SelectMenuOption {
    group: JointConfigGroup

    constructor(group: JointConfigGroup) {
        super(group.id, group.name)
        this.group = group
    }
}

interface ConfigSubsystemProps {
    selectedRobot: MirabufSceneObject
}

const ConfigureSubsystemsInterface: React.FC<ConfigSubsystemProps> = ({ selectedRobot }) => {
    const [selectedGroup, setSelectedGroup] = useState<JointGroupSelectionOption | undefined>(undefined)

    const behaviors = useMemo<SequentialBehaviorPreferences[]>(
        () =>
            selectedRobot.robotPreferences.sequentialConfig ??
            (selectedRobot.brain as SynthesisBrain).behaviors
                .filter(b => b instanceof SequenceableBehavior)
                .map(b => defaultSequentialConfig(b.jointIndex, b instanceof GenericArmBehavior ? "Arm" : "Elevator")),
        [selectedRobot.robotPreferences, selectedRobot.brain]
    )

    const options = useMemo(
        () => buildJointConfigGroups(selectedRobot, behaviors).map(g => new JointGroupSelectionOption(g)),
        [selectedRobot, behaviors]
    )

    return (
        <>
            <SelectMenu
                options={options}
                onOptionSelected={val => {
                    if (val !== undefined) EventSystem.dispatch("ConfigurationSavedEvent")
                    setSelectedGroup(val as JointGroupSelectionOption)
                }}
                defaultHeaderText="Select a Subsystem"
            />
            {selectedGroup !== undefined && (
                <SubsystemRowInterface
                    robot={selectedRobot}
                    group={selectedGroup.group}
                    saveBehaviors={() => {
                        selectedRobot.robotPreferences.sequentialConfig = behaviors
                        selectedRobot.savePreferences()
                    }}
                />
            )}
        </>
    )
}

export default ConfigureSubsystemsInterface
