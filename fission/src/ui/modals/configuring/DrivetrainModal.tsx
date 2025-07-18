import { MenuItem, Select } from "@mui/material"
import type React from "react"
import { useState } from "react"
import type { ModalImplProps } from "@/ui/components/Modal"

type DrivetrainType = "None" | "Tank" | "Arcade" | "Swerve"

const DrivetrainModal: React.FC<ModalImplProps<void>> = () => {
    const [drivetrain, setDrivetrain] = useState<DrivetrainType>("None")

    return (
        <Select label="Type" value={drivetrain} onChange={e => setDrivetrain(e.target.value as DrivetrainType)}>
            {["None", "Tank", "Arcade", "Swerve"].map(opt => (
                <MenuItem value={opt}>{opt}</MenuItem>
            ))}
        </Select>
    )
}

export default DrivetrainModal
