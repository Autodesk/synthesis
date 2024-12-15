import React, { useCallback, useEffect, useMemo, useState } from "react"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import { FaInfinity, FaRobot } from "react-icons/fa6"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import { usePanelControlContext } from "@/ui/PanelContext"
import Label from "@/ui/components/Label"
import Button from "@/ui/components/Button"
import Jolt from "@barclah/jolt-physics"
import JOLT from "@/util/loading/JoltSyncLoader"
import { JoltMat44_ThreeMatrix4, ThreeQuaternion_JoltQuat, ThreeVector3_JoltVec3 } from "@/util/TypeConversions"
import * as THREE from "three"
import { AllianceStation, RobotSimMode, SimDriverStation } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import { styled } from "@mui/system"
import Input from "@/ui/components/Input"

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
    "borderColor": "transparent",
    "fontFamily": "Artifakt",
    "fontWeight": 700,
    "color": "#5f60ff",
    "&.Mui-selected": {
        color: "black",
        backgroundImage: `linear-gradient(to right, #5f60ff, #5f60ff)`,
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
    "borderColor": "transparent",
    "fontFamily": "Artifakt",
    "fontWeight": 700,
    "color": "#d74e26",
    "&.Mui-selected": {
        color: "black",
        backgroundImage: `linear-gradient(to right, #d74e26, #d74e26)`,
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
    World.SceneRenderer.sceneObjects.forEach(sceneObj => {
        if (sceneObj instanceof MirabufSceneObject) {
            sceneObj.mechanism.nodeToBody.forEach(bodyId => {
                const body = World.PhysicsSystem.GetBody(bodyId)
                const transform = body.GetWorldTransform()
                const translation = new THREE.Vector3(0, 0, 0)
                const rotation = new THREE.Quaternion(0, 0, 0, 1)
                JoltMat44_ThreeMatrix4(transform).decompose(translation, rotation, new THREE.Vector3(1, 1, 1))
                captures.push({
                    id: bodyId,
                    pos: ThreeVector3_JoltVec3(translation),
                    rot: ThreeQuaternion_JoltQuat(rotation),
                })
            })
        }
    })
    return captures
}

function resetBodies(captures: BodyCapture[]) {
    const zero = new JOLT.Vec3(0, 0, 0)
    captures.forEach(x => {
        World.PhysicsSystem.SetBodyPositionRotationAndVelocity(x.id, x.pos, x.rot, zero, zero)
    })
    JOLT.destroy(zero)
}

function End({ assembly, setStaging, captures }: EndProps) {
    useEffect(() => {
        SimDriverStation.SetMode(RobotSimMode.Disabled)
    }, [])

    const reset = useCallback(() => {
        resetBodies(captures)
        setStaging?.({ state: "Staging", assembly: assembly })
    }, [assembly, captures, setStaging])

    return (
        <>
            <Button className="self-center" value="Reset" onClick={reset} />
        </>
    )
}

function Playing({ assembly, setEnd, countdown, captures }: PlayingProps) {
    const [remaining, setRemaining] = useState<number>(countdown)

    useEffect(() => {
        World.PhysicsSystem.ReleasePause(AUTO_TEST_PAUSE_REF)
        SimDriverStation.SetMode(RobotSimMode.Auto)
    }, [])

    const end = useCallback(() => {
        SimDriverStation.SetMode(RobotSimMode.Disabled)
        World.PhysicsSystem.HoldPause(AUTO_TEST_PAUSE_REF)
        setEnd?.({ assembly: assembly, captures: captures, state: "End" })
    }, [assembly, captures, setEnd])

    useEffect(() => {
        let handle: number | undefined = undefined
        const endTime = Date.now() / 1000.0 + countdown
        const func = () => {
            if (handle != undefined) cancelAnimationFrame(handle)

            setRemaining(endTime - Date.now() / 1000.0)

            handle = requestAnimationFrame(func)
        }
        if (countdown > 0) {
            func()
        }

        return () => {
            if (handle != undefined) cancelAnimationFrame(handle)
        }
    }, [countdown])

    useEffect(() => {
        if (remaining <= 0) {
            end()
        }
    }, [end, remaining])

    return (
        <>
            <Label className="text-center">{Math.max(remaining, 0).toFixed(1)}s</Label>
            <Button className="self-center" value="Stop" onClick={end} />
        </>
    )
}

function Staging({ assembly, setPlaying }: StagingProps) {
    const [countdown, setCountdown] = useState<number>(15)
    const [station, setStation] = useState<AllianceStation>("red1")
    const [gameData, setGameData] = useState<string>("")

    const next = useCallback(() => {
        SimDriverStation.SetGameData(gameData)
        SimDriverStation.SetStation(station)

        const captures = captureBodies()
        setPlaying?.({ assembly: assembly, captures: captures, countdown: countdown, state: "Playing" })
    }, [assembly, countdown, gameData, setPlaying, station])

    return (
        <>
            <div className="flex flex-col gap-1">
                <Label className="text-center">Countdown</Label>
                <ToggleButtonGroup
                    value={countdown}
                    exclusive
                    onChange={(_, v) => {
                        setCountdown(v)
                    }}
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
            </div>
            <div className="flex flex-col gap-1">
                <Label className="text-center">Alliance Station</Label>
                <ToggleButtonGroup
                    value={station}
                    exclusive
                    onChange={(_, v) => {
                        setStation(v)
                    }}
                    className="self-center"
                >
                    <RedAllianceToggleButton value={"red1"}>1</RedAllianceToggleButton>
                    <RedAllianceToggleButton value={"red2"}>2</RedAllianceToggleButton>
                    <RedAllianceToggleButton value={"red3"}>3</RedAllianceToggleButton>
                    <BlueAllianceToggleButton value={"blue1"}>1</BlueAllianceToggleButton>
                    <BlueAllianceToggleButton value={"blue2"}>2</BlueAllianceToggleButton>
                    <BlueAllianceToggleButton value={"blue3"}>3</BlueAllianceToggleButton>
                </ToggleButtonGroup>
            </div>
            <div className="flex flex-col gap-1">
                <Input label="Game Data" placeholder="..." defaultValue={gameData} onInput={setGameData} />
            </div>
            <div className="flex flex-col gap-1">
                <Label className="text-center">Placement</Label>
                <TransformGizmoControl parent={assembly} size={3} defaultMode={"translate"} scaleDisabled />
            </div>
            <Button className="self-center" value="Test" onClick={next} />
        </>
    )
}

const AutoTestPanel: React.FC<PanelPropsImpl> = ({ panelId, sidePadding }) => {
    const { closePanel } = usePanelControlContext()
    const [activeProps, setActiveProps] = useState<StagingProps | PlayingProps | EndProps | undefined>(undefined)

    const assembly = useMemo(
        () =>
            [...World.SceneRenderer.sceneObjects.values()].find(
                x => (x as MirabufSceneObject).brain?.brainType == "wpilib"
            ) as MirabufSceneObject,
        []
    )

    useEffect(() => {
        SimDriverStation.SetMode(RobotSimMode.Disabled)
        return () => {
            SimDriverStation.SetMode(RobotSimMode.Disabled)
        }
    }, [])

    useEffect(() => {
        World.PhysicsSystem.HoldPause(AUTO_TEST_PAUSE_REF)

        setActiveProps({
            state: "Staging",
            assembly: assembly,
            setPlaying: setActiveProps,
        })

        return () => {
            World.PhysicsSystem.ReleasePause(AUTO_TEST_PAUSE_REF)
        }
    }, [assembly])

    useEffect(() => {
        closePanel("configure")
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [])

    return (
        <Panel
            name="Auto Testing"
            icon={<FaRobot />}
            panelId={panelId}
            openLocation={"right"}
            sidePadding={sidePadding}
            cancelEnabled={false}
            acceptName="Done"
        >
            <div className="flex flex-col bg-background-secondary rounded-md p-2 gap-4">
                {activeProps != undefined ? (
                    activeProps.state == "Staging" ? (
                        <Staging assembly={activeProps.assembly} setPlaying={setActiveProps} state="Staging" />
                    ) : activeProps.state == "Playing" ? (
                        <Playing
                            assembly={activeProps.assembly}
                            captures={activeProps.captures}
                            countdown={activeProps.countdown}
                            setEnd={setActiveProps}
                            state="Playing"
                        />
                    ) : activeProps.state == "End" ? (
                        <End
                            assembly={activeProps.assembly}
                            setStaging={setActiveProps}
                            captures={activeProps.captures}
                            state="End"
                        />
                    ) : (
                        <></>
                    )
                ) : (
                    <></>
                )}
            </div>
        </Panel>
    )
}

export default AutoTestPanel
