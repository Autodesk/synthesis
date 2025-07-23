import InfoIcon from "@mui/icons-material/Info"
import { Box, Divider, IconButton, styled, Tooltip } from "@mui/material"
import { AiFillWarning, AiOutlineDoubleRight, AiOutlineInfoCircle } from "react-icons/ai"
import { BiRefresh } from "react-icons/bi"
import { BsCodeSquare } from "react-icons/bs"
import {
    FaAngleRight,
    FaArrowLeft,
    FaBasketball,
    FaBug,
    FaCar,
    FaChessBoard,
    FaFileImport,
    FaGamepad,
    FaGear,
    FaMagnifyingGlass,
    FaMinus,
    FaPlus,
    FaQuestion,
    FaScrewdriverWrench,
    FaWrench,
    FaXmark,
} from "react-icons/fa6"
import { GiSteeringWheel } from "react-icons/gi"
import { GrConnect } from "react-icons/gr"
import { HiDownload } from "react-icons/hi"
import { IoCheckmark, IoPencil, IoPeople, IoTrashBin } from "react-icons/io5"
import { colorNameToVar } from "../helpers/UseThemeHelpers"
import Button, { ButtonProps, ButtonSize } from "./Button"
import Label, { LabelSize } from "./Label"

export class SynthesisIcons {
    /** Regular icons: used for panels, modals, and main hud buttons */
    public static readonly BASKET_BALL = <FaBasketball />
    public static readonly GAMEPAD = <FaGamepad />
    public static readonly GEAR = <FaGear />
    public static readonly MAGNIFYING_GLASS = <FaMagnifyingGlass />
    public static readonly ADD = <FaPlus />
    public static readonly MINUS = <FaMinus />
    public static readonly IMPORT = <FaFileImport />
    public static readonly WRENCH = <FaWrench />
    public static readonly SCREWDRIVER_WRENCH = <FaScrewdriverWrench />
    public static readonly QUESTION = <FaQuestion />
    public static readonly XMARK = <FaXmark />
    public static readonly PEOPLE = <IoPeople />
    public static readonly CHESS_BOARD = <FaChessBoard />
    public static readonly FILL_WARNING = <AiFillWarning />
    public static readonly CAR = <FaCar />
    public static readonly CODE_SQUARE = <BsCodeSquare />
    public static readonly STEERING_WHEEL = <GiSteeringWheel />
    public static readonly OUTLINED_DOUBLE_RIGHT = <AiOutlineDoubleRight />
    public static readonly CONNECT = <GrConnect />
    public static readonly INFO = <AiOutlineInfoCircle />
    public static readonly BUG = <FaBug />

    /** Large icons: used for icon buttons */
    public static readonly DELETE_LARGE = <IoTrashBin size={"1.25rem"} />
    public static readonly DOWNLOAD_LARGE = <HiDownload size={"1.25rem"} />
    public static readonly ADD_LARGE = <FaPlus size={"1.25rem"} />
    public static readonly GEAR_LARGE = <FaGear size={"1.25rem"} />
    public static readonly REFRESH_LARGE = <BiRefresh size={"1.25rem"} />
    public static readonly SELECT_LARGE = <IoCheckmark size={"1.25rem"} />
    public static readonly EDIT_LARGE = <IoPencil size={"1.25rem"} />
    public static readonly LEFT_ARROW_LARGE = <FaArrowLeft size={"1.25rem"} />
    public static readonly BUG_LARGE = <FaBug size={"1.25rem"} />
    public static readonly XMARK_LARGE = <FaXmark size={"1.25rem"} />

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
            color={colorNameToVar("BackgroundSecondary")}
        />
    )
}

export const SectionDivider = styled(Divider)({
    borderColor: "grey",
})

export const SectionLabel = styled(Label)({
    fontWeight: 700,
    margin: "0pt",
})

export const Spacer = (heightPx?: number, widthPx?: number) => {
    return <Box minHeight={`${heightPx}px`} minWidth={`${widthPx}px`} />
}

export const PositiveButton: React.FC<ButtonProps> = ({ value, onClick }) => {
    return (
        <Button
            size={ButtonSize.MEDIUM}
            value={value}
            onClick={onClick}
            colorOverrideClass="bg-accept-button hover:brightness-90"
        />
    )
}

export const DownloadButton = (onClick: () => void) => {
    return <PositiveButton value={SynthesisIcons.DELETE_LARGE} onClick={onClick} />
}

export const AddButton = (onClick: () => void) => {
    return <PositiveButton value={SynthesisIcons.DELETE_LARGE} onClick={onClick} />
}

export const SelectButton = (onClick: () => void) => {
    return <PositiveButton value={SynthesisIcons.SELECT_LARGE} onClick={onClick} />
}

export const EditButton = (onClick: () => void) => {
    return <PositiveButton value={SynthesisIcons.EDIT_LARGE} onClick={onClick} />
}

export const NegativeButton: React.FC<ButtonProps> = ({ value, onClick, id }) => {
    return (
        <Button
            size={ButtonSize.MEDIUM}
            value={value}
            onClick={onClick}
            colorOverrideClass="bg-cancel-button hover:brightness-90"
            id={id}
        />
    )
}

export const DeleteButton = (onClick: () => void, id?: string) => {
    return <NegativeButton value={SynthesisIcons.DELETE_LARGE} onClick={onClick} id={id} />
}

export const ButtonIcon: React.FC<ButtonProps> = ({ value, onClick, id }) => {
    return (
        <Button
            value={value}
            onClick={onClick}
            colorOverrideClass="bg-[#00000000] hover:brightness-90"
            sizeOverrideClass="p-[0.25rem]"
            id={id}
            className="h-fit"
        />
    )
}

export const RefreshButton = (onClick: () => void) => {
    return <ButtonIcon value={SynthesisIcons.REFRESH_LARGE} onClick={onClick} />
}

export const AddButtonInteractiveColor = (onClick: () => void, id?: string) => {
    return <Button value={SynthesisIcons.ADD_LARGE} onClick={onClick} id={id} />
}

export const CustomTooltip = (text: string) => {
    return (
        <Tooltip title={text}>
            <IconButton
                size="small"
                disableRipple
                sx={{
                    color: "#ffffff77",
                    "&:hover": {
                        borderStyle: "solid",
                        borderColor: "grey",
                        backgroundColor: "transparent",
                    },
                    position: "relative",
                    overflow: "hidden",
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

export const LabelWithTooltip = (labelText: string, tooltipText: string, size?: LabelSize) => {
    return (
        <Box display={"flex"} flexDirection={"row"} alignItems={"center"} textAlign={"center"}>
            <Label size={size ?? LabelSize.SMALL}>{labelText}</Label>
            {CustomTooltip(tooltipText)}
        </Box>
    )
}
