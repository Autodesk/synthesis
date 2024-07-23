import React, { useEffect, useState } from "react"
import { BsCodeSquare } from "react-icons/bs"
import { FaCar, FaGear, FaMagnifyingGlass, FaPlus } from "react-icons/fa6"
import { BiMenuAltLeft } from "react-icons/bi"
import { GrConnect, GrFormClose } from "react-icons/gr"
import { GiSteeringWheel } from "react-icons/gi"
import { HiDownload } from "react-icons/hi"
import { IoBasketball, IoBug, IoGameControllerOutline, IoPeople, IoRefresh, IoTimer } from "react-icons/io5"
import { useModalControlContext } from "@/ui/ModalContext"
import { usePanelControlContext } from "@/ui/PanelContext"
import { motion } from "framer-motion"
import logo from "@/assets/autodesk_logo.png"
import { ToastType, useToastContext } from "@/ui/ToastContext"
import { Random } from "@/util/Random"
import WPILibBrain from "@/systems/simulation/wpilib_brain/WPILibBrain"
import APS, { APS_USER_INFO_UPDATE_EVENT } from "@/aps/APS"
import { UserIcon } from "./UserIcon"
import World from "@/systems/World"
import JOLT from "@/util/loading/JoltSyncLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { Button } from "@mui/base/Button"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import Jolt from "@barclah/jolt-physics"
import { AiOutlineDoubleRight } from "react-icons/ai"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"

type ButtonProps = {
    value: string
    icon: React.ReactNode
    onClick?: () => void
    larger?: boolean
}

const MainHUDButton: React.FC<ButtonProps> = ({ value, icon, onClick, larger }) => {
    if (larger == null) larger = false
    return (
        <Button
            onClick={onClick}
            className={`relative flex flex-row cursor-pointer bg-background w-full m-auto px-2 py-1 text-main-text border-none rounded-md ${larger ? "justify-center" : ""} items-center hover:brightness-105 focus:outline-0 focus-visible:outline-0`}
        >
            {larger && icon}
            {!larger && <span className="absolute left-3 text-main-hud-icon">{icon}</span>}
            <span className={`px-2 ${larger ? "py-2" : "py-1 ml-6"} text-main-text cursor-pointer`}>{value}</span>
        </Button>
    )
}

export let MainHUD_AddToast: (type: ToastType, title: string, description: string) => void = (_a, _b, _c) => {}

const variants = {
    open: { opacity: 1, y: "-50%", x: 0 },
    closed: { opacity: 0, y: "-50%", x: "-100%" },
}

