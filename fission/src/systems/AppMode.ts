export const APP_MODES = ["Configure", "Codesim", "Gameplay"] as const
export type AppMode = (typeof APP_MODES)[number]
