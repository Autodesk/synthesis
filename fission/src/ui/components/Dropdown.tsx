import { ReactElement, useEffect, useRef, useState } from "react"
import { alpha, styled } from "@mui/system"
import { Button, Menu, MenuItem, Tooltip, Checkbox, Chip, Box } from "@mui/material"
import { colorNameToVar } from "../helpers/UseThemeHelpers"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"

/** The clickable button for a dropdown that shows the selected item and opens the menu. Custom styling over the MUI material button.*/
const CustomButton = styled(Button)({
    border: `2px solid ${colorNameToVar("InteractiveElementRight")}`,
    color: colorNameToVar("InteractiveElementText"),
    backgroundColor: colorNameToVar("BackgroundSecondary"),
    width: "100%",
    minHeight: "40px",
    height: "auto",
    justifyContent: "flex-start",
    textAlign: "left",
    alignItems: "flex-start",
    padding: "8px 12px",
    "&:focus": {
        outline: "none !important",
        border: `2px solid ${colorNameToVar("InteractiveElementRight")} !important`,
        boxShadow: "none !important",
    },
    "&:hover": {
        outline: "none !important",
        border: `2px solid ${colorNameToVar("InteractiveElementLeft")} !important`,
        boxShadow: "none !important",
        backgroundColor: colorNameToVar("BackgroundSecondary"),
    },
    "&:focus-visible": {
        outline: "none !important",
        border: `2px solid ${colorNameToVar("InteractiveElementLeft")} !important`,
        boxShadow: "none !important",
        backgroundColor: colorNameToVar("BackgroundSecondary"),
    },
    "&:active": {
        outline: "none !important",
        border: `2px solid ${colorNameToVar("InteractiveElementLeft")} !important`,
        boxShadow: "none !important",
        backgroundColor: colorNameToVar("BackgroundSecondary"),
    },
    "&::-moz-focus-inner": {
        border: "0 !important",
        backgroundColor: colorNameToVar("BackgroundSecondary"),
    },
    "& .MuiTouchRipple-root": {
        color: "#ffffff30",
    },
})

/** The menu that appears when the dropdown is opened and allows an item to be selected. Custom styling over the MUI material menu. */
const CustomMenu = styled(Menu)({
    "& .MuiPaper-root": {
        backgroundColor: colorNameToVar("BackgroundSecondary"),
        color: colorNameToVar("MainText"),
        border: `2px solid ${colorNameToVar("InteractiveElementRight")} !important`,
        minWidth: "unset",
    },
    "& .MuiMenuItem-root": {
        transition: "background-color 0.3s ease, color 0.3s ease, transform 0.2s ease",
        transform: "scale(1.06)",
        "&:hover": {
            color: "#da6659",
            transform: "scale(1.05)",
        },
        "&:active": {
            transform: "scale(1.03)",
        },
        "& .MuiTouchRipple-root": {
            color: alpha("#d44a3e", 0.3),
        },
    },
})

/** Custom styled chip for displaying selected items in multi-select mode */
const CustomChip = styled(Chip)({
    backgroundColor: colorNameToVar("InteractiveElementLeft"),
    color: colorNameToVar("InteractiveElementText"),
    fontSize: "0.75rem",
    height: "20px",
    "& .MuiChip-deleteIcon": {
        color: colorNameToVar("InteractiveElementText"),
        fontSize: "14px",
        "&:hover": {
            color: colorNameToVar("InteractiveElementText"),
        },
    },
    "&:hover": {
        backgroundColor: "#fc3903",
    },
})

// Overloaded interfaces for single and multi-select modes
interface SingleSelectDropdownProps<T extends string> {
    options: T[]
    onSelect: (value: T) => void
    defaultValue?: T
    label?: string
    className?: string
    multiSelect?: false
    textAlign?: "left" | "center" | "right"
}

interface MultiSelectDropdownProps<T extends string> {
    options: T[]
    onSelect: (values: T[]) => void
    defaultValue?: T[]
    label?: string
    className?: string
    multiSelect: true
    maxWidth?: string
    textAlign?: "left" | "center" | "right"
}

type DropdownProps<T extends string> = SingleSelectDropdownProps<T> | MultiSelectDropdownProps<T>

/**
 * Dropdown component that renders a button which, when clicked, displays a dropdown menu with selectable options.
 * Supports both single-select and multi-select modes.
 *
 * @param {DropdownProps} props - The properties object.
 * @param {string[]} props.options - An array of strings representing the dropdown options.
 * @param {function} props.onSelect - Callback function to handle selection of an option (single value) or options (array of values).
 * @param {string|string[]} [props.defaultValue] - The default selected value(s) for the dropdown.
 * @param {string} [props.label] - An optional label to be displayed above the dropdown.
 * @param {boolean} [props.multiSelect=false] - Whether to enable multi-select mode.
 * @param {string} [props.maxWidth="15rem"] - (Multi-select only) The maximum width of the chip container.
 *
 * @returns {JSX.Element} The rendered Dropdown component.
 */
