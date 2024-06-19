import React, { useEffect, useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { FaPlus } from "react-icons/fa6"
import Label from "@/components/Label"
import Stack, { StackDirection } from "@/components/Stack"

type Client = {
    name: string
    ping: number
}

const clients_source: Client[] = [
    { name: "Client 1", ping: 100 },
    { name: "Client 2", ping: 330 },
    { name: "Client 3", ping: 50 },
    { name: "Client 4", ping: 1000 },
]

const ServerHostingModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const [clients, setClients] = useState<Client[]>([])
    // should replace with actual clients when communication works
    useEffect(() => {
        setTimeout(() => {
            setClients(clients_source)
        }, 2_000)
    }, [clients])
    return (
        <Modal name={"Server Hosting"} icon={<FaPlus />} modalId={modalId}>
            {clients.length == 0 ? (
                <Label>Waiting for clients...</Label>
            ) : (
                clients.map(c => (
                    <Stack direction={StackDirection.Horizontal}>
                        <Label>{c.name}</Label>
                        <Label>{c.ping}ms</Label>
                    </Stack>
                ))
            )}
        </Modal>
    )
}

export default ServerHostingModal
