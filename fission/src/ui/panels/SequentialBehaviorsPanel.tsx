import React, { useMemo, useReducer, useState } from "react"
import Panel, { PanelPropsImpl } from "../components/Panel"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import Label, { LabelSize } from "../components/Label"
import World from "@/systems/World"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { FaArrowRightArrowLeft, FaGear, FaXmark } from "react-icons/fa6"
import { Box, Button as MUIButton, Divider, styled, alpha, Icon } from "@mui/material"
import Button, { ButtonSize } from "../components/Button"
import GenericArmBehavior from "@/systems/simulation/behavior/GenericArmBehavior"
import { DefaultSequentialConfig, SequentialBehaviorPreferences } from "@/systems/preferences/PreferenceTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import SequenceableBehavior from "@/systems/simulation/behavior/SequenceableBehavior"
import Checkbox from "../components/Checkbox"

const UnselectParentIcon = <FaXmark size={"1.25rem"} />
const InvertIcon = <FaArrowRightArrowLeft size={"1.25rem"} style={{ transform: "rotate(90deg)" }} />

/** White label for a behavior name */
const LabelStyled = styled(Label)({
    fontWeight: 700,
    margin: "0pt",
    textWrap: "nowrap",
})

/** Grey label for a child behavior name */
const ChildLabelStyled = styled(Label)({
    fontWeight: 700,
    margin: "0pt",
    color: "#bbbbbb",
    textWrap: "nowrap",
})

const DividerStyled = styled(Divider)({
    borderColor: "white",
})

/** A button used to select a parent behavior. Appears at a grey outline when the 'set' button is pressed on a different behavior */
const CustomButton = styled(MUIButton)({
    "borderStyle": "solid",
    "borderWidth": "1px",
    "transition": "border-color 0.3s ease",
    "&:hover": {
        borderColor: "white",
    },
    "position": "relative",
    "overflow": "hidden",
    "& .MuiTouchRipple-root span": {
        backgroundColor: alpha("#ffffff", 0.3), // Set your desired ripple color here
        animationDuration: "300ms",
    },
})

