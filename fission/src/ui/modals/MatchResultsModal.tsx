import { Divider, Stack, styled, Typography } from "@mui/material"
import type React from "react"
import { useEffect } from "react"
import MatchMode from "@/systems/match_mode/MatchMode"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import { useThemeContext } from "@/ui/helpers/ThemeProviderHelpers.ts"
import Label from "../components/Label"
import type { ModalImplProps } from "../components/Modal"
import { Button } from "../components/StyledComponents"
import { CloseType, useUIContext } from "../helpers/UIProviderHelpers"

type Entry = {
    name: string
    value: number
}

const getMatchWinner = (): { message: string; color: string } => {
    const { redAllianceColor, blueAllianceColor, secondaryColor } = useThemeContext()
    if (ScoreTracker.redScore > ScoreTracker.blueScore) {
        return { message: "Red Team Wins!", color: redAllianceColor }
    } else if (ScoreTracker.blueScore > ScoreTracker.redScore) {
        return { message: "Blue Team Wins!", color: blueAllianceColor }
    } else {
        return { message: "It's a Tie!", color: secondaryColor }
    }
}

const getPerRobotScores = (): { redRobotScores: Entry[]; blueRobotScores: Entry[] } => {
    const redRobotScores: Entry[] = []
    const blueRobotScores: Entry[] = []
    ScoreTracker.perRobotScore.forEach((score, robot) => {
        if (robot.alliance === "red") {
            redRobotScores.push({ name: `${robot.nameTag?.text()}`, value: score })
        } else {
            blueRobotScores.push({ name: `${robot.nameTag?.text()}`, value: score })
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
    const { redAllianceColor, blueAllianceColor, primaryColor } = useThemeContext()
    const entries: Entry[] = [
        { name: "Red Score", value: ScoreTracker.redScore },
        { name: "Blue Score", value: ScoreTracker.blueScore },
    ]

    const { redRobotScores, blueRobotScores } = getPerRobotScores()

    useEffect(() => {
        configureScreen(
            modal!,
            { title: "Match Results", hideCancel: true, hideAccept: true, allowClickAway: false },
            { onClose: () => MatchMode.getInstance().sandboxModeStart() }
        )
    }, [configureScreen, modal])

    return (
        <>
            <LabelStyled winnerColor={color} fontSize="1.5rem">
                {message}
            </LabelStyled>
            <Divider sx={{ my: "1rem" }} />
            <Stack>
                {entries.map(e => (
                    <Stack key={e.name} direction="row" justifyContent={"space-between"}>
                        <Label size="md">{e.name}</Label>
                        <Label size="md">{e.value}</Label>
                    </Stack>
                ))}
            </Stack>
            <Divider sx={{ my: "0.5rem" }} />
            <LabelStyled winnerColor={primaryColor} fontSize="1.25rem">
                Robot Score Contributions
            </LabelStyled>
            <Stack direction={"row"} justifyContent={"space-between"} gap={"1rem"}>
                <Stack direction={"column"}>
                    <LabelStyled winnerColor={redAllianceColor} fontSize="1rem">
                        Red Alliance
                    </LabelStyled>
                    <div className="flex flex-col">
                        {redRobotScores.map(e => (
                            <Stack key={e.name} direction="row" justifyContent={"space-between"}>
                                <Label size="md">{e.name}</Label>
                                <Label size="md">{e.value}</Label>
                            </Stack>
                        ))}
                    </div>
                </Stack>
                <Stack direction={"column"}>
                    <LabelStyled winnerColor={blueAllianceColor} fontSize="1rem">
                        Blue Alliance
                    </LabelStyled>
                    <div className="flex flex-col">
                        {blueRobotScores.map(e => (
                            <Stack key={e.name} direction="row" justifyContent={"space-between"}>
                                <Label size="md">{e.name}</Label>
                                <Label size="md">{e.value}</Label>
                            </Stack>
                        ))}
                    </div>
                </Stack>
            </Stack>
            <Button
                onClick={() => {
                    closeModal(CloseType.Accept)
                }}
                className="w-full"
                sx={{ my: "1rem" }}
            >
                Back to Sandbox Mode
            </Button>
        </>
    )
}

export default MatchResultsModal
