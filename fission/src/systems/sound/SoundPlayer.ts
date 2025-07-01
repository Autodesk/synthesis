import PreferencesSystem from "../preferences/PreferencesSystem"
import { clamp } from "@/util/Utility"

export class SoundPlayer {
    private static audioElements: HTMLAudioElement[] = []

    constructor() {}

    public static async play(filePath: string): Promise<void> {
        const audio = new Audio(filePath)

        SoundPlayer.audioElements.push(audio)
        audio.addEventListener("ended", () => {
            const index = SoundPlayer.audioElements.indexOf(audio)
            if (index !== -1) {
                SoundPlayer.audioElements.splice(index, 1)
            }
        })

        audio.volume = PreferencesSystem.getGlobalPreference("MuteAllSound")
            ? 0
            : clamp(PreferencesSystem.getGlobalPreference("SFXVolume") / 100, 0, 1)

        return audio.play().catch(error => {
            console.error("Error playing the audio file:", error)
        })
    }

    public static changeVolume(): void {
        const volume = PreferencesSystem.getGlobalPreference("MuteAllSound")
            ? 0
            : clamp(PreferencesSystem.getGlobalPreference("SFXVolume") / 100, 0, 1)
        SoundPlayer.audioElements.forEach(audio => (audio.volume = volume))
    }
}