const Dropdown = <T extends string>(props: DropdownProps<T>): ReactElement => {
    const { options, onSelect, defaultValue, label, multiSelect = false, textAlign = "center" } = props
    const maxWidth = multiSelect ? (props as MultiSelectDropdownProps<T>).maxWidth ?? "15rem" : "15rem"

    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
    const [selectedValue, setSelectedValue] = useState<string>(
        multiSelect ? "" : (defaultValue as string) || ""
    )
    const [selectedValues, setSelectedValues] = useState<T[]>(
        multiSelect ? (defaultValue as T[]) || [] : []
    )
    const buttonRef = useRef<HTMLButtonElement>(null)
    const [menuWidth, setMenuWidth] = useState<number>(0)

    useEffect(() => {
        if (buttonRef.current) {
            setMenuWidth(buttonRef.current.clientWidth)
        }
    }, [])

    /** Handles clicking the button to open the dropdown  menu. */
    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget)
    }

    /** Handles closing the dropdown menu. */
    const handleClose = () => {
        setAnchorEl(null)
        SoundPlayer.dropdownSoundEffects().onMouseDown?.()
    }

    /** Handles the selection of a dropdown option. */
    const handleSelect = (value: T) => {
        if (multiSelect) {
            const newSelectedValues = selectedValues.includes(value)
                ? selectedValues.filter(v => v !== value)
                : [...selectedValues, value]
            
            setSelectedValues(newSelectedValues)
            ;(onSelect as (values: T[]) => void)(newSelectedValues)
        } else {
            setSelectedValue(value)
            ;(onSelect as (value: T) => void)(value)
            handleClose()
        }
    }

    /** Handles removing a selected item (for chips in multi-select mode) */
    const handleRemoveItem = (value: T, event: React.MouseEvent) => {
        event.stopPropagation()
        const newSelectedValues = selectedValues.filter(v => v !== value)
        setSelectedValues(newSelectedValues)
        ;(onSelect as (values: T[]) => void)(newSelectedValues)
    }

    /** Renders the content inside the button for multi-select mode */
    const renderMultiSelectContent = () => {
        if (selectedValues.length === 0) {
            return "Select options"
        }

        return (
            <Box sx={{ 
                display: "flex", 
                flexWrap: "wrap", 
                gap: 0.5, 
                alignItems: "flex-start",
                maxWidth: maxWidth,
                overflow: "hidden"
            }}>
                {selectedValues.map((value) => (
                    <CustomChip
                        key={value}
                        label={value}
                        size="small"
                        onDelete={(event) => handleRemoveItem(value, event)}
                        onClick={(event) => handleRemoveItem(value, event)}
                    />
                ))}
            </Box>
        )
    }

    return (
        <div style={{ display: "inline-block", position: "relative" }}>
            {label && (
                <div
                    style={{
                        marginBottom: "4px",
                        fontSize: "0.875rem",
                        color: "white",
                        textAlign: textAlign,
                    }}
                >
                    {label}
                </div>
            )}
            <Tooltip title={label || ""}>
                <div>
                    <CustomButton
                        onClick={handleClick}
                        {...SoundPlayer.dropdownSoundEffects()}
                        ref={buttonRef}
                        className={`transform transition-transform hover:scale-[1.012] active:scale-[1.024]`}
                    >
                        {multiSelect ? renderMultiSelectContent() : (selectedValue || "Select an option")}
                    </CustomButton>
                </div>
            </Tooltip>
            <CustomMenu
                anchorEl={anchorEl}
                open={Boolean(anchorEl)}
                onClose={handleClose}
                MenuListProps={{ style: { minWidth: menuWidth } }}
            >
                {options.map((option, index) => (
                    <MenuItem key={index} onClick={() => handleSelect(option)}>
                        {multiSelect && (
                            <Checkbox
                                checked={selectedValues.includes(option)}
                                sx={{
                                    color: colorNameToVar("InteractiveElementRight"),
                                    "&.Mui-checked": {
                                        color: colorNameToVar("InteractiveElementLeft"),
                                    },
                                    marginRight: 1,
                                }}
                            />
                        )}
                        {option}
                    </MenuItem>
                ))}
            </CustomMenu>
        </div>
    )
}

export default Dropdown
