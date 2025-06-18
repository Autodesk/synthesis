import buttonPressSound from "@/assets/sound-files/ButtonPress.mp3"

class SoundPlayer {
    private audio: HTMLAudioElement

    constructor(filePath: string) {
        this.audio = new Audio(filePath)
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

    // TODO allow the user to change volume in settings
}

export function buttonPressSFX() {
    const buttonPressedSFX = new SoundPlayer(buttonPressSound)
    buttonPressedSFX.Play()
}

// TODO add different SFX
