import React from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaPlus } from "react-icons/fa6"
import { useTooltipControlContext } from "@/TooltipContext"
import { CreateMirabufFromUrl } from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import Button from "@/components/Button"
import Label from "@/components/Label"
import { useModalControlContext } from "@/ModalContext"

interface RobotCardProps {
    robot: string;
    select: (robot: string) => void;
}

const RobotCard: React.FC<RobotCardProps> = ({ robot, select }) => {
    return (
        <div className="flex flex-row align-middle justify-between items-center bg-background rounded-sm p-2 gap-2">
            <Label className="text-wrap break-all">{robot}</Label>
            <Button
                value="Spawn"
                onClick={() => select(robot)}
            />
        </div>
    )
}

const RobotsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    // update tooltip based on type of drivetrain, receive message from Synthesis
    const { showTooltip } = useTooltipControlContext()
    const { closeModal } = useModalControlContext()

    const robots = ["Dozer_v2.mira", "Team_2471_(2018)_v7.mira"]

    const selectRobot = (robot: string) => {
        showTooltip("controls", [
            { control: "WASD", description: "Drive" },
            { control: "E", description: "Intake" },
            { control: "Q", description: "Dispense" },
        ])

        CreateMirabufFromUrl(`test_mira/${robot}`).then(x => {
            if (x) {
                World.SceneRenderer.RegisterSceneObject(x)
            }
        })

        closeModal()
    }

    return (
        <Modal
            name={"Robot Selection"}
            icon={<FaPlus />}
            modalId={modalId}
            acceptEnabled={false}
        >
            <div className="flex flex-col gap-2 min-w-[50vw] max-h-[60vh] bg-background-secondary rounded-md p-2">
                {robots.map(x => RobotCard({robot: x, select: selectRobot}))}
            </div>
        </Modal>
    )
}

export default RobotsModal
