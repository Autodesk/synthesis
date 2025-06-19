import PreferencesSystem from "../preferences/PreferencesSystem"
import { clamp } from "@/util/MathematicalFunctions"

import buttonPressSound from "@/assets/sound-files/ButtonPress.mp3"
import checkboxPressSound from "@/assets/sound-files/CheckboxPress.wav"
import dropdownMenuSound from "@/assets/sound-files/DullClick.wav"

class SoundPlayer {
    private audio: HTMLAudioElement

    constructor(filePath: string) {
        this.audio = new Audio(filePath)
        
        this.audio.volume = PreferencesSystem.getGlobalPreference<boolean>("MuteAllSound")
            ? 0
            : clamp(PreferencesSystem.getGlobalPreference<number>("SFXVolume") / 100, 0, 1)
    }

    async play(): Promise<void> {
        return this.audio.play().catch((error) => {
            console.error("Error playing the audio file:", error)
        })
    }

    pause(): void {
        this.audio.pause()
    }

    stop(): void {
        this.audio.pause()
        this.audio.currentTime = 0
    }
}

export function buttonPressSFX() {
    const buttonPressedSFX = new SoundPlayer(buttonPressSound)
    buttonPressedSFX.play()
}

export function checkboxPressedSFX() {
    const checkboxPressedSFX = new SoundPlayer(checkboxPressSound)
    checkboxPressedSFX.play()
}

export function dropdownMenuSFX() {
    const dropdownMenuSFX = new SoundPlayer(dropdownMenuSound)
    dropdownMenuSFX.play()
}
