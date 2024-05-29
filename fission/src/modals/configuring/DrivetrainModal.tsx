import React, { useState } from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaCar } from "react-icons/fa6"
import Dropdown from "../../components/Dropdown"
import { TooltipControl, useTooltipControlContext } from "@/TooltipContext"

type DrivetrainType = "None" | "Tank" | "Arcade" | "Swerve"

const DrivetrainModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { showTooltip } = useTooltipControlContext()
    const [drivetrain, setDrivetrain] = useState<DrivetrainType>("None")

    const controls: { [key in DrivetrainType]: TooltipControl[] } = {
        None: [{ control: "None", description: "Cannot Move" }],
        Tank: [
            { control: "WS", description: "Drivetrain Left" },
            { control: "IK", description: "Drivetrain Right" },
            { control: "E", description: "Intake" },
            { control: "Q", description: "Dispense" },
        ],
        Arcade: [
            { control: "WASD", description: "Drive" },
            { control: "E", description: "Intake" },
            { control: "Q", description: "Dispense" },
        ],
        Swerve: [
            { control: "WASD", description: "Drive" },
            { control: "< >", description: "Turn" },
            { control: "E", description: "Intake" },
            { control: "Q", description: "Dispense" },
        ],
    }

    return (
        <Modal
            name="Change Drivetrain"
            icon={<FaCar />}
            modalId={modalId}
            onAccept={() => showTooltip("controls", controls[drivetrain])}
        >
            <Dropdown
                label="Type"
                options={
                    ["None", "Tank", "Arcade", "Swerve"] as DrivetrainType[]
                }
                onSelect={(selected: string) =>
                    setDrivetrain(selected as DrivetrainType)
                }
            />
        </Modal>
    )
}

export default DrivetrainModal
