import { createTheme, useMediaQuery } from "@mui/material"
import { useMemo } from "react";

export function useSynthesisTheme() {
    const prefersDarkMode = useMediaQuery("(prefers-color-scheme: dark)");
    const theme = useMemo(() => createTheme({
        palette: {
            mode: prefersDarkMode ? 'dark' : 'light'
        },
        components: {
            MuiButton: {
                defaultProps: {
                    size: "small",
                },
            },
            MuiFilledInput: {
                defaultProps: {
                    margin: "dense",
                },
            },
            MuiFormControl: {
                defaultProps: {
                    margin: "dense",
                },
            },
            MuiFormHelperText: {
                defaultProps: {
                    margin: "dense",
                },
            },
            MuiSelect: {
                defaultProps: {
                    size: "small",
                },
            },
            MuiIconButton: {
                defaultProps: {
                    size: "small",
                },
            },
            MuiInputBase: {
                defaultProps: {
                    margin: "dense",
                },
            },
            MuiInputLabel: {
                defaultProps: {
                    margin: "dense",
                },
            },
            MuiListItem: {
                defaultProps: {
                    dense: true,
                },
            },
            MuiOutlinedInput: {
                defaultProps: {
                    margin: "dense",
                },
            },
            MuiFab: {
                defaultProps: {
                    size: "small",
                },
            },
            MuiTable: {
                defaultProps: {
                    size: "small",
                },
            },
            MuiTextField: {
                defaultProps: {
                    margin: "dense",
                },
            },
            MuiToolbar: {
                defaultProps: {
                    variant: "dense",
                },
            },
        },
    }), [prefersDarkMode]);
    
    return theme;
}
