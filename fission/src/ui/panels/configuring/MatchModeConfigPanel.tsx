import { PanelImplProps } from "@/ui/components/Panel"
import React, { ChangeEvent, useEffect, useMemo, useRef, useState } from "react"
import { SynthesisIcons, PositiveButton, NegativeButton } from "@/ui/components/StyledComponents"
import { Box, Button, Divider, Typography } from "@mui/material"
import MatchMode, { DEFAULT_AUTONOMOUS_TIME, DEFAULT_TELEOP_TIME, DEFAULT_ENDGAME_TIME } from "@/systems/MatchMode"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import DefaultMatchModeConfigs from "@/systems/DefaultMatchModeConfigs"
import { CloseType, OpenModalFn, useUIContext } from "@/ui/UIProvider"
import { Stack } from "@mui/system"

export interface MatchModeConfig {
    id: string // Required
    name: string // Required
    isDefault: boolean // Track if this is a default config (auto-filled)
    autonomousTime: number // Optional, defaults to 15
    teleopTime: number // Optional, defaults to 135
    endgameTime: number // Optional, defaults to 20
}

function matchConfigSelected(config: MatchModeConfig, openModal: OpenModalFn) {
    if (MatchMode.getInstance().isMatchEnabled()) {
        globalAddToast(
            "error",
            "Match Mode Already Running\nYou can't modify the match mode ruleset while a match is running"
        )
        return
    }

    MatchMode.getInstance().setMatchModeConfig(config)

    MatchMode.getInstance().start(openModal)
}

interface ItemCardProps {
    id: string
    name: string
    primaryOnClick: () => void
    secondaryOnClick?: () => void
}

const ItemCard: React.FC<ItemCardProps> = ({ id, name, primaryOnClick, secondaryOnClick }) => {
    return (
        <Stack direction="row" key={id} justifyContent={"space-between"} alignItems={"center"} gap={"1rem"}>
            <Typography variant="h5" className="text-wrap break-all">
                {name.replace(/.mira$/, "")}
            </Typography>
            <Stack
                key={`button-box-${id}`}
                direction="row-reverse"
                gap={"0.25rem"}
                justifyContent={"center"}
                alignItems={"center"}
            >
                {secondaryOnClick && (
                    <NegativeButton onClick={secondaryOnClick}>{SynthesisIcons.DELETE_LARGE}</NegativeButton>
                )}
                <PositiveButton onClick={primaryOnClick}>{SynthesisIcons.SELECT_LARGE}</PositiveButton>
            </Stack>
        </Stack>
    )
}

