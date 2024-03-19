import React from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaPlus } from "react-icons/fa6"
import Dropdown from "../../components/Dropdown"
import { useTooltipControlContext } from "@/TooltipContext"
import { CreateMirabufFromUrl } from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"

const RobotsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    // update tooltip based on type of drivetrain, receive message from Synthesis
    const { showTooltip } = useTooltipControlContext()

    let selectedRobot: string | null = null;

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
            <Dropdown
                options={["Dozer_v2.mira", "Team_2471_(2018)_v7.mira"]}
                onSelect={(op) => {
                    selectedRobot = op;
                }}
            />
        </Modal>
    )
}

export default RobotsModal
