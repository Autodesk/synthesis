import { Stack, styled, Typography } from "@mui/material"
import { Button } from "../components/StyledComponents"
import type React from "react"
import { useEffect } from "react"
import MatchMode from "@/systems/match_mode/MatchMode"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import Label from "../components/Label"
import type { ModalImplProps } from "../components/Modal"
import { CloseType, useUIContext } from "../helpers/UIProviderHelpers"

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

const MatchResultsModal: React.FC<ModalImplProps<void, void>> = ({ modal }) => {
    const { configureScreen, closeModal } = useUIContext()

    const { message, color } = getMatchWinner()

    const entries: Entry[] = [
        { name: "Red Score", value: SimulationSystem.redScore },
        { name: "Blue Score", value: SimulationSystem.blueScore },
    ]

    const { redRobotScores, blueRobotScores } = getPerRobotScores()

    useEffect(() => {
        configureScreen(
            modal!,
            { title: "Match Results", hideCancel: true, hideAccept: true, allowClickAway: false },
            {}
        )
    }, [])

    return (
        <>
            <LabelStyled winnerColor={color} fontSize="1.5rem">
                {message}
            </LabelStyled>
            <Stack>
                {entries.map(e => (
                    <Stack key={e.name} direction="row">
                        <Label size="md">{e.name}</Label>
                        <Label size="md">{e.value}</Label>
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
                        <Label size="md">{e.name}</Label>
                        <Label size="md">{e.value}</Label>
                    </Stack>
                ))}
            </div>
            <LabelStyled winnerColor={"#1818ff"} fontSize="1rem">
                Blue Alliance
            </LabelStyled>
            <div className="flex flex-col">
                {blueRobotScores.map(e => (
                    <Stack key={e.name} direction="row">
                        <Label size="md">{e.name}</Label>
                        <Label size="md">{e.value}</Label>
                    </Stack>
                ))}
            </div>
            <Button
                onClick={() => {
                    closeModal(CloseType.Accept)
                    MatchMode.getInstance().sandboxModeStart()
                }}
                className="w-full"
            >
                Back to Sandbox Mode
            </Button>
        </>
    )
}

export default MatchResultsModal
