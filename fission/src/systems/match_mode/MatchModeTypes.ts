export enum MatchModeType {
    SANDBOX = "Sandbox",
    AUTONOMOUS = "Autonomous",
    TELEOP = "Teleop",
    ENDGAME = "Endgame",
    MATCH_ENDED = "Match Ended",
}

// Default match mode timing values
export const DEFAULT_AUTONOMOUS_TIME = 15
export const DEFAULT_TELEOP_TIME = 135
export const DEFAULT_ENDGAME_TIME = 20
export const DEFAULT_IGNORE_ROTATION = true
export const DEFAULT_MAX_HEIGHT = Infinity
export const DEFAULT_HEIGHT_LIMIT_PENALTY = 2
export const DEFAULT_SIDE_MAX_EXTENSION = Infinity
export const DEFAULT_SIDE_EXTENSION_PENALTY = 2
