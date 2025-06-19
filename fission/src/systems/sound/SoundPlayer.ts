import PreferencesSystem from "../preferences/PreferencesSystem"
import { clamp } from "@/util/Utility"

export class SoundPlayer {
    constructor() {}

    public static async play(filePath: string): Promise<void> {
        const audio = new Audio(filePath)

        audio.volume = PreferencesSystem.getGlobalPreference<boolean>("MuteAllSound")
            ? 0
            : clamp(PreferencesSystem.getGlobalPreference<number>("SFXVolume") / 100, 0, 1)

        return audio.play().catch(error => {
            console.error("Error playing the audio file:", error)
        })
    }
}
