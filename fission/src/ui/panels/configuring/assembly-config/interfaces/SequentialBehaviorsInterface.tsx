import { Stack, Tooltip } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useReducer, useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { defaultSequentialConfig, type SequentialBehaviorPreferences } from "@/systems/preferences/PreferenceTypes"
import GenericArmBehavior from "@/systems/simulation/behavior/synthesis/GenericArmBehavior"
import SequenceableBehavior from "@/systems/simulation/behavior/synthesis/SequenceableBehavior"
import type SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { Button, Spacer, SynthesisIcons } from "@/ui/components/StyledComponents"

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
            {/* Button used for selecting a parent (shows up as an outline) */}
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

                        // sx={hasChild ? { bgcolor: "background.default", "&:hover": { filter: "brightness(100%)" } } : {}}
                    >
                        {hasParent ? <SynthesisIcons.UNLINK /> : lookingForParent == behavior ? "Cancel" : "Follow"}
                    </Button>
                </div>
            </Tooltip>
        </Stack>
    )
}

/** Groups behaviors by putting children after parents, sorting by joint index as much as possible
 *
 * @param behaviors a list of behaviors sorted in any order
 *
 * Example:
 *
 * Joint 1
 * * Joint 3 (child of 1)
 * * Joint 5 (child of 1)
 *
 * Joint 4
 * * Joint 2 (child of 4)
 */
function sortBehaviors(behaviors: SequentialBehaviorPreferences[]): SequentialBehaviorPreferences[] {
    // Sort the behaviors in order of joint index
    behaviors.sort((a, b) => {
        return a.jointIndex - b.jointIndex
    })

    const sortedBehaviors: SequentialBehaviorPreferences[] = []

    // Append all parent behaviors to the sorted list
    behaviors.forEach(b => {
        if (b.parentJointIndex === undefined) sortedBehaviors.push(b)
    })

    // Append all child behaviors to the sorted list directly after their parent
    // This loop is backwards so that the children say in the right order
    for (let i = behaviors.length - 1; i >= 0; i--) {
        const b = behaviors[i]

        // Skip parent behaviors (they were added to the array in the previous step)
        if (b.parentJointIndex === undefined) continue

        const parentIndex = sortedBehaviors.findIndex(sb => b.parentJointIndex === sb.jointIndex)

        if (parentIndex === -1) throw new Error("Parent behavior not found!")

        sortedBehaviors.splice(parentIndex + 1, 0, b)
    }

    return sortedBehaviors
}

interface SequentialBehaviorProps {
    selectedRobot: MirabufSceneObject
}

const SequentialBehaviorsInterface: React.FC<SequentialBehaviorProps> = ({ selectedRobot }) => {
    const [behaviors, setBehaviors] = useState<SequentialBehaviorPreferences[]>(
        PreferencesSystem.getRobotPreferences(selectedRobot.assemblyName)?.sequentialConfig ??
            (selectedRobot.brain as SynthesisBrain).behaviors
                .filter(b => b instanceof SequenceableBehavior)
                .map(b => defaultSequentialConfig(b.jointIndex, b instanceof GenericArmBehavior ? "Arm" : "Elevator"))
    )
    const [lookingForParent, setLookingForParent] = useState<SequentialBehaviorPreferences | undefined>(undefined)

    const [_, update] = useReducer(x => {
        setBehaviors(sortBehaviors(behaviors))
        return !x
    }, false)

    const saveEvent = useCallback(() => {
        if (selectedRobot === undefined || behaviors === undefined) return

        PreferencesSystem.getRobotPreferences(selectedRobot.assemblyName).sequentialConfig = behaviors
        PreferencesSystem.savePreferences()
    }, [behaviors, selectedRobot])

    useEffect(() => {
        return EventSystem.listen("ConfigurationSavedEvent", saveEvent)
    }, [saveEvent])

    return (
        <Stack direction="column" gap={2} className="overflow-y-auto bg-background-secondary">
            {behaviors.map(behavior => {
                const jointIndex = behavior.jointIndex
                return (
                    <BehaviorCard
                        elementKey={jointIndex}
                        name={behavior.type === "Arm" ? `Joint ${jointIndex} (Pivot)` : `Joint ${jointIndex} (Slider)`}
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
                        hasChild={behaviors.some(b => b.parentJointIndex === behavior.jointIndex)}
                    />
                )
            })}
        </Stack>
    )
}

export default SequentialBehaviorsInterface
