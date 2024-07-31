import { ToggleButton as ToggleButtonMUI, ToggleButtonGroup as ToggleButtonGroupMUI } from "@mui/material"
import { styled } from "@mui/system"
import { colorNameToVar } from "../ThemeContext"

export const ToggleButton = styled(ToggleButtonMUI)({
    // backgroundColor: "white"
    "borderColor": "transparent",
    "fontFamily": "Artifakt",
    "fontWeight": 700,
    "color": "white",
    "&.Mui-selected": {
        color: "white",
        backgroundImage: `linear-gradient(to right, ${colorNameToVar("InteractiveElementLeft")}, ${colorNameToVar("InteractiveElementRight")})`,
        borderColor: "transparent",
    },
    "&:focus": {
        borderColor: "transparent",
    },
    "&:hover": {
        borderColor: "white",
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

export const ToggleButtonGroup = styled(ToggleButtonGroupMUI)({
    backgroundColor: colorNameToVar("Background"),
    fontFamily: "Artifakt",
    fontWeight: 700,
    width: "fit-content",
})
