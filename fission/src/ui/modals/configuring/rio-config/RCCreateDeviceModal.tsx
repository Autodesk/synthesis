import React, { useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { useModalControlContext } from "@/ui/ModalContext"
import { FaPlus } from "react-icons/fa6"
import Dropdown from "@/components/Dropdown"
import WPILibBrain from "@/systems/simulation/wpilib_brain/WPILibBrain"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"

type DeviceType = "PWM" | "CAN" | "Encoder"

const RCCreateDeviceModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    const [type, setType] = useState<DeviceType>("PWM")

    return (
        <Modal
            name="Create Device"
            icon={<FaPlus />}
            modalId={modalId}
            acceptName="Next"
            onAccept={() => {
                console.log(type)
                const miraObjs = [...World.SceneRenderer.sceneObjects.entries()].filter(
                    x => x[1] instanceof MirabufSceneObject
                )
                if (miraObjs.length > 0) {
                    const mechanism = (miraObjs[0][1] as MirabufSceneObject).mechanism
                    const simLayer = World.SimulationSystem.GetSimulationLayer(mechanism)
                    console.log("simlayer", simLayer)
                    if (!(simLayer?.brain instanceof WPILibBrain)) simLayer?.SetBrain(new WPILibBrain(mechanism))
                }
                switch (type) {
                    case "PWM":
                        openModal("config-pwm")
                        break
                    case "CAN":
                        openModal("config-can")
                        break
                    case "Encoder":
                        openModal("config-encoder")
                        break
                    default:
                        break
                }
            }}
            onCancel={() => {
                openModal("roborio")
            }}
        >
            <Dropdown
                label={"Type"}
                options={["PWM", "CAN", "Encoder"] as DeviceType[]}
                onSelect={selected => {
                    setType(selected as DeviceType)
                }}
            />
        </Modal>
    )
}

export default RCCreateDeviceModal
