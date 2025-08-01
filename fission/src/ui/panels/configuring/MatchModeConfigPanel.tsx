import { Box } from "@mui/material"
import React, { ChangeEvent, useEffect, useMemo, useRef, useState } from "react"
import { LabelSize } from "@/components/Label"
import DefaultMatchModeConfigs from "@/systems/match_mode/DefaultMatchModeConfigs"
import MatchMode from "@/systems/match_mode/MatchMode"
import Button from "@/ui/components/Button"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import {
    NegativeButton,
    PositiveButton,
    SectionDivider,
    SectionLabel,
    SynthesisIcons,
} from "@/ui/components/StyledComponents"
import { useModalControlContext } from "@/ui/helpers/UseModalManager"
import { usePanelControlContext } from "@/ui/helpers/UsePanelManager"

/**
 * Configuration for match mode rules and timing.
 *
 * This interface defines the rules and timing for competitive matches,
 * including autonomous/teleop periods, robot height restrictions, and penalties.
 */
export interface MatchModeConfig {
    /** Unique identifier for this match mode configuration */
    id: string

    /** Human-readable name for this match mode configuration */
    name: string

    /** Whether this is a built-in default configuration (cannot be deleted) */
    isDefault: boolean

    /** Duration of autonomous period in seconds (default: 15) */
    autonomousTime: number

    /** Duration of teleoperated period in seconds (default: 135) */
    teleopTime: number

    /** Duration of endgame period in seconds (default: 20) */
    endgameTime: number

    /**
     * Whether to ignore robot rotation when calculating height violations.
     * If true, the height limit will be calculated relative to the base of the robot, rather than the base of the field
     * (default: true)
     */
    ignoreRotation: boolean

    /**
     * Maximum allowed robot height in meters (stored internally).
     * Set to Infinity for no height limit. (default: Infinity)
     */
    maxHeight: number

    /**
     * Points to penalize for height violations (default: 2).
     * Applied each time a robot exceeds maxHeight after cooldown period.
     */
    heightPenalty: number
}

const props: Readonly<{ id: keyof MatchModeConfig; expectedType: string; required: boolean }>[] = [
    { id: "id", expectedType: "string", required: true },
    { id: "name", expectedType: "string", required: true },
    { id: "autonomousTime", expectedType: "number", required: false },
    { id: "teleopTime", expectedType: "number", required: false },
    { id: "endgameTime", expectedType: "number", required: false },
    { id: "ignoreRotation", expectedType: "boolean", required: false },
    { id: "maxHeight", expectedType: "number", required: false },
    { id: "heightPenalty", expectedType: "number", required: false },
]

function matchConfigSelected(config: MatchModeConfig, openModal: (modalName: string) => void) {
    if (MatchMode.getInstance().isMatchEnabled()) {
        globalAddToast(
            "error",
            "Match Mode Already Running",
            "You can't modify the match mode ruleset while a match is running"
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
        <Box
            component={"div"}
            display={"flex"}
            key={id}
            justifyContent={"space-between"}
            alignItems={"center"}
            gap={"1rem"}
        >
            <SectionLabel className="text-wrap break-all">{name.replace(/.mira$/, "")}</SectionLabel>
            <Box
                component={"div"}
                display={"flex"}
                key={`button-box-${id}`}
                flexDirection={"row-reverse"}
                gap={"0.25rem"}
                justifyContent={"center"}
                alignItems={"center"}
            >
                {secondaryOnClick && <NegativeButton value={SynthesisIcons.DELETE_LARGE} onClick={secondaryOnClick} />}
                <PositiveButton value={SynthesisIcons.SELECT_LARGE} onClick={primaryOnClick} />
            </Box>
        </Box>
    )
}

const MatchModeConfigPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { closePanel } = usePanelControlContext()
    const { openModal } = useModalControlContext()

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
                globalAddToast("error", "Error Loading Match Mode Configs", "Please check the console for more details")
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
                            closePanel("match-mode-config")
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
                                          "Match Mode Config Deleted",
                                          `Successfully deleted "${config.name}"`
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
        // Type guard to check if config is an object
        if (typeof config !== "object" || config === null) {
            console.error("Match mode config validation failed: config must be an object")
            globalAddToast("error", "Invalid Match Mode Config", "Configuration must be an object")
            return null
        }
        const configObj = config as Record<string, unknown>

        const typeError = (id: string, expectedType?: string) => {
            const errorMessage = expectedType ? `must be a ${expectedType}` : "is required"
            console.error(`Match mode config validation failed: the '${id}' field ${errorMessage}`)
            globalAddToast("error", "Invalid Match Mode Config", `The '${id}' field ${errorMessage}`)
        }

        function checkValidity(configObj: Record<string, unknown>): configObj is Partial<MatchModeConfig> {
            for (const prop of props) {
                if (configObj[prop.id] == undefined) {
                    if (prop.required) {
                        typeError(prop.id)
                        return false
                    }
                } else if (typeof configObj[prop.id] != prop.expectedType) {
                    if (prop.required) {
                        typeError(prop.id, prop.expectedType)
                        return false
                    } else {
                        globalAddToast(
                            "warning",
                            "Invalid Match Mode Config",
                            `The '${prop.id}' field must be a ${prop.expectedType}, ignoring ${prop.id} field`
                        )
                    }
                }
            }
            return true
        }

        if (!checkValidity(configObj)) {
            return null
        }

        // If validation passes, normalize the config with defaults for missing fields
        return {
            ...DefaultMatchModeConfigs.fallbackValues(),
            ...configObj,
        }
    }

    const handleFileUpload = async (file: File) => {
        // Check if it's a JSON file
        if (!file.name.toLowerCase().endsWith(".json")) {
            globalAddToast("error", "Invalid File Type", "Please select a JSON file")
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
                    "Invalid Match Mode Config",
                    "The JSON file does not match the required MatchModeConfig structure"
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
                    "Match Mode Config Already Exists",
                    "There is already a match mode config with this ID"
                )
                return
            }

            setMatchModeConfigs(prev => [...prev, normalizedConfig])
            // Only save custom configs to local storage
            const customConfigs = [...matchModeConfigs.filter(c => !c.isDefault), normalizedConfig]
            window.localStorage.setItem("match-mode-configs", JSON.stringify(customConfigs))

            globalAddToast("info", "Match Mode Config Added", `Successfully added "${normalizedConfig.name}"`)
        } catch (_error) {
            globalAddToast("error", "Invalid JSON File", "The file is not valid JSON or could not be read")
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
        <Panel
            name={"Match Mode Config"}
            icon={SynthesisIcons.IMPORT}
            panelId={panelId}
            acceptEnabled={false}
            cancelName="Back"
            openLocation="center"
            onCancel={() => {
                closePanel("match-mode-config")
            }}
        >
            <SectionLabel size={LabelSize.MEDIUM} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                {matchModeConfigs.length} Match Mode{matchModeConfigs.length === 1 ? "" : "s"}
            </SectionLabel>
            <SectionDivider />
            {matchModeConfigElements}
            <input ref={fileUploadRef} onChange={onInputChanged} type="file" hidden={true} accept=".json" />

            <Box alignSelf={"center"}>
                <Button value="Upload File" onClick={uploadClicked} />
            </Box>
        </Panel>
    )
}

export default MatchModeConfigPanel
