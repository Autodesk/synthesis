import { Stack, Typography } from "@mui/material"
import { useEffect, useState } from "react"
import type React from "react"
import type { PanelImplProps } from "../components/Panel"
import { useUIContext } from "../helpers/UIProviderHelpers"
import { ACHIEVEMENTS_UPDATED_EVENT } from "@/systems/achievements/AchievementsSystem"
import World from "@/systems/World"

type PanelCustomProps = Record<string, never>

const placeholderImg = "/synthesis-logo.svg"

const AchievementsPanel: React.FC<PanelImplProps<void, PanelCustomProps>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const [, setNonce] = useState(0)

    useEffect(() => {
        configureScreen(panel!, { title: "Achievements", position: "right", hideAccept: true }, {})
        const onUpdate = () => setNonce(x => x + 1)
        window.addEventListener(ACHIEVEMENTS_UPDATED_EVENT, onUpdate)
        return () => window.removeEventListener(ACHIEVEMENTS_UPDATED_EVENT, onUpdate)
    }, [configureScreen, panel])

    const achievements = World.achievementsSystem?.list() ?? []
    const stats = World.achievementsSystem?.stats() ?? new Map()

    return (
        <Stack gap={1} className="w-[360px] max-w-[90vw]">
            {achievements.map(a => {
                const unlocked = !!a.state
                const displayTitle = unlocked || !a.hidden ? a.title : "Hidden"
                const displayDesc = unlocked || !a.hidden ? a.description : "Unlock to reveal"
                const imgSrc = unlocked || !a.hidden ? (a.imageSrc ?? placeholderImg) : placeholderImg
                const percent = stats.get(a.key)?.percentUnlocked
                return (
                    <div
                        key={a.key}
                        className={`flex flex-row gap-3 items-center p-2 rounded-md ${unlocked ? "bg-green-800/20" : "bg-gray-600/20"}`}
                    >
                        <img src={imgSrc} alt={displayTitle} style={{ width: 48, height: 48, borderRadius: 8 }} />
                        <div className="flex-1">
                            <Typography variant="subtitle1">{displayTitle}</Typography>
                            <Typography variant="body2" color="text.secondary">
                                {displayDesc}
                            </Typography>
                            {percent != null && (
                                <Typography variant="caption" color="text.secondary">
                                    {percent.toFixed(1)}% of users unlocked
                                </Typography>
                            )}
                        </div>
                    </div>
                )
            })}
        </Stack>
    )
}

export default AchievementsPanel
