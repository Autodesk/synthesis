import { useCallback, useEffect, useRef, useState } from "react"
import Label from "./Label"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"

function TouchControls() {
    const inputRef = useRef<HTMLInputElement>(null)

    const [isPlaceButtonVisible, setIsPlaceButtonVisible] = useState<boolean>(false)
    const [isJoystickVisible, setIsJoystickVisible] = useState<boolean>(
        PreferencesSystem.getGlobalPreference<boolean>("TouchControls")
    )

    useEffect(() => {
        const handlePlaceButtonEvent = (e: Event) => {
            setIsPlaceButtonVisible((e as TouchControlsEvent).value!)
        }

        const handleJoystickEvent = () => {
            PreferencesSystem.setGlobalPreference("TouchControls", !isJoystickVisible)
            PreferencesSystem.savePreferences()
            setIsJoystickVisible(!isJoystickVisible)
        }

        TouchControlsEvent.Listen(TouchControlsEventKeys.PLACE_BUTTON, handlePlaceButtonEvent)
        TouchControlsEvent.Listen(TouchControlsEventKeys.JOYSTICK, handleJoystickEvent)

        window.dispatchEvent(new Event("touchcontrolsloaded"))

        return () => {
            TouchControlsEvent.RemoveListener(TouchControlsEventKeys.PLACE_BUTTON, handlePlaceButtonEvent)
            TouchControlsEvent.RemoveListener(TouchControlsEventKeys.JOYSTICK, handleJoystickEvent)
        }
    }, [isJoystickVisible, isPlaceButtonVisible])

    /** simulates an enter key press and release within a 100 millisecond succession */
    const PlaceMirabufAssembly = useCallback(() => {
        if (inputRef.current) {
            const pressEvent = new KeyboardEvent("keydown", {
                key: "ConfirmAssemblyButton",
                code: "ConfirmAssemblyButton",
                bubbles: true,
            })
            inputRef.current.dispatchEvent(pressEvent)

            setTimeout(() => {
                const releaseEvent = new KeyboardEvent("keyup", {
                    key: "ConfirmAssemblyButton",
                    code: "ConfirmAssemblyButton",
                    bubbles: true,
                })
                inputRef.current?.dispatchEvent(releaseEvent)
            }, 100)
        }
    }, [])

    return (
        <div className="select-none">
            <input ref={inputRef} className="hidden" />
            <div
                id="place-assembly-button"
                className={`absolute top-5 right-5 w-36 h-36 touch-none bg-gray-100 bg-opacity-20 rounded-full ${isPlaceButtonVisible ? "" : "hidden"}`}
                onClick={PlaceMirabufAssembly}
            >
                <Label></Label>
            </div>

            <div
                id="joystick-base-left"
                className={`fixed bottom-[5vh] left-[5vw] w-60 h-60 touch-none ${isJoystickVisible ? "" : "hidden"}`}
            >
                <div
                    id="joystick-left-circle"
                    className="relative w-36 h-36 bg-gray-100 bg-blend-difference bg-opacity-30 rounded-full left-1/2 top-1/2 transform -translate-x-1/2 -translate-y-1/2"
                >
                    <div
                        id="joystick-stick-left"
                        className="absolute w-14 h-14 bg-black bg-opacity-70 rounded-full top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2"
                    ></div>
                </div>
            </div>
            <div
                id="joystick-base-right"
                className={`fixed bottom-[5vh] right-[5vw] w-60 h-60 touch-none ${isJoystickVisible ? "" : "hidden"}`}
            >
                <div
                    id="joystick-right-circle"
                    className="relative w-36 h-36 bg-gray-100 bg-blend-difference bg-opacity-30 rounded-full left-1/2 top-1/2 transform -translate-x-1/2 -translate-y-1/2"
                >
                    <div
                        id="joystick-stick-right"
                        className="absolute w-14 h-14 bg-black bg-opacity-70 rounded-full top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2"
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

    public static Listen(eventKey: TouchControlsEventKeys, func: (e: Event) => void) {
        window.addEventListener(eventKey, func)
    }

    public static RemoveListener(eventKey: TouchControlsEventKeys, func: (e: Event) => void) {
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
