import { Box, Divider, styled, IconButton, Tooltip } from "@mui/material"
import Label, { LabelSize } from "./Label"
import Button, { ButtonProps, ButtonSize } from "./Button"
import { IoCheckmark, IoPencil, IoPeople, IoTrashBin } from "react-icons/io5"
import { HiDownload } from "react-icons/hi"
import { AiOutlineInfoCircle, AiOutlinePlus } from "react-icons/ai"
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
import { colorNameToVar } from "../ThemeContext"

export class SynthesisIcons {
    /** Regular icons: used for panels, modals, and main hud buttons */
    public static Basketball = (<FaBasketball />)
    public static Gamepad = (<FaGamepad />)
    public static Gear = (<FaGear />)
    public static MagnifyingGlass = (<FaMagnifyingGlass />)
    public static Add = (<FaPlus />)
    public static Minus = (<FaMinus />)
    public static Import = (<FaFileImport />)
    public static Wrench = (<FaWrench />)
    public static ScrewdriverWrench = (<FaScrewdriverWrench />)
    public static Question = (<FaQuestion />)
    public static Xmark = (<FaXmark />)
    public static People = (<IoPeople />)
    public static ChessBoard = (<FaChessBoard />)
    public static FillWarning = (<AiFillWarning />)
    public static Car = (<FaCar />)
    public static CodeSquare = (<BsCodeSquare />)
    public static SteeringWheel = (<GiSteeringWheel />)
    public static OutlineDoubleRight = (<AiOutlineDoubleRight />)
    public static Connect = (<GrConnect />)
    public static Info = (<AiOutlineInfoCircle />)
    public static Bug = (<FaBug />)

    /** Large icons: used for icon buttons */
    public static DeleteLarge = (<IoTrashBin size={"1.25rem"} />)
    public static DownloadLarge = (<HiDownload size={"1.25rem"} />)
    public static AddLarge = (<AiOutlinePlus size={"1.25rem"} />)
    public static RefreshLarge = (<BiRefresh size={"1.25rem"} />)
    public static SelectLarge = (<IoCheckmark size={"1.25rem"} />)
    public static EditLarge = (<IoPencil size={"1.25rem"} />)
    public static LeftArrowLarge = (<FaArrowLeft size={"1.25rem"} />)
    public static BugLarge = (<FaBug size={"1.25rem"} />)

    public static OpenHudIcon = (
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
            size={ButtonSize.Medium}
            value={value}
            onClick={onClick}
            colorOverrideClass="bg-accept-button hover:brightness-90"
        />
    )
}

export const DownloadButton = (onClick: () => void) => {
    return <PositiveButton value={SynthesisIcons.DeleteLarge} onClick={onClick} />
}

export const AddButton = (onClick: () => void) => {
    return <PositiveButton value={SynthesisIcons.DeleteLarge} onClick={onClick} />
}

export const SelectButton = (onClick: () => void) => {
    return <PositiveButton value={SynthesisIcons.SelectLarge} onClick={onClick} />
}

export const EditButton = (onClick: () => void) => {
    return <PositiveButton value={SynthesisIcons.EditLarge} onClick={onClick} />
}

export const NegativeButton: React.FC<ButtonProps> = ({ value, onClick }) => {
    return (
        <Button
            size={ButtonSize.Medium}
            value={value}
            onClick={onClick}
            colorOverrideClass="bg-cancel-button hover:brightness-90"
        />
    )
}

export const DeleteButton = (onClick: () => void) => {
    return <NegativeButton value={SynthesisIcons.DeleteLarge} onClick={onClick} />
}

export const ButtonIcon: React.FC<ButtonProps> = ({ value, onClick }) => {
    return (
        <Button
            value={value}
            onClick={onClick}
            colorOverrideClass="bg-[#00000000] hover:brightness-90"
            sizeOverrideClass="p-[0.25rem]"
            className="h-fit"
        />
    )
}

export const RefreshButton = (onClick: () => void) => {
    return <ButtonIcon value={SynthesisIcons.RefreshLarge} onClick={onClick} />
}

export const AddButtonInteractiveColor = (onClick: () => void) => {
    return <Button value={SynthesisIcons.AddLarge} onClick={onClick} />
}

export const CustomTooltip = (text: string) => {
    return (
        <Tooltip title={text}>
            <IconButton
                size="small"
                disableRipple
                sx={{
                    "color": "#ffffff77",
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

export const LabelWithTooltip = (labelText: string, tooltipText: string, size?: LabelSize) => {
    return (
        <Box display={"flex"} flexDirection={"row"} alignItems={"center"} textAlign={"center"}>
            <Label size={size ?? LabelSize.Small}>{labelText}</Label>
            {CustomTooltip(tooltipText)}
        </Box>
    )
}
