import React from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Stack, { StackDirection } from "@/components/Stack"
import Label from "@/components/Label"
import { SynthesisIcons, Spacer } from "../components/StyledComponents"
import Button from "@/components/Button"
import { useModalControlContext } from "@/ui/helpers/UseModalManager"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import MatchMode from "@/systems/match_mode/MatchMode"
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

const getPerRobotScores = (): { redRobotScores: Entry[]; blueRobotScores: Entry[] } => {
    const redRobotScores: Entry[] = []
    const blueRobotScores: Entry[] = []
    SimulationSystem.perRobotScore.forEach((score, robot) => {
        if (robot.alliance === "red") {
            redRobotScores.push({ name: `${robot.nameTag?.text()} (${robot.assemblyName})`, value: score })
        } else {
            blueRobotScores.push({ name: `${robot.nameTag?.text()} (${robot.assemblyName})`, value: score })
        }
    })
    return { redRobotScores, blueRobotScores }
}

const LabelStyled = styled(Label)<{ winnerColor: string; fontSize: string }>(({ winnerColor, fontSize }) => ({
    fontWeight: 700,
    fontSize: fontSize,
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

    const { redRobotScores: redRobotScores, blueRobotScores: blueRobotScores } = getPerRobotScores()

    const { closeModal } = useModalControlContext()

    return (
        <Modal
            name={"Match Results"}
            icon={SynthesisIcons.GAMEPAD}
            modalId={modalId}
            cancelEnabled={false}
            acceptEnabled={false}
            allowClickAway={false}
        >
            <LabelStyled winnerColor={color} fontSize="1.5rem">
                {message}
            </LabelStyled>
            <div className="flex flex-col">
                {entries.map(e => (
                    <Stack key={e.name} direction={StackDirection.HORIZONTAL}>
                        <Label>{e.name}</Label>
                        <Label>{e.value}</Label>
                    </Stack>
                ))}
            </div>
            <LabelStyled winnerColor={"#ffffff"} fontSize="1.25rem">
                Robot Score Contributions
            </LabelStyled>
            <LabelStyled winnerColor={"#ff0000"} fontSize="1rem">
                Red Alliance
            </LabelStyled>
            <div className="flex flex-col">
                {redRobotScores.map(e => (
                    <Stack key={e.name} direction={StackDirection.HORIZONTAL}>
                        <Label>{e.name}</Label>
                        <Label>{e.value}</Label>
                    </Stack>
                ))}
            </div>
            <LabelStyled winnerColor={"#1818ff"} fontSize="1rem">
                Blue Alliance
            </LabelStyled>
            <div className="flex flex-col">
                {blueRobotScores.map(e => (
                    <Stack key={e.name} direction={StackDirection.HORIZONTAL}>
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
