import React from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaPlus } from "react-icons/fa6"
import Dropdown from "../../components/Dropdown"
import { useTooltipControlContext } from "@/TooltipContext"
import { CreateMirabufFromUrl } from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import Button from "@/components/Button"
import Label from "@/components/Label"

interface RobotCardProps {
    robot: string;
}

const RobotCard: React.FC<RobotCardProps> = ({ robot }) => {
    return (
        <div className="bg-background-secondary rounded-sm p-2">
            <Label>{robot}</Label>
            <Button value="Spawn"></Button>
        </div>
    )
}

const RobotsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    // update tooltip based on type of drivetrain, receive message from Synthesis
    const { showTooltip } = useTooltipControlContext()

    let selectedRobot: string | null = null;

    const robots = ["Dozer_v2.mira", "Team_2471_(2018)_v7.mira"]

    return (
        <Modal
            name={"Robot Selection"}
            icon={<FaPlus />}
            modalId={modalId}
            onAccept={() => {
                    showTooltip("controls", [
                        { control: "WASD", description: "Drive" },
                        { control: "E", description: "Intake" },
                        { control: "Q", description: "Dispense" },
                    ]);

                    if (selectedRobot) {
                        CreateMirabufFromUrl(`test_mira/${selectedRobot}`).then(x => {
                            if (x) {
                                World.SceneRenderer.RegisterSceneObject(x);
                            }
                        });
                    }
                }
            }
        >
            <div className="flex flex-col gap-2">
                {robots.map(x => RobotCard({robot: x}))}
            </div>
            {/* <Dropdown
                options={["Dozer_v2.mira", "Team_2471_(2018)_v7.mira"]}
                onSelect={(op) => {
                    selectedRobot = op;
                }}
            /> */}
        </Modal>
    )
}

export default RobotsModal
