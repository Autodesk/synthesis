import { globalAddToast } from "@/components/GlobalUIControls"
import MultiplayerSystem from "@/systems/multiplayer/MultiplayerSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"

/**
 * Shared `startWorldCallback` for {MultiplayerStartModal}. Returns whether the
 * multiplayer session was set up successfully.
 *
 * for both the desktop (GameplayControls) and mobile (MobileHUD) entry points.
 */
export async function startMultiplayerWorld(name: string, room?: string): Promise<boolean> {
    const isHost = room == null
    const roomId = room ?? Math.random().toString(10).substring(2, 8)
    PreferencesSystem.setUserPreference("MultiplayerUsername", name)
    PreferencesSystem.savePreferences()
    const success = await MultiplayerSystem.setup(roomId, name, isHost)
    if (success && isHost) globalAddToast("info", "Room Code", roomId)
    return success
}
