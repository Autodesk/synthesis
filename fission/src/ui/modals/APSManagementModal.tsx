import { Stack, Typography } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import { HiUser } from "react-icons/hi"
import APS from "@/aps/APS"
import type { ModalImplProps } from "@/ui/components/Modal"
import { useUIContext } from "../UIProvider"
import Label from "../components/Label"

const APSManagementModal: React.FC<ModalImplProps<void>> = ({ modal }) => {
    const { configureScreen } = useUIContext()
    const [userInfo, _] = useState(APS.userInfo)
    useEffect(() => {
        const onAccept = () => {
            APS.logout()
        }

        configureScreen(modal!, { title: "Not signed in" }, { onAccept })
    }, [modal, userInfo?.name])

    return (
        <Stack spacing={10} direction="row">
            {userInfo?.picture ? (
                <img alt={userInfo?.name} src={userInfo?.picture} className="h-10 rounded-full" />
            ) : (
                <HiUser />
            )}
            <Label size="md">{userInfo?.name ?? "Not signed in"}</Label>
        </Stack>
    )
}

export default APSManagementModal
