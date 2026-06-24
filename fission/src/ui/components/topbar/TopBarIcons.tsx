import type React from "react"

/**
 * Topbar icons are file-based SVGs served from `/public/icons`. To swap a
 * placeholder for final art, replace the matching file in `/public/icons` —
 * no code change required.
 */
export const TOP_BAR_ICONS = {
    "mode-configure": "/icons/mode-configure.svg",
    "mode-codesim": "/icons/mode-codesim.svg",
    "mode-gameplay": "/icons/mode-gameplay.svg",
    add: "/icons/add-icon.svg",
    settings: "/icons/settings.svg",
    login: "/icons/adsk-login.svg",
    "cfg-1": "/icons/cfg-controls.svg",
    "cfg-2": "/icons/cfg-drivetrain.svg",
    "cfg-3": "/icons/cfg-intake.svg",
    "cfg-4": "/icons/cfg-ejector.svg",
    "cfg-5": "/icons/cfg-joints.svg",
    "cfg-6": "/icons/cfg-alliance.svg",
    "cfg-7": "/icons/cfg-protected-zones.svg",
    "cfg-8": "/icons/cfg-scoring-zones.svg",
    "gp-1": "/icons/gp-multiplayer.svg",
    "gp-2": "/icons/gp-match-mode.svg",
} as const

export type TopBarIconName = keyof typeof TOP_BAR_ICONS

export const TopBarIcon: React.FC<{ name: TopBarIconName; size?: number; className?: string }> = ({
    name,
    size = 24,
    className,
}) => <img src={TOP_BAR_ICONS[name]} alt={name} width={size} height={size} className={className} draggable={false} />
