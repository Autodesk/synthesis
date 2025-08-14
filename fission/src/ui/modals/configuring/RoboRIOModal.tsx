import { FormControlLabel } from "@mui/material"
import type React from "react"
import { useEffect } from "react"
import type { ModalImplProps } from "@/ui/components/Modal"
import { Button } from "@/ui/components/StyledComponents"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import RCCreateDeviceModal from "./rio-config/RCCreateDeviceModal"

const RoboRIOModal: React.FC<ModalImplProps<void, void>> = ({ modal }) => {
    const { openModal, configureScreen } = useUIContext()
    useEffect(() => {
        configureScreen(modal!, { title: "RoboRIO Configuration" }, {})
    }, [])

    return (
        <FormControlLabel
            label="cbdbcc,ds,vsdv"
            control={<Button value="Create Device" onClick={() => openModal(RCCreateDeviceModal, undefined)} />}
        />
    )
}

export default RoboRIOModal
