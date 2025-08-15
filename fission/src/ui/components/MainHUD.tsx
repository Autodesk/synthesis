import { Box, Stack } from "@mui/material"
import { Button, IconButton } from "./StyledComponents"
import { motion } from "framer-motion"
import type React from "react"
import { useEffect, useState } from "react"
import { FaXmark } from "react-icons/fa6"
import APS, { APS_USER_INFO_UPDATE_EVENT } from "@/aps/APS"
import logo from "@/assets/autodesk_logo.png"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import MatchMode, { MatchStateChangeEvent } from "@/systems/match_mode/MatchMode"
import { deobf } from "@/util/Utility"
import { useThemeContext } from "../helpers/ThemeProviderHelpers"
import { useUIContext } from "../helpers/UIProviderHelpers"
import APSManagementModal from "../modals/APSManagementModal"
import SettingsModal from "../modals/configuring/SettingsModal"
import type { ConfigurationType } from "../panels/configuring/assembly-config/ConfigTypes"
import ConfigurePanel from "../panels/configuring/assembly-config/ConfigurePanel"
import MatchModeConfigPanel from "../panels/configuring/MatchModeConfigPanel"
import DebugPanel from "../panels/DebugPanel"
import DeveloperToolPanel from "../panels/DeveloperToolPanel"
import ImportMirabufPanel from "../panels/mirabuf/ImportMirabufPanel"
import { setAddToast, setOpenModal, setOpenPanel } from "./GlobalUIControls"
import { SynthesisIcons } from "./StyledComponents"
import { TouchControlsEvent, TouchControlsEventKeys } from "./TouchControls"
import UserIcon from "./UserIcon"

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
                w-full m-auto px-2 py-1 border-none rounded-md ${larger ? "justify-center" : ""}
                items-center hover:brightness-105 focus:outline-0 focus-visible:outline-0
                transform
                transition-transform
                hover:scale-[1.015]
                active:scale-[1.03]`}
            color="primary"
            sx={{
                borderRadius: "8px",
            }}
        >
            {larger && icon}
            {!larger && <span className="absolute left-3">{icon}</span>}
            <span
                className={`px-2 ${larger ? "py-2" : "py-0.5 ml-6"} cursor-pointer`}
                style={{
                    userSelect: "none",
                    MozUserSelect: "none",
                    msUserSelect: "none",
                    WebkitUserSelect: "none",
                }}
            >
                {value}
            </span>
        </Button>
    )
}

const variants = {
    open: { opacity: 1, y: "-50%", x: 0 },
    closed: { opacity: 0, y: "-50%", x: "-100%" },
}

const MainHUD: React.FC = () => {
    const { mode } = useThemeContext()
    const { openModal, openPanel, addToast } = useUIContext()
    const [isOpen, setIsOpen] = useState(false)

    const touchCompatibility = matchMedia("(hover: none)").matches

    setAddToast(addToast)
    setOpenPanel(openPanel)
    setOpenModal(openModal)

    const [userInfo, setUserInfo] = useState(APS.userInfo)
    const [matchModeRunning, setMatchModeRunning] = useState(MatchMode.getInstance().isMatchEnabled())

    useEffect(() => {
        document.addEventListener(APS_USER_INFO_UPDATE_EVENT, () => {
            setUserInfo(APS.userInfo)
        })

        // biome-ignore-start lint/suspicious/noExplicitAny: allow any
        try {
            const k: string[] = deobf("NmM2ZjYzNjE2YzUzNzQ2ZjcyNjE2NzY1MmU3NDY4NjU2ZDY1").split(String.fromCharCode(46))
            const v = JSON.parse((window as any)[k[0]][k[1]])[deobf("NjM2ZjZmNmM0ZDZmNjQ2NQ==")]
            if (v === deobf("Nzk2NTcz")) {
                const r = (document as any)[deobf("Njc2NTc0NDU2YzY1NmQ2NTZlNzQ0Mjc5NDk2NA==")](deobf("NzI2ZjZmNzQ="))
                if (r) {
                    const w = (document as any)[deobf("NjM3MjY1NjE3NDY1NDU2YzY1NmQ2NTZlNzQ=")](
                        deobf("NmQ2MTcyNzE3NTY1NjU=")
                    )
                    r[deobf("NzA2MTcyNjU2ZTc0NGU2ZjY0NjU=")][deobf("Njk2ZTczNjU3Mjc0NDI2NTY2NmY3MjY1")](w, r)
                    w[deobf("NjE3MDcwNjU2ZTY0NDM2ODY5NmM2NA==")](r)
                }
            }
        } catch (_e) {
            // noop
        }
        // biome-ignore-end lint/suspicious/noExplicitAny: disallow any
    }, [])

    useEffect(() => {
        MatchStateChangeEvent.addListener(() => {
            setMatchModeRunning(MatchMode.getInstance().isMatchEnabled())
        })
    }, [])

    return (
        <>
            {!isOpen && (
                <Stack
                    direction="row"
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
                        className="transform transition-transform hover:scale-[1.02] active:scale-[1.04]"
                        bgcolor="secondary.dark"
                        sx={{
                            borderTopRightRadius: "100px",
                            borderBottomRightRadius: "100px",
                            borderTopLeftRadius: "0",
                            borderBottomLeftRadius: "0",
                        }}
                    >
                        <Stack className="w-full h-full" alignItems="center" justifyContent="center">
                            <IconButton
                                onClick={() => setIsOpen(!isOpen)}
                                color="primary"
                                disableRipple
                                sx={{
                                    "&:focus": {
                                        borderColor: "transparent !important",
                                        outline: "none",
                                    },
                                    "&:selected": {
                                        outline: "none",
                                        borderColor: "transparent",
                                    },
                                }}
                            >
                                {SynthesisIcons.OPEN_HUD_ICON}
                            </IconButton>
                        </Stack>
                    </Box>
                </Stack>
            )}
            <Box
                component={motion.div}
                initial="closed"
                animate={isOpen ? "open" : "closed"}
                variants={variants}
                className="fixed flex flex-col gap-2 w-min p-4 rounded-3xl ml-4 top-1/2 -translate-y-1/2"
                bgcolor="background.default"
            >
                <div className="flex flex-row gap-2 w-60 h-10">
                    <img
                        alt="Autodesk"
                        src={logo}
                        className="w-[80%] h-[100%] object-contain"
                        style={{
                            userSelect: "none",
                            MozUserSelect: "none",
                            msUserSelect: "none",
                            WebkitUserSelect: "none",
                            filter: mode === "dark" ? "invert(1)" : "none",
                        }}
                    />
                    <IconButton
                        sx={{
                            "&:focus": {
                                borderColor: "transparent !important",
                                outline: "none",
                            },
                            "&:selected": {
                                outline: "none",
                                borderColor: "transparent",
                            },
                            color: "text.primary",
                        }}
                        onClick={() => setIsOpen(false)}
                    >
                        <FaXmark size={23} />
                    </IconButton>
                </div>
                <MainHUDButton
                    value={"Spawn Asset"}
                    icon={SynthesisIcons.ADD}
                    larger={true}
                    onClick={() => openPanel(ImportMirabufPanel, { configurationType: "ROBOTS" as ConfigurationType })}
                />
                <Stack direction="column" sx={{ borderRadius: "7px", padding: "4px" }} bgcolor="primary.main" gap={0.5}>
                    <MainHUDButton
                        value={"Configure Assets"}
                        icon={SynthesisIcons.WRENCH}
                        onClick={() => openPanel(ConfigurePanel, {})}
                    />
                    <MainHUDButton
                        value={"General Settings"}
                        icon={SynthesisIcons.GEAR}
                        onClick={() => openModal(SettingsModal, undefined, undefined, { allowClickAway: false })}
                    />
                    <MainHUDButton
                        value={"Developer Tool"}
                        icon={SynthesisIcons.CODE_SQUARE}
                        onClick={() => openPanel(DeveloperToolPanel, undefined)}
                    />
                    {/** Will be coming soonish...tm */}
                    {/* <MainHUDButton
                        value={"View"}
                        icon={SynthesisIcons.MAGNIFYING_GLASS}
                        onClick={() => openModal(<ViewModal />, undefined)}
                    /> */}
                    <MainHUDButton
                        value={"Debug Tools"}
                        icon={SynthesisIcons.BUG}
                        onClick={() => {
                            openPanel(DebugPanel, undefined)
                        }}
                    />
                    {touchCompatibility && (
                        <MainHUDButton
                            value={"Touch Controls"}
                            icon={SynthesisIcons.GAMEPAD}
                            onClick={() => new TouchControlsEvent(TouchControlsEventKeys.JOYSTICK)}
                        />
                    )}
                </Stack>
                {userInfo ? (
                    <MainHUDButton
                        value={`Hi, ${userInfo.givenName}`}
                        icon={<UserIcon className="h-[20pt] m-[5pt] rounded-full" />}
                        larger={true}
                        onClick={() => openModal(APSManagementModal, undefined)}
                    />
                ) : (
                    <MainHUDButton
                        value={"APS Login"}
                        icon={SynthesisIcons.PEOPLE}
                        larger={true}
                        onClick={() => APS.requestAuthCode()}
                    />
                )}
                {!matchModeRunning ? (
                    <MainHUDButton
                        value={"Start Match Mode"}
                        icon={SynthesisIcons.GAMEPAD}
                        larger={true}
                        onClick={() => {
                            openPanel(MatchModeConfigPanel, undefined)
                            setIsOpen(false)
                        }}
                    />
                ) : (
                    <MainHUDButton
                        value={"Abort Match Mode"}
                        icon={SynthesisIcons.XMARK_LARGE}
                        larger={true}
                        onClick={() => {
                            MatchMode.getInstance().sandboxModeStart()
                            globalAddToast("info", "Match Mode Cancelled")
                        }}
                    />
                )}
            </Box>
        </>
    )
}

export default MainHUD
