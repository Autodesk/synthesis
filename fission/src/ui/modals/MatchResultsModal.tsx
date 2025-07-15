import { Button, Stack, styled, Typography } from "@mui/material"
import type React from "react"
import { useContext } from "react"
import MatchMode from "@/systems/MatchMode"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import type { ModalImplProps } from "../components/Modal"
import { CloseType, UIContext } from "../UIProvider"

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

const LabelStyled = styled(Typography)<{ winnerColor: string; fontSize: string }>(({ winnerColor, fontSize }) => ({
    fontWeight: 700,
    fontSize: fontSize,
    margin: "0pt",
    marginTop: "0.5rem",
    color: winnerColor,
}))

const MatchResultsModal: React.FC<ModalImplProps> = ({ modal, parent }) => {
    const { message, color } = getMatchWinner()

    const entries: Entry[] = [
        { name: "Red Score", value: SimulationSystem.redScore },
        { name: "Blue Score", value: SimulationSystem.blueScore },
    ]

    const { redRobotScores: redRobotScores, blueRobotScores: blueRobotScores } = getPerRobotScores()

    const { closeModal } = useContext(UIContext)
    // TODO: disallow clickaway
    // TODO: hide buttons

    return (
        <>
            <LabelStyled winnerColor={color} fontSize="1.5rem">
                {message}
            </LabelStyled>
            <Stack>
                {entries.map(e => (
                    <Stack key={e.name} direction="row">
                        <Typography>{e.name}</Typography>
                        <Typography>{e.value}</Typography>
                    </Stack>
                ))}
            </Stack>
            <LabelStyled winnerColor={"#ffffff"} fontSize="1.25rem">
                Robot Score Contributions
            </LabelStyled>
            <LabelStyled winnerColor={"#ff0000"} fontSize="1rem">
                Red Alliance
            </LabelStyled>
            <div className="flex flex-col">
                {redRobotScores.map(e => (
                    <Stack key={e.name} direction="row">
                        <Typography>{e.name}</Typography>
                        <Typography>{e.value}</Typography>
                    </Stack>
                ))}
            </div>
            <LabelStyled winnerColor={"#1818ff"} fontSize="1rem">
                Blue Alliance
            </LabelStyled>
            <div className="flex flex-col">
                {blueRobotScores.map(e => (
                    <Stack key={e.name} direction="row">
                        <Typography>{e.name}</Typography>
                        <Typography>{e.value}</Typography>
                    </Stack>
                ))}
            </div>
            <Button
                onClick={() => {
                    closeModal(CloseType.Accept)
                    MatchMode.getInstance().sandboxModeStart()
                }}
                className="w-full"
            >Back to Sandbox Mode</Button>
        </>
    )
}

export default MatchResultsModal
