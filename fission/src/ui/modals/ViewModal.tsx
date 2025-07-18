import { MenuItem, Select } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import type { ModalImplProps } from "../components/Modal"

type ViewType = "Orbit" | "Freecam" | "Overview" | "Driver Station"

const ViewModal: React.FC<ModalImplProps<void>> = ({ modal }) => {
    const [view, setView] = useState<ViewType>("Orbit")

    useEffect(() => {
        const onAccept = () => {
            console.log("Selected view:", view)
        }

        modal!.onAccept.addFunc(onAccept)

        return () => {
            modal!.onAccept.removeFunc(onAccept)
        }
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
