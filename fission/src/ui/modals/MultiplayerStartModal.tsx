import React, {useState} from "react"
import Button from "@/components/Button.tsx"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { SynthesisIcons } from "../components/StyledComponents"
import { useModalControlContext } from "../helpers/UseModalManager"
import { Stack } from "@mui/system"
import {Divider, TextField} from "@mui/material";

const MultiplayerStartModal: React.FC<
    ModalPropsImpl & {
    startWorldCallback: (roomId?:string) => void
    }
> = ({ modalId, startWorldCallback }) => {
    const { closeModal } = useModalControlContext()
    const [room, setRoom] = useState<string>("")
    return (
        <Modal
            name={"Start Multiplayer"}
            icon={SynthesisIcons.PEOPLE}
            modalId={modalId}
            middleEnabled={false}
            cancelEnabled={false}
            acceptEnabled={false}
            allowClickAway={false}
        >
            <Stack direction="column">
                <Button
                    value={"Create Game"}
                    onClick={() => {
                        closeModal()
                        startWorldCallback()
                    }}
                    className="w-full my-1"
                />
                <Divider/>
                <TextField type={"text"} value={room} sx={{input: {color:"#ffffff"}}} inputProps={{onInput:(e) => {
                    setRoom(e.currentTarget.value.replace(/\D/, "").slice(0, 6)) // 6-digit numbers
                }}}/>
                <Button
                    value={"Join Game"}
                    onClick={() => {
                        closeModal()
                        startWorldCallback(room)
                    }}
                    className="w-full mt-1 mb-3"
                />
            </Stack>
        </Modal>
    )
}

export default MultiplayerStartModal
