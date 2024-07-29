import React, { useEffect, useRef, useState } from "react"
import { styled } from "@mui/system"
import { Menu, MenuItem, Button, Tooltip } from "@mui/material"

const CustomButton = styled(Button)({
    "border": "2px solid #da6659",
    "color": "white",
    "width": "100%",
    "&:focus": {
        outline: "none !important",
        border: "2px solid #da6659 !important",
        boxShadow: "none !important",
    },
    "&:hover": {
        outline: "none !important",
        border: "2px solid #da6659 !important",
        boxShadow: "none !important",
    },
    "&:focus-visible": {
        outline: "none !important",
        border: "2px solid #da6659 !important",
        boxShadow: "none !important",
    },
    "&:active": {
        outline: "none !important",
        border: "2px solid #da6659 !important",
        boxShadow: "none !important",
    },
    "&::-moz-focus-inner": {
        border: "0 !important",
    },
})

const CustomMenu = styled(Menu)({
    "& .MuiPaper-root": {
        backgroundColor: "#333",
        color: "white",
        border: "1px solid #da6659",
        minWidth: "unset", // Ensure minWidth is unset so it can be overridden
    },
    "& .MuiMenuItem-root": {
        "transition": "background-color 0.3s ease, color 0.3s ease",
        "&:hover": {
            backgroundColor: "#444",
            color: "#da6659",
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
                    <CustomButton onClick={handleClick} ref={buttonRef}>
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
