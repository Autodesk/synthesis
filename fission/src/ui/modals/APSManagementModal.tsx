import React, { useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Stack, { StackDirection } from "@/components/Stack"
import { HiUser } from "react-icons/hi"
import APS from "@/aps/APS"

const APSManagementModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const [userInfo, _] = useState(APS.userInfo)
    return (
        <Modal
            name={userInfo?.name ?? "Not signed in"}
            icon={userInfo?.picture ? <img src={userInfo?.picture} className="h-10 rounded-full" /> : <HiUser />}
            modalId={modalId}
            acceptName="Logout"
            onAccept={() => {
                APS.logout()
            }}
        >
            <Stack direction={StackDirection.Vertical} spacing={10}></Stack>
        </Modal>
    )
}

export default APSManagementModal
