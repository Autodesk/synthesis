import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Button from "@/components/Button"
import { FaGear } from "react-icons/fa6"
import Stack, { StackDirection } from "@/components/Stack"
import Dropdown from "@/components/Dropdown"
import Label from "@/components/Label"

const ConnectToMultiplayerModal: React.FC<ModalPropsImpl> = ({ modalId }) => (
    <Modal
        name={"Connect to Multiplayer"}
        icon={<FaGear />}
        acceptEnabled={false}
        cancelName="Disconnect"
        modalId={modalId}
    >
        <Stack direction={StackDirection.Horizontal} spacing={8}>
            <div className="w-full bg-gray-700 p-4 rounded-md">
                <Label>Disconnected from server.</Label>
                <br />
                <Label>Waiting for server...</Label>
            </div>
            <Stack direction={StackDirection.Vertical}>
                <Dropdown
                    options={["Dozer_v9.mira", "Team_2471_2018_v7.mira"]}
                    onSelect={() => {}}
                />
                <Button value="Choose Robot" />
                <Button value="Refresh robot list" />
            </Stack>
        </Stack>
    </Modal>
)

export default ConnectToMultiplayerModal
