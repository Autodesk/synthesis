import { Stack, Tooltip } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useMemo, useReducer, useState } from "react"
import { FaUnlink } from "react-icons/fa"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { defaultSequentialConfig, type SequentialBehaviorPreferences } from "@/systems/preferences/PreferenceTypes"
import GenericArmBehavior from "@/systems/simulation/behavior/synthesis/GenericArmBehavior"
import SequenceableBehavior from "@/systems/simulation/behavior/synthesis/SequenceableBehavior"
import type SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import Label from "@/ui/components/Label"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import { Button, Spacer } from "@/ui/components/StyledComponents"
import { buildJointConfigGroups, type JointConfigGroup } from "../jointConfigGroups"
import SubsystemRowInterface from "./SubsystemRowInterface"

class JointGroupSelectionOption extends SelectMenuOption {
    group: JointConfigGroup

    constructor(group: JointConfigGroup) {
        super(group.id, group.name)
        this.group = group
    }
}

interface BehaviorCardProps {
    elementKey: number
    name: string
    behavior: SequentialBehaviorPreferences
    lookingForParent: SequentialBehaviorPreferences | undefined
    update: () => void
    onSetPressed: () => void
    onBehaviorSelected: () => void
    hasChild: boolean
}

const BehaviorCard: React.FC<BehaviorCardProps> = ({
    elementKey,
    name,
    behavior,
    update,
    onSetPressed,
    lookingForParent,
    onBehaviorSelected,
    hasChild,
}) => {
    const [selectable, setSelectable] = useState(false)
    const [hasParent, setHasParent] = useState(false)
    useEffect(() => {
        setSelectable(
            lookingForParent !== undefined && lookingForParent !== behavior && behavior.parentJointIndex === undefined
        )
    }, [lookingForParent, behavior])
    useEffect(() => {
        setHasParent(behavior.parentJointIndex !== undefined)
    })

    return (
        <Stack direction="row" textAlign="center" gap={1} key={elementKey}>
            {hasParent && Spacer(0, 10)}
            <Tooltip
                title={hasParent ? "Following Joint " + behavior.parentJointIndex : selectable ? "Set as parent" : ""}
            >
                <div>
                    <Button
                        size="small"
                        className="text-center mx-[5%] h-full"
                        onClick={() => {
                            onBehaviorSelected()
                            update()
                        }}
                        disabled={!selectable}
                        color={"secondary"}
                        sx={{
                            borderColor: !selectable ? "transparent" : "#888888",
                            color: selectable || hasParent ? undefined : "white !important",
                        }}
                    >
                        {name}
                    </Button>
                </div>
            </Tooltip>
            <Tooltip
                title={
                    hasParent
                        ? "Unfollow parent"
                        : selectable
                          ? ""
                          : hasChild
                            ? "This joint has a follower"
                            : "Follow another joint"
                }
            >
                <div>
                    <Button
                        onClick={() => {
                            onSetPressed()
                            update()
                        }}
                        className={"h-full"}
                        color={hasParent ? "warning" : "primary"}
                        disabled={selectable || hasChild}
                    >
                        {hasParent ? <FaUnlink /> : lookingForParent == behavior ? "Cancel" : "Follow"}
                    </Button>
                </div>
            </Tooltip>
        </Stack>
    )
}

function sortBehaviors(behaviors: SequentialBehaviorPreferences[]): SequentialBehaviorPreferences[] {
    behaviors.sort((a, b) => a.jointIndex - b.jointIndex)

    const sortedBehaviors: SequentialBehaviorPreferences[] = []
    behaviors.forEach(b => {
        if (b.parentJointIndex === undefined) sortedBehaviors.push(b)
    })

    for (let i = behaviors.length - 1; i >= 0; i--) {
        const b = behaviors[i]
        if (b.parentJointIndex === undefined) continue
        const parentIndex = sortedBehaviors.findIndex(sb => b.parentJointIndex === sb.jointIndex)
        if (parentIndex === -1) throw new Error("Parent behavior not found!")
        sortedBehaviors.splice(parentIndex + 1, 0, b)
    }

    return sortedBehaviors
}

interface ConfigureJointsProps {
    selectedRobot: MirabufSceneObject
}

