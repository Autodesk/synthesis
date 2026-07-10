import { alpha, useTheme } from "@mui/material/styles"
import type React from "react"
import { useCallback, useEffect, useRef, useState } from "react"
import { Joystick } from "react-joystick-component"
import type { IJoystickUpdateEvent } from "react-joystick-component/build/lib/Joystick"
import EventSystem from "@/systems/EventSystem.ts"
import InputSystem from "@/systems/input/InputSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"

const JOYSTICK_SIZE = 120

const TouchControls: React.FC = () => {
    const theme = useTheme()

    const [isJoystickVisible, setIsJoystickVisible] = useState(PreferencesSystem.getUserPreference("TouchControls"))

    // Number of joysticks currently being held. Joysticks can't unmount mid-drag,
    // so we ignore toggles while one is held to keep the visibility state in sync
    // with what's shown (otherwise a held toggle desyncs the two and a later press
    // appears to do nothing).
    const heldJoystickCount = useRef(0)

    useEffect(() => {
        const visibilityUnsubscriber = EventSystem.listen("ToggleTouchControlsVisibilityEvent", () => {
            if (heldJoystickCount.current > 0) return
            setIsJoystickVisible(prev => {
                const next = !prev
                PreferencesSystem.setUserPreference("TouchControls", next)
                PreferencesSystem.savePreferences()
                return next
            })
        })

        const setVisibilityUnsubscriber = EventSystem.listen("SetTouchControlsVisibilityEvent", (visible: boolean) => {
            setIsJoystickVisible(prev => {
                if (prev === visible) return prev
                PreferencesSystem.setUserPreference("TouchControls", visible)
                PreferencesSystem.savePreferences()
                return visible
            })
        })

        return () => {
            visibilityUnsubscriber()
            setVisibilityUnsubscriber()
        }
    }, [])

    const handleStart = useCallback((_event: IJoystickUpdateEvent) => {
        heldJoystickCount.current += 1
    }, [])

    const handleLeftMove = useCallback((event: IJoystickUpdateEvent) => {
        InputSystem.setLeftJoystick(event.x ?? 0, event.y ?? 0)
    }, [])

    const handleLeftStop = useCallback((_event: IJoystickUpdateEvent) => {
        heldJoystickCount.current = Math.max(0, heldJoystickCount.current - 1)
        InputSystem.setLeftJoystick(0, 0)
    }, [])

    const handleRightMove = useCallback((event: IJoystickUpdateEvent) => {
        InputSystem.setRightJoystick(event.x ?? 0, event.y ?? 0)
    }, [])

    const handleRightStop = useCallback((_event: IJoystickUpdateEvent) => {
        heldJoystickCount.current = Math.max(0, heldJoystickCount.current - 1)
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
                    start={handleStart}
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
                    start={handleStart}
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