const MatchModeConfigPanel: React.FC<PanelImplProps<void>> = ({ panel, parent }) => {
    const { closePanel, openModal } = useUIContext()

    const [matchModeConfigs, setMatchModeConfigs] = useState<MatchModeConfig[]>([])

    useEffect(() => {
        const loadConfigs = () => {
            try {
                const defaultConfigs = DefaultMatchModeConfigs.defaultMatchModeConfigCopies
                const localConfigs = JSON.parse(window.localStorage.getItem("match-mode-configs") || "[]")

                const combinedConfigs = [...defaultConfigs, ...localConfigs]
                const uniqueConfigsById = Array.from(new Map(combinedConfigs.map(item => [item.id, item])).values())

                setMatchModeConfigs(uniqueConfigsById)
            } catch (err) {
                console.error("Error loading match mode configs:", err)
                globalAddToast("error", "Error Loading Match Mode Configs\nPlease check the console for more details")
            }
        }

        loadConfigs()
    }, [])

    const matchModeConfigElements = useMemo(
        () =>
            matchModeConfigs.map(config => {
                return (
                    <ItemCard
                        key={config.id}
                        id={config.id}
                        name={config.name || config.id || "Unnamed Match Mode"}
                        primaryOnClick={() => {
                            matchConfigSelected(config, openModal)
                            closePanel(panel.id, CloseType.Accept)
                        }}
                        secondaryOnClick={
                            !config.isDefault
                                ? () => {
                                      // Delete the config from the local storage
                                      const updatedConfigs = matchModeConfigs.filter(c => c.id !== config.id)
                                      setMatchModeConfigs(updatedConfigs)
                                      // Only save custom configs to local storage
                                      const customConfigs = updatedConfigs.filter(c => !c.isDefault)
                                      window.localStorage.setItem("match-mode-configs", JSON.stringify(customConfigs))
                                      globalAddToast(
                                          "info",
                                          `Match Mode Config Deleted\nSuccessfully deleted "${config.name}"`
                                      )
                                  }
                                : undefined
                        }
                    />
                )
            }),
        [matchModeConfigs, openModal, closePanel]
    )

    const fileUploadRef = useRef<HTMLInputElement>(null)

    const uploadClicked = () => {
        if (fileUploadRef.current) {
            fileUploadRef.current.click()
        }
    }

    const validateAndNormalizeMatchModeConfig = (config: unknown): MatchModeConfig | null => {
        let valid = true

        // Type guard to check if config is an object
        if (typeof config !== "object" || config === null) {
            console.error("Match mode config validation failed: config must be an object")
            globalAddToast("error", "Invalid Match Mode Config\nConfiguration must be an object")
            return null
        }

        const configObj = config as Record<string, unknown>

        const props: { id: string; expectedType: string; required: boolean }[] = [
            { id: "id", expectedType: "string", required: true },
            { id: "name", expectedType: "string", required: true },
            { id: "autonomousTime", expectedType: "number", required: false },
            { id: "teleopTime", expectedType: "number", required: false },
            { id: "endgameTime", expectedType: "number", required: false },
        ]

        const typeError = (id: string, expectedType?: string) => {
            const errorMessage = expectedType ? `must be a ${expectedType}` : "is required"
            console.error(`Match mode config validation failed: the '${id}' field ${errorMessage}`)
            globalAddToast("error", `Invalid Match Mode Config\nThe '${id}' field ${errorMessage}`)
        }

        for (const prop of props) {
            if (configObj[prop.id] == undefined) {
                if (prop.required) {
                    typeError(prop.id)
                    valid = false
                }
            } else if (typeof configObj[prop.id] != prop.expectedType) {
                if (prop.required) {
                    typeError(prop.id, prop.expectedType)
                    valid = false
                } else {
                    globalAddToast(
                        "warning",
                        `Invalid Match Mode Config\nThe '${prop.id}' field must be a ${prop.expectedType}, ignoring ${prop.id} field`
                    )
                }
            }
        }

        if (!valid) {
            return null
        }

        // If validation passes, normalize the config with defaults for missing fields
        const normalizedConfig: MatchModeConfig = {
            id: configObj.id as string,
            name: configObj.name as string,
            isDefault: false, // User-uploaded configs are not default configs
            autonomousTime:
                typeof configObj.autonomousTime === "number" ? configObj.autonomousTime : DEFAULT_AUTONOMOUS_TIME,
            teleopTime: typeof configObj.teleopTime === "number" ? configObj.teleopTime : DEFAULT_TELEOP_TIME,
            endgameTime: typeof configObj.endgameTime === "number" ? configObj.endgameTime : DEFAULT_ENDGAME_TIME,
        }

        return normalizedConfig
    }

    const handleFileUpload = async (file: File) => {
        // Check if it's a JSON file
        if (!file.name.toLowerCase().endsWith(".json")) {
            globalAddToast("error", "Invalid File Type\nPlease select a JSON file")
            return
        }

        try {
            // Read file content
            const fileContent = await file.text()
            const parsedConfig = JSON.parse(fileContent) // ?? {}

            // Validate structure and normalize config
            const normalizedConfig = validateAndNormalizeMatchModeConfig(parsedConfig)
            if (!normalizedConfig) {
                globalAddToast(
                    "error",
                    "Invalid Match Mode Config\nThe JSON file does not match the required MatchModeConfig structure"
                )
                return
            }

            // Ensures that the config id is unique
            if (matchModeConfigs.find(config => config.id === normalizedConfig.id)) {
                console.error(
                    `Match mode config validation failed: A config with id '${normalizedConfig.id}' already exists`
                )
                globalAddToast(
                    "error",
                    "Match Mode Config Already Exists\nThere is already a match mode config with this ID"
                )
                return
            }

            setMatchModeConfigs(prev => [...prev, normalizedConfig])
            // Only save custom configs to local storage
            const customConfigs = [...matchModeConfigs.filter(c => !c.isDefault), normalizedConfig]
            window.localStorage.setItem("match-mode-configs", JSON.stringify(customConfigs))

            globalAddToast("info", `Match Mode Config Added\nSuccessfully added "${normalizedConfig.name}"`)
        } catch (error) {
            globalAddToast("error", "Invalid JSON File\nThe file is not valid JSON or could not be read")
        }
    }

    const onInputChanged = async (e: ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files[0]) {
            await handleFileUpload(e.target.files[0])
        }
        // Reset the input value so the same file can be selected again
        e.target.value = ""
    }

    return (
        <>
            <Typography variant="h5" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                {matchModeConfigs.length} Match Mode
                {matchModeConfigs.length === 1 ? "" : "s"}
            </Typography>
            <Divider />
            {matchModeConfigElements}
            <input ref={fileUploadRef} onChange={onInputChanged} type="file" hidden={true} accept=".json" />

            <Box alignSelf={"center"}>
                <Button onClick={uploadClicked}>Upload File</Button>
            </Box>
        </>
    )
}

export default MatchModeConfigPanel
