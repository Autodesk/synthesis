import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Stack, { StackDirection } from "@/components/Stack"
import Label from "@/components/Label"
import { SynthesisIcons, Spacer } from "../components/StyledComponents"
import Button from "@/components/Button"
import { useModalControlContext } from "@/ui/ModalContext"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import MatchMode from "@/systems/MatchMode"

type Entry = {
    name: string
    value: number
}

const MatchResultsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const entries: Entry[] = [
        { name: "Red Score", value: SimulationSystem.redScore },
        { name: "Blue Score", value: SimulationSystem.blueScore },
    ]

    const { closeModal } = useModalControlContext()

    return (
        <Modal
            name={"Match Results"}
            icon={SynthesisIcons.Gamepad}
            modalId={modalId}
            cancelEnabled={false}
            acceptEnabled={false}
            allowClickAway={false}
        >
            <div className="flex flex-col">
                {entries.map(e => (
                    <Stack key={e.name} direction={StackDirection.Horizontal}>
                        <Label>{e.name}</Label>
                        <Label>{e.value}</Label>
                    </Stack>
                ))}
            </div>
            <Button
                value="Back to Sandbox Mode"
                onClick={() => {
                    closeModal()
                    MatchMode.getInstance().sandboxModeStart()
                }}
                className="w-full"
            />
            {Spacer(8)}
        </Modal>
    )
}

export default MatchResultsModal
