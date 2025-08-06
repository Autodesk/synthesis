import { FormControl, InputLabel, MenuItem, Select } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import WPILibBrain from "@/systems/simulation/wpilib_brain/WPILibBrain"
import type { ModalImplProps } from "@/ui/components/Modal"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import RoboRIOModal from "../RoboRIOModal"
import RCConfigCANGroupModal from "./RCConfigCANGroupModal"
import RCConfigEncoderModal from "./RCConfigEncoderModal"
import RCConfigPWMGroupModal from "./RCConfigPWMGroupModal"
import SceneRenderer from "@/systems/scene/SceneRenderer"
import SimulationSystem from "@/systems/simulation/SimulationSystem"

type DeviceType = "PWM" | "CAN" | "Encoder"

const RCCreateDeviceModal: React.FC<ModalImplProps<void, void>> = ({ modal }) => {
    const { openModal, configureScreen } = useUIContext()
    const [type, setType] = useState<DeviceType>("PWM")

    useEffect(() => {
        const onBeforeAccept = () => {
            console.log(type)
            const miraObj = SceneRenderer.mirabufSceneObjects.getRobots()[0]
            if (miraObj != null) {
                const mechanism = miraObj.mechanism
                const simLayer = SimulationSystem.getSimulationLayer(mechanism)
                console.log("simlayer", simLayer)
                if (!(simLayer?.brain instanceof WPILibBrain)) simLayer?.setBrain(new WPILibBrain(miraObj))
            }
            switch (type) {
                case "PWM":
                    openModal(RCConfigPWMGroupModal, undefined, modal)
                    break
                case "CAN":
                    openModal(RCConfigCANGroupModal, undefined, modal)
                    break
                case "Encoder":
                    openModal(RCConfigEncoderModal, undefined, modal)
                    break
                default:
                    break
            }
        }
        const onCancel = () => openModal(RoboRIOModal, undefined, modal)

        configureScreen(modal!, { title: "Create Device", acceptText: "Next" }, { onBeforeAccept, onCancel })
    }, [])

    return (
        <FormControl fullWidth>
            <InputLabel id="device-type">Type</InputLabel>
            <Select
                labelId="device-type"
                label={"Type"}
                onChange={e => {
                    setType(e.target.value as DeviceType)
                }}
            >
                {["PWM", "CAN", "Encoder"].map(t => (
                    <MenuItem key={t} value={t}>
                        {t}
                    </MenuItem>
                ))}
            </Select>
        </FormControl>
    )
}

export default RCCreateDeviceModal
