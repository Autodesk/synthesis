import defaultTheme from "@/MuiTheme";
import Synthesis from "@/Synthesis";

import { ThemeProvider } from "@mui/material";

function SynthesisThemed() {

    const theme = defaultTheme

    return (
        <ThemeProvider theme={theme}>
            <Synthesis />
        </ThemeProvider>
    )
}

export default SynthesisThemed