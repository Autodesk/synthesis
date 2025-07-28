import { MenuItem, Select } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import type { ModalImplProps } from "@/ui/components/Modal"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"

type DrivetrainType = "None" | "Tank" | "Arcade" | "Swerve"

const DrivetrainModal: React.FC<ModalImplProps<void>> = ({ modal }) => {
    const { configureScreen } = useUIContext()
    const [drivetrain, setDrivetrain] = useState<DrivetrainType>("None")

    useEffect(() => {
        configureScreen(modal!, { title: "Change Drivetrain" }, {})
    }, [])

    return (
        <Select label="Type" value={drivetrain} onChange={e => setDrivetrain(e.target.value as DrivetrainType)}>
            {["None", "Tank", "Arcade", "Swerve"].map(opt => (
                <MenuItem key={`drive-type-${opt}`} value={opt}>
                    {opt}
                </MenuItem>
            ))}
        </Select>
    )
}

export default DrivetrainModal
