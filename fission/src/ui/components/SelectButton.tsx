import React, { useCallback, useEffect, useRef, useState } from "react"
import Button, { ButtonSize } from "./Button"
import Label, { LabelSize } from "./Label"
import Stack, { StackDirection } from "./Stack"
import World from "@/systems/World"
import { ThreeVector3_JoltVec3 } from "@/util/TypeConversions"
import Jolt from "@barclah/jolt-physics"

// raycasting constants
const RAY_MAX_LENGTH = 20.0

function SelectNode(e: MouseEvent) {
    const origin = World.SceneRenderer.mainCamera.position

    const worldSpace = World.SceneRenderer.PixelToWorldSpace(e.clientX, e.clientY)
    const dir = worldSpace.sub(origin).normalize().multiplyScalar(RAY_MAX_LENGTH)

    const res = World.PhysicsSystem.RayCast(ThreeVector3_JoltVec3(origin), ThreeVector3_JoltVec3(dir))

    if (res) {
        const body = World.PhysicsSystem.GetBody(res.data.mBodyID)
        if (!body.IsDynamic()) {
            return null
        }
        return body
    }

    return null
}

type SelectButtonProps = {
    colorClass?: string
    size?: ButtonSize
    placeholder?: string
    onSelect?: (value: Jolt.Body) => boolean
    className?: string
}

const SelectButton: React.FC<SelectButtonProps> = ({ colorClass, size, placeholder, onSelect, className }) => {
    const [value, setValue] = useState<string>()
    const [selecting, setSelecting] = useState<boolean>(false)
    const timeoutRef = useRef<NodeJS.Timeout>()

    const onReceiveSelection = useCallback(
        (value: Jolt.Body) => {
            if (onSelect) {
                if (onSelect(value)) {
                    setValue("Node")
                    clearTimeout(timeoutRef.current)
                    setSelecting(false)
                } else {
                    setSelecting(true)
                }
            }
        },
        [setValue, setSelecting, onSelect]
    )

    useEffect(() => {
        const onClick = (e: MouseEvent) => {
            if (selecting) {
                const body = SelectNode(e)
                if (body) {
                    onReceiveSelection(body)
                }
            }
        }

        World.SceneRenderer.renderer.domElement.addEventListener("click", onClick)

        return () => {
            World.SceneRenderer.renderer.domElement.removeEventListener("click", onClick)
        }
    }, [selecting, onReceiveSelection])

    // should send selecting state when clicked and then receive string value to set selecting to false

    return (
        <Stack direction={StackDirection.Horizontal}>
            <Label size={LabelSize.Medium}>{value || placeholder || "Click to select"}</Label>
            <Button
                value={selecting ? "..." : "Select"}
                colorOverrideClass={selecting ? "bg-background-secondary" : colorClass}
                size={size}
                onClick={() => {
                    // send selecting state
                    if (selecting) {
                        // cancel selection
                        clearTimeout(timeoutRef.current)
                    } else {
                        setSelecting(true)
                    }
                }}
                className={className}
            />
        </Stack>
    )
}

export default SelectButton
