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
import GenericElevatorBehavior from "@/systems/simulation/behavior/GenericElevatorBehavior"
import Behavior from "@/systems/simulation/behavior/Behavior"

const UnselectParent = <FaXmark size={"1.25rem"} />

const LabelStyled = styled(Label)({
    fontWeight: 700,
    margin: "0pt",
})

const ChildLabelStyled = styled(Label)({
    fontWeight: 700,
    margin: "0pt",
    color: "grey",
})

const DividerStyled = styled(Divider)({
    borderColor: "white",
})

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
    behavior: Behavior
    parent: BehaviorConfiguration | undefined
}

interface BehaviorCardProps {
    elementKey: number
    name: string
    behavior: BehaviorConfiguration
    update: () => void
    onParentPressed: () => void
    parentPressed: BehaviorConfiguration | undefined
    onBehaviorSelected: () => void
    hasChild: boolean
}

const BehaviorCard: React.FC<BehaviorCardProps> = ({
    elementKey,
    name,
    behavior,
    update,
    onParentPressed,
    parentPressed,
    onBehaviorSelected,
    hasChild,
}) => {
    return (
        <Box display="flex" textAlign={"center"} key={elementKey}>
            <Box position="absolute" alignSelf={"center"} display="flex">
                <Box width={behavior.parent != undefined || parentPressed == behavior ? "25px" : "8px"} />
                {behavior.parent != undefined ? (
                    <>
                        <ChildLabelStyled
                            key={`arm-nodes-notation ${elementKey}`}
                            size={LabelSize.Small}
                            className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
                        >
                            {name}
                        </ChildLabelStyled>
                    </>
                ) : (
                    <>
                        <LabelStyled
                            key={`arm-nodes-notation ${elementKey}`}
                            size={LabelSize.Small}
                            className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
                        >
                            {name}
                        </LabelStyled>
                    </>
                )}
            </Box>
            <CustomButton
                fullWidth={true}
                onClick={() => {
                    onBehaviorSelected()
                    update()
                }}
                disabled={parentPressed == undefined || parentPressed == behavior || behavior.parent != undefined}
                sx={{
                    borderColor:
                        parentPressed == undefined || parentPressed == behavior || behavior.parent != undefined
                            ? "transparent"
                            : "#888888",
                }}
            />
            <Box display="flex" gap="3px">
                <Box width={"5px"} />

                <Button
                    key="set"
                    size={ButtonSize.Small}
                    value={parentPressed == behavior || behavior.parent != undefined ? UnselectParent : "set"}
                    onClick={() => {
                        if (hasChild) return

                        onParentPressed()
                        update()
                    }}
                    colorOverrideClass={hasChild ? "bg-background-secondary hover:brightness-100" : undefined}
                />
            </Box>
        </Box>
    )
}

function sortBehaviors(behaviors: BehaviorConfiguration[] | undefined): BehaviorConfiguration[] | undefined {
    if (behaviors == undefined) return undefined
    behaviors.sort((a, b) => {
        if (!(a.behavior instanceof GenericArmBehavior) && !(a.behavior instanceof GenericElevatorBehavior))
            throw new Error("Wrong behavior type")
        if (!(b.behavior instanceof GenericArmBehavior) && !(b.behavior instanceof GenericElevatorBehavior))
            throw new Error("Wrong behavior type")

        return a.behavior.jointIndex - b.behavior.jointIndex
    })

    const sortedBehaviors: BehaviorConfiguration[] = []

    behaviors.forEach(b => {
        if (b.parent == undefined) sortedBehaviors.push(b)
    })

    for (let i = behaviors.length - 1; i >= 0; i--) {
        const b = behaviors[i]

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
                                                .map(b => {
                                                    return { behavior: b, parent: undefined }
                                                })
                                                .filter(
                                                    b =>
                                                        b.behavior instanceof GenericElevatorBehavior ||
                                                        b.behavior instanceof GenericArmBehavior
                                                )
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
                        if (
                            behavior.behavior instanceof GenericArmBehavior ||
                            behavior.behavior instanceof GenericElevatorBehavior
                        ) {
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
                                    onParentPressed={() => {
                                        if (behavior.parent != undefined) {
                                            behavior.parent = undefined
                                            update()
                                        } else {
                                            setLookingForParent(lookingForParent == behavior ? undefined : behavior)
                                        }
                                        update()
                                    }}
                                    parentPressed={lookingForParent}
                                    onBehaviorSelected={() => {
                                        if (lookingForParent) lookingForParent.parent = behavior
                                        setLookingForParent(undefined)
                                        update()
                                    }}
                                    hasChild={behaviors.some(b => b.parent == behavior)}
                                />
                            )
                        } else {
                            throw new Error("Behavior not of correct type")
                        }
                    })}
                </>
            )}
        </Panel>
    )
}

export default SequentialBehaviorsPanel
