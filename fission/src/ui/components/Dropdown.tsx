import React, { useEffect, useRef, useState } from "react"
import { alpha, styled } from "@mui/system"
import { Menu, MenuItem, Button, Tooltip } from "@mui/material"
import { colorNameToVar } from "../ThemeContext"

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
    defaultValue?: string
    onSelect: (value: string) => void
    label?: string
    className?: string
}

const Dropdown: React.FC<DropdownProps> = ({ options, defaultValue, onSelect, label }) => {
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
    const [selectedValue, setSelectedValue] = useState<string>(defaultValue || "")
    const buttonRef = useRef<HTMLButtonElement>(null)
    const [menuWidth, setMenuWidth] = useState<number>(0)

    useEffect(() => {
        if (buttonRef.current) {
            setMenuWidth(buttonRef.current.clientWidth)
        }
    }, [])

    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget)
    }

    const handleClose = () => {
        setAnchorEl(null)
    }

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
