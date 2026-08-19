import { Stack, Tooltip } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useMemo, useReducer, useState } from "react"
import { FaUnlink } from "react-icons/fa"
import EventSystem from "@/systems/EventSystem.ts"
import { defaultSequentialConfig, type SequentialBehaviorPreferences } from "@/systems/preferences/PreferenceTypes"
import GenericArmBehavior from "@/systems/simulation/behavior/synthesis/GenericArmBehavior"
import SequenceableBehavior from "@/systems/simulation/behavior/synthesis/SequenceableBehavior"
import type SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import Label from "@/ui/components/Label"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import { Button, Spacer } from "@/ui/components/StyledComponents"
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

interface BehaviorCardProps {
    elementKey: number
    name: string
    parentName: string | undefined
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
    parentName,
    behavior,
    update,
    onSetPressed,
    lookingForParent,
    onBehaviorSelected,
    hasChild,
}) => {
    const [selectable, setSelectable] = useState(false)
    useEffect(() => {
        setSelectable(
            lookingForParent !== undefined && lookingForParent !== behavior && behavior.parentJointIndex === undefined
        )
    }, [lookingForParent, behavior])
    const hasParent = behavior.parentJointIndex !== undefined

    return (
        <Stack direction="row" textAlign="center" gap={1} key={elementKey}>
            {hasParent && <Spacer width={10} />}
            <Tooltip title={hasParent ? `Following ${parentName}` : selectable ? "Set as parent" : ""}>
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

    const sortedBehaviors: SequentialBehaviorPreferences[] = behaviors.filter(b => b.parentJointIndex === undefined)

    for (let i = behaviors.length - 1; i >= 0; i--) {
        const b = behaviors[i]
        if (b.parentJointIndex === undefined) continue
        const parentIndex = sortedBehaviors.findIndex(sb => b.parentJointIndex === sb.jointIndex)
        if (parentIndex === -1) throw new Error("Parent behavior not found!")
        sortedBehaviors.splice(parentIndex + 1, 0, b)
    }

    return sortedBehaviors
}

const ConfigureJointsInterface: ConfigurationSubpanelComponent = ({ selectedAssembly, registerCleanupFunction }) => {
    const [selectedGroup, setSelectedGroup] = useState<JointGroupSelectionOption | undefined>(undefined)

    const behaviors = useMemo<SequentialBehaviorPreferences[]>(
        () =>
            selectedAssembly.robotPreferences.sequentialConfig ??
            (selectedAssembly.brain as SynthesisBrain).behaviors
                .filter(b => b instanceof SequenceableBehavior)
                .map(b => defaultSequentialConfig(b.jointIndex, b instanceof GenericArmBehavior ? "Arm" : "Elevator")),
        [selectedAssembly]
    )

    // Covers both sections below: motor config mutates `unstickStrength` and the
    // `inverted` flags inside `sequentialConfig`, sequencing mutates `parentJointIndex`.
    useEffect(() => {
        const originalPrefs = structuredClone(selectedAssembly.robotPreferences.sequentialConfig)
        const originalUnstickStrength = selectedAssembly.robotPreferences.unstickStrength
        registerCleanupFunction(undefined, () => {
            selectedAssembly.robotPreferences.sequentialConfig = originalPrefs
            selectedAssembly.robotPreferences.unstickStrength = originalUnstickStrength
        })
    }, [registerCleanupFunction, selectedAssembly])

    const options = useMemo(
        () => buildJointConfigGroups(selectedAssembly, behaviors).map(g => new JointGroupSelectionOption(g)),
        [selectedAssembly, behaviors]
    )

    // reusing the joint names from the config groups
    const jointNamesByIndex = useMemo(() => {
        const names = new Map<number, string>()
        for (const option of options) {
            const sequential = option.group.sequential
            if (sequential !== undefined) names.set(sequential.jointIndex, option.group.name)
        }
        return names
    }, [options])

    const getJointDisplayName = useCallback(
        (behavior: SequentialBehaviorPreferences) =>
            jointNamesByIndex.get(behavior.jointIndex) ??
            (behavior.type === "Arm"
                ? `Joint ${behavior.jointIndex} (Pivot)`
                : `Joint ${behavior.jointIndex} (Slider)`),
        [jointNamesByIndex]
    )

    const [seqBehaviors, setSeqBehaviors] = useState<SequentialBehaviorPreferences[]>(
        selectedAssembly.robotPreferences.sequentialConfig ??
            (selectedAssembly.brain as SynthesisBrain).behaviors
                .filter(b => b instanceof SequenceableBehavior)
                .map(b => defaultSequentialConfig(b.jointIndex, b instanceof GenericArmBehavior ? "Arm" : "Elevator"))
    )
    const [lookingForParent, setLookingForParent] = useState<SequentialBehaviorPreferences | undefined>(undefined)

    const [_, update] = useReducer(x => {
        setSeqBehaviors(sortBehaviors(seqBehaviors))
        return !x
    }, false)

    const saveEvent = useCallback(() => {
        if (selectedAssembly === undefined || seqBehaviors === undefined) return
        selectedAssembly.robotPreferences.sequentialConfig = seqBehaviors
        selectedAssembly.savePreferences()
    }, [seqBehaviors, selectedAssembly])

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
                    setSelectedGroup(val)
                }}
                defaultHeaderText="Select a Joint"
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

            {/* Section 2: Joint Sequencing */}
            {seqBehaviors.length > 0 && (
                <>
                    <Label size="md" sx={{ mt: 3, mb: 0.5 }}>
                        Joint Sequencing
                    </Label>
                    <Stack direction="column" gap={2} className="overflow-y-auto">
                        {seqBehaviors.map(behavior => {
                            const jointIndex = behavior.jointIndex
                            const parentBehavior =
                                behavior.parentJointIndex !== undefined
                                    ? seqBehaviors.find(b => b.jointIndex === behavior.parentJointIndex)
                                    : undefined
                            return (
                                <BehaviorCard
                                    elementKey={jointIndex}
                                    name={getJointDisplayName(behavior)}
                                    parentName={parentBehavior ? getJointDisplayName(parentBehavior) : undefined}
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
