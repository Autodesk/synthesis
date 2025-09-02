import checkdownSound from "@/assets/sound-files/checkdown.wav"
import checkupSound from "@/assets/sound-files/checkup.wav"
import clickdownSound from "@/assets/sound-files/clickdown.wav"
import clickupSound from "@/assets/sound-files/clickup.wav"
import dropdownMenuSound from "@/assets/sound-files/DullClick.wav"
import { clamp } from "@/util/Utility"
import PreferencesSystem from "../preferences/PreferencesSystem"

const preloadSounds = [dropdownMenuSound, clickdownSound, clickupSound, checkdownSound, checkupSound]
type SoundEffect = {
    onMouseDown?: () => void
    onMouseUp?: () => void
}
export class SoundPlayer {
    private _audioElements: Map<string, HTMLAudioElement> = new Map()
    private static _instance: SoundPlayer | undefined
    public static getInstance() {
        SoundPlayer._instance ??= new SoundPlayer()
        return SoundPlayer._instance
    }
    constructor() {
        preloadSounds.forEach(sound => {
            this.loadSound(sound).catch(e => console.warn("failed to load sound", sound, e))
        })
    }
    private async loadSound(filePath: string): Promise<HTMLAudioElement> {
        let audio = this._audioElements.get(filePath)
        if (audio == null) {
            audio = new Audio(filePath)
            this._audioElements.set(filePath, audio)
            audio.volume = PreferencesSystem.getGlobalPreference("MuteAllSound")
                ? 0
                : clamp(PreferencesSystem.getGlobalPreference("SFXVolume") / 100, 0, 1)
        }
        return audio
    }

    public async play(filePath: string): Promise<void> {
        const audio = await this.loadSound(filePath)
        if (!audio.ended) {
            audio.pause()
            audio.currentTime = 0
        }
        return audio.play().catch(error => {
            console.error("Error playing the audio file:", error)
        })
    }

    public buttonSoundEffects(): SoundEffect {
        return {
            onMouseDown: () => this.play(clickdownSound),
            onMouseUp: () => {
                if (this._audioElements.get(clickdownSound)?.ended) {
                    return this.play(clickupSound)
                }
            },
        }
    }
    public checkboxSoundEffects(): SoundEffect {
        return {
            onMouseDown: () => this.play(checkdownSound),
            onMouseUp: () => {
                if (this._audioElements.get(checkdownSound)?.ended) {
                    return this.play(checkupSound)
                }
            },
        }
    }
    public dropdownSoundEffects(): SoundEffect {
        return {
            onMouseDown: () => this.play(dropdownMenuSound),
        }
    }

    public changeVolume(): void {
        const volume = PreferencesSystem.getGlobalPreference("MuteAllSound")
            ? 0
            : clamp(PreferencesSystem.getGlobalPreference("SFXVolume") / 100, 0, 1)
        this._audioElements.forEach(audio => {
            audio.volume = volume
        })
    }
}
