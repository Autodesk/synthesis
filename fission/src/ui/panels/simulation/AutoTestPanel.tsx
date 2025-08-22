import type Jolt from "@azaleacolburn/jolt-physics"
import { TextField } from "@mui/material"
import { Stack, styled } from "@mui/system"
import type React from "react"
import { useCallback, useEffect, useMemo, useState } from "react"
import { FaInfinity } from "react-icons/fa6"
import * as THREE from "three"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import SimDriverStation from "@/systems/simulation/wpilib_brain/sim/SimDriverStation"
import { type AllianceStation, RobotSimMode } from "@/systems/simulation/wpilib_brain/WPILibTypes"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import { Button, ToggleButton, ToggleButtonGroup } from "@/ui/components/StyledComponents"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import JOLT from "@/util/loading/JoltSyncLoader"
import {
    convertJoltMat44ToThreeMatrix4,
    convertThreeQuaternionToJoltQuat,
    convertThreeVector3ToJoltRVec3,
} from "@/util/TypeConversions"

type StagingProps = {
    state: "Staging"
    assembly: MirabufSceneObject
    setPlaying?: (props: PlayingProps) => void
}

type PlayingProps = {
    state: "Playing"
    assembly: MirabufSceneObject
    countdown: number
    captures: BodyCapture[]
    setEnd?: (props: EndProps) => void
}

type EndProps = {
    state: "End"
    assembly: MirabufSceneObject
    captures: BodyCapture[]
    setStaging?: (props: StagingProps) => void
}

type BodyCapture = {
    id: Jolt.BodyID
    pos: Jolt.RVec3
    rot: Jolt.Quat
}

const AUTO_TEST_PAUSE_REF = "auto-testing"

export const BlueAllianceToggleButton = styled(ToggleButton)({
    borderColor: "transparent",
    fontFamily: "Artifakt",
    fontWeight: 700,
    color: "#5f60ff",
    "&.Mui-selected": {
        color: "black",
        backgroundImage: "linear-gradient(to right, #5f60ff, #5f60ff)",
        borderColor: "transparent",
    },
    ".MuiTouchRipple-ripple": {
        color: "#ffffff30",
    },
    "&:focus": {
        borderColor: "transparent !important",
        outline: "none",
    },
    "&:selected": {
        outline: "none",
        borderColor: "transparent",
    },
    "&:hover": {
        outline: "none",
        borderColor: "transparent",
        backgroundColor: "#ffffff20",
    },
    "&:focus-visible": {
        outline: "none",
        borderColor: "transparent",
    },
    "&:active": {
        outline: "none",
        borderColor: "transparent",
    },
    "&::-moz-focus-inner": {
        outline: "none",
        borderColor: "transparent",
    },
})

export const RedAllianceToggleButton = styled(ToggleButton)({
    borderColor: "transparent",
    fontFamily: "Artifakt",
    fontWeight: 700,
    color: "#d74e26",
    "&.Mui-selected": {
        color: "black",
        backgroundImage: "linear-gradient(to right, #d74e26, #d74e26)",
        borderColor: "transparent",
    },
    ".MuiTouchRipple-ripple": {
        color: "#ffffff30",
    },
    "&:focus": {
        borderColor: "transparent !important",
        outline: "none",
    },
    "&:selected": {
        outline: "none",
        borderColor: "transparent",
    },
    "&:hover": {
        outline: "none",
        borderColor: "transparent",
        backgroundColor: "#ffffff20",
    },
    "&:focus-visible": {
        outline: "none",
        borderColor: "transparent",
    },
    "&:active": {
        outline: "none",
        borderColor: "transparent",
    },
    "&::-moz-focus-inner": {
        outline: "none",
        borderColor: "transparent",
    },
})

function captureBodies(): BodyCapture[] {
    const captures: BodyCapture[] = []
    World.sceneRenderer.mirabufSceneObjects.getAll().forEach(sceneObj => {
        sceneObj.getAllBodies().forEach(body => {
            const transform = body.GetWorldTransform()
            const translation = new THREE.Vector3(0, 0, 0)
            const rotation = new THREE.Quaternion(0, 0, 0, 1)
            convertJoltMat44ToThreeMatrix4(transform).decompose(translation, rotation, new THREE.Vector3(1, 1, 1))
            captures.push({
                id: body.GetID(),
                pos: convertThreeVector3ToJoltRVec3(translation),
                rot: convertThreeQuaternionToJoltQuat(rotation),
            })
        })
    })
    return captures
}

function resetBodies(captures: BodyCapture[]) {
    const zero = new JOLT.Vec3(0, 0, 0)
    captures.forEach(x => {
        World.physicsSystem.setBodyPositionRotationAndVelocity(x.id, x.pos, x.rot, zero, zero)
    })
    JOLT.destroy(zero)
}

const End: React.FC<EndProps> = ({ assembly, setStaging, captures }) => {
    useEffect(() => {
        SimDriverStation.setMode(RobotSimMode.DISABLED)
    }, [])

    const reset = useCallback(() => {
        resetBodies(captures)
        setStaging?.({ state: "Staging", assembly })
    }, [assembly, captures, setStaging])

    return (
        <Button className="self-center" onClick={reset}>
            Reset
        </Button>
    )
}

