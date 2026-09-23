import checkdownSound from "@/assets/sound-files/checkdown.wav"
import checkupSound from "@/assets/sound-files/checkup.wav"
import clickdownSound from "@/assets/sound-files/clickdown.wav"
import clickupSound from "@/assets/sound-files/clickup.wav"
import dropdownMenuSound from "@/assets/sound-files/DullClick.wav"
import { clamp } from "@/util/Utility"
import PreferencesSystem from "../preferences/PreferencesSystem"

const preloadSounds = [dropdownMenuSound, clickdownSound, clickupSound, checkdownSound, checkupSound]
type SoundEffect = {
    onMouseDown?: (e?: MouseEvent | unknown) => void
    onMouseUp?: (e?: MouseEvent | unknown) => void
}
export class SoundPlayer {
    private readonly _audioContext = new AudioContext()
    /** Decoded audio data for each sound file, cached so it's only fetched/decoded once. */
    private readonly _buffers: Map<string, Promise<AudioBuffer>> = new Map()
    /** The most recent playback of each sound, used to gate follow-up sounds and live-update volume. */
    private readonly _active: Map<string, { source: AudioBufferSourceNode; gain: GainNode }> = new Map()
    private static _instance: SoundPlayer | undefined
    public static getInstance() {
        SoundPlayer._instance ??= new SoundPlayer()
        return SoundPlayer._instance
    }
    constructor() {
        preloadSounds.forEach(sound => this.getBuffer(sound))

        // Ambient session type mixes with other apps audio instead of pausing it (currently only on Safari)
        if (navigator.audioSession) {
            navigator.audioSession.type = "ambient"
        }
    }

    private get _currentVolume(): number {
        return PreferencesSystem.getUserPreference("MuteAllSound")
            ? 0
            : clamp(PreferencesSystem.getUserPreference("SFXVolume") / 100, 0, 1)
    }

    private getBuffer(filePath: string): Promise<AudioBuffer> {
        let buffer = this._buffers.get(filePath)
        if (buffer == null) {
            buffer = fetch(filePath)
                .then(response => response.arrayBuffer())
                .then(data => this._audioContext.decodeAudioData(data))
            this._buffers.set(filePath, buffer)
        }
        return buffer
    }

    public async play(filePath: string): Promise<void> {
        try {
            const buffer = await this.getBuffer(filePath)
            if (this._audioContext.state === "suspended") {
                await this._audioContext.resume()
            }

            const gain = this._audioContext.createGain()
            gain.gain.value = this._currentVolume
            gain.connect(this._audioContext.destination)

            const source = this._audioContext.createBufferSource()
            source.buffer = buffer
            source.connect(gain)

            this._active.set(filePath, { source, gain })
            source.addEventListener(
                "ended",
                () => {
                    source.disconnect()
                    gain.disconnect()
                    if (this._active.get(filePath)?.source === source) {
                        this._active.delete(filePath)
                    }
                },
                { once: true }
            )
            source.start()
        } catch (error) {
            console.error("Error playing the audio file:", error)
        }
    }

    private isPlaying(filePath: string): boolean {
        return this._active.has(filePath)
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
            onMouseDown: e => {
                if (
                    typeof e == "object" &&
                    e != null &&
                    "target" in e &&
                    e.target instanceof HTMLElement &&
                    e.target.getAttribute("aria-disabled") === "true"
                ) {
                    return
                }
                void this.playDropdownSound()
            },
        }
    }

    public playDropdownSound(): Promise<void> {
        return this.play(dropdownMenuSound)
    }

    public changeVolume(): void {
        this._active.forEach(({ gain }) => {
            gain.gain.value = this._currentVolume
        })
    }
}
