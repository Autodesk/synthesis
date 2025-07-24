import { Button, FormControlLabel } from "@mui/material"
import type React from "react"
import type { ModalImplProps } from "@/ui/components/Modal"
import { useUIContext } from "@/ui/UIProvider"
import RCCreateDeviceModal from "./rio-config/RCCreateDeviceModal"
import { useEffect } from "react"

const RoboRIOModal: React.FC<ModalImplProps<void>> = ({ modal }) => {
    const { openModal, configureScreen } = useUIContext()
    useEffect(() => {
        configureScreen(modal!, { title: "RoboRIO Configuration" }, {})
    }, [])

    return (
        <FormControlLabel
            label="cbdbcc,ds,vsdv"
            control={<Button value="Create Device" onClick={() => openModal(<RCCreateDeviceModal />)} />}
        />
    )
}

export default RoboRIOModal
