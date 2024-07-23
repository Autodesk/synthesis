import React, { useMemo, useReducer, useState } from "react"
import Panel, { PanelPropsImpl } from "../components/Panel"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import Label, { LabelSize } from "../components/Label"
import World from "@/systems/World"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { FaArrowLeft, FaArrowRight, FaChevronDown, FaChevronUp, FaGear } from "react-icons/fa6"
import { Box, Button as MUIButton, Divider, styled } from "@mui/material"
import Button from "../components/Button"
import GenericArmBehavior from "@/systems/simulation/behavior/GenericArmBehavior"
import GenericElevatorBehavior from "@/systems/simulation/behavior/GenericElevatorBehavior"
import Behavior from "@/systems/simulation/behavior/Behavior"

const ShiftRight = <FaArrowRight size={"1.25rem"} />
const ShiftLeft = <FaArrowLeft size={"1.25rem"} />
const UpArrow = <FaChevronUp size={"1.25rem"} />
const DownArrow = <FaChevronDown size={"1.25rem"} />

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

function shiftArrayElementUp<T>(arr: T[], elem: T): void {
    const index = arr.indexOf(elem)
    if (index > 0 && index < arr.length) {
        let temp = arr[index]
        arr[index] = arr[index - 1]
        arr[index - 1] = temp
    }
}

function shiftArrayElementDown<T>(arr: T[], elem: T): void {
    const index = arr.indexOf(elem)
    if (index >= 0 && index < arr.length - 1) {
        let temp = arr[index]
        arr[index] = arr[index + 1]
        arr[index + 1] = temp
    }
}

type BehaviorConfiguration = {
    behavior: Behavior
    child: boolean
}

interface BehaviorCardProps {
    elementKey: number
    name: string
    behavior: BehaviorConfiguration
    update: () => void
    shiftUp: () => void
    shiftDown: () => void
    canShiftUp: boolean
    canShiftDown: boolean
    canBecomeChild: boolean
}

const BehaviorCard: React.FC<BehaviorCardProps> = ({
    elementKey,
    name,
    behavior,
    update,
    shiftUp,
    shiftDown,
    canShiftUp,
    canShiftDown,
    canBecomeChild,
}) => {
    return (
        <Box display="flex" textAlign={"center"} key={elementKey}>
            <Box position="absolute" alignSelf={"center"} display="flex">
                {behavior.child ? (
                    <>
                        <Box width="25px" />
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
            <MUIButton fullWidth={true}></MUIButton>
            <Box display="flex" gap="3px">
                <>
                    <Button
                        key="up"
                        sizeOverrideClass="px-1 py-0.5"
                        value={UpArrow}
                        onClick={() => {
                            shiftUp()
                            update()
                        }}
                        colorOverrideClass={canShiftUp ? undefined : "disabled"}
                    />
                    <Button
                        key="down"
                        sizeOverrideClass="px-1 py-0.5"
                        value={DownArrow}
                        onClick={() => {
                            shiftDown()
                            update()
                        }}
                        colorOverrideClass={canShiftDown ? undefined : "disabled"}
                    />

                    {behavior.child ? (
                        <Button
                            key="shift"
                            sizeOverrideClass="px-1 py-0.5"
                            value={ShiftLeft}
                            onClick={() => {
                                behavior.child = false
                                update()
                            }}
                        />
                    ) : (
                        <Button
                            key="shift"
                            sizeOverrideClass="px-1 py-0.5"
                            value={ShiftRight}
                            onClick={() => {
                                if (canBecomeChild) {
                                    behavior.child = true
                                    update()
                                }
                            }}
                            colorOverrideClass={canBecomeChild ? undefined : "disabled"}
                        />
                    )}
                </>
            </Box>
        </Box>
    )
}

function GetBehaviorCards(behaviors: BehaviorConfiguration[], update: () => void): JSX.Element[] {
    const output: JSX.Element[] = []
    let elementKey = 0

    /* Iterate through each behavior of the robot */
    behaviors.forEach(behavior => {
        if (behavior.behavior instanceof GenericArmBehavior) {
            output.push(
                <BehaviorCard
                    elementKey={elementKey}
                    name={`Joint ${behavior.behavior.jointIndex} (Arm)`}
                    behavior={behavior}
                    key={elementKey}
                    update={update}
                    shiftUp={() => {
                        shiftArrayElementUp(behaviors, behavior)
                    }}
                    shiftDown={() => {
                        shiftArrayElementDown(behaviors, behavior)
                    }}
                    canShiftUp={behaviors.indexOf(behavior) > 0}
                    canShiftDown={behaviors.indexOf(behavior) < behaviors.length - 1}
                    canBecomeChild={(() => {
                        const index = behaviors.indexOf(behavior)
                        if (index < behaviors.length - 1 && behaviors[index + 1].child) return false

                        for (let i = index; i > 0; i--)
                            if (!behaviors[i].child) {
                                return true
                            }
                        return false
                    })()}
                />
            )
            elementKey++
        } else if (behavior.behavior instanceof GenericElevatorBehavior) {
            /* Adds the joints that the elevator is associated with */
            // Get the rigid node associates for the two bodies
            output.push(
                <BehaviorCard
                    elementKey={elementKey}
                    name={`Joint ${behavior.behavior.jointIndex} (Elevator)`}
                    behavior={behavior}
                    key={elementKey}
                    update={update}
                    shiftUp={() => {
                        shiftArrayElementUp(behaviors, behavior)
                    }}
                    shiftDown={() => {
                        shiftArrayElementDown(behaviors, behavior)
                    }}
                    canShiftUp={behaviors.indexOf(behavior) > 0}
                    canShiftDown={behaviors.indexOf(behavior) < behaviors.length - 1}
                    canBecomeChild={(() => {
                        const index = behaviors.indexOf(behavior)
                        if (index < behaviors.length - 1 && behaviors[index + 1].child) return false

                        for (let i = index; i > 0; i--)
                            if (!behaviors[i].child) {
                                return true
                            }
                        return false
                    })()}
                />
            )
            elementKey++
        }
    })

    return output
}

const SequentialJointsPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const [selectedRobot, setSelectedRobot] = useState<MirabufSceneObject | undefined>(undefined)
    const [behaviors, setBehaviors] = useState<BehaviorConfiguration[] | undefined>(undefined)

    const robots = useMemo(() => {
        const assemblies = [...World.SceneRenderer.sceneObjects.values()].filter(x => {
            if (x instanceof MirabufSceneObject) {
                return x.miraType === MiraType.ROBOT
            }
            return false
        }) as MirabufSceneObject[]
        return assemblies
    }, [])

    const [_, update] = useReducer(x => !x, false)

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
                                                    return { behavior: b, child: false }
                                                })
                                                .filter(
                                                    b =>
                                                        b.behavior instanceof GenericElevatorBehavior ||
                                                        b.behavior instanceof GenericArmBehavior
                                                )
                                        )
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
                        Behaviors
                    </LabelStyled>
                    <DividerStyled />
                    {GetBehaviorCards(behaviors, update)}
                </>
            )}
        </Panel>
    )
}

export default SequentialJointsPanel
