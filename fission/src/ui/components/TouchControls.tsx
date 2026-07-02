import { alpha, useTheme } from "@mui/material/styles"
import type React from "react"
import { useCallback, useEffect, useState } from "react"
import { Joystick } from "react-joystick-component"
import type { IJoystickUpdateEvent } from "react-joystick-component/build/lib/Joystick"
import EventSystem from "@/systems/EventSystem.ts"
import InputSystem from "@/systems/input/InputSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"

const JOYSTICK_SIZE = 120

const TouchControls: React.FC = () => {
    const theme = useTheme()

    const [isJoystickVisible, setIsJoystickVisible] = useState(PreferencesSystem.getGlobalPreference("TouchControls"))

    useEffect(() => {
        const visibilityUnsubscriber = EventSystem.listen("ToggleTouchControlsVisibilityEvent", () => {
            setIsJoystickVisible(prev => {
                const next = !prev
                PreferencesSystem.setGlobalPreference("TouchControls", next)
                PreferencesSystem.savePreferences()
                return next
            })
        })

        const setVisibilityUnsubscriber = EventSystem.listen("SetTouchControlsVisibilityEvent", (visible: unknown) => {
            setIsJoystickVisible(prev => {
                const next = visible as boolean
                if (prev === next) return prev
                PreferencesSystem.setGlobalPreference("TouchControls", next)
                PreferencesSystem.savePreferences()
                return next
            })
        })

        return () => {
            visibilityUnsubscriber()
            setVisibilityUnsubscriber()
        }
    }, [])

    const handleLeftMove = useCallback((event: IJoystickUpdateEvent) => {
        InputSystem.setLeftJoystick(event.x ?? 0, event.y ?? 0)
    }, [])

    const handleLeftStop = useCallback((_event: IJoystickUpdateEvent) => {
        InputSystem.setLeftJoystick(0, 0)
    }, [])

    const handleRightMove = useCallback((event: IJoystickUpdateEvent) => {
        InputSystem.setRightJoystick(event.x ?? 0, event.y ?? 0)
    }, [])

    const handleRightStop = useCallback((_event: IJoystickUpdateEvent) => {
        InputSystem.setRightJoystick(0, 0)
    }, [])

    if (!isJoystickVisible) return null

    return (
        <div className="select-none" style={{ pointerEvents: "none" }}>
            {/* Left Joystick */}
            <div
                style={{
                    position: "fixed",
                    bottom: "5vh",
                    left: "5vw",
                    pointerEvents: "auto",
                }}
            >
                <Joystick
                    size={JOYSTICK_SIZE}
                    baseColor={alpha(theme.palette.primary.main, 0.15)}
                    stickColor={alpha(theme.palette.primary.main, 0.6)}
                    move={handleLeftMove}
                    stop={handleLeftStop}
                    throttle={16}
                />
            </div>
            {/* Right Joystick */}
            <div
                style={{
                    position: "fixed",
                    bottom: "5vh",
                    right: "5vw",
                    pointerEvents: "auto",
                }}
            >
                <Joystick
                    size={JOYSTICK_SIZE}
                    baseColor={alpha(theme.palette.primary.main, 0.15)}
                    stickColor={alpha(theme.palette.primary.main, 0.6)}
                    move={handleRightMove}
                    stop={handleRightStop}
                    throttle={16}
                />
            </div>
        </div>
    )
}

export default TouchControls

/** Notates the left and right joysticks with their x and y axis */
export const enum TouchControlsAxes {
    NONE,
    LEFT_X,
    LEFT_Y,
    RIGHT_X,
    RIGHT_Y,
}
