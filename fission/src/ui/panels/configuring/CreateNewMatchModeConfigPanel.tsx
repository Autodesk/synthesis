import type { PanelImplProps } from "@/ui/components/Panel"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { Box, TextField, FormControlLabel, Checkbox, Stack, Divider, Button } from "@mui/material"
import { useEffect, useState, useCallback } from "react"
import type { MatchModeConfig } from "./MatchModeConfigPanel"
import {
    DEFAULT_AUTONOMOUS_TIME,
    DEFAULT_TELEOP_TIME,
    DEFAULT_ENDGAME_TIME,
    DEFAULT_IGNORE_ROTATION,
    DEFAULT_MAX_HEIGHT,
    DEFAULT_HEIGHT_PENALTY,
} from "@/systems/match_mode/MatchModeTypes"
import { matchConfigSelected, validateAndNormalizeMatchModeConfig } from "./MatchModeConfigPanel"

interface ValidationRule {
    validate: (value: unknown) => boolean
    message: string
}

interface FormField<T = unknown> {
    value: T
    error: boolean
    errorText: string
    rules: ValidationRule[]
}

interface FormState {
    name: FormField<string>
    autonomousTime: FormField<string>
    teleopTime: FormField<string>
    endgameTime: FormField<string>
    ignoreRotation: FormField<boolean>
    maxHeight: FormField<string>
    heightPenalty: FormField<string>
}

// Validation rules
const VALIDATION_RULES = {
    required: (message = "This field is required"): ValidationRule => ({
        validate: (value: unknown) => {
            if (typeof value === "string") return value.trim() !== ""
            return value != null
        },
        message,
    }),

    nonNegativeInteger: (message = "Must be a non-negative whole number"): ValidationRule => ({
        validate: (value: unknown) => {
            if (typeof value !== "string") return false
            const num = parseInt(value, 10)
            return !isNaN(num) && num >= 0 && Number.isInteger(num) && value === num.toString()
        },
        message,
    }),

    numberOrInfinity: (message = "Must be a positive number or 'Infinity'"): ValidationRule => ({
        validate: (value: unknown) => {
            if (typeof value !== "string") return false
            if (value.toLowerCase() === "infinity") return true
            const num = parseFloat(value)
            return !isNaN(num) && num > 0
        },
        message,
    }),
}

// Initial form state
const createInitialFormState = (): FormState => ({
    name: {
        value: "Input Config Name",
        error: false,
        errorText: "",
        rules: [VALIDATION_RULES.required("Name is required")],
    },
    autonomousTime: {
        value: DEFAULT_AUTONOMOUS_TIME.toString(),
        error: false,
        errorText: "",
        rules: [VALIDATION_RULES.nonNegativeInteger("Autonomous time must be a non-negative whole number")],
    },
    teleopTime: {
        value: DEFAULT_TELEOP_TIME.toString(),
        error: false,
        errorText: "",
        rules: [VALIDATION_RULES.nonNegativeInteger("Teleop time must be a non-negative whole number")],
    },
    endgameTime: {
        value: DEFAULT_ENDGAME_TIME.toString(),
        error: false,
        errorText: "",
        rules: [VALIDATION_RULES.nonNegativeInteger("Endgame time must be a non-negative whole number")],
    },
    ignoreRotation: {
        value: DEFAULT_IGNORE_ROTATION,
        error: false,
        errorText: "",
        rules: [],
    },
    maxHeight: {
        value: DEFAULT_MAX_HEIGHT === Infinity ? "Infinity" : DEFAULT_MAX_HEIGHT.toString(),
        error: false,
        errorText: "",
        rules: [VALIDATION_RULES.numberOrInfinity("Max height must be a positive number or 'Infinity'")],
    },
    heightPenalty: {
        value: DEFAULT_HEIGHT_PENALTY.toString(),
        error: false,
        errorText: "",
        rules: [VALIDATION_RULES.nonNegativeInteger("Height penalty must be a non-negative whole number")],
    },
})

const CreateNewMatchModeConfigPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const [formState, setFormState] = useState<FormState>(createInitialFormState)

    const validateField = useCallback((field: FormField, value: unknown): { error: boolean; errorText: string } => {
        for (const rule of field.rules) {
            if (!rule.validate(value)) {
                return { error: true, errorText: rule.message }
            }
        }
        return { error: false, errorText: "" }
    }, [])

    const updateField = useCallback(
        <K extends keyof FormState>(fieldName: K, value: FormState[K]["value"]) => {
            setFormState(prev => {
                const field = prev[fieldName]
                const validation = validateField(field, value)

                return {
                    ...prev,
                    [fieldName]: {
                        ...field,
                        value,
                        error: validation.error,
                        errorText: validation.errorText,
                    },
                }
            })
        },
        [validateField]
    )

    const handleTextFieldChange = useCallback(
        (fieldName: keyof FormState) => (event: React.ChangeEvent<HTMLInputElement>) => {
            updateField(fieldName, event.target.value)
        },
        [updateField]
    )

    const handleCheckboxChange = useCallback(
        (fieldName: keyof FormState) => (event: React.ChangeEvent<HTMLInputElement>) => {
            updateField(fieldName, event.target.checked)
        },
        [updateField]
    )

    const isFormValid = useCallback(() => {
        return Object.values(formState).every(field => !field.error)
    }, [formState])

    const createConfigFromForm = useCallback((): MatchModeConfig => {
        const parseHeight = (value: string): number => {
            return value.toLowerCase() === "infinity" ? Infinity : parseFloat(value)
        }

        return {
            id: crypto.randomUUID(),
            name: formState.name.value.trim(),
            isDefault: false,
            autonomousTime: parseInt(formState.autonomousTime.value, 10),
            teleopTime: parseInt(formState.teleopTime.value, 10),
            endgameTime: parseInt(formState.endgameTime.value, 10),
            ignoreRotation: formState.ignoreRotation.value,
            maxHeight: parseHeight(formState.maxHeight.value),
            heightPenalty: parseFloat(formState.heightPenalty.value),
        }
    }, [formState])

    const downloadConfig = useCallback(() => {
        const config = createConfigFromForm()

        // Create a blob with the JSON data
        const jsonString = JSON.stringify(config, null, 2)
        const blob = new Blob([jsonString], { type: "application/json" })

        // Create a download link
        const url = URL.createObjectURL(blob)
        const link = document.createElement("a")
        link.href = url
        link.download = `${config.name.replace(/[^a-z0-9]/gi, "_").toLowerCase()}_match_mode_config.json`
        document.body.appendChild(link)
        link.click()

        // Cleanup
        document.body.removeChild(link)
        URL.revokeObjectURL(url)
    }, [createConfigFromForm])

    useEffect(() => {
        configureScreen(
            panel!,
            {
                title: "Create New Match Mode Config",
                cancelText: "Cancel",
                hideAccept: !isFormValid(),
            },
            {
                onBeforeAccept: () => {
                    const config = createConfigFromForm()
                    const validatedConfig = validateAndNormalizeMatchModeConfig(config)

                    if (!validatedConfig) {
                        return "Validation failed"
                    }

                    matchConfigSelected(validatedConfig)
                    const customConfigs = window.localStorage.getItem("match-mode-configs")
                    if (customConfigs) {
                        const customConfigsArray = JSON.parse(customConfigs)
                        customConfigsArray.push(validatedConfig)
                        window.localStorage.setItem("match-mode-configs", JSON.stringify(customConfigsArray))
                    } else {
                        window.localStorage.setItem("match-mode-configs", JSON.stringify([validatedConfig]))
                    }

                    return undefined
                },
            }
        )
    }, [isFormValid, createConfigFromForm, configureScreen, panel])

    return (
        <Box
            component="div"
            sx={{
                padding: "1rem",
                overflowY: "auto",
                borderRadius: "0.5rem",
                minWidth: "350px",
                maxWidth: "500px",
            }}
        >
            <Stack spacing={3}>
                {/* Basic Configuration Section */}
                <Box>
                    <TextField
                        fullWidth
                        label="Configuration Name"
                        placeholder="Enter a descriptive name"
                        value={formState.name.value}
                        onChange={handleTextFieldChange("name")}
                        error={formState.name.error}
                        helperText={formState.name.errorText}
                    />
                </Box>

                <Divider />

                {/* Timing Configuration Section */}
                <Box>
                    <Stack spacing={2}>
                        <TextField
                            fullWidth
                            label="Autonomous Time (seconds)"
                            type="number"
                            value={formState.autonomousTime.value}
                            onChange={handleTextFieldChange("autonomousTime")}
                            error={formState.autonomousTime.error}
                            helperText={formState.autonomousTime.errorText}
                            inputProps={{
                                min: 0,
                                step: 1,
                                pattern: "[0-9]*",
                            }}
                            onKeyPress={e => {
                                if (e.key === "." || e.key === "-" || e.key === "+" || e.key === "e" || e.key === "E") {
                                    e.preventDefault()
                                }
                            }}
                        />

                        <TextField
                            fullWidth
                            label="Teleop Time (seconds)"
                            type="number"
                            value={formState.teleopTime.value}
                            onChange={handleTextFieldChange("teleopTime")}
                            error={formState.teleopTime.error}
                            helperText={formState.teleopTime.errorText}
                            inputProps={{
                                min: 0,
                                step: 1,
                                pattern: "[0-9]*",
                            }}
                            onKeyPress={e => {
                                if (e.key === "." || e.key === "-" || e.key === "+" || e.key === "e" || e.key === "E") {
                                    e.preventDefault()
                                }
                            }}
                        />

                        <TextField
                            fullWidth
                            label="Endgame Time (seconds)"
                            type="number"
                            value={formState.endgameTime.value}
                            onChange={handleTextFieldChange("endgameTime")}
                            error={formState.endgameTime.error}
                            helperText={formState.endgameTime.errorText}
                            inputProps={{
                                min: 0,
                                step: 1,
                                pattern: "[0-9]*",
                            }}
                            onKeyPress={e => {
                                if (e.key === "." || e.key === "-" || e.key === "+" || e.key === "e" || e.key === "E") {
                                    e.preventDefault()
                                }
                            }}
                        />
                    </Stack>
                </Box>

                <Divider />

                {/* Robot Constraints Section */}
                <Box>
                    <Stack spacing={2}>
                        <FormControlLabel
                            control={
                                <Checkbox
                                    checked={formState.ignoreRotation.value}
                                    onChange={handleCheckboxChange("ignoreRotation")}
                                />
                            }
                            label="Ignore Robot Rotation for Height Calculations"
                        />

                        <TextField
                            fullWidth
                            label="Maximum Height (feet or 'Infinity')"
                            value={formState.maxHeight.value}
                            onChange={handleTextFieldChange("maxHeight")}
                            error={formState.maxHeight.error}
                            helperText={formState.maxHeight.errorText || "Enter 'Infinity' for unlimited height"}
                            placeholder="Enter number or 'Infinity'"
                        />

                        <TextField
                            fullWidth
                            label="Height Penalty (points)"
                            type="number"
                            value={formState.heightPenalty.value}
                            onChange={handleTextFieldChange("heightPenalty")}
                            error={formState.heightPenalty.error}
                            helperText={formState.heightPenalty.errorText}
                            inputProps={{
                                min: 0,
                                step: 1,
                                pattern: "[0-9]*",
                            }}
                            onKeyPress={e => {
                                if (e.key === "." || e.key === "-" || e.key === "+" || e.key === "e" || e.key === "E") {
                                    e.preventDefault()
                                }
                            }}
                        />
                    </Stack>
                </Box>

                <Divider />

                {/* Download Button */}
                <Box display="flex" justifyContent="center">
                    <Button variant="outlined" onClick={downloadConfig} disabled={!isFormValid()}>
                        Download as JSON
                    </Button>
                </Box>
            </Stack>
        </Box>
    )
}

export default CreateNewMatchModeConfigPanel
