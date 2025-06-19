import PreferencesSystem from "../preferences/PreferencesSystem"
import { clamp } from "@/util/MathematicalFunctions"

import buttonPressSound from "@/assets/sound-files/ButtonPress.mp3"
import checkboxPressSound from "@/assets/sound-files/CheckboxPress.wav"
import dropdownMenuSound from "@/assets/sound-files/DullClick.wav"

class SoundPlayer {
    constructor() {}

    public async play(filePath: string): Promise<void> {
        const audio = new Audio(filePath)

        audio.volume = PreferencesSystem.getGlobalPreference<boolean>("MuteAllSound")
            ? 0
            : clamp(PreferencesSystem.getGlobalPreference<number>("SFXVolume") / 100, 0, 1)

        return audio.play().catch(error => {
            console.error("Error playing the audio file:", error)
        })
    }
}

const soundPlayer = new SoundPlayer()

export function buttonPressSFX() {
    soundPlayer.play(buttonPressSound)
}

export function checkboxPressedSFX() {
    soundPlayer.play(checkboxPressSound)
}

export function dropdownMenuSFX() {
    soundPlayer.play(dropdownMenuSound)
}
