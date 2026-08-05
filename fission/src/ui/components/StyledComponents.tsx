import InfoIcon from "@mui/icons-material/Info"
import { forwardRef, useCallback, useState } from "react"
import {
    Box,
    type ButtonProps,
    type IconButtonProps,
    Button as MuiButton,
    IconButton as MuiIconButton,
    ToggleButton as MuiToggleButton,
    ToggleButtonGroup as MuiToggleButtonGroup,
    Stack,
    type ToggleButtonGroupProps,
    Select as MuiSelect,
    type SelectProps,
    Accordion as MuiAccordion,
    type AccordionProps,
    AccordionSummary as MuiAccordionSummary,
    type AccordionSummaryProps,
    AccordionDetails as MuiAccordionDetails,
    type AccordionDetailsProps,
    type ToggleButtonProps,
    Tooltip,
} from "@mui/material"
import { AiFillWarning, AiOutlineDoubleRight, AiOutlineInfoCircle, AiOutlineClose } from "react-icons/ai"
import { BiRefresh } from "react-icons/bi"
import { BsCodeSquare } from "react-icons/bs"
import {
    FaAngleRight,
    FaArrowLeft,
    FaArrowsUpDownLeftRight,
    FaBasketball,
    FaBrain,
    FaBug,
    FaCamera,
    FaCar,
    FaChessBoard,
    FaCheck,
    FaFileImport,
    FaGamepad,
    FaGear,
    FaInfinity,
    FaMagnifyingGlass,
    FaMicrochip,
    FaMinus,
    FaPlus,
    FaQuestion,
    FaScrewdriverWrench,
    FaTags,
    FaWrench,
    FaXmark,
    FaArrowRight,
} from "react-icons/fa6"
import { FaHandPaper, FaUnlink } from "react-icons/fa"
import { GiPerspectiveDiceSixFacesOne, GiSteeringWheel } from "react-icons/gi"
import { GrConnect } from "react-icons/gr"
import { HiDownload, HiUser } from "react-icons/hi"
import { IoMdArrowDropdown } from "react-icons/io"
import { IoCheckmark, IoPencil, IoPeople, IoPlayOutline, IoTrashBin } from "react-icons/io5"
import { MdExpandMore, MdFitScreen, MdZoomInMap, MdZoomOutMap, MdCode, MdCodeOff } from "react-icons/md"
import type { IconBaseProps, IconType } from "react-icons"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import Label from "./Label"
import React from "react"

/** Wraps an icon with default props (eg. a default size) that callers can still override. */
function withDefaultProps(icon: IconType, defaultProps: IconBaseProps): IconType {
    return (props?: IconBaseProps) => React.createElement(icon, { ...defaultProps, ...props })
}

export class SynthesisIcons {
    /** Regular icons: used for panels, modals, and main hud buttons */
    public static readonly BASKET_BALL = FaBasketball
    public static readonly GAMEPAD = FaGamepad
    public static readonly GEAR = FaGear
    public static readonly MAGNIFYING_GLASS = FaMagnifyingGlass
    public static readonly ADD = FaPlus
    public static readonly MINUS = FaMinus
    public static readonly IMPORT = FaFileImport
    public static readonly WRENCH = FaWrench
    public static readonly SCREWDRIVER_WRENCH = FaScrewdriverWrench
    public static readonly QUESTION = FaQuestion
    public static readonly XMARK = FaXmark
    public static readonly PEOPLE = IoPeople
    public static readonly CHESS_BOARD = FaChessBoard
    public static readonly FILL_WARNING = AiFillWarning
    public static readonly CAR = FaCar
    public static readonly CODE_SQUARE = BsCodeSquare
    public static readonly STEERING_WHEEL = GiSteeringWheel
    public static readonly OUTLINED_DOUBLE_RIGHT = AiOutlineDoubleRight
    public static readonly CONNECT = GrConnect
    public static readonly INFO = AiOutlineInfoCircle
    public static readonly STOP = AiOutlineClose
    public static readonly BUG = FaBug
    public static readonly PLAY = IoPlayOutline
    public static readonly CAMERA = FaCamera
    public static readonly HAND = FaHandPaper
    public static readonly CHECK = FaCheck
    public static readonly FIT_SCREEN = MdFitScreen
    public static readonly ZOOM_IN = MdZoomInMap
    public static readonly ZOOM_OUT = MdZoomOutMap
    public static readonly USER = HiUser
    public static readonly INFINITY = FaInfinity
    public static readonly UNLINK = FaUnlink
    public static readonly DICE = GiPerspectiveDiceSixFacesOne
    public static readonly DROPDOWN_CARET = IoMdArrowDropdown
    public static readonly BRAIN = FaBrain
    public static readonly MOVE = FaArrowsUpDownLeftRight
    public static readonly METADATA = FaTags
    public static readonly MICROCHIP = FaMicrochip
    public static readonly CODE_CONNECTION = MdCode
    public static readonly NO_CODE_CONNECTION = MdCodeOff
    public static readonly REFRESH = BiRefresh

