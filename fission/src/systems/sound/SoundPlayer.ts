import PreferencesSystem from "../preferences/PreferencesSystem"
import { clamp } from "@/util/Utility"
import dropdownMenuSound from "@/assets/sound-files/DullClick.wav"
import clickdownSound from "@/assets/sound-files/clickdown.mp3"
import clickupSound from "@/assets/sound-files/clickup.mp3"
import checkdownSound from "@/assets/sound-files/checkdown.mp3"
import checkupSound from "@/assets/sound-files/checkup.mp3"

const preloadSounds = [dropdownMenuSound, clickdownSound, clickupSound, checkdownSound, checkupSound]
type SoundEffect = {
    onMouseDown?: (event: MouseEvent) => void
    onMouseUp?: (event: MouseEvent) => void
}
export class SoundPlayer {
    private static audioElements: Map<string, HTMLAudioElement> = new Map()

    constructor() {}
    static {
        preloadSounds.forEach(sound => {
            setTimeout(() => SoundPlayer.loadSound(sound))
        })
    }
    private static async loadSound(filePath: string): Promise<HTMLAudioElement> {
        let audio = this.audioElements.get(filePath)
        if (audio == null) {
            audio = new Audio(filePath)
            SoundPlayer.audioElements.set(filePath, audio)
            audio.volume = PreferencesSystem.getGlobalPreference<boolean>("MuteAllSound")
                ? 0
                : clamp(PreferencesSystem.getGlobalPreference<number>("SFXVolume") / 100, 0, 1)
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
                if (SoundPlayer.audioElements.get(clickdownSound)?.ended) {
                    return SoundPlayer.play(clickupSound)
                }
            },
        }
    }
    public static checkboxSoundEffects(): SoundEffect {
        return {
            onMouseDown: () => SoundPlayer.play(checkdownSound),
            onMouseUp: () => {
                if (SoundPlayer.audioElements.get(checkdownSound)?.ended) {
                    return SoundPlayer.play(checkupSound)
                }
            },
        }
    }
    public static playDropdownMenuSound(): SoundEffect {
        return {
            onMouseDown: () => SoundPlayer.play(dropdownMenuSound),
        }
    }

    public static changeVolume(): void {
        const volume = PreferencesSystem.getGlobalPreference<boolean>("MuteAllSound")
            ? 0
            : clamp(PreferencesSystem.getGlobalPreference<number>("SFXVolume") / 100, 0, 1)
        SoundPlayer.audioElements.forEach(audio => (audio.volume = volume))
    }
}
