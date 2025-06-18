import buttonPressSound from "@/assets/sound-files/ButtonPress.mp3"
import PreferencesSystem from "../preferences/PreferencesSystem"
import { clamp } from "@/util/MathematicalFunctions"

enum SoundType {
    SFX = 0,
    Music = 1
}

class SoundPlayer {
    private audio: HTMLAudioElement

    constructor(filePath: string, soundType: SoundType) {
        this.audio = new Audio(filePath)
        
        if (soundType == SoundType.SFX) {
            let sfxVolume = PreferencesSystem.getGlobalPreference<number>("SFXVolume")
            sfxVolume /= 100 // Changes value from percent (0 - 100) to decimal (0 - 1)
            sfxVolume = clamp(sfxVolume, 0, 1)
            this.audio.volume = sfxVolume;
        }
    }

    Play(): Promise<void> {
        return this.audio.play().catch((error) => {
            console.error("Error playing the audio file:", error)
        })
    }

    Pause(): void {
        this.audio.pause()
    }

    Stop(): void {
        this.audio.pause()
        this.audio.currentTime = 0
    }
}

export function buttonPressSFX() {
    const buttonPressedSFX = new SoundPlayer(buttonPressSound, SoundType.SFX)
    buttonPressedSFX.Play()
}

// TODO add different SFX