    /** Large icons: used for icon buttons */
    public static readonly DELETE_LARGE = withDefaultProps(IoTrashBin, { size: "1.25rem" })
    public static readonly DOWNLOAD_LARGE = withDefaultProps(HiDownload, { size: "1.25rem" })
    public static readonly ADD_LARGE = withDefaultProps(FaPlus, { size: "1.25rem" })
    public static readonly GEAR_LARGE = withDefaultProps(FaGear, { size: "1.25rem" })
    public static readonly REFRESH_LARGE = withDefaultProps(BiRefresh, { size: "1.25rem" })
    public static readonly SELECT_LARGE = withDefaultProps(IoCheckmark, { size: "1.25rem" })
    public static readonly EDIT_LARGE = withDefaultProps(IoPencil, { size: "1.25rem" })
    public static readonly LEFT_ARROW_LARGE = withDefaultProps(FaArrowLeft, { size: "1.25rem" })
    public static readonly RIGHT_ARROW_LARGE = withDefaultProps(FaArrowRight, { size: "1.25rem" })
    public static readonly BUG_LARGE = withDefaultProps(FaBug, { size: "1.25rem" })
    public static readonly XMARK_LARGE = withDefaultProps(FaXmark, { size: "1.25rem" })
    public static readonly XMARK_LARGE_HUD = withDefaultProps(FaXmark, { size: 23 })
    public static readonly PLAY_LARGE = withDefaultProps(IoPlayOutline, { size: "1.25rem" })
    public static readonly EXPAND_MORE_LARGE = withDefaultProps(MdExpandMore, { size: 24 })

    public static readonly OPEN_HUD_ICON = withDefaultProps(FaAngleRight, {
        size: "5vh",
        style: {
            alignSelf: "middle",
            justifySelf: "center",
            minHeight: "40px",
            minWidth: "40px",
            maxHeight: "50px",
            maxWidth: "50px",
        },
        // color={colorNameToVar("BackgroundSecondary")}
    })
}

interface SpacerProps {
    height?: number
    width?: number
}

export const Spacer: React.FC<SpacerProps> = ({ height = 0, width = 0 }) => {
    return <Box minHeight={`${height}px`} minWidth={`${width}px`} />
}

export const Button: React.FC<ButtonProps> = ({ children, onClick, onMouseDown, onMouseUp, ...props }) => {
    return (
        <MuiButton onClick={onClick} {...SoundPlayer.getInstance().buttonSoundEffects()} {...props}>
            {children}
        </MuiButton>
    )
}

export const ProgressButton: React.FC<
    ButtonProps & { onClick: () => Promise<void>; refreshLabel: React.ReactNode }
> = ({ children, refreshLabel, onClick, disabled, ...props }) => {
    const [inProgress, setInProgress] = useState(false)

    const onClickReal = useCallback(async () => {
        setInProgress(true)
        await onClick()
        setInProgress(false)
    }, [onClick])
    return (
        <Button onClick={onClickReal} {...props} disabled={disabled || inProgress}>
            {inProgress ? refreshLabel : children}
        </Button>
    )
}

export type IconButtonSound = "button" | "dropdown"

export const IconButton = forwardRef<HTMLButtonElement, IconButtonProps & { sound?: IconButtonSound }>(
    ({ children, onClick, onMouseDown, onMouseUp, sound = "button", ...props }, ref) => {
        const soundPlayer = SoundPlayer.getInstance()
        const soundEffects =
            sound === "dropdown" ? soundPlayer.dropdownSoundEffects() : soundPlayer.buttonSoundEffects()
        return (
            <MuiIconButton ref={ref} onClick={onClick} {...soundEffects} {...props}>
                {children}
            </MuiIconButton>
        )
    }
)
IconButton.displayName = "IconButton"

export const ToggleButton: React.FC<ToggleButtonProps> = ({ children, onClick, onMouseDown, onMouseUp, ...props }) => {
    return (
        <MuiToggleButton onClick={onClick} {...SoundPlayer.getInstance().buttonSoundEffects()} {...props}>
            {children}
        </MuiToggleButton>
    )
}

export const ToggleButtonGroup: React.FC<ToggleButtonGroupProps> = ({ children, ...props }) => {
    // The sound is played by the individual ToggleButton that was clicked
    return <MuiToggleButtonGroup {...props}>{children}</MuiToggleButtonGroup>
}

export const Select: React.FC<SelectProps> = ({ children, ...props }) => {
    return (
        <MuiSelect {...SoundPlayer.getInstance().dropdownSoundEffects()} {...props}>
            {children}
        </MuiSelect>
    )
}

