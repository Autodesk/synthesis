import { Box, ButtonGroup, ButtonProps, Stack } from "@mui/material"
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
import { Button, IconButton, SynthesisIcons } from "./StyledComponents"
import { TouchControlsEvent, TouchControlsEventKeys } from "./TouchControls"
import UserIcon from "./UserIcon"

const MainHUDButton: React.FC<ButtonProps> = ({ startIcon, endIcon, children, ...props }) => {
    return (
        <Button
            {...props}
            startIcon={props.size === "large" ? startIcon : null}
            className="relative flex flex-row"
            variant="contained"
            sx={{
                "&:focus": {
                    outline: "none",
                },
            }}
        >
            {props.size !== "large" && <span className="absolute left-3">{startIcon}</span>}
            <span className={props.size === "large" ? "py-1" : "py-0.5 ml-6"}>{children}</span>
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
                className="fixed flex flex-col gap-2 w-min p-4 rounded-3xl ml-4 top-1/2"
                bgcolor="background.default"
            >
                <div className="flex flex-row gap-2 w-60 h-10">
                    <img
                        alt="Autodesk"
                        src={logo}
                        className="w-[80%] h-full object-contain"
                        style={{
                            userSelect: "none",
                            MozUserSelect: "none",
                            msUserSelect: "none",
                            WebkitUserSelect: "none",
                            filter: mode === "dark" ? "invert(1)" : "none",
                        }}
                        draggable={false}
                        onDragStart={e => e.preventDefault()}
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
                    startIcon={SynthesisIcons.ADD}
                    size="large"
                    onClick={() =>
                        openPanel(ImportMirabufPanel, {
                            configurationType: "ROBOTS" as ConfigurationType,
                        })
                    }
                >
                    Spawn Asset
                </MainHUDButton>
                <ButtonGroup orientation="vertical" variant="contained">
                    <MainHUDButton startIcon={SynthesisIcons.WRENCH} onClick={() => openPanel(ConfigurePanel, {})}>
                        Configure Assets
                    </MainHUDButton>
                    <MainHUDButton
                        startIcon={SynthesisIcons.GEAR}
                        onClick={() =>
                            openModal(SettingsModal, undefined, undefined, {
                                allowClickAway: false,
                            })
                        }
                    >
                        General Settings
                    </MainHUDButton>
                    <MainHUDButton
                        startIcon={SynthesisIcons.CODE_SQUARE}
                        onClick={() => openPanel(DeveloperToolPanel, undefined)}
                    >
                        Developer Tool
                    </MainHUDButton>
                    {/** Will be coming soonish...tm */}
                    {/* <MainHUDButton
                        value={"View"}
                        icon={SynthesisIcons.MAGNIFYING_GLASS}
                        onClick={() => openModal(<ViewModal />, undefined)}
                    /> */}
                    <MainHUDButton
                        startIcon={SynthesisIcons.BUG}
                        onClick={() => {
                            openPanel(DebugPanel, undefined)
                        }}
                    >
                        Debug Tools
                    </MainHUDButton>
                    {touchCompatibility && (
                        <MainHUDButton
                            startIcon={SynthesisIcons.GAMEPAD}
                            onClick={() => new TouchControlsEvent(TouchControlsEventKeys.JOYSTICK)}
                        >
                            Touch Controls
                        </MainHUDButton>
                    )}
                </ButtonGroup>
                {userInfo ? (
                    <MainHUDButton
                        startIcon={<UserIcon className="h-6 rounded-full" />}
                        size="large"
                        onClick={() => openModal(APSManagementModal, undefined)}
                    >{`Hi, ${userInfo.givenName}`}</MainHUDButton>
                ) : (
                    <MainHUDButton startIcon={SynthesisIcons.PEOPLE} onClick={() => APS.requestAuthCode()} size="large">
                        APS Login
                    </MainHUDButton>
                )}
                {!matchModeRunning ? (
                    <MainHUDButton
                        startIcon={SynthesisIcons.GAMEPAD}
                        size="large"
                        onClick={() => {
                            openPanel(MatchModeConfigPanel, undefined)
                            setIsOpen(false)
                        }}
                    >
                        Start Match Mode
                    </MainHUDButton>
                ) : (
                    <MainHUDButton
                        startIcon={SynthesisIcons.XMARK_LARGE}
                        size="large"
                        onClick={() => {
                            MatchMode.getInstance().sandboxModeStart()
                            globalAddToast("info", "Match Mode Cancelled")
                        }}
                    >
                        Abort Match Mode
                    </MainHUDButton>
                )}
            </Box>
        </>
    )
}

export default MainHUD
