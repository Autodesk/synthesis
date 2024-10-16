import React, { useEffect, useState } from "react"
import {
    FaGear,
    // FaMagnifyingGlass,
    FaPlus,
    FaGamepad,
    FaBasketball,
    FaFileImport,
    FaWrench,
    FaScrewdriverWrench,
} from "react-icons/fa6"
import { BiMenuAltLeft } from "react-icons/bi"
import { FaXmark } from "react-icons/fa6"
import { IoPeople } from "react-icons/io5"
import { useModalControlContext } from "@/ui/ModalContext"
import { usePanelControlContext } from "@/ui/PanelContext"
import { motion } from "framer-motion"
import logo from "@/assets/autodesk_logo.png"
import { ToastType, useToastContext } from "@/ui/ToastContext"
import APS, { APS_USER_INFO_UPDATE_EVENT } from "@/aps/APS"
import { UserIcon } from "./UserIcon"
import { Button } from "@mui/base/Button"

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
                        <FaXmark color="bg-icon" size={20} className="text-main-hud-close-icon" />
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
                        icon={<FaWrench />}
                        onClick={() => openModal("manage-assemblies")}
                    />
                    <MainHUDButton value={"Settings"} icon={<FaGear />} onClick={() => openModal("settings")} />
                    {/* <MainHUDButton value={"View"} icon={<FaMagnifyingGlass />} onClick={() => openModal("view")} /> */}
                    <MainHUDButton value={"Controls"} icon={<FaGamepad />} onClick={() => openModal("change-inputs")} />
                    <MainHUDButton
                        value={"Import Local Mira"}
                        icon={<FaFileImport />}
                        onClick={() => openModal("import-local-mirabuf")}
                    />
                </div>
                <div className="flex flex-col gap-0 bg-background w-full rounded-3xl">
                    <MainHUDButton
                        value={"Edit Scoring Zones"}
                        icon={<FaBasketball />}
                        onClick={() => {
                            openPanel("scoring-zones")
                        }}
                    />
                    <MainHUDButton value={"Configure"} icon={<FaGear />} onClick={() => openModal("config-robot")} />
                    <MainHUDButton
                        value={"Debug Tools"}
                        icon={<FaScrewdriverWrench />}
                        onClick={() => {
                            openPanel("debug")
                        }}
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

export default MainHUD
