import { Box } from "@mui/material"
import type { FC } from "react"
import type { IconType } from "react-icons"
import { TopBarIcon, type TopBarIconName } from "@/ui/components/topbar/TopBarIcons"

export type ConfigureIconSource = { sprite: TopBarIconName } | { glyph: IconType }

// GLYPHS have a different scale than the SVGs so they match
const GLYPH_SCALE = 0.87
const glyphFontSize = (size: number | string) =>
    typeof size === "number" ? size * GLYPH_SCALE : `calc(${size} * ${GLYPH_SCALE})`

export const ConfigureIcon: FC<{ icon: ConfigureIconSource; size: number | string }> = ({ icon, size }) =>
    "sprite" in icon ? (
        <TopBarIcon name={icon.sprite} size={size} />
    ) : (
        <Box
            sx={{
                width: size,
                height: size,
                fontSize: glyphFontSize(size),
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
            }}
        >
            <icon.glyph />
        </Box>
    )