export const Accordion: React.FC<AccordionProps> = ({ children, ...props }) => {
    return <MuiAccordion {...props}>{children}</MuiAccordion>
}

export const AccordionSummary: React.FC<AccordionSummaryProps> = ({ children, ...props }) => {
    return (
        <MuiAccordionSummary {...SoundPlayer.getInstance().dropdownSoundEffects()} {...props}>
            {children}
        </MuiAccordionSummary>
    )
}

export const AccordionDetails: React.FC<AccordionDetailsProps> = ({ children, ...props }) => {
    return <MuiAccordionDetails {...props}>{children}</MuiAccordionDetails>
}

export const PositiveButton: React.FC<ButtonProps> = ({ children, onClick, ...props }) => {
    return (
        <Button onClick={onClick} {...props} color="success">
            {children}
        </Button>
    )
}

export const PositiveIconButton: React.FC<IconButtonProps> = ({ children, onClick, ...props }) => {
    return (
        <IconButton onClick={onClick} {...props} color="success">
            {children}
        </IconButton>
    )
}

export const DownloadButton: React.FC<IconButtonProps> = ({ onClick, ...props }) => {
    return (
        <PositiveIconButton onClick={onClick} {...props}>
            <SynthesisIcons.DELETE_LARGE />
        </PositiveIconButton>
    )
}

export const AddButton: React.FC<IconButtonProps> = ({ onClick, ...props }) => {
    return (
        <PositiveIconButton onClick={onClick} {...props}>
            <SynthesisIcons.ADD_LARGE />
        </PositiveIconButton>
    )
}

export const SelectButton: React.FC<IconButtonProps> = ({ onClick, ...props }) => {
    return (
        <PositiveIconButton onClick={onClick} {...props}>
            <SynthesisIcons.SELECT_LARGE />
        </PositiveIconButton>
    )
}

export const EditButton: React.FC<IconButtonProps> = ({ onClick, ...props }) => {
    return (
        <PositiveIconButton onClick={onClick} {...props}>
            <SynthesisIcons.EDIT_LARGE />
        </PositiveIconButton>
    )
}

export const NegativeButton: React.FC<ButtonProps> = ({ children, onClick, id, ...props }) => {
    return (
        <Button onClick={onClick} {...props} id={id} color="error">
            {children}
        </Button>
    )
}

export const NegativeIconButton: React.FC<IconButtonProps> = ({ children, onClick, ...props }) => {
    return (
        <IconButton onClick={onClick} {...props} color="error">
            {children}
        </IconButton>
    )
}

export const DeleteButton: React.FC<IconButtonProps> = ({ onClick, id, ...props }) => {
    return (
        <NegativeIconButton onClick={onClick} id={id} {...props}>
            <SynthesisIcons.DELETE_LARGE />
        </NegativeIconButton>
    )
}

export const RefreshButton: React.FC<IconButtonProps> = ({ onClick, ...props }) => {
    return (
        <IconButton onClick={onClick} {...props}>
            <SynthesisIcons.REFRESH_LARGE />
        </IconButton>
    )
}

export const CustomTooltip: React.FC<{ text: string }> = ({ text }) => {
    return (
        <Tooltip title={text}>
            <MuiIconButton
                size="small"
                disableRipple
                sx={{
                    // "color": "#ffffff77",
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
            </MuiIconButton>
        </Tooltip>
    )
}

interface TooltipToggleButtonProps extends ToggleButtonProps {
    title: string
}

export const TooltipToggleButton = React.forwardRef<HTMLButtonElement, TooltipToggleButtonProps>(
    ({ title, ...props }, ref) => {
        return (
            <Tooltip title={title}>
                <MuiToggleButton ref={ref} {...props} />
            </Tooltip>
        )
    }
)

interface TooltipButtonProps extends ButtonProps {
    tooltip?: string
}

export const TooltipButton = React.forwardRef<HTMLButtonElement, TooltipButtonProps>(({ tooltip, ...props }, ref) => {
    return (
        <Tooltip title={tooltip}>
            <MuiButton ref={ref} {...props} />
        </Tooltip>
    )
})

interface LabelWithTooltipProps {
    labelText: string
    tooltipText: string
}

export const LabelWithTooltip: React.FC<LabelWithTooltipProps> = ({ labelText, tooltipText }) => {
    return (
        <Stack direction="row" alignItems={"center"} textAlign={"center"}>
            <Label size="sm">{labelText}</Label>
            <CustomTooltip text={tooltipText} />
        </Stack>
    )
}

// Export the raw MUI components for cases where sound effects are not wanted
export { MuiButton, MuiIconButton, MuiToggleButton, MuiToggleButtonGroup, MuiSelect }
