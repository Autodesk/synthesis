import { Divider, Stack, styled, Typography } from "@mui/material"
import type React from "react"
import type { ReactNode } from "react"
import { useEffect } from "react"
import MatchMode from "@/systems/match_mode/MatchMode"
import { useThemeContext } from "@/ui/helpers/ThemeProviderHelpers.ts"
import Label from "../components/Label"
import type { ModalImplProps } from "../components/Modal"
import { Button } from "../components/StyledComponents"
import { CloseType, useUIContext } from "../helpers/UIProviderHelpers"
import World from "@/systems/World.ts"
import { Box } from "@mui/system"

type Entry = {
    name: string
    value: number
}

const getMatchWinner = (): { message: string; color: string } => {
    const { redAllianceColor, blueAllianceColor, secondaryColor } = useThemeContext()
    if (World.scoreTracker.redScore > World.scoreTracker.blueScore) {
        return { message: "Red Team Wins!", color: redAllianceColor }
    } else if (World.scoreTracker.blueScore > World.scoreTracker.redScore) {
        return { message: "Blue Team Wins!", color: blueAllianceColor }
    } else {
        return { message: "It's a Tie!", color: secondaryColor }
    }
}

const getPerRobotScores = (): { redRobotScores: Entry[]; blueRobotScores: Entry[] } => {
    const redRobotScores: Entry[] = []
    const blueRobotScores: Entry[] = []
    World.scoreTracker.perRobotScore.forEach((score, robot) => {
        if (robot.alliance === "red") {
            redRobotScores.push({ name: `${robot.nameTag?.text()}`, value: score })
        } else {
            blueRobotScores.push({ name: `${robot.nameTag?.text()}`, value: score })
        }
    })
    return { redRobotScores, blueRobotScores }
}

const LabelStyled = styled(Typography)<{ color: string; fontSize: string }>(({ color, fontSize }) => ({
    fontWeight: 700,
    fontSize: fontSize,
    margin: "0pt",
    marginTop: "0.5rem",
    color: color,
}))

const MatchResultsModal: React.FC<ModalImplProps<void, void>> = ({ modal }) => {
    const { configureScreen, closeModal } = useUIContext()

    const { message, color } = getMatchWinner()
    const { redAllianceColor, blueAllianceColor, primaryColor } = useThemeContext()
    const entries: Entry[] = [
        { name: "Red Score", value: World.scoreTracker.redScore },
        { name: "Blue Score", value: World.scoreTracker.blueScore },
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
        <Stack direction={"column"}>
            <LabelStyled color={color} fontSize="1.5rem">
                {message}
            </LabelStyled>
            <Divider sx={{ my: "1rem" }} />
            <Stack gap={1}>
                {entries.map(e => (
                    <Stack key={e.name} direction="row" justifyContent={"space-between"}>
                        <Label size="md">{e.name}</Label>
                        <ScoreBox>
                            <Label size="md" sx={{ minWidth: "3em", textAlign: "center" }}>
                                {e.value}
                            </Label>
                        </ScoreBox>
                    </Stack>
                ))}
            </Stack>
            <Divider sx={{ my: "0.5rem" }} />
            <LabelStyled color={primaryColor} fontSize="1.25rem" mb={1}>
                Robot Score Contributions
            </LabelStyled>
            <Stack direction={"row"} justifyContent={"space-between"} gap={2}>
                <RobotContributions allianceColor={redAllianceColor} label={"Red Alliance"} scores={redRobotScores} />
                <Divider orientation={"vertical"} flexItem />
                <RobotContributions
                    allianceColor={blueAllianceColor}
                    label={"Blue Alliance"}
                    scores={blueRobotScores}
                />
            </Stack>
            <Button
                onClick={() => {
                    closeModal(CloseType.ACCEPT)
                }}
                className="w-full"
                sx={{ my: "1rem" }}
            >
                Back to Sandbox Mode
            </Button>
        </Stack>
    )
}

interface RobotContributionProps {
    allianceColor: string
    scores: Entry[]
    label: string
}

const RobotContributions: React.FC<RobotContributionProps> = ({ allianceColor, scores, label }) => {
    return (
        <Stack direction={"column"}>
            <LabelStyled color={allianceColor} fontSize="1rem">
                {label}
            </LabelStyled>
            <div className="flex flex-col">
                {scores.map(e => (
                    <Stack key={e.name} direction="row" alignItems="center" justifyContent={"space-between"} gap={2}>
                        <Label size="md">{e.name}</Label>
                        <ScoreBox key={e.name}>
                            <Label size="sm" sx={{ minWidth: "2em", textAlign: "center" }}>
                                {e.value}
                            </Label>
                        </ScoreBox>
                    </Stack>
                ))}
            </div>
        </Stack>
    )
}

const ScoreBox: React.FC<{ children: ReactNode }> = ({ children }) => {
    return <Box sx={{ bgcolor: "background.paper", padding: 0.5, borderRadius: 2 }}>{children}</Box>
}

export default MatchResultsModal
