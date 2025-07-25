import React, { useEffect, useState } from "react"
import Label from "@/components/Label"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Stack, { StackDirection } from "@/components/Stack"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

type Client = {
    name: string
    ping: number
}

const CLIENTS_SOURCE: Client[] = [
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
            setClients(CLIENTS_SOURCE)
        }, 2_000)
    }, [])
    return (
        <Modal name={"Server Hosting"} icon={SynthesisIcons.ADD} modalId={modalId}>
            {clients.length == 0 ? (
                <Label>Waiting for clients...</Label>
            ) : (
                clients.map(c => (
                    <Stack key={c.name} direction={StackDirection.HORIZONTAL}>
                        <Label>{c.name}</Label>
                        <Label>{c.ping}ms</Label>
                    </Stack>
                ))
            )}
        </Modal>
    )
}

export default ServerHostingModal