const MainHUD: React.FC = () => {
    const { openModal } = useModalControlContext()
    const { openPanel } = usePanelControlContext()
    const { addToast } = useToastContext()
    const [isOpen, setIsOpen] = useState(false)

    MainHUD_AddToast = addToast

    const [userInfo, setUserInfo] = useState(APS.userInfo)

    useEffect(() => {
        document.addEventListener(APS_USER_INFO_UPDATE_EVENT, () => {
            setUserInfo(APS.userInfo)
        })
    }, [])

    return (
        <>
            {!isOpen && (
                <button
                    onClick={() => setIsOpen(!isOpen)}
                    className="absolute left-6 top-6 focus:outline-0 focus-visible:outline-0"
                >
                    <BiMenuAltLeft size={40} className="text-main-hud-close-icon" />
                </button>
            )}
            <motion.div
                initial="closed"
                animate={isOpen ? "open" : "closed"}
                variants={variants}
                className="fixed flex flex-col gap-2 bg-gradient-to-b from-interactive-element-right to-interactive-element-left w-min p-4 rounded-3xl ml-4 top-1/2 -translate-y-1/2"
            >
                <div className="flex flex-row gap-2 w-60 h-10">
                    <img src={logo} className="w-[80%] h-[100%] object-contain" />
                    <Button
                        onClick={() => setIsOpen(false)}
                        className={`bg-none border-none focus-visible:outline-0 focus:outline-0 select-none`}
                    >
                        <GrFormClose color="bg-icon" size={20} className="text-main-hud-close-icon" />
                    </Button>
                </div>
                <MainHUDButton
                    value={"Spawn Asset"}
                    icon={<FaPlus />}
                    larger={true}
                    onClick={() => openPanel("import-mirabuf")}
                />
                <div className="flex flex-col gap-0 bg-background w-full rounded-3xl">
                    <MainHUDButton
                        value={"Manage Assemblies"}
                        icon={<FaGear />}
                        onClick={() => openModal("manage-assemblies")}
                    />
                    <MainHUDButton value={"Settings"} icon={<FaGear />} onClick={() => openModal("settings")} />
                    <MainHUDButton value={"View"} icon={<FaMagnifyingGlass />} onClick={() => openModal("view")} />
                    <MainHUDButton
                        value={"Controls"}
                        icon={<IoGameControllerOutline />}
                        onClick={() => openModal("change-inputs")}
                    />
                    <MainHUDButton value={"MultiBot"} icon={<IoPeople />} onClick={() => openPanel("multibot")} />
                    <MainHUDButton
                        value={"Import Local Mira"}
                        icon={<IoPeople />}
                        onClick={() => openModal("import-local-mirabuf")}
                    />
                    <MainHUDButton
                        value={"The Poker"}
                        icon={<AiOutlineDoubleRight />}
                        onClick={() => openPanel("poker")}
                    />
                    <MainHUDButton value={"Test God Mode"} icon={<IoGameControllerOutline />} onClick={TestGodMode} />
                    <MainHUDButton
                        value={"Clear Prefs"}
                        icon={<IoBug />}
                        onClick={() => PreferencesSystem.clearPreferences()}
                    />
                    <MainHUDButton
                        value={"Refresh APS Token"}
                        icon={<IoRefresh />}
                        onClick={async () =>
                            APS.isSignedIn() && APS.refreshAuthToken((await APS.getAuth())!.refresh_token, true)
                        }
                    />
                    <MainHUDButton
                        value={"Expire APS Token"}
                        icon={<IoTimer />}
                        onClick={() => {
                            if (APS.isSignedIn()) {
                                APS.setExpiresAt(Date.now())
                                APS.getAuthOrLogin()
                            }
                        }}
                    />
                    <MainHUDButton value={"WS Viewer"} icon={<GrConnect />} onClick={() => openPanel("ws-view")} />
                </div>
                <div className="flex flex-col gap-0 bg-background w-full rounded-3xl">
                    <MainHUDButton
                        value={"Download Asset"}
                        icon={<HiDownload />}
                        onClick={() => openModal("download-assets")}
                    />
                    <MainHUDButton value={"RoboRIO"} icon={<BsCodeSquare />} onClick={() => openModal("roborio")} />
                    <MainHUDButton
                        value={"Driver Station"}
                        icon={<GiSteeringWheel />}
                        onClick={() => openPanel("driver-station")}
                    />
                    {/* MiraMap and OPFS Temp Buttons */}
                    <MainHUDButton
                        value={"Print Mira Maps"}
                        icon={<BsCodeSquare />}
                        onClick={() => {
                            console.log(MirabufCachingService.GetCacheMap(MiraType.ROBOT))
                            console.log(MirabufCachingService.GetCacheMap(MiraType.FIELD))
                        }}
                    />
                    <MainHUDButton
                        value={"Clear Mira"}
                        icon={<GiSteeringWheel />}
                        onClick={() => MirabufCachingService.RemoveAll()}
                    />
                    <MainHUDButton
                        value={"Edit Scoring Zones"}
                        icon={<IoBasketball />}
                        onClick={() => {
                            openPanel("scoring-zones")
                        }}
                    />
                    {/* <MainHUDButton value={"Drivetrain"} icon={<FaCar />} onClick={() => openModal("drivetrain")} />
                    <MainHUDButton
                        value={"WS Test"}
                        icon={<FaCar />}
                        onClick={() => {
                            // worker?.postMessage({ command: 'connect' });
                            const miraObjs = [...World.SceneRenderer.sceneObjects.entries()].filter(
                                x => x[1] instanceof MirabufSceneObject
                            )
                            console.log(`Number of mirabuf scene objects: ${miraObjs.length}`)
                            if (miraObjs.length > 0) {
                                const mechanism = (miraObjs[0][1] as MirabufSceneObject).mechanism
                                const simLayer = World.SimulationSystem.GetSimulationLayer(mechanism)
                                simLayer?.SetBrain(new WPILibBrain(mechanism))
                            }
                        }}
                    />
                    <MainHUDButton
                        value={"Toasts"}
                        icon={<FaCar />}
                        onClick={() => {
                            const type: ToastType = ["info", "warning", "error"][Math.floor(Random() * 3)] as ToastType
                            addToast(type, type, "This is a test toast to test the toast system")
                        }}
                    /> */}
                    <MainHUDButton value={"Configure"} icon={<FaGear />} onClick={() => openModal("config-robot")} />
                    <MainHUDButton
                        value={"Sequential Joints"}
                        icon={<FaGear />}
                        onClick={() => openPanel("sequential-joints")}
                    />
                    <MainHUDButton
                        value={"Configure Brain"}
                        icon={<FaGear />}
                        onClick={() => openPanel("config-robot-brain")}
                    />
                </div>
                {userInfo ? (
                    <MainHUDButton
                        value={`Hi, ${userInfo.givenName}`}
                        icon={<UserIcon className="h-[20pt] m-[5pt] rounded-full" />}
                        larger={true}
                        onClick={() => APS.logout()}
                    />
                ) : (
                    <MainHUDButton
                        value={`APS Login`}
                        icon={<IoPeople />}
                        larger={true}
                        onClick={() => APS.requestAuthCode()}
                    />
                )}
            </motion.div>
        </>
    )
}

async function TestGodMode() {
    const robot: MirabufSceneObject = [...World.SceneRenderer.sceneObjects.entries()]
        .filter(x => {
            const y = x[1] instanceof MirabufSceneObject
            return y
        })
        .map(x => x[1])[0] as MirabufSceneObject
    const rootNodeId = robot.GetRootNodeId()
    if (rootNodeId == undefined) {
        console.error("Robot root node not found for god mode")
        return
    }
    const robotPosition = World.PhysicsSystem.GetBody(rootNodeId).GetPosition()
    const [ghostBody, _ghostConstraint] = World.PhysicsSystem.CreateGodModeBody(rootNodeId, robotPosition as Jolt.Vec3)

    // Move ghostBody to demonstrate godMode movement
    await new Promise(f => setTimeout(f, 1000))
    World.PhysicsSystem.SetBodyPosition(
        ghostBody.GetID(),
        new JOLT.Vec3(robotPosition.GetX(), robotPosition.GetY() + 2, robotPosition.GetZ())
    )
    await new Promise(f => setTimeout(f, 1000))
    World.PhysicsSystem.SetBodyPosition(ghostBody.GetID(), new JOLT.Vec3(2, 2, 2))
}

export default MainHUD