interface BehaviorCardProps {
    elementKey: number
    name: string
    // The behavior displayed in this card
    behavior: SequentialBehaviorPreferences
    // The behavior whose 'set' button was just pressed
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
    return (
        /* Box containing the entire card */
        <Box display="flex" textAlign={"center"} key={elementKey}>
            {/* Box containing the label */}
            <Box position="absolute" alignSelf={"center"} display="flex">
                {/* Indentation before the name */}
                <Box width={behavior.parentJointIndex != undefined || lookingForParent == behavior ? "25px" : "8px"} />
                {/* Label for joint index and type (grey if child) */}
                {behavior.parentJointIndex != undefined ? (
                    <ChildLabelStyled
                        key={`arm-nodes-notation ${elementKey}`}
                        size={LabelSize.Small}
                        className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
                    >
                        {name}
                    </ChildLabelStyled>
                ) : (
                    <LabelStyled
                        key={`arm-nodes-notation ${elementKey}`}
                        size={LabelSize.Small}
                        className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
                    >
                        {name}
                    </LabelStyled>
                )}
            </Box>

            {/* Button used for selecting a parent (shows up as an outline) */}
            <CustomButton
                fullWidth={true}
                onClick={() => {
                    onBehaviorSelected()
                    update()
                }}
                disabled={
                    lookingForParent == undefined ||
                    lookingForParent == behavior ||
                    behavior.parentJointIndex != undefined
                }
                sx={{
                    borderColor:
                        lookingForParent == undefined ||
                        lookingForParent == behavior ||
                        behavior.parentJointIndex != undefined
                            ? "transparent"
                            : "#888888",
                }}
            />

            {/* Spacer between the CustomButton and invert button */}
            <Box width={"16px"} />

            <Box display="flex" position="relative" alignSelf={"center"} alignItems={"center"}>
                {/* Invert joint icon & checkbox */}
                <Icon>{InvertIcon}</Icon>
                <Checkbox
                    label={""}
                    defaultState={behavior.inverted}
                    onClick={val => (behavior.inverted = val)}
                    hideLabel={true}
                />
            </Box>

            <Box display="flex" position="relative" alignSelf={"center"} alignItems={"center"}>
                {/* Button to set the parent of this behavior */}
                <Button
                    key="set"
                    size={ButtonSize.Small}
                    value={
                        lookingForParent == behavior || behavior.parentJointIndex != undefined
                            ? UnselectParentIcon
                            : "set"
                    }
                    onClick={() => {
                        if (hasChild) return

                        onSetPressed()
                        update()
                    }}
                    colorOverrideClass={hasChild ? "bg-background hover:brightness-100" : undefined}
                />
            </Box>
        </Box>
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
function sortBehaviors(
    behaviors: SequentialBehaviorPreferences[] | undefined
): SequentialBehaviorPreferences[] | undefined {
    if (behaviors == undefined) return undefined

    // Sort the behaviors in order of joint index
    behaviors.sort((a, b) => {
        return a.jointIndex - b.jointIndex
    })

    const sortedBehaviors: SequentialBehaviorPreferences[] = []

    // Append all parent behaviors to the sorted list
    behaviors.forEach(b => {
        if (b.parentJointIndex == undefined) sortedBehaviors.push(b)
    })

    // Append all child behaviors to the sorted list directly after their parent
    // This loop is backwards so that the children say in the right order
    for (let i = behaviors.length - 1; i >= 0; i--) {
        const b = behaviors[i]

        // Skip parent behaviors (they were added to the array in the previous step)
        if (b.parentJointIndex == undefined) continue

        const parentIndex = sortedBehaviors.findIndex(sb => b.parentJointIndex == sb.jointIndex)

        if (parentIndex == -1) throw new Error("Parent behavior not found!")

        sortedBehaviors.splice(parentIndex + 1, 0, b)
    }

    return sortedBehaviors
}

const SequentialBehaviorsPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const [selectedRobot, setSelectedRobot] = useState<MirabufSceneObject | undefined>(undefined)
    const [behaviors, setBehaviors] = useState<SequentialBehaviorPreferences[] | undefined>(undefined)
    const [lookingForParent, setLookingForParent] = useState<SequentialBehaviorPreferences | undefined>(undefined)

    const robots = useMemo(() => {
        const assemblies = [...World.SceneRenderer.sceneObjects.values()].filter(x => {
            if (x instanceof MirabufSceneObject) {
                return x.miraType === MiraType.ROBOT
            }
            return false
        }) as MirabufSceneObject[]
        return assemblies
    }, [])

    const [_, update] = useReducer(x => {
        setBehaviors(sortBehaviors(behaviors))
        return !x
    }, false)

    return (
        <Panel
            openLocation="right"
            name={"Sequential Joints"}
            icon={<FaGear />}
            panelId={panelId}
            cancelEnabled={false}
            onAccept={() => {
                if (selectedRobot == undefined || behaviors == undefined) return
                PreferencesSystem.getRobotPreferences(selectedRobot.assemblyName).sequentialConfig = behaviors
                PreferencesSystem.savePreferences()
            }}
        >
            {selectedRobot == undefined || behaviors == undefined ? (
                <>
                    <Label>Select a robot</Label>
                    {/** Scroll view for selecting a robot to configure */}
                    <div className="flex overflow-y-auto flex-col gap-2 min-w-[20vw] max-h-[40vh] bg-background-secondary rounded-md p-2">
                        {robots.map(mirabufSceneObject => {
                            return (
                                <Button
                                    value={mirabufSceneObject.assemblyName}
                                    onClick={() => {
                                        setSelectedRobot(mirabufSceneObject)

                                        const prefs = PreferencesSystem.getRobotPreferences(
                                            mirabufSceneObject.assemblyName
                                        )
                                        if (prefs.sequentialConfig) setBehaviors(prefs.sequentialConfig)
                                        else {
                                            setBehaviors(
                                                mirabufSceneObject.brain?.behaviors
                                                    .filter(b => b instanceof SequenceableBehavior)
                                                    .map(b => {
                                                        return DefaultSequentialConfig(
                                                            b.jointIndex,
                                                            b instanceof GenericArmBehavior ? "Arm" : "Elevator"
                                                        )
                                                    })
                                            )
                                        }

                                        update()
                                    }}
                                    key={mirabufSceneObject.id}
                                ></Button>
                            )
                        })}
                    </div>
                </>
            ) : (
                <div className="flex overflow-y-auto flex-col gap-2 min-w-[20vw] max-h-[40vh] bg-background-secondary rounded-md p-2">
                    <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                        Set Parent Behaviors
                    </LabelStyled>
                    <DividerStyled />
                    {behaviors.map(behavior => {
                        const jointIndex = behavior.jointIndex
                        return (
                            <BehaviorCard
                                elementKey={jointIndex}
                                name={
                                    behavior.type == "Arm"
                                        ? `Joint ${jointIndex} (Arm)`
                                        : `Joint ${jointIndex} (Elevator)`
                                }
                                behavior={behavior}
                                key={jointIndex}
                                update={update}
                                onSetPressed={() => {
                                    if (behavior.parentJointIndex != undefined) {
                                        behavior.parentJointIndex = undefined
                                        update()
                                    } else {
                                        setLookingForParent(lookingForParent == behavior ? undefined : behavior)
                                    }
                                    update()
                                }}
                                lookingForParent={lookingForParent}
                                onBehaviorSelected={() => {
                                    if (lookingForParent) lookingForParent.parentJointIndex = behavior.jointIndex
                                    setLookingForParent(undefined)
                                    update()
                                }}
                                hasChild={behaviors.some(b => b.parentJointIndex == behavior.jointIndex)}
                            />
                        )
                    })}
                </div>
            )}
        </Panel>
    )
}

export default SequentialBehaviorsPanel
