import MultiplayerSystem from "@/systems/multiplayer/MultiplayerSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { MultiplayerInitProps } from "@/modals/multiplayer/MultiplayerStartModal.tsx"

/**
 * Shared `startWorldCallback` for {MultiplayerStartModal}, used by both the
 * desktop (GameplayControls) and mobile (MobileHUD) entry points. Returns
 * whether the multiplayer session was set up successfully.
 */
export async function startMultiplayerWorld(info: MultiplayerInitProps): Promise<boolean> {
    PreferencesSystem.setUserPreference("MultiplayerUsername", info.displayName)
    PreferencesSystem.savePreferences()
    return await MultiplayerSystem.setup(info.ws, info.displayName)
}
