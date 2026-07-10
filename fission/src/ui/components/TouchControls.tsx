import type React from "react"
import { useEffect, useRef, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"

const TouchControls: React.FC = () => {
    const inputRef = useRef<HTMLInputElement>(null)

    const [_isPlaceButtonVisible, setIsPlaceButtonVisible] = useState(false)
    const [isJoystickVisible, setIsJoystickVisible] = useState(PreferencesSystem.getUserPreference("TouchControls"))

    useEffect(() => {
        const placeButtonUnsubscriber = EventSystem.listen("SetPlaceAssetButtonVisibleEvent", visible => {
            setIsPlaceButtonVisible(visible)
        })

        const visibilityUnsubscriber = EventSystem.listen("ToggleTouchControlsVisibilityEvent", () => {
            PreferencesSystem.setUserPreference("TouchControls", !isJoystickVisible)
            PreferencesSystem.savePreferences()
            setIsJoystickVisible(!isJoystickVisible)
        })

        EventSystem.dispatch("TouchControlsLoaded")

        return () => {
            placeButtonUnsubscriber()
            visibilityUnsubscriber()
        }
    }, [isJoystickVisible])

    return (
        <div className="select-none">
            <input ref={inputRef} className="hidden" />
            {/* Left Joystick */}
            <div
                id="joystick-base-left"
                className={`fixed bottom-[5vh] left-[5vw] w-[35vmin] h-[35vmin] max-w-60 max-h-60 touch-none ${
                    isJoystickVisible ? "" : "hidden"
                }`}
            >
                <div
                    id="joystick-left-circle"
                    className="relative w-[60%] h-[60%] bg-gray-100 bg-blend-difference bg-opacity-30 rounded-full left-1/2 top-1/2 transform -translate-x-1/2 -translate-y-1/2"
                >
                    <div
                        id="joystick-stick-left"
                        className="absolute w-[40%] h-[40%] bg-black bg-opacity-70 rounded-full top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2"
                    ></div>
                </div>
            </div>
            {/* Right Joystick */}
            <div
                id="joystick-base-right"
                className={`fixed bottom-[5vh] right-[5vw] w-[35vmin] h-[35vmin] max-w-60 max-h-60 touch-none ${
                    isJoystickVisible ? "" : "hidden"
                }`}
            >
                <div
                    id="joystick-right-circle"
                    className="relative w-[60%] h-[60%] bg-gray-100 bg-blend-difference bg-opacity-30 rounded-full left-1/2 top-1/2 transform -translate-x-1/2 -translate-y-1/2"
                >
                    <div
                        id="joystick-stick-right"
                        className="absolute w-[40%] h-[40%] bg-black bg-opacity-70 rounded-full top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2"
                    ></div>
                </div>
            </div>
        </div>
    )
}

export default TouchControls

export const MAX_JOYSTICK_RADIUS: number = 55

/** Notates the left and right joysticks with their x and y axis */
export const enum TouchControlsAxes {
    NONE,
    LEFT_X,
    LEFT_Y,
    RIGHT_X,
    RIGHT_Y,
}
