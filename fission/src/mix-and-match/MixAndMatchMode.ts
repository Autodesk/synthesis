import EventSystem from "@/systems/EventSystem"
import { PAUSE_REF_MIX_AND_MATCH } from "@/systems/physics/PhysicsTypes"
import World from "@/systems/World"
import MixAndMatchBuild from "./MixAndMatchBuild"
import type { MixAndMatchSession } from "./MixAndMatchTypes"

/**
 * Lifecycle for mix-and-match build mode.
 *
 * Physics is paused for the entire session rather than per part: the user is arranging geometry, not
 * simulating it, and parts are expected to interpenetrate while being positioned.
 */
class MixAndMatchMode {
    private static _build: MixAndMatchBuild | undefined

    public static get build(): MixAndMatchBuild | undefined {
        return this._build
    }

    public static get isActive(): boolean {
        return this._build != null
    }

    /**
     * Enters build mode.
     *
     * @param   session Existing session to resume, e.g. one read off a saved mira. Omit for a new build.
     * @returns The active build. Re-entering while already active returns the current one untouched.
     */
    public static enter(session?: MixAndMatchSession): MixAndMatchBuild {
        if (this._build) return this._build

        World.physicsSystem.holdPause(PAUSE_REF_MIX_AND_MATCH)
        this._build = new MixAndMatchBuild(session)
        EventSystem.dispatch("MixAndMatchStateChangedEvent")

        return this._build
    }

    public static exit() {
        if (!this._build) return

        this._build = undefined
        World.physicsSystem.releasePause(PAUSE_REF_MIX_AND_MATCH)
        EventSystem.dispatch("MixAndMatchStateChangedEvent")
    }
}

export default MixAndMatchMode
