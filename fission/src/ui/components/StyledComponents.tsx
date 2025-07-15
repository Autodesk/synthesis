import { Box, Button, Divider, styled, IconButton, Tooltip, type ButtonProps, IconButtonProps, Stack, Typography } from "@mui/material"
import { IoCheckmark, IoPencil, IoPeople, IoTrashBin } from "react-icons/io5"
import { HiDownload } from "react-icons/hi"
import { AiOutlineInfoCircle } from "react-icons/ai"
import { BiRefresh } from "react-icons/bi"
import { AiFillWarning } from "react-icons/ai"
import { BsCodeSquare } from "react-icons/bs"
import { GiSteeringWheel } from "react-icons/gi"
import { AiOutlineDoubleRight } from "react-icons/ai"
import { GrConnect } from "react-icons/gr"
import InfoIcon from "@mui/icons-material/Info"

import {
    FaGear,
    FaMagnifyingGlass,
    FaPlus,
    FaGamepad,
    FaBasketball,
    FaFileImport,
    FaWrench,
    FaScrewdriverWrench,
    FaQuestion,
    FaXmark,
    FaChessBoard,
    FaCar,
    FaArrowLeft,
    FaMinus,
    FaBug,
    FaAngleRight,
} from "react-icons/fa6"
// import { colorNameToVar } from "../ThemeContext"

export class SynthesisIcons {
    /** Regular icons: used for panels, modals, and main hud buttons */
    public static readonly BASKET_BALL = (<FaBasketball />)
    public static readonly GAMEPAD = (<FaGamepad />)
    public static readonly GEAR = (<FaGear />)
    public static readonly MAGNIFYING_GLASS = (<FaMagnifyingGlass />)
    public static readonly ADD = (<FaPlus />)
    public static readonly MINUS = (<FaMinus />)
    public static readonly IMPORT = (<FaFileImport />)
    public static readonly WRENCH = (<FaWrench />)
    public static readonly SCREWDRIVER_WRENCH = (<FaScrewdriverWrench />)
    public static readonly QUESTION = (<FaQuestion />)
    public static readonly XMARK = (<FaXmark />)
    public static readonly PEOPLE = (<IoPeople />)
    public static readonly CHESS_BOARD = (<FaChessBoard />)
    public static readonly FILL_WARNING = (<AiFillWarning />)
    public static readonly CAR = (<FaCar />)
    public static readonly CODE_SQUARE = (<BsCodeSquare />)
    public static readonly STEERING_WHEEL = (<GiSteeringWheel />)
    public static readonly OUTLINED_DOUBLE_RIGHT = (<AiOutlineDoubleRight />)
    public static readonly CONNECT = (<GrConnect />)
    public static readonly INFO = (<AiOutlineInfoCircle />)
    public static readonly BUG = (<FaBug />)

    /** Large icons: used for icon buttons */
    public static readonly DELETE_LARGE = (<IoTrashBin size={"1.25rem"} />)
    public static readonly DOWNLOAD_LARGE = (<HiDownload size={"1.25rem"} />)
    public static readonly ADD_LARGE = (<FaPlus size={"1.25rem"} />)
    public static readonly GEAR_LARGE = (<FaGear size={"1.25rem"} />)
    public static readonly REFRESH_LARGE = (<BiRefresh size={"1.25rem"} />)
    public static readonly SELECT_LARGE = (<IoCheckmark size={"1.25rem"} />)
    public static readonly EDIT_LARGE = (<IoPencil size={"1.25rem"} />)
    public static readonly LEFT_ARROW_LARGE = (<FaArrowLeft size={"1.25rem"} />)
    public static readonly BUG_LARGE = (<FaBug size={"1.25rem"} />)
    public static readonly XMARK_LARGE = (<FaXmark size={"1.25rem"} />)

    public static readonly OPEN_HUD_ICON = (
        <FaAngleRight
            size={"5vh"}
            style={{
                alignSelf: "middle",
                justifySelf: "center",
                minHeight: "40px",
                minWidth: "40px",
                maxHeight: "50px",
                maxWidth: "50px",
            }}
            // color={colorNameToVar("BackgroundSecondary")}
        />
    )
}

export const Spacer = (heightPx?: number, widthPx?: number) => {
    return <Box minHeight={`${heightPx}px`} minWidth={`${widthPx}px`} />
}

export const PositiveButton: React.FC<ButtonProps> = ({ children, onClick }) => {
    return <Button onClick={onClick} color="success">{children}</Button>
}

export const PositiveIconButton: React.FC<IconButtonProps> = ({ children, onClick }) => {
    return <IconButton onClick={onClick} color="success">{children}</IconButton>
}

export const DownloadButton = (onClick: () => void) => {
    return <PositiveIconButton onClick={onClick}>{SynthesisIcons.DELETE_LARGE}</PositiveIconButton>
}

export const AddButton = (onClick: () => void) => {
    return <PositiveIconButton onClick={onClick}>{SynthesisIcons.ADD_LARGE}</PositiveIconButton>
}

export const SelectButton = (onClick: () => void) => {
    return <PositiveIconButton onClick={onClick}>{SynthesisIcons.SELECT_LARGE}</PositiveIconButton>
}

export const EditButton = (onClick: () => void) => {
    return <PositiveIconButton onClick={onClick}>{SynthesisIcons.EDIT_LARGE}</PositiveIconButton>
}

export const NegativeButton: React.FC<ButtonProps> = ({ children, onClick, id }) => {
    return <Button onClick={onClick} id={id} color="error">{children}</Button>
}

export const NegativeIconButton: React.FC<IconButtonProps> = ({ children, onClick }) => {
    return <IconButton onClick={onClick} color="error">{children}</IconButton>
}

export const DeleteButton = (onClick: () => void, id?: string) => {
    return <NegativeIconButton onClick={onClick} id={id}>{SynthesisIcons.DELETE_LARGE}</NegativeIconButton>
}

export const RefreshButton = (onClick: () => void) => {
    return <IconButton onClick={onClick}>{SynthesisIcons.REFRESH_LARGE}</IconButton>
}

export const CustomTooltip = (text: string) => {
    return (
        <Tooltip title={text}>
            <IconButton
                size="small"
                disableRipple
                sx={{
                    // "color": "#ffffff77",
                    "&:hover": {
                        borderStyle: "solid",
                        borderColor: "grey",
                        backgroundColor: "transparent",
                    },
                    "position": "relative",
                    "overflow": "hidden",
                    "& .MuiTouchRipple-root span": {
                        backgroundColor: "#ffffffaa",
                        animationDuration: "300ms",
                    },
                    "&:focus": {
                        borderColor: "grey",
                        backgroundColor: "transparent",
                        outline: "none",
                    },
                    "&:selected": {
                        outline: "none",
                        backgroundColor: "transparent",
                        borderColor: "none",
                    },
                }}
            >
                <InfoIcon fontSize="small" />
            </IconButton>
        </Tooltip>
    )
}

export const LabelWithTooltip = (labelText: string, tooltipText: string) => {
    return (
        <Stack direction="row" alignItems={"center"} textAlign={"center"}>
            <Typography variant="h5">{labelText}</Typography>
            {CustomTooltip(tooltipText)}
        </Stack>
    )
}
