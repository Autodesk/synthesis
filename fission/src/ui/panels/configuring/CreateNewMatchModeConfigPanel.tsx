import { Box, Button, Divider, FormControlLabel, Stack, TextField, Typography } from "@mui/material"
import { useCallback, useEffect, useState } from "react"
import DefaultMatchModeConfigs from "@/systems/match_mode/DefaultMatchModeConfigs"
import Checkbox from "@/ui/components/Checkbox"
import type { PanelImplProps } from "@/ui/components/Panel"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import type { MatchModeConfig } from "./MatchModeConfigPanel"
import { validateAndNormalizeMatchModeConfig } from "./MatchModeConfigPanel"
import { createMatchEventFromConfig } from "@/systems/match_mode/MatchModeAnalyticsUtils"
import World from "@/systems/World"

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
    type?: "text" | "number" | "decimal" | "checkbox"
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

    nonNegativeNumber: (message = "Must be a non-negative number"): ValidationRule => ({
        validate: (value: unknown) => {
            if (typeof value !== "string") return false
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
    enableHeightPenalty: {
        defaultValue: false,
        rules: [],
        type: "checkbox",
    },
    maxHeight: {
        defaultValue: fallbackConfig.maxHeight === -1 ? 1.2 : fallbackConfig.maxHeight,
        rules: [VALIDATION_RULES.nonNegativeNumber("Max height must be a non-negative number")],
        type: "decimal",
    },
    heightLimitPenalty: {
        defaultValue: fallbackConfig.heightLimitPenalty === 0 ? 2 : fallbackConfig.heightLimitPenalty,
        rules: [VALIDATION_RULES.nonNegativeInteger("Height penalty must be a non-negative whole number")],
        type: "number",
    },
    enableSideExtensionPenalty: {
        defaultValue: false,
        rules: [],
        type: "checkbox",
    },
    sideMaxExtension: {
        defaultValue: fallbackConfig.sideMaxExtension === -1 ? 0.5 : fallbackConfig.sideMaxExtension,
        rules: [VALIDATION_RULES.nonNegativeNumber("Side max extension must be a non-negative number")],
        type: "decimal",
    },
    sideExtensionPenalty: {
        defaultValue: fallbackConfig.sideExtensionPenalty === 0 ? 2 : fallbackConfig.sideExtensionPenalty,
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
    const { configureScreen, openPanel, closePanel } = useUIContext()
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
        (fieldName: string, label: string, helperText?: string, conditionalOn?: string) => {
            const field = formState[fieldName]
            const config = FIELD_CONFIGS[fieldName]

            // Check if this field should be conditionally rendered
            if (conditionalOn && !formState[conditionalOn]?.value) {
                return null
            }

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
            const isDecimal = config.type === "decimal"
            const isNumericInput = isNumber || isDecimal

            return (
                <TextField
                    key={fieldName}
                    fullWidth
                    label={label}
                    type={isNumericInput ? "number" : "text"}
                    value={field.value}
                    onChange={handleFieldChange(fieldName)}
                    error={field.error}
                    helperText={field.errorText || helperText}
                    inputProps={
                        isNumericInput
                            ? {
                                  min: 0,
                                  step: isDecimal ? 0.1 : 1,
                                  ...(isNumber ? { pattern: "[0-9]*" } : {}),
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
        return {
            id: crypto.randomUUID(),
            name: (formState.name.value as string).trim(),
            isDefault: false,
            autonomousTime: parseInt(formState.autonomousTime.value as string, 10),
            teleopTime: parseInt(formState.teleopTime.value as string, 10),
            endgameTime: parseInt(formState.endgameTime.value as string, 10),
            ignoreRotation: formState.ignoreRotation.value as boolean,
            maxHeight: formState.enableHeightPenalty.value ? parseFloat(formState.maxHeight.value as string) : -1,
            heightLimitPenalty: formState.enableHeightPenalty.value
                ? parseFloat(formState.heightLimitPenalty.value as string)
                : 0,
            sideMaxExtension: formState.enableSideExtensionPenalty.value
                ? parseFloat(formState.sideMaxExtension.value as string)
                : -1,
            sideExtensionPenalty: formState.enableSideExtensionPenalty.value
                ? parseFloat(formState.sideExtensionPenalty.value as string)
                : 0,
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
                acceptText: "Save",
                hideAccept: !isFormValid(),
            },
            {
                onBeforeAccept: () => {
                    const config = createConfigFromForm()
                    const validatedConfig = validateAndNormalizeMatchModeConfig(config)

                    if (!validatedConfig) {
                        return "Validation failed"
                    }

                    const customConfigs = window.localStorage.getItem("match-mode-configs")
                    if (customConfigs) {
                        const customConfigsArray = JSON.parse(customConfigs)
                        customConfigsArray.push(validatedConfig)
                        window.localStorage.setItem("match-mode-configs", JSON.stringify(customConfigsArray))
                    } else {
                        window.localStorage.setItem("match-mode-configs", JSON.stringify([validatedConfig]))
                    }

                    setTimeout(async () => {
                        const { default: MatchModeConfigPanelComponent } = await import("./MatchModeConfigPanel")
                        openPanel(MatchModeConfigPanelComponent, undefined)
                        closePanel(panel!.id, CloseType.Overwrite)
                    }, 0)

                    const matchEvent = createMatchEventFromConfig(validatedConfig, { isDefault: undefined })
                    World.analyticsSystem?.event("Match Mode Config Created", matchEvent)

                    return validatedConfig
                },
            }
        )
    }, [isFormValid, createConfigFromForm, configureScreen, panel, openPanel, closePanel])

    // Field groups for organized rendering
    const fieldGroups: Array<{
        title: string
        fields: Array<{
            name: string
            label: string
            helperText?: string
            conditionalOn?: string
        }>
    }> = [
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
                { name: "enableHeightPenalty", label: "Enable Height Penalty" },
                {
                    name: "maxHeight",
                    label: "Maximum Height (meters)",
                    helperText: "Height limit for the robot",
                    conditionalOn: "enableHeightPenalty",
                },
                {
                    name: "heightLimitPenalty",
                    label: "Height Penalty (points)",
                    conditionalOn: "enableHeightPenalty",
                },
                { name: "enableSideExtensionPenalty", label: "Enable Side Extension Penalty" },
                {
                    name: "sideMaxExtension",
                    label: "Side Max Extension (meters)",
                    helperText: "Maximum side extension limit for the robot",
                    conditionalOn: "enableSideExtensionPenalty",
                },
                {
                    name: "sideExtensionPenalty",
                    label: "Side Extension Penalty (points)",
                    conditionalOn: "enableSideExtensionPenalty",
                },
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
                            <Typography variant="h6" gutterBottom>
                                {group.title}
                            </Typography>
                            {group.fields.map(field =>
                                renderField(field.name, field.label, field.helperText, field.conditionalOn)
                            )}
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
