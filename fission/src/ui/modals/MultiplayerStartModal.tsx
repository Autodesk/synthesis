import React, {useState} from "react"
import Button from "@/components/Button.tsx"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { SynthesisIcons } from "../components/StyledComponents"
import { useModalControlContext } from "../helpers/UseModalManager"
import { Stack } from "@mui/system"
import {Divider, TextField} from "@mui/material";

const MultiplayerStartModal: React.FC<
    ModalPropsImpl & {
    startWorldCallback: (name:string, roomId?:string) => void
    }
> = ({ modalId, startWorldCallback }) => {
    const { closeModal } = useModalControlContext()
    const [room, setRoom] = useState<string>("")
    const [name, setName] = useState<string>("")
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
                <TextField type={"text"} value={name} sx={{input: {color:"#ffffff"}}} placeholder="Name" inputProps={{onInput:(e) => {
                    setName(e.currentTarget.value.replace(/\W/, "").slice(0,12))
                }}}/>
                <Button
                    value={"Create Game"}
                    onClick={() => {
                        closeModal()
                        startWorldCallback(name)
                    }}
                    className="w-full my-1"
                />
                <Divider/>
                <TextField type={"text"} value={room} sx={{input: {color:"#ffffff"}}} placeholder="000000" inputProps={{onInput:(e) => {
                    setRoom(e.currentTarget.value.replace(/\D/, "").slice(0, 6)) // 6-digit numbers
                }}}/>
                <Button
                    value={"Join Game"}
                    onClick={() => {
                        closeModal()
                        startWorldCallback(name, room)
                    }}
                    className="w-full mt-1 mb-3"
                />
            </Stack>
        </Modal>
    )
}

export default MultiplayerStartModal
