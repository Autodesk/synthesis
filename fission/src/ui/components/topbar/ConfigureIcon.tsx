import { Box } from "@mui/material"
import type { FC } from "react"
import type { IconType } from "react-icons"
import { TopBarIcon, type TopBarIconName } from "@/ui/components/topbar/TopBarIcons"

export type ConfigureIconSource = { sprite: TopBarIconName } | { glyph: IconType }

// GLYPHS have a different scale than the SVGs so they match
const GLYPH_SCALE = 0.87
const glyphFontSize = (size: number | string) =>
    typeof size === "number" ? size * GLYPH_SCALE : `calc(${size} * ${GLYPH_SCALE})`

const GLYPH_BOX_SX = { display: "flex", alignItems: "center", justifyContent: "center" } as const

export const ConfigureIcon: FC<{ icon: ConfigureIconSource; size: number | string }> = ({ icon, size }) => {
    if ("sprite" in icon) return <TopBarIcon name={icon.sprite} size={size} />

    return (
        <Box sx={{ ...GLYPH_BOX_SX, width: size, height: size, fontSize: glyphFontSize(size) }}>
            <icon.glyph />
        </Box>
    )
}
