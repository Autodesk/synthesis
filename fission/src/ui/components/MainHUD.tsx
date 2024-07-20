import React, { useEffect, useState } from "react"
import { FaXmark } from "react-icons/fa6"
import { useModalControlContext } from "@/ui/ModalContext"
import { usePanelControlContext } from "@/ui/PanelContext"
import { motion } from "framer-motion"
import logo from "@/assets/autodesk_logo.png"
import { ToastType, useToastContext } from "@/ui/ToastContext"
import APS, { APS_USER_INFO_UPDATE_EVENT } from "@/aps/APS"
import { UserIcon } from "./UserIcon"
import { ButtonIcon, SynthesisIcons } from "./StyledComponents"
import { Button } from "@mui/base"
import { Box } from "@mui/material"

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
            className={`relative flex flex-row
                cursor-pointer
                bg-background w-full m-auto px-2 py-1 text-main-text border-none rounded-md ${larger ? "justify-center" : ""}
                items-center hover:brightness-105 focus:outline-0 focus-visible:outline-0
                transform
                transition-transform
                hover:scale-[1.015]
                active:scale-[1.03]`}
        >
            {larger && icon}
            {!larger && <span className="absolute left-3 text-main-hud-icon">{icon}</span>}
            <span
                className={`px-2 ${larger ? "py-2" : "py-0.5 ml-6"} text-main-text cursor-pointer`}
                style={{ userSelect: "none", MozUserSelect: "none", msUserSelect: "none", WebkitUserSelect: "none" }}
            >
                {value}
            </span>
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
                <Box
                    display="flex"
                    alignItems={"center"}
                    height="100%"
                    position={"absolute"}
                    sx={{ top: "0", left: "0" }}
                >
                    <Box
                        position="absolute"
                        width={"5vw"}
                        minWidth={"50px"}
                        maxWidth={"60px"}
                        style={{ aspectRatio: " 1 / 1.5" }}
                        className="bg-gradient-to-b from-interactive-element-right to-interactive-element-left transform transition-transform hover:scale-[1.02] active:scale-[1.04]"
                        sx={{
                            borderTopRightRadius: "100px",
                            borderBottomRightRadius: "100px",
                            borderTopLeftRadius: "0",
                            borderBottomLeftRadius: "0",
                        }}
                    >
                        <Box className="flex w-full h-full items-center justify-center">
                            <ButtonIcon
                                onClick={() => setIsOpen(!isOpen)}
                                value={SynthesisIcons.OpenHudIcon}
                                className=""
                            />
                        </Box>
                    </Box>
                </Box>
            )}
            <motion.div
                initial="closed"
                animate={isOpen ? "open" : "closed"}
                variants={variants}
                className="fixed flex flex-col gap-2 bg-gradient-to-b from-interactive-element-right to-interactive-element-left w-min p-4 rounded-3xl ml-4 top-1/2 -translate-y-1/2"
            >
                <div className="flex flex-row gap-2 w-60 h-10">
                    <img
                        src={logo}
                        className="w-[80%] h-[100%] object-contain"
                        style={{
                            userSelect: "none",
                            MozUserSelect: "none",
                            msUserSelect: "none",
                            WebkitUserSelect: "none",
                        }}
                    />
                    <ButtonIcon
                        value={<FaXmark color="bg-icon" size={23} className="text-main-hud-close-icon" />}
                        onClick={() => setIsOpen(false)}
                    />
                </div>
                <MainHUDButton
                    value={"Spawn Asset"}
                    icon={SynthesisIcons.Add}
                    larger={true}
                    onClick={() => openPanel("import-mirabuf")}
                />
                <Box
                    display="flex"
                    flexDirection={"column"}
                    sx={{ backgroundColor: "black", borderRadius: "7px", padding: "3px" }}
                >
                    <MainHUDButton
                        value={"General Settings"}
                        icon={SynthesisIcons.Gear}
                        onClick={() => openModal("settings")}
                    />
                    <MainHUDButton
                        value={"View"}
                        icon={SynthesisIcons.MagnifyingGlass}
                        onClick={() => openModal("view")}
                    />
                    <MainHUDButton
                        value={"Configure Assets"}
                        icon={SynthesisIcons.Wrench}
                        onClick={() => openPanel("configure")}
                    />
                    <MainHUDButton
                        value={"Debug Tools"}
                        icon={SynthesisIcons.Bug}
                        onClick={() => {
                            openPanel("debug")
                        }}
                    />
                </Box>
                {userInfo ? (
                    <MainHUDButton
                        value={`Hi, ${userInfo.givenName}`}
                        icon={<UserIcon className="h-[20pt] m-[5pt] rounded-full" />}
                        larger={true}
                        onClick={() => openModal("aps-management")}
                    />
                ) : (
                    <MainHUDButton
                        value={`APS Login`}
                        icon={SynthesisIcons.People}
                        larger={true}
                        onClick={() => APS.requestAuthCode()}
                    />
                )}
            </motion.div>
        </>
    )
}

export default MainHUD
