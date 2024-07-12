import { 
    ToggleButton as ToggleButtonMUI,
    ToggleButtonGroup as ToggleButtonGroupMUI
} from "@mui/material";
import { styled } from "@mui/system";
import { colorNameToVar } from "../ThemeContext";

export const ToggleButton = styled(ToggleButtonMUI)({
    // backgroundColor: "white"
    borderColor: "none",
    color: "white",
    "&.Mui-selected": {
        color: "white",
        backgroundImage: `linear-gradient(to right, ${colorNameToVar("InteractiveElementLeft")}, ${colorNameToVar("InteractiveElementRight")})`
    }
})

export const ToggleButtonGroup = styled(ToggleButtonGroupMUI)({
    backgroundColor: colorNameToVar("Background"),
    width: "fit-content"
})