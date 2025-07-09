import PreferencesSystem from "../preferences/PreferencesSystem"
import { clamp } from "@/util/Utility"
import dropdownMenuSound from "@/assets/sound-files/DullClick.wav"
import clickdownSound from "@/assets/sound-files/clickdown.wav"
import clickupSound from "@/assets/sound-files/clickup.wav"
import checkdownSound from "@/assets/sound-files/checkdown.wav"
import checkupSound from "@/assets/sound-files/checkup.wav"

const preloadSounds = [dropdownMenuSound, clickdownSound, clickupSound, checkdownSound, checkupSound]
type SoundEffect = {
    onMouseDown?: () => void
    onMouseUp?: () => void
}
export class SoundPlayer {
    private static _audioElements: Map<string, HTMLAudioElement> = new Map()

    constructor() {}
    static {
        preloadSounds.forEach(sound => {
            setTimeout(() => SoundPlayer.loadSound(sound))
        })
    }
    private static async loadSound(filePath: string): Promise<HTMLAudioElement> {
        let audio = this._audioElements.get(filePath)
        if (audio == null) {
            audio = new Audio(filePath)
            SoundPlayer._audioElements.set(filePath, audio)
            audio.volume = PreferencesSystem.getGlobalPreference("MuteAllSound")
                ? 0
                : clamp(PreferencesSystem.getGlobalPreference("SFXVolume") / 100, 0, 1)
        }
        return audio
    }

    public static async play(filePath: string): Promise<void> {
        const audio = await SoundPlayer.loadSound(filePath)
        if (!audio.ended) {
            audio.pause()
            audio.currentTime = 0
        }
        return audio.play().catch(error => {
            console.error("Error playing the audio file:", error)
        })
    }

    public static buttonSoundEffects(): SoundEffect {
        return {
            onMouseDown: () => SoundPlayer.play(clickdownSound),
            onMouseUp: () => {
                if (SoundPlayer._audioElements.get(clickdownSound)?.ended) {
                    return SoundPlayer.play(clickupSound)
                }
            },
        }
    }
    public static checkboxSoundEffects(): SoundEffect {
        return {
            onMouseDown: () => SoundPlayer.play(checkdownSound),
            onMouseUp: () => {
                if (SoundPlayer._audioElements.get(checkdownSound)?.ended) {
                    return SoundPlayer.play(checkupSound)
                }
            },
        }
    }
    public static dropdownSoundEffects(): SoundEffect {
        return {
            onMouseDown: () => SoundPlayer.play(dropdownMenuSound),
        }
    }

    public static changeVolume(): void {
        const volume = PreferencesSystem.getGlobalPreference("MuteAllSound")
            ? 0
            : clamp(PreferencesSystem.getGlobalPreference("SFXVolume") / 100, 0, 1)
        SoundPlayer._audioElements.forEach(audio => (audio.volume = volume))
    }
}
