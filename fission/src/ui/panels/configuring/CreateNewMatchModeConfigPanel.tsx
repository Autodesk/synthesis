import type { PanelImplProps } from "@/ui/components/Panel"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { Box, TextField, FormControlLabel, Stack, Divider, Button } from "@mui/material"
import Checkbox from "@/ui/components/Checkbox"
import { useEffect, useState, useCallback } from "react"
import type { MatchModeConfig } from "./MatchModeConfigPanel"
import DefaultMatchModeConfigs from "@/systems/match_mode/DefaultMatchModeConfigs"
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

type FormState = Record<string, FormField>

type FieldConfig = {
    defaultValue: string | number | boolean
    rules: ValidationRule[]
    type?: "text" | "number" | "checkbox" | "numberOrInfinity"
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

    numberOrInfinity: (message = "Must be a non-negative number or 'Infinity'"): ValidationRule => ({
        validate: (value: unknown) => {
            if (typeof value !== "string") return false
            if (value.toLowerCase() === "infinity") return true
            const num = parseFloat(value)
            return !isNaN(num) && num >= 0
        },
        message,
    }),
}

const fallbackConfig = DefaultMatchModeConfigs.fallbackValues()

// Field configurations
const FIELD_CONFIGS: Record<string, FieldConfig> = {
    name: {
        defaultValue: "Input Config Name",
        rules: [VALIDATION_RULES.required("Name is required")],
        type: "text",
    },
    autonomousTime: {
        defaultValue: fallbackConfig.autonomousTime,
        rules: [VALIDATION_RULES.nonNegativeInteger("Autonomous time must be a non-negative whole number")],
        type: "number",
    },
    teleopTime: {
        defaultValue: fallbackConfig.teleopTime,
        rules: [VALIDATION_RULES.nonNegativeInteger("Teleop time must be a non-negative whole number")],
        type: "number",
    },
    endgameTime: {
        defaultValue: fallbackConfig.endgameTime,
        rules: [VALIDATION_RULES.nonNegativeInteger("Endgame time must be a non-negative whole number")],
        type: "number",
    },
    ignoreRotation: {
        defaultValue: fallbackConfig.ignoreRotation,
        rules: [],
        type: "checkbox",
    },
    maxHeight: {
        defaultValue: fallbackConfig.maxHeight === Infinity ? "Infinity" : fallbackConfig.maxHeight,
        rules: [VALIDATION_RULES.numberOrInfinity("Max height must be a non-negative number or 'Infinity'")],
        type: "numberOrInfinity",
    },
    heightLimitPenalty: {
        defaultValue: fallbackConfig.heightLimitPenalty,
        rules: [VALIDATION_RULES.nonNegativeInteger("Height penalty must be a non-negative whole number")],
        type: "number",
    },
    sideMaxExtension: {
        defaultValue: fallbackConfig.sideMaxExtension === Infinity ? "Infinity" : fallbackConfig.sideMaxExtension,
        rules: [VALIDATION_RULES.numberOrInfinity("Side max extension must be a positive number or 'Infinity'")],
        type: "numberOrInfinity",
    },
    sideExtensionPenalty: {
        defaultValue: fallbackConfig.sideExtensionPenalty,
        rules: [VALIDATION_RULES.nonNegativeInteger("Side extension penalty must be a non-negative whole number")],
        type: "number",
    },
}

