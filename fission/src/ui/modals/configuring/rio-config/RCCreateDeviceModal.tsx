import { FormControl, InputLabel, MenuItem, Select } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import WPILibBrain from "@/systems/simulation/wpilib_brain/WPILibBrain"
import World from "@/systems/World"
import type { ModalImplProps } from "@/ui/components/Modal"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import RoboRIOModal from "../RoboRIOModal"
import RCConfigCANGroupModal from "./RCConfigCANGroupModal"
import RCConfigEncoderModal from "./RCConfigEncoderModal"
import RCConfigPWMGroupModal from "./RCConfigPWMGroupModal"

type DeviceType = "PWM" | "CAN" | "Encoder"

const RCCreateDeviceModal: React.FC<ModalImplProps<void, void>> = ({ modal }) => {
    const { openModal, configureScreen } = useUIContext()
    const [type, setType] = useState<DeviceType>("PWM")

    useEffect(() => {
        const onBeforeAccept = () => {
            console.log(type)
            const miraObjs = MirabufSceneObject.getRobots()
            if (miraObjs.length > 0) {
                const mechanism = miraObjs[0].mechanism
                const simLayer = World.simulationSystem.getSimulationLayer(mechanism)
                console.log("simlayer", simLayer)
                if (!(simLayer?.brain instanceof WPILibBrain)) simLayer?.setBrain(new WPILibBrain(miraObjs[0]))
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
