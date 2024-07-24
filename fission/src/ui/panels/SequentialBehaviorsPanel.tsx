import React, { useMemo, useReducer, useState } from "react"
import Panel, { PanelPropsImpl } from "../components/Panel"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import Label, { LabelSize } from "../components/Label"
import World from "@/systems/World"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { FaGear, FaXmark } from "react-icons/fa6"
import { Box, Button as MUIButton, Divider, styled, alpha } from "@mui/material"
import Button, { ButtonSize } from "../components/Button"
import GenericArmBehavior from "@/systems/simulation/behavior/GenericArmBehavior"
import { SequenceableBehavior } from "@/systems/simulation/behavior/Behavior"

const UnselectParent = <FaXmark size={"1.25rem"} />

/** White label for a behavior name */
const LabelStyled = styled(Label)({
    fontWeight: 700,
    margin: "0pt",
})

/** Grey label for a child behavior name */
const ChildLabelStyled = styled(Label)({
    fontWeight: 700,
    margin: "0pt",
    color: "grey",
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

type BehaviorConfiguration = {
    behavior: SequenceableBehavior
    parent: BehaviorConfiguration | undefined
}

interface BehaviorCardProps {
    elementKey: number
    name: string
    // The behavior displayed in this card
    behavior: BehaviorConfiguration
    // The behavior whose 'set' button was just pressed
    lookingForParent: BehaviorConfiguration | undefined
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
                <Box width={behavior.parent != undefined || lookingForParent == behavior ? "25px" : "8px"} />
                {/* Label for joint index and type (grey if child) */}
                {behavior.parent != undefined ? (
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
                disabled={lookingForParent == undefined || lookingForParent == behavior || behavior.parent != undefined}
                sx={{
                    borderColor:
                        lookingForParent == undefined || lookingForParent == behavior || behavior.parent != undefined
                            ? "transparent"
                            : "#888888",
                }}
            />

            {/* Spacer between the CustomButton and 'set' button */}
            <Box width={"8px"} />

            {/* Button to set the parent of this behavior */}
            <Button
                key="set"
                size={ButtonSize.Small}
                value={lookingForParent == behavior || behavior.parent != undefined ? UnselectParent : "set"}
                onClick={() => {
                    if (hasChild) return

                    onSetPressed()
                    update()
                }}
                colorOverrideClass={hasChild ? "bg-background-secondary hover:brightness-100" : undefined}
            />
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
function sortBehaviors(behaviors: BehaviorConfiguration[] | undefined): BehaviorConfiguration[] | undefined {
    if (behaviors == undefined) return undefined

    // Sort the behaviors in order of joint index
    behaviors.sort((a, b) => {
        return a.behavior.jointIndex - b.behavior.jointIndex
    })

    const sortedBehaviors: BehaviorConfiguration[] = []

    // Append all parent behaviors to the sorted list
    behaviors.forEach(b => {
        if (b.parent == undefined) sortedBehaviors.push(b)
    })

    // Append all child behaviors to the sorted list directly after their parent
    // This loop is backwards so that the children say in the right order
    for (let i = behaviors.length - 1; i >= 0; i--) {
        const b = behaviors[i]

        // Skip parent behaviors (they were added to the array in the previous step)
        if (b.parent == undefined) continue

        const parentIndex = sortedBehaviors.findIndex(sb => b.parent == sb)

        if (parentIndex == -1) throw new Error("Parent behavior not found!")

        sortedBehaviors.splice(parentIndex + 1, 0, b)
    }

    return sortedBehaviors
}

const SequentialBehaviorsPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const [selectedRobot, setSelectedRobot] = useState<MirabufSceneObject | undefined>(undefined)
    const [behaviors, setBehaviors] = useState<BehaviorConfiguration[] | undefined>(undefined)
    const [lookingForParent, setLookingForParent] = useState<BehaviorConfiguration | undefined>(undefined)

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
            acceptEnabled={false}
            cancelName="Close"
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
                                        setBehaviors(
                                            mirabufSceneObject.brain?.behaviors
                                                .filter(b => "jointIndex" in b)
                                                .map(b => {
                                                    return {
                                                        behavior: b as SequenceableBehavior,
                                                        parent: undefined,
                                                    }
                                                })
                                        )
                                        update()
                                    }}
                                    key={mirabufSceneObject.id}
                                ></Button>
                            )
                        })}
                    </div>
                </>
            ) : (
                <>
                    <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                        Set Parent Behaviors
                    </LabelStyled>
                    <DividerStyled />
                    {behaviors.map(behavior => {
                        const jointIndex = behavior.behavior.jointIndex
                        return (
                            <BehaviorCard
                                elementKey={jointIndex}
                                name={
                                    behavior.behavior instanceof GenericArmBehavior
                                        ? `Joint ${jointIndex} (Arm)`
                                        : `Joint ${jointIndex} (Elevator)`
                                }
                                behavior={behavior}
                                key={jointIndex}
                                update={update}
                                onSetPressed={() => {
                                    if (behavior.parent != undefined) {
                                        behavior.parent = undefined
                                        update()
                                    } else {
                                        setLookingForParent(lookingForParent == behavior ? undefined : behavior)
                                    }
                                    update()
                                }}
                                lookingForParent={lookingForParent}
                                onBehaviorSelected={() => {
                                    if (lookingForParent) lookingForParent.parent = behavior
                                    setLookingForParent(undefined)
                                    update()
                                }}
                                hasChild={behaviors.some(b => b.parent == behavior)}
                            />
                        )
                    })}
                </>
            )}
        </Panel>
    )
}

export default SequentialBehaviorsPanel
