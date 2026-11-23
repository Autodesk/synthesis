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

    // joysticks that are currently being dragged to avoid desyncing by toggling while being dragged
    const heldJoystickCount = useRef(0)

    const isVisibleRef = useRef(isJoystickVisible)

    const applyVisibility = useCallback((visible: boolean) => {
        if (isVisibleRef.current === visible) return
        isVisibleRef.current = visible
        setIsJoystickVisible(visible)
        PreferencesSystem.setUserPreference("TouchControls", visible)
        PreferencesSystem.savePreferences()
        EventSystem.dispatch("TouchControlsVisibilityChangedEvent", { visible })
    }, [])

    useEffect(() => {
        const visibilityUnsubscriber = EventSystem.listen("ToggleTouchControlsVisibilityEvent", () => {
            if (heldJoystickCount.current > 0) return
            applyVisibility(!isVisibleRef.current)
        })

        const setVisibilityUnsubscriber = EventSystem.listen("SetTouchControlsVisibilityEvent", (visible: boolean) =>
            applyVisibility(visible)
        )

        return () => {
            visibilityUnsubscriber()
            setVisibilityUnsubscriber()
        }
    }, [applyVisibility])

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