const ConfigureJointsInterface: React.FC<ConfigureJointsProps> = ({ selectedRobot }) => {
    const [selectedGroup, setSelectedGroup] = useState<JointGroupSelectionOption | undefined>(undefined)

    const behaviors = useMemo<SequentialBehaviorPreferences[]>(
        () =>
            PreferencesSystem.getRobotPreferences(selectedRobot.assemblyName)?.sequentialConfig ??
            (selectedRobot.brain as SynthesisBrain).behaviors
                .filter(b => b instanceof SequenceableBehavior)
                .map(b => defaultSequentialConfig(b.jointIndex, b instanceof GenericArmBehavior ? "Arm" : "Elevator")),
        [selectedRobot.assemblyName, selectedRobot.brain]
    )

    const options = useMemo(
        () => buildJointConfigGroups(selectedRobot, behaviors).map(g => new JointGroupSelectionOption(g)),
        [selectedRobot, behaviors]
    )

    const [seqBehaviors, setSeqBehaviors] = useState<SequentialBehaviorPreferences[]>(
        PreferencesSystem.getRobotPreferences(selectedRobot.assemblyName)?.sequentialConfig ??
            (selectedRobot.brain as SynthesisBrain).behaviors
                .filter(b => b instanceof SequenceableBehavior)
                .map(b => defaultSequentialConfig(b.jointIndex, b instanceof GenericArmBehavior ? "Arm" : "Elevator"))
    )
    const [lookingForParent, setLookingForParent] = useState<SequentialBehaviorPreferences | undefined>(undefined)

    const [_, update] = useReducer(x => {
        setSeqBehaviors(sortBehaviors(seqBehaviors))
        return !x
    }, false)

    const saveEvent = useCallback(() => {
        if (selectedRobot === undefined || seqBehaviors === undefined) return
        PreferencesSystem.getRobotPreferences(selectedRobot.assemblyName).sequentialConfig = seqBehaviors
        PreferencesSystem.savePreferences()
    }, [seqBehaviors, selectedRobot])

    useEffect(() => {
        return EventSystem.listen("ConfigurationSavedEvent", saveEvent)
    }, [saveEvent])

    return (
        <>
            {/* Section 1: Motor Configuration */}
            <Label size="md" className="mt-1 mb-1">
                Motor Configuration
            </Label>
            <SelectMenu
                options={options}
                onOptionSelected={val => {
                    if (val !== undefined) EventSystem.dispatch("ConfigurationSavedEvent")
                    setSelectedGroup(val as JointGroupSelectionOption)
                }}
                defaultHeaderText="Select a Joint"
            />
            {selectedGroup !== undefined && (
                <SubsystemRowInterface
                    robot={selectedRobot}
                    group={selectedGroup.group}
                    saveBehaviors={() => {
                        PreferencesSystem.getRobotPreferences(selectedRobot.assemblyName).sequentialConfig = behaviors
                        PreferencesSystem.savePreferences()
                    }}
                />
            )}

            {/* Section 2: Joint Sequencing */}
            {seqBehaviors.length > 0 && (
                <>
                    <Label size="md" sx={{ mt: 3, mb: 0.5 }}>
                        Joint Sequencing
                    </Label>
                    <Stack direction="column" gap={2} className="overflow-y-auto">
                        {seqBehaviors.map(behavior => {
                            const jointIndex = behavior.jointIndex
                            return (
                                <BehaviorCard
                                    elementKey={jointIndex}
                                    name={
                                        behavior.type === "Arm"
                                            ? `Joint ${jointIndex} (Pivot)`
                                            : `Joint ${jointIndex} (Slider)`
                                    }
                                    behavior={behavior}
                                    key={jointIndex}
                                    update={update}
                                    onSetPressed={() => {
                                        if (behavior.parentJointIndex !== undefined) {
                                            behavior.parentJointIndex = undefined
                                        } else {
                                            setLookingForParent(lookingForParent === behavior ? undefined : behavior)
                                        }
                                        update()
                                    }}
                                    lookingForParent={lookingForParent}
                                    onBehaviorSelected={() => {
                                        if (lookingForParent) lookingForParent.parentJointIndex = behavior.jointIndex
                                        setLookingForParent(undefined)
                                        update()
                                    }}
                                    hasChild={seqBehaviors.some(b => b.parentJointIndex === behavior.jointIndex)}
                                />
                            )
                        })}
                    </Stack>
                </>
            )}
        </>
    )
}

export default ConfigureJointsInterface
