import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject, { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import Label, { LabelSize } from "@/ui/components/Label"
import Button from "@/ui/components/Button"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import { useMemo, useState } from "react"
import { FaGear } from "react-icons/fa6"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import { Divider, styled } from "@mui/material"
import GenericElevatorBehavior from "@/systems/simulation/behavior/synthesis/GenericElevatorBehavior"
import Stack, { StackDirection } from "@/ui/components/Stack"
import { JSX } from "react/jsx-runtime"
import ArcadeDriveBehavior from "@/systems/simulation/behavior/synthesis/ArcadeDriveBehavior"
import GenericArmBehavior from "@/systems/simulation/behavior/synthesis/GenericArmBehavior"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"

// eslint-disable-next-line react-refresh/only-export-components
export enum ConfigureRobotBrainTypes {
    SYNTHESIS = 0,
    WIPLIB = 1,
}

const LabelStyled = styled(Label)({
    fontWeight: 700,
    margin: "0pt",
})

const DividerStyled = styled(Divider)({
    borderColor: "white",
})

/**
 * Retrieves the joints of a robot and generates JSX elements for each joint.
 * @param robot The MirabufSceneObject representing the robot.
 * @returns An array of JSX elements representing the joints of the robot.
 */
function GetJoints(robot: MirabufSceneObject): JSX.Element[] {
    const output: JSX.Element[] = []
    let elementKey = 0

    /* Iterate through each behavior of the robot */
    const brain = robot.brain as SynthesisBrain
    brain.behaviors.forEach(behavior => {
        /* Adds the joints that the wheels are associated with */
        if (behavior instanceof ArcadeDriveBehavior) {
            behavior.wheels.forEach(wheel => {
                const assoc = World.PhysicsSystem.GetBodyAssociation(
                    wheel.constraint.GetVehicleBody().GetID()
                ) as RigidNodeAssociate

                if (!assoc || assoc.sceneObject !== robot) {
                    return
                }

                output.push(
                    <Stack
                        key={`Behavior ${elementKey}`}
                        direction={StackDirection.Horizontal}
                        className="items-center"
                    >
                        <LabelStyled
                            key={`wheel-node-notation ${elementKey}`}
                            size={LabelSize.Small}
                            className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
                        >
                            Wheel Node {elementKey}
                        </LabelStyled>
                        <LabelStyled
                            key={`wheel-node-number ${elementKey}`}
                            size={LabelSize.Small}
                            className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
                        >
                            {assoc.rigidNodeId}
                        </LabelStyled>
                    </Stack>
                )
                elementKey++
            })
            output.push(<DividerStyled key={`divider-${elementKey}`} />)
        } else if (behavior instanceof GenericArmBehavior) {
            /* Adds the joints that the arm is associated with */
            // Get the rigid node associates for the two bodies
            const assoc1 = World.PhysicsSystem.GetBodyAssociation(
                behavior.hingeDriver.constraint.GetBody1().GetID()
            ) as RigidNodeAssociate
            const assoc2 = World.PhysicsSystem.GetBodyAssociation(
                behavior.hingeDriver.constraint.GetBody2().GetID()
            ) as RigidNodeAssociate

            if (!assoc1 || assoc1.sceneObject !== robot || !assoc2 || assoc2.sceneObject !== robot) {
                return
            }

            output.push(
                <Stack key={`Behavior ${elementKey}`} direction={StackDirection.Horizontal} className="items-center">
                    <LabelStyled
                        key={`arm-nodes-notation ${elementKey}`}
                        size={LabelSize.Small}
                        className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
                    >
                        Arm Nodes
                    </LabelStyled>
                    <LabelStyled
                        key={`arm-nodes ${elementKey}`}
                        size={LabelSize.Small}
                        className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
                    >
                        {assoc1.rigidNodeId + " " + assoc2.rigidNodeId}
                    </LabelStyled>
                </Stack>
            )
            elementKey++
        } else if (behavior instanceof GenericElevatorBehavior) {
            /* Adds the joints that the elevator is associated with */
            // Get the rigid node associates for the two bodies
            const assoc1 = World.PhysicsSystem.GetBodyAssociation(
                behavior.sliderDriver.constraint.GetBody1().GetID()
            ) as RigidNodeAssociate
            const assoc2 = World.PhysicsSystem.GetBodyAssociation(
                behavior.sliderDriver.constraint.GetBody2().GetID()
            ) as RigidNodeAssociate

            if (!assoc1 || assoc1.sceneObject !== robot || !assoc2 || assoc2.sceneObject !== robot) {
                return
            }

            output.push(
                <Stack key={`Behavior ${elementKey}`} direction={StackDirection.Horizontal} className="items-center">
                    <LabelStyled
                        key={`elevator-nodes-notation ${elementKey}`}
                        size={LabelSize.Small}
                        className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
                    >
                        Elevator Nodes
                    </LabelStyled>
                    <LabelStyled
                        key={`elevator-nodes ${elementKey}`}
                        size={LabelSize.Small}
                        className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
                    >
                        {assoc1.rigidNodeId + " " + assoc2.rigidNodeId}
                    </LabelStyled>
                </Stack>
            )
            elementKey++
        }
    })

    return output
}

const ConfigureRobotBrainPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const [selectedRobot, setSelectedRobot] = useState<MirabufSceneObject | undefined>(undefined)
    const [viewType, setViewType] = useState<ConfigureRobotBrainTypes>(ConfigureRobotBrainTypes.SYNTHESIS)
    const robots = useMemo(() => {
        const assemblies = [...World.SceneRenderer.sceneObjects.values()].filter(x => {
            if (x instanceof MirabufSceneObject) {
                return x.miraType === MiraType.ROBOT
            }
            return false
        }) as MirabufSceneObject[]
        return assemblies
    }, [])

    return (
        <Panel
            name="Configure Robot Brain"
            icon={<FaGear />}
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            onAccept={() => {}}
            onCancel={() => {}}
        >
            {selectedRobot?.ejectorPreferences == undefined ? (
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
                                    }}
                                    key={mirabufSceneObject.id}
                                ></Button>
                            )
                        })}
                    </div>
                    {/* TODO: remove the accept button on this version */}
                </>
            ) : (
                <>
                    <div className="flex flex-col gap-2 bg-background-secondary rounded-md p-2">
                        <ToggleButtonGroup
                            value={viewType}
                            exclusive
                            onChange={(_, v) => v != null && setViewType(v)}
                            sx={{
                                alignSelf: "center",
                            }}
                        >
                            <ToggleButton value={ConfigureRobotBrainTypes.SYNTHESIS}>SynthesisBrain</ToggleButton>
                            <ToggleButton value={ConfigureRobotBrainTypes.WIPLIB}>WIPLIBBrain</ToggleButton>
                        </ToggleButtonGroup>
                        {viewType === ConfigureRobotBrainTypes.SYNTHESIS ? (
                            <>
                                <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                    Behaviors
                                </LabelStyled>
                                <DividerStyled />
                                {GetJoints(selectedRobot)}
                            </>
                        ) : (
                            <>
                                <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                    Example WIPLIB Brain
                                </LabelStyled>
                                <DividerStyled />

                                <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                    Example 2
                                </LabelStyled>
                                <DividerStyled />

                                <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                    Example 3
                                </LabelStyled>
                                <DividerStyled />

                                <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                    Example 4
                                </LabelStyled>
                                <DividerStyled />
                            </>
                        )}
                    </div>
                </>
            )}
        </Panel>
    )
}

export default ConfigureRobotBrainPanel
