import { useCallback, useRef, useState } from "react"
import Label from "./Label"

function TouchControls() {
    const inputRef = useRef<HTMLInputElement>(null)

    const [showPlaceButton, setShowPlaceButton] = useState(true)
    const [showJoystick, setShowJoystick] = useState<boolean>(true)

    const touch = matchMedia("(hover: none)").matches

    console.log(touch) // true if the device has touch capabilities

    /** simulates an enter key press and release within a 100 millisecond succession */
    const PlaceMirabufAssembly = useCallback(() => {
        if (inputRef.current) {
            const pressEvent = new KeyboardEvent("keydown", {
                key: "Enter",
                code: "Enter",
                bubbles: true,
            })
            inputRef.current.dispatchEvent(pressEvent)

            setTimeout(() => {
                const releaseEvent = new KeyboardEvent("keyup", {
                    key: "Enter",
                    code: "Enter",
                    bubbles: true,
                })
                inputRef.current?.dispatchEvent(releaseEvent)
            }, 100)
        }
    }, [])

    return (
        <>
            <input ref={inputRef} />
            <div
                id="place-assembly-button"
                className={`absolute top-5 right-5 w-36 h-36 touch-none bg-black bg-opacity-20 rounded-full ${showPlaceButton ? "" : "hidden"}`}
                onClick={PlaceMirabufAssembly}
            >
                <Label></Label>
            </div>

            <div
                id="joystick-left"
                className={`absolute bottom-5 left-5 w-36 h-36 touch-none ${showJoystick ? "" : "hidden"}`}
            >
                <div id="joystick-base-left" className="relative w-full h-full bg-black bg-opacity-20 rounded-full">
                    <div
                        id="joystick-stick-left"
                        className="absolute w-14 h-14 bg-black bg-opacity-50 rounded-full top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2"
                    ></div>
                </div>
            </div>
            <div
                id="joystick-right"
                className={`absolute bottom-5 right-5 w-36 h-36 touch-none ${showJoystick ? "" : "hidden"}`}
            >
                <div id="joystick-base-right" className="relative w-full h-full bg-black bg-opacity-20 rounded-full">
                    <div
                        id="joystick-stick-right"
                        className="absolute w-14 h-14 bg-black bg-opacity-50 rounded-full top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2"
                    ></div>
                </div>
            </div>
        </>
    )
}

export default TouchControls
