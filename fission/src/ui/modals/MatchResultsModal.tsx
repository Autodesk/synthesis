import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Stack, { StackDirection } from "@/components/Stack"
import Label from "@/components/Label"
import { SynthesisIcons, Spacer } from "../components/StyledComponents"
import Button from "@/components/Button"
import { useModalControlContext } from "@/ui/helpers/UseModalManager"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import MatchMode from "@/systems/MatchMode"
import { styled } from "@mui/material"

type Entry = {
    name: string
    value: number
}

const getMatchWinner = (): { message: string; color: string } => {
    if (SimulationSystem.redScore > SimulationSystem.blueScore) {
        return { message: "Red Team Wins!", color: "#ff0000" }
    } else if (SimulationSystem.blueScore > SimulationSystem.redScore) {
        return { message: "Blue Team Wins!", color: "#1818ff" }
    } else {
        return { message: "It's a Tie!", color: "#ffffff" }
    }
}

const LabelStyled = styled(Label)<{ winnerColor: string }>(({ winnerColor }) => ({
    fontWeight: 700,
    fontSize: "1.5rem",
    margin: "0pt",
    marginTop: "0.5rem",
    color: winnerColor,
}))

const MatchResultsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { message, color } = getMatchWinner()

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
            <LabelStyled winnerColor={color}>{message}</LabelStyled>
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
