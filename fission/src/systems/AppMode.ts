export const APP_MODES = ["Configure", "Codesim", "Gameplay", "MixAndMatch"] as const
export type AppMode = (typeof APP_MODES)[number]
