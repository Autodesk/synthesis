import { Box } from "@mui/material"
import { type FC, useEffect, useMemo, useRef } from "react"

/**
 * Topbar icons are SVGs inlined into the bundle at build time and rendered as
 * real DOM `<svg>` nodes (not `<img src>`). This matters: an `<img>` is a
 * separate, purgeable resource, and the browser will drop its decoded bitmap
 * under memory pressure — which the APS login popup reliably triggers by loading
 * a second copy of the app (Jolt WASM + asset pack). The icon then repaints
 * blank with no error to recover from. Inline SVG paints straight from the DOM
 * every frame, so it survives.
 *
 * To swap a placeholder for final art, replace the matching file in `./icons` —
 * the filename is the value in `TOP_BAR_ICONS` below, no other code change
 * required.
 */
/**
 * Inlining many SVGs into one document puts their internal ids (clip paths,
 * gradients, embedded `<image>` refs) into a single shared namespace, so ids
 * that repeat across files collide — e.g. `settings.svg` and `add-icon.svg` both
 * define `img1` and reference it with `<use href="#img1">`, so whichever renders
 * second grabs the wrong image. As separate `<img>` documents these were
 * isolated; inlined they are not. Prefix every defined id (and its `href="#…"` /
 * `url(#…)` references) with a per-icon scope to keep each icon self-contained.
 */
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
                // The inlined SVGs carry a viewBox, so forcing the box size scales them cleanly.
                "& > svg": { width: "100%", height: "100%", display: "block" },
            }}
        />
    )
}
