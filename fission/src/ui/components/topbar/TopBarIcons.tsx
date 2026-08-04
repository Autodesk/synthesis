import { Box } from "@mui/material"
import { type FC, useEffect, useMemo, useRef } from "react"

// we render these as real dom svgs instead of <img> so they dont repaint blank when the browser drops decoded bitmaps under memory pressure (the aps login popup loading a 2nd copy of the app causes it)
// inlining svgs into one doc makes their internal ids collide, so we prefix each icons ids to keep them isolated
function scopeSvgIds(markup: string, scope: string): string {
    const ids = [...markup.matchAll(/\bid="([^"]+)"/g)].map(match => match[1])
    let scoped = markup
    for (const id of ids) {
        const pattern = id.replace(/[.*+?^${}()|[\]\\\s]/g, "\\$&")
        const replacement = `${scope}__${id}`
        scoped = scoped
            .replace(new RegExp(`id="${pattern}"`, "g"), `id="${replacement}"`)
            .replace(new RegExp(`href="#${pattern}"`, "g"), `href="#${replacement}"`)
            .replace(new RegExp(`url\\(#${pattern}\\)`, "g"), `url(#${replacement})`)
    }
    return scoped
}

const RAW_ICON_MARKUP = import.meta.glob("./icons/*.svg", {
    query: "?raw",
    import: "default",
    eager: true,
}) as Record<string, string>

const ICON_MARKUP: Record<string, string> = Object.fromEntries(
    Object.entries(RAW_ICON_MARKUP).map(([path, markup]) => {
        const scope = path
            .replace(/^.*\//, "")
            .replace(/\.svg$/, "")
            .replace(/[^a-zA-Z0-9_-]/g, "-")
        return [path, scopeSvgIds(markup, scope)]
    })
)

function parseSvgMarkup(markup: string): SVGSVGElement | null {
    const document = new DOMParser().parseFromString(markup, "image/svg+xml")
    const svg = document.querySelector("svg")
    return svg instanceof SVGSVGElement ? svg : null
}

const ICON_SVGS: Record<string, SVGSVGElement | null> = Object.fromEntries(
    Object.entries(ICON_MARKUP).map(([path, markup]) => [path, parseSvgMarkup(markup)])
)

export const TOP_BAR_ICONS = {
    "mode-configure": "mode-configure.svg",
    "mode-codesim": "mode-codesim.svg",
    "mode-gameplay": "mode-gameplay.svg",
    add: "add-icon.svg",
    settings: "settings.svg",
    login: "adsk-login.svg",
    "cfg-1": "cfg-controls.svg",
    "cfg-2": "cfg-drivetrain.svg",
    "cfg-3": "cfg-intake.svg",
    "cfg-4": "cfg-ejector.svg",
    "cfg-5": "cfg-joints.svg",
    "cfg-6": "cfg-alliance.svg",
    "cfg-7": "cfg-protected-zones.svg",
    "cfg-8": "cfg-scoring-zones.svg",
    "cfg-9": "cfg-camera-positions.svg",
    "gp-1": "gp-multiplayer.svg",
    "gp-2": "gp-match-mode.svg",
} as const

export type TopBarIconName = keyof typeof TOP_BAR_ICONS

export const TopBarIcon: FC<{ name: TopBarIconName; size?: number | string; className?: string }> = ({
    name,
    size = 24,
    className,
}) => {
    const hostRef = useRef<HTMLSpanElement | null>(null)
    const iconSvg = useMemo(() => ICON_SVGS[`./icons/${TOP_BAR_ICONS[name]}`], [name])

    useEffect(() => {
        if (!hostRef.current) return

        hostRef.current.replaceChildren()
        if (!iconSvg) return

        hostRef.current.append(iconSvg.cloneNode(true))
    }, [iconSvg])

    return (
        <Box
            component="span"
            role="img"
            aria-label={name}
            className={className}
            ref={hostRef}
            sx={{
                display: "inline-flex",
                width: size,
                height: size,
                // the svgs carry a viewBox so forcing the box size scales them cleanly
                "& > svg": { width: "100%", height: "100%", display: "block" },
            }}
        />
    )
}
