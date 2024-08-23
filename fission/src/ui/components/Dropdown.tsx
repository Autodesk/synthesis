import React, { useEffect, useRef, useState } from "react"
import { alpha, styled } from "@mui/system"
import { Menu, MenuItem, Button, Tooltip } from "@mui/material"
import { colorNameToVar } from "../ThemeContext"

/** The clickable button for a dropdown that shows the selected item and opens the menu. Custom styling over the MUI material button.*/
const CustomButton = styled(Button)({
    "border": `2px solid ${colorNameToVar("InteractiveElementRight")}`,
    "color": colorNameToVar("InteractiveElementText"),
    "backgroundColor": colorNameToVar("BackgroundSecondary"),
    "width": "100%",
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
        "transition": "background-color 0.3s ease, color 0.3s ease, transform 0.2s ease",
        "transform": "scale(1.06)",
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

interface DropdownProps {
    options: string[]
    onSelect: (value: string) => void
    defaultValue?: string
    label?: string
    className?: string
}

/**
 * Dropdown component that renders a button which, when clicked, displays a dropdown menu with a list of selectable options.
 *
 * @param {DropdownProps} props - The properties object.
 * @param {string[]} props.options - An array of strings representing the dropdown options.
 * @param {function} props.onSelect - Callback function to handle selection of an option.
 * @param {string} [props.defaultValue] - The default selected value for the dropdown.
 * @param {string} [props.label] - An optional label to be displayed above the dropdown.
 *
 * @returns {JSX.Element} The rendered Dropdown component.
 */
const Dropdown: React.FC<DropdownProps> = ({ options, onSelect, defaultValue, label }) => {
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
    const [selectedValue, setSelectedValue] = useState<string>(defaultValue || "")
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
    }

    /** Handles the selection of a dropdown option. */
    const handleSelect = (value: string) => {
        setSelectedValue(value)
        onSelect(value)
        handleClose()
    }

    return (
        <div style={{ display: "inline-block", position: "relative" }}>
            {label && (
                <div
                    style={{
                        marginBottom: "4px",
                        fontSize: "0.875rem",
                        color: "white",
                        textAlign: "center",
                    }}
                >
                    {label}
                </div>
            )}
            <Tooltip title={label || ""}>
                <div>
                    <CustomButton
                        onClick={handleClick}
                        ref={buttonRef}
                        className={`transform transition-transform hover:scale-[1.012] active:scale-[1.024]`}
                    >
                        {selectedValue || "Select an option"}
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
                        {option}
                    </MenuItem>
                ))}
            </CustomMenu>
        </div>
    )
}

export default Dropdown
