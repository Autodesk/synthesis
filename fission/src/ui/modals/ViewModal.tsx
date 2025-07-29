import { MenuItem, Select } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import type { ModalImplProps } from "../components/Modal"
import { useUIContext } from "../helpers/UIProviderHelpers"

type ViewType = "Orbit" | "Freecam" | "Overview" | "Driver Station"

const ViewModal: React.FC<ModalImplProps<void>> = ({ modal }) => {
    const { configureScreen } = useUIContext()
    const [view, setView] = useState<ViewType>("Orbit")

    useEffect(() => {
        const onBeforeAccept = () => {
            console.log("Selected view:", view)
        }

        configureScreen(modal!, { title: "Camera View" }, { onBeforeAccept })
    }, [modal, view])

    return (
        <Select
            value={view}
            onChange={e => {
                setView(e.target.value as ViewType)
            }}
            label={"Camera View"}
        >
            {["Orbit", "Freecam", "Overview", "Driver Station"].map(opt => (
                <MenuItem key={opt} value={opt}>
                    {opt}
                </MenuItem>
            ))}
        </Select>
    )
}

export default ViewModal
