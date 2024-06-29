import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { GrFormClose } from "react-icons/gr"
import Stack, { StackDirection } from "@/components/Stack"
import Label from "@/components/Label"

type Entry = {
    name: string
    value: number
}

const MatchResultsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const entries: Entry[] = [
        { name: "Red Score", value: 10 },
        { name: "Blue Score", value: 5 },
    ]

    return (
        <Modal
            name={"Match Results"}
            icon={<GrFormClose />}
            modalId={modalId}
            cancelName="Exit"
            middleName="Configure"
            acceptName="Restart"
            middleEnabled={true}
        >
            <div className="flex flex-col">
                {entries.map(e => (
                    <Stack direction={StackDirection.Horizontal}>
                        <Label>{e.name}</Label>
                        <Label>{e.value}</Label>
                    </Stack>
                ))}
            </div>
        </Modal>
    )
}

export default MatchResultsModal
