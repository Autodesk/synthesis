import { createTheme } from "@mui/material"

export const theme = createTheme({
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
})
