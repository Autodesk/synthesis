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
    "carat-down": "/icons/carat-down.svg",
    add: "/icons/add.svg",
    settings: "/icons/settings.svg",
    login: "/icons/login.svg",
    "cfg-1": "/icons/cfg-1.svg",
    "cfg-2": "/icons/cfg-2.svg",
    "cfg-3": "/icons/cfg-3.svg",
    "cfg-4": "/icons/cfg-4.svg",
    "cfg-5": "/icons/cfg-5.svg",
} as const

export type TopBarIconName = keyof typeof TOP_BAR_ICONS

export const TopBarIcon: React.FC<{ name: TopBarIconName; size?: number; className?: string }> = ({
    name,
    size = 24,
    className,
}) => <img src={TOP_BAR_ICONS[name]} alt={name} width={size} height={size} className={className} draggable={false} />