const Playing: React.FC<PlayingProps> = ({ assembly, setEnd, countdown, captures }) => {
    const [remaining, setRemaining] = useState<number>(countdown)

    useEffect(() => {
        World.physicsSystem.releasePause(AUTO_TEST_PAUSE_REF)
        SimDriverStation.setMode(RobotSimMode.AUTO)
    }, [])

    const end = useCallback(() => {
        SimDriverStation.setMode(RobotSimMode.DISABLED)
        World.physicsSystem.holdPause(AUTO_TEST_PAUSE_REF)
        setEnd?.({ assembly: assembly, captures: captures, state: "End" })
    }, [assembly, captures, setEnd])

    useEffect(() => {
        let handle: number | undefined
        const endTime = Date.now() / 1000.0 + countdown
        const func = () => {
            if (handle !== undefined) cancelAnimationFrame(handle)

            setRemaining(endTime - Date.now() / 1000.0)

            handle = requestAnimationFrame(func)
        }
        if (countdown > 0) {
            func()
        }

        return () => {
            if (handle !== undefined) cancelAnimationFrame(handle)
        }
    }, [countdown])

    useEffect(() => {
        if (remaining <= 0) {
            end()
        }
    }, [end, remaining])

    return (
        <>
            <Label size="md" className="text-center">
                {Math.max(remaining, 0).toFixed(1)}s
            </Label>
            <Button className="self-center" onClick={end}>
                Stop
            </Button>
        </>
    )
}

const Staging: React.FC<StagingProps> = ({ assembly, setPlaying }) => {
    const [countdown, setCountdown] = useState<number>(15)
    const [station, setStation] = useState<AllianceStation>("red1")
    const [gameData, setGameData] = useState<string>("")

    const next = useCallback(() => {
        SimDriverStation.setGameData(gameData)
        SimDriverStation.setStation(station)

        const captures = captureBodies()
        setPlaying?.({ assembly, captures, countdown, state: "Playing" })
    }, [assembly, countdown, gameData, setPlaying, station])

    return (
        <>
            <Stack>
                <Label size="md" textAlign="center">
                    Countdown
                </Label>
                <ToggleButtonGroup
                    value={countdown}
                    exclusive
                    onChange={(_, v) => setCountdown(v)}
                    className="self-center"
                >
                    <ToggleButton value={5}>5</ToggleButton>
                    <ToggleButton value={10}>10</ToggleButton>
                    <ToggleButton value={15}>15</ToggleButton>
                    <ToggleButton value={20}>20</ToggleButton>
                    <ToggleButton value={30}>30</ToggleButton>
                    <ToggleButton value={-1}>
                        <FaInfinity />
                    </ToggleButton>
                </ToggleButtonGroup>
            </Stack>
            <Stack>
                <Label size="md" textAlign="center">
                    Alliance Station
                </Label>
                <ToggleButtonGroup value={station} exclusive onChange={(_, v) => setStation(v)} className="self-center">
                    <RedAllianceToggleButton value="red1">1</RedAllianceToggleButton>
                    <RedAllianceToggleButton value="red2">2</RedAllianceToggleButton>
                    <RedAllianceToggleButton value="red3">3</RedAllianceToggleButton>
                    <BlueAllianceToggleButton value="blue1">1</BlueAllianceToggleButton>
                    <BlueAllianceToggleButton value="blue2">2</BlueAllianceToggleButton>
                    <BlueAllianceToggleButton value="blue3">3</BlueAllianceToggleButton>
                </ToggleButtonGroup>
            </Stack>
            <Stack>
                <TextField
                    label="Game Data"
                    placeholder="..."
                    defaultValue={gameData}
                    onInput={(e: React.ChangeEvent<HTMLInputElement>) => setGameData(e.target.value)}
                />
            </Stack>
            <Stack>
                <Label size="md" textAlign="center">
                    Placement
                </Label>
                <TransformGizmoControl parent={assembly} size={3} defaultMode="translate" scaleDisabled />
            </Stack>
            <Button className="self-center" onClick={next}>
                Test
            </Button>
        </>
    )
}

const AutoTestPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const [activeProps, setActiveProps] = useState<StagingProps | PlayingProps | EndProps | undefined>(undefined)
    const { configureScreen } = useUIContext()

    const assembly = useMemo(
        () => World.sceneRenderer.mirabufSceneObjects.findWhere(x => x.brain?.brainType === "wpilib"),
        []
    )

    useEffect(() => {
        configureScreen(panel!, { title: "Auto Testing", hideCancel: true, acceptText: "Done" }, {})
    }, [])

    useEffect(() => {
        SimDriverStation.setMode(RobotSimMode.DISABLED)
        return () => {
            SimDriverStation.setMode(RobotSimMode.DISABLED)
        }
    }, [])

    useEffect(() => {
        World.physicsSystem.holdPause(AUTO_TEST_PAUSE_REF)
        if (assembly == null) {
            console.warn("Couldn't find assembly with wpilib brain")
            return
        }
        setActiveProps({
            state: "Staging",
            assembly: assembly,
            setPlaying: setActiveProps,
        })

        return () => {
            World.physicsSystem.releasePause(AUTO_TEST_PAUSE_REF)
        }
    }, [assembly])

    return (
        <Stack gap={4}>
            {activeProps !== undefined &&
                (activeProps.state === "Staging" ? (
                    <Staging assembly={activeProps.assembly} setPlaying={setActiveProps} state="Staging" />
                ) : activeProps.state === "Playing" ? (
                    <Playing
                        assembly={activeProps.assembly}
                        captures={activeProps.captures}
                        countdown={activeProps.countdown}
                        setEnd={setActiveProps}
                        state="Playing"
                    />
                ) : activeProps.state === "End" ? (
                    <End
                        assembly={activeProps.assembly}
                        setStaging={setActiveProps}
                        captures={activeProps.captures}
                        state="End"
                    />
                ) : (
                    <></>
                ))}
        </Stack>
    )
}

export default AutoTestPanel
