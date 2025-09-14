import type React from "react"
import { useEffect, useRef, useState } from "react"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"

const TouchControls: React.FC = () => {
    const inputRef = useRef<HTMLInputElement>(null)

    const [_isPlaceButtonVisible, setIsPlaceButtonVisible] = useState(false)
    const [isJoystickVisible, setIsJoystickVisible] = useState(PreferencesSystem.getGlobalPreference("TouchControls"))

    useEffect(() => {
        const handlePlaceButtonEvent = (e: Event) => {
            setIsPlaceButtonVisible((e as TouchControlsEvent).value!)
        }

        const handleJoystickEvent = () => {
            PreferencesSystem.setGlobalPreference("TouchControls", !isJoystickVisible)
            PreferencesSystem.savePreferences()
            setIsJoystickVisible(!isJoystickVisible)
        }

        TouchControlsEvent.listen(TouchControlsEventKeys.PLACE_BUTTON, handlePlaceButtonEvent)
        TouchControlsEvent.listen(TouchControlsEventKeys.JOYSTICK, handleJoystickEvent)

        window.dispatchEvent(new Event("touchcontrolsloaded"))

        return () => {
            TouchControlsEvent.removeListener(TouchControlsEventKeys.PLACE_BUTTON, handlePlaceButtonEvent)
            TouchControlsEvent.removeListener(TouchControlsEventKeys.JOYSTICK, handleJoystickEvent)
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

export const enum TouchControlsEventKeys {
    PLACE_BUTTON = "PlaceButtonEvent",
    JOYSTICK = "JoystickEvent",
}

export class TouchControlsEvent extends Event {
    public value: boolean | undefined

    constructor(eventKey: TouchControlsEventKeys, value?: boolean) {
        super(eventKey)

        if (value) this.value = value

        window.dispatchEvent(this)
    }

    public static listen(eventKey: TouchControlsEventKeys, func: (e: Event) => void) {
        window.addEventListener(eventKey, func)
    }

    public static removeListener(eventKey: TouchControlsEventKeys, func: (e: Event) => void) {
        window.removeEventListener(eventKey, func)
    }
}

/** Notates the left and right joysticks with their x and y axis */
export const enum TouchControlsAxes {
    NONE,
    LEFT_X,
    LEFT_Y,
    RIGHT_X,
    RIGHT_Y,
}
