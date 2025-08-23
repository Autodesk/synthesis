import InfoIcon from "@mui/icons-material/Info"
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
import { IoCheckmark, IoPencil, IoPeople, IoPlayOutline, IoTrashBin } from "react-icons/io5"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import Label from "./Label"

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
    public static readonly PLAY = <IoPlayOutline />

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
    public static readonly PLAY_LARGE = <IoPlayOutline size={"1.25rem"} />

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

export const Button: React.FC<ButtonProps> = ({ children, onClick, onMouseDown, onMouseUp, ...props }) => {
    return (
        <MuiButton onClick={onClick} {...SoundPlayer.getInstance().buttonSoundEffects()} {...props}>
            {children}
        </MuiButton>
    )
}

export const IconButton: React.FC<IconButtonProps> = ({ children, onClick, onMouseDown, onMouseUp, ...props }) => {
    return (
        <MuiIconButton onClick={onClick} {...SoundPlayer.getInstance().buttonSoundEffects()} {...props}>
            {children}
        </MuiIconButton>
    )
}

export const ToggleButton: React.FC<ToggleButtonProps> = ({ children, onClick, onMouseDown, onMouseUp, ...props }) => {
    return (
        <MuiToggleButton onClick={onClick} {...SoundPlayer.getInstance().buttonSoundEffects()} {...props}>
            {children}
        </MuiToggleButton>
    )
}

export const ToggleButtonGroup: React.FC<ToggleButtonGroupProps> = ({ children, onMouseDown, onMouseUp, ...props }) => {
    return (
        <MuiToggleButtonGroup {...SoundPlayer.getInstance().buttonSoundEffects()} {...props}>
            {children}
        </MuiToggleButtonGroup>
    )
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

export const DownloadButton = (onClick: () => void, props: IconButtonProps = {}) => {
    return (
        <PositiveIconButton onClick={onClick} {...props}>
            {SynthesisIcons.DELETE_LARGE}
        </PositiveIconButton>
    )
}

export const AddButton = (onClick: () => void, props: IconButtonProps = {}) => {
    return (
        <PositiveIconButton onClick={onClick} {...props}>
            {SynthesisIcons.ADD_LARGE}
        </PositiveIconButton>
    )
}

export const SelectButton = (onClick: () => void, props: IconButtonProps = {}) => {
    return (
        <PositiveIconButton onClick={onClick} {...props}>
            {SynthesisIcons.SELECT_LARGE}
        </PositiveIconButton>
    )
}

export const EditButton = (onClick: () => void, props: IconButtonProps = {}) => {
    return (
        <PositiveIconButton onClick={onClick} {...props}>
            {SynthesisIcons.EDIT_LARGE}
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

export const DeleteButton = (onClick: () => void, id?: string, props: IconButtonProps = {}) => {
    return (
        <NegativeIconButton onClick={onClick} id={id} {...props}>
            {SynthesisIcons.DELETE_LARGE}
        </NegativeIconButton>
    )
}

export const RefreshButton = (onClick: () => void, props: IconButtonProps = {}) => {
    return (
        <IconButton onClick={onClick} {...props}>
            {SynthesisIcons.REFRESH_LARGE}
        </IconButton>
    )
}

export const CustomTooltip = (text: string) => {
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

export const LabelWithTooltip = (labelText: string, tooltipText: string) => {
    return (
        <Stack direction="row" alignItems={"center"} textAlign={"center"}>
            <Label size="sm">{labelText}</Label>
            {CustomTooltip(tooltipText)}
        </Stack>
    )
}

// Export the raw MUI components for cases where sound effects are not wanted
export { MuiButton, MuiIconButton, MuiToggleButton, MuiToggleButtonGroup, MuiSelect }
