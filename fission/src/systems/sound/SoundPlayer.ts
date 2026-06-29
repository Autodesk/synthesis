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
    /** Decoded audio elements, kept only to warm the browser cache. */
    private _templates: Map<string, HTMLAudioElement> = new Map()
    /** The most recent element actively playing each sound, used to gate follow-up sounds. */
    private _active: Map<string, HTMLAudioElement> = new Map()
    private static _instance: SoundPlayer | undefined
    public static getInstance() {
        SoundPlayer._instance ??= new SoundPlayer()
        return SoundPlayer._instance
    }
    constructor() {
        preloadSounds.forEach(sound => this.getTemplate(sound))
    }

    private get _currentVolume(): number {
        return PreferencesSystem.getGlobalPreference("MuteAllSound")
            ? 0
            : clamp(PreferencesSystem.getGlobalPreference("SFXVolume") / 100, 0, 1)
    }

    private getTemplate(filePath: string): HTMLAudioElement {
        let template = this._templates.get(filePath)
        if (template == null) {
            template = new Audio(filePath)
            this._templates.set(filePath, template)
        }
        return template
    }

    public play(filePath: string): Promise<void> {
        const audio = this.getTemplate(filePath).cloneNode(true) as HTMLAudioElement
        audio.volume = this._currentVolume
        this._active.set(filePath, audio)
        audio.addEventListener(
            "ended",
            () => {
                if (this._active.get(filePath) === audio) {
                    this._active.delete(filePath)
                }
            },
            { once: true }
        )
        return audio.play().catch(error => {
            console.error("Error playing the audio file:", error)
        })
    }

    private isPlaying(filePath: string): boolean {
        const audio = this._active.get(filePath)
        return audio != null && !audio.ended && !audio.paused
    }

    public buttonSoundEffects(): SoundEffect {
        return {
            onMouseDown: () => this.play(clickdownSound),
            onMouseUp: () => {
                if (!this.isPlaying(clickdownSound)) {
                    return this.play(clickupSound)
                }
            },
        }
    }
    public checkboxSoundEffects(): SoundEffect {
        return {
            onMouseDown: () => this.play(checkdownSound),
            onMouseUp: () => {
                if (!this.isPlaying(checkdownSound)) {
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
        this._active.forEach(audio => {
            audio.volume = this._currentVolume
        })
    }
}
