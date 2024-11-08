import { ToggleButton as ToggleButtonMUI, ToggleButtonGroup as ToggleButtonGroupMUI } from "@mui/material"
import { styled } from "@mui/system"
import { colorNameToVar } from "../ThemeContext"

export const ToggleButton = styled(ToggleButtonMUI)({
    "borderColor": "transparent",
    "fontFamily": "Artifakt",
    "fontWeight": 700,
    "color": colorNameToVar("MainText"),
    /*     "transition": "transform 0.2s ease",
    "transform": "scale(1)", */
    "&.Mui-selected": {
        color: colorNameToVar("MainText"),
        backgroundImage: `linear-gradient(to right, ${colorNameToVar("InteractiveElementLeft")}, ${colorNameToVar("InteractiveElementRight")})`,
        borderColor: "transparent",
    },
    ".MuiTouchRipple-ripple": {
        color: "#ffffff30",
    },
    "&:focus": {
        borderColor: "transparent !important",
        outline: "none",
    },
    "&:selected": {
        outline: "none",
        borderColor: "transparent",
    },
    "&:hover": {
        outline: "none",
        borderColor: "transparent",
        backgroundColor: "#ffffff20",

        // transform: "scale(1.03)",
    },
    "&:focus-visible": {
        outline: "none",
        borderColor: "transparent",
    },
    "&:active": {
        outline: "none",
        borderColor: "transparent",
        // transform: "scale(1.06)",
    },
    "&::-moz-focus-inner": {
        outline: "none",
        borderColor: "transparent",
    },
})

export const ToggleButtonGroup = styled(ToggleButtonGroupMUI)({
    "backgroundColor": colorNameToVar("Background"),
    "fontFamily": "Artifakt",
    "fontWeight": 700,
    "width": "fit-content",
    "&:focus": {
        borderColor: "transparent",
    },
    "&:selected": {
        borderColor: "transparent",
    },
    "&:hover": {
        borderColor: "transparent",
    },
    "&:focus-visible": {
        borderColor: "transparent",
    },
    "&:active": {
        borderColor: "transparent",
    },
    "&::-moz-focus-inner": {
        borderColor: "transparent",
    },
})
