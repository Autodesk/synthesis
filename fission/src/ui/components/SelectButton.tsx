import type Jolt from "@azaleacolburn/jolt-physics"
import { Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useRef, useState } from "react"
import World from "@/systems/World"
import { convertThreeVector3ToJoltVec3 } from "@/util/TypeConversions"
import { Button, LabelWithTooltip } from "./StyledComponents"

// raycasting constants
const RAY_MAX_LENGTH = 20.0

function selectNode(e: MouseEvent) {
    const origin = World.sceneRenderer.mainCamera.position

    const worldSpace = World.sceneRenderer.pixelToWorldSpace(e.clientX, e.clientY)
    const dir = worldSpace.sub(origin).normalize().multiplyScalar(RAY_MAX_LENGTH)

    const res = World.physicsSystem.rayCast(convertThreeVector3ToJoltVec3(origin), convertThreeVector3ToJoltVec3(dir))

    if (res) return World.physicsSystem.getBody(res.data.mBodyID)

    return null
}

type SelectButtonProps = {
    color?: string
    placeholder?: string
    onSelect?: (value: Jolt.Body) => boolean
    className?: string
    value?: string
}

const SelectButton: React.FC<SelectButtonProps> = ({ value, color, placeholder, onSelect, className }) => {
    const [selecting, setSelecting] = useState<boolean>(false)
    const timeoutRef = useRef<NodeJS.Timeout>()

    const onReceiveSelection = useCallback(
        (value: Jolt.Body) => {
            if (onSelect) {
                if (onSelect(value)) {
                    clearTimeout(timeoutRef.current)
                    setSelecting(false)
                } else {
                    setSelecting(true)
                }
            }
        },
        [onSelect]
    )

    useEffect(() => {
        const onClick = (e: MouseEvent) => {
            if (selecting) {
                const body = selectNode(e)
                if (body) {
                    onReceiveSelection(body)
                }
            }
        }

        World.sceneRenderer.renderer.domElement.addEventListener("click", onClick)

        return () => {
            World.sceneRenderer.renderer.domElement.removeEventListener("click", onClick)
        }
    }, [selecting, onReceiveSelection])

    // should send selecting state when clicked and then receive string value to set selecting to false

    return (
        <Stack direction="row">
            {LabelWithTooltip(
                "Select parent node",
                "Select the parent node for this object to follow. Click the button below, then click a part of the robot or field."
            )}
            <Button
                sx={{ bgcolor: color }}
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
            >
                {selecting ? "..." : value || placeholder || "Click to select"}
            </Button>
        </Stack>
    )
}

export default SelectButton