// Initial form state factory
const createInitialFormState = (): FormState => {
    const formState: FormState = {}

    Object.entries(FIELD_CONFIGS).forEach(([fieldName, config]) => {
        // Preserve boolean values for checkbox fields, convert others to strings
        let value: unknown
        if (config.type === "checkbox") {
            value = config.defaultValue
        } else {
            value = typeof config.defaultValue === "string" ? config.defaultValue : config.defaultValue.toString()
        }

        formState[fieldName] = {
            value,
            error: false,
            errorText: "",
            rules: config.rules,
        }
    })

    return formState
}

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
        (fieldName: string, value: unknown) => {
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

    const handleFieldChange = useCallback(
        (fieldName: string, isCheckbox = false) =>
            (event: React.ChangeEvent<HTMLInputElement>) => {
                const value = isCheckbox ? event.target.checked : event.target.value
                updateField(fieldName, value)
            },
        [updateField]
    )

    // Prevent non-integer input for number fields
    const preventNonIntegerKeys = useCallback((e: React.KeyboardEvent) => {
        if (e.key === "." || e.key === "-" || e.key === "+" || e.key === "e" || e.key === "E") {
            e.preventDefault()
        }
    }, [])

    const isFormValid = useCallback(() => {
        return Object.values(formState).every(field => !field.error)
    }, [formState])

    const renderField = useCallback(
        (fieldName: string, label: string, helperText?: string) => {
            const field = formState[fieldName]
            const config = FIELD_CONFIGS[fieldName]

            if (config.type === "checkbox") {
                return (
                    <FormControlLabel
                        key={fieldName}
                        control={
                            <Checkbox
                                checked={field.value as boolean}
                                onClick={value => updateField(fieldName, value)}
                                label={""}
                            />
                        }
                        label={label}
                    />
                )
            }

            const isNumber = config.type === "number"
            const isNumberOrInfinity = config.type === "numberOrInfinity"

            return (
                <TextField
                    key={fieldName}
                    fullWidth
                    label={label}
                    type={isNumber ? "number" : "text"}
                    value={field.value}
                    onChange={handleFieldChange(fieldName)}
                    error={field.error}
                    helperText={field.errorText || helperText}
                    placeholder={isNumberOrInfinity ? "Enter number or 'Infinity'" : undefined}
                    inputProps={
                        isNumber
                            ? {
                                  min: 0,
                                  step: 1,
                                  pattern: "[0-9]*",
                              }
                            : undefined
                    }
                    onKeyPress={isNumber ? preventNonIntegerKeys : undefined}
                />
            )
        },
        [formState, handleFieldChange, preventNonIntegerKeys, updateField]
    )

    const createConfigFromForm = useCallback((): MatchModeConfig => {
        const parseHeight = (value: string): number => {
            return value.toLowerCase() === "infinity" ? Number.MAX_SAFE_INTEGER : parseFloat(value)
        }

        return {
            id: crypto.randomUUID(),
            name: (formState.name.value as string).trim(),
            isDefault: false,
            autonomousTime: parseInt(formState.autonomousTime.value as string, 10),
            teleopTime: parseInt(formState.teleopTime.value as string, 10),
            endgameTime: parseInt(formState.endgameTime.value as string, 10),
            ignoreRotation: formState.ignoreRotation.value as boolean,
            maxHeight: parseHeight(formState.maxHeight.value as string),
            heightLimitPenalty: parseFloat(formState.heightLimitPenalty.value as string),
            sideMaxExtension: parseHeight(formState.sideMaxExtension.value as string),
            sideExtensionPenalty: parseFloat(formState.sideExtensionPenalty.value as string),
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

    // Field groups for organized rendering
    const fieldGroups = [
        {
            title: "Basic Configuration",
            fields: [{ name: "name", label: "Configuration Name" }],
        },
        {
            title: "Timing Configuration",
            fields: [
                { name: "autonomousTime", label: "Autonomous Time (seconds)" },
                { name: "teleopTime", label: "Teleop Time (seconds)" },
                { name: "endgameTime", label: "Endgame Time (seconds)" },
            ],
        },
        {
            title: "Robot Constraints",
            fields: [
                { name: "ignoreRotation", label: "Ignore Robot Rotation for Height Calculations" },
                {
                    name: "maxHeight",
                    label: "Maximum Height (meters or 'Infinity')",
                    helperText: "Enter 'Infinity' for unlimited height",
                },
                { name: "heightLimitPenalty", label: "Height Penalty (points)" },
                {
                    name: "sideMaxExtension",
                    label: "Side Max Extension (meters or 'Infinity')",
                    helperText: "Enter 'Infinity' for unlimited side extension",
                },
                { name: "sideExtensionPenalty", label: "Side Extension Penalty (points)" },
            ],
        },
    ]

    return (
        <Box
            component="div"
            sx={{
                padding: "1rem",
                overflowY: "auto",
                borderRadius: "0.5rem",
                minWidth: "350px",
                maxWidth: "500px",
                maxHeight: "70vh",
                height: "fit-content",
            }}
        >
            <Stack spacing={3}>
                {fieldGroups.map((group, groupIndex) => (
                    <Box key={group.title}>
                        <Stack spacing={2}>
                            {group.fields.map(field => renderField(field.name, field.label, field.helperText))}
                        </Stack>
                        {groupIndex < fieldGroups.length - 1 && <Divider />}
                    </Box>
                ))}

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
