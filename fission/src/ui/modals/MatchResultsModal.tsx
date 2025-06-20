import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Stack, { StackDirection } from "@/components/Stack"
import Label from "@/components/Label"
import { SynthesisIcons, Spacer } from "../components/StyledComponents"
import Button from "@/components/Button"
import { useModalControlContext } from "@/ui/ModalContext"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import MatchMode from "@/systems/MatchMode"
import { styled } from "@mui/material"

type Entry = {
    name: string
    value: number
}

const getMatchWinner = (): string => {
    if (SimulationSystem.redScore > SimulationSystem.blueScore) {
        return "Red Team Wins!"
    } else if (SimulationSystem.blueScore > SimulationSystem.redScore) {
        return "Blue Team Wins!"
    } else {
        return "It's a Tie!"
    }
}

const LabelStyled = styled(Label)({
    fontWeight: 700,
    fontSize: "1.5rem",
    margin: "0pt",
    marginTop: "0.5rem",
})

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
            <LabelStyled>{getMatchWinner()}</LabelStyled>
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
