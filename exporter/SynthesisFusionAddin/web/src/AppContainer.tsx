import { Box, CssBaseline, ThemeProvider } from "@mui/material";
import { useSynthesisTheme } from "./hooks/useSynthesisTheme";

export function AppContainer({ children }: React.PropsWithChildren<{}>) {
    const theme = useSynthesisTheme();

    return (
        <ThemeProvider theme={theme}>
            <CssBaseline />
            <Box
                sx={{
                    position: 'fixed',
                    left: 0,
                    right: 0,
                    top: 0,
                    bottom: 0,
                    backgroundColor: theme.palette.background.default
                }}
            >
                {children}
            </Box>
        </ThemeProvider>
    )
}
