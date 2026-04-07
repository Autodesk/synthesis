import { Box, Divider } from "@mui/material"
import { Stack } from "@mui/system"
import type React from "react"
import { type ChangeEvent, useEffect, useMemo, useRef, useState } from "react"
import DefaultMatchModeConfigs from "@/systems/match_mode/DefaultMatchModeConfigs"
import MatchMode from "@/systems/match_mode/MatchMode"
import World from "@/systems/World.ts"
import Checkbox from "@/ui/components/Checkbox"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import { Button, NegativeButton, PositiveButton, SynthesisIcons } from "@/ui/components/StyledComponents"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import CreateNewMatchModeConfigPanel from "./CreateNewMatchModeConfigPanel"
import { createMatchEventFromConfig } from "@/systems/match_mode/MatchModeAnalyticsUtils"

/**
 * Configuration for match mode rules and timing.
 *
 * This interface defines the rules and timing for competitive matches,
 * including autonomous/teleop periods, robot height restrictions, and penalties.
 */
export interface MatchModeConfig {
    /** Unique identifier for this match mode configuration */
    readonly id: string

    /** Human-readable name for this match mode configuration */
    readonly name: string

    /** Whether this is a built-in default configuration (cannot be deleted) */
    readonly isDefault: boolean

    /** Duration of autonomous period in seconds (default: 15) */
    readonly autonomousTime: number

    /** Duration of teleoperated period in seconds (default: 135) */
    readonly teleopTime: number

    /** Duration of endgame period in seconds (default: 20) */
    readonly endgameTime: number

    /**
     * Whether to ignore robot rotation when calculating height violations.
     * If true, the height limit will be calculated relative to the base of the robot, rather than the base of the field
     * (default: true)
     */
    readonly ignoreRotation: boolean

    /**
     * Maximum allowed robot height in meters (stored internally).
     * Set to Infinity for no height limit. (default: Infinity)
     */
    readonly maxHeight: number

    /**
     * Points to penalize for height limit violations (default: 2).
     * Applied each time a robot exceeds maxHeight after cooldown period.
     */
    readonly heightLimitPenalty: number

    /**
     * Maximum allowed robot side extension in meters
     * User input is in feet but converted to meters during config processing.
     * Set to Infinity for no side extension limit. (default: Infinity)
     */
    readonly sideMaxExtension: number

    /**
     * Points to penalize for side extension violations (default: 2).
     * Applied each time a robot exceeds sideMaxExtension after cooldown period.
     */
    readonly sideExtensionPenalty: number
}

const props: Readonly<{
    id: keyof MatchModeConfig
    expectedType: string
    required: boolean
}>[] = [
    { id: "id", expectedType: "string", required: true },
    { id: "name", expectedType: "string", required: true },
    { id: "autonomousTime", expectedType: "number", required: false },
    { id: "teleopTime", expectedType: "number", required: false },
    { id: "endgameTime", expectedType: "number", required: false },
    { id: "ignoreRotation", expectedType: "boolean", required: false },
    { id: "maxHeight", expectedType: "number", required: false },
    { id: "heightLimitPenalty", expectedType: "number", required: false },
    { id: "sideMaxExtension", expectedType: "number", required: false },
    { id: "sideExtensionPenalty", expectedType: "number", required: false },
]

export const validateAndNormalizeMatchModeConfig = (config: unknown): MatchModeConfig | null => {
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

    // If validation passes, use the default values in any missing fields
    const normalizedConfig = {
        ...DefaultMatchModeConfigs.fallbackValues(),
        ...configObj,
    }
    normalizedConfig.isDefault = false
    return normalizedConfig
}

interface ItemCardProps {
    id: string
    name: string
    primaryOnClick: () => void
    secondaryOnClick?: () => void
}

const ItemCard: React.FC<ItemCardProps> = ({ id, name, primaryOnClick, secondaryOnClick }) => {
    return (
        <Stack
            direction="row"
            key={id}
            justifyContent={"space-between"}
            alignItems={"center"}
            gap={"1rem"}
            sx={{ px: 1, py: 0.5 }}
        >
            <Label size="sm" className="text-wrap break-all">
                {name.replace(/.mira$/, "")}
            </Label>
            <Stack
                key={`button-box-${id}`}
                direction="row-reverse"
                gap={"0.5rem"}
                justifyContent={"center"}
                alignItems={"center"}
            >
                {secondaryOnClick && (
                    <NegativeButton onClick={secondaryOnClick}>{SynthesisIcons.DELETE_LARGE}</NegativeButton>
                )}
                <PositiveButton onClick={primaryOnClick}>{SynthesisIcons.PLAY_LARGE}</PositiveButton>
            </Stack>
        </Stack>
    )
}

const MatchModeConfigPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { openPanel, closePanel, openModal, configureScreen } = useUIContext()

    const [matchModeConfigs, setMatchModeConfigs] = useState<MatchModeConfig[]>([])
    const [useSpawnPositions, setUseSpawnPositions] = useState(false)
    const [spawnPositionsConfigured, setSpawnPositionsConfigured] = useState(false)

    useEffect(() => {
        configureScreen(panel!, { title: "Match Mode Config", hideAccept: true, cancelText: "Back" }, {})
    }, [configureScreen, panel])

    useEffect(() => {
        const loadConfigs = () => {
            try {
                const defaultConfigs = DefaultMatchModeConfigs.configs
                console.log(defaultConfigs)
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

    useEffect(() => {
        setSpawnPositionsConfigured(
            World?.sceneRenderer?.mirabufSceneObjects?.getField()?.fieldPreferences?.spawnLocations
                ?.hasConfiguredLocations === true
        )
    })

    const matchModeConfigElements = useMemo(
        () =>
            matchModeConfigs.map(config => {
                return (
                    <ItemCard
                        key={config.id}
                        id={config.id}
                        name={config.name || config.id || "Unnamed Match Mode"}
                        primaryOnClick={async () => {
                            if (MatchMode.getInstance().isMatchEnabled()) {
                                globalAddToast(
                                    "error",
                                    "Match Mode Already Running",
                                    "You can't modify the match mode ruleset while a match is running"
                                )
                                return
                            }
                            MatchMode.getInstance().setMatchModeConfig(config)

                            await MatchMode.getInstance().start(true, useSpawnPositions)
                            closePanel(panel!.id, CloseType.Accept)
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
                                  }
                                : undefined
                        }
                    />
                )
            }),
        [matchModeConfigs, openModal, closePanel, useSpawnPositions]
    )

    const fileUploadRef = useRef<HTMLInputElement>(null)

    const uploadClicked = () => {
        if (fileUploadRef.current) {
            fileUploadRef.current.click()
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

            const matchEvent = createMatchEventFromConfig(normalizedConfig, { isDefault: undefined })
            World.analyticsSystem?.event("Match Mode Config Uploaded", matchEvent)

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

    const createNewMatchModeConfig = () => {
        openPanel(CreateNewMatchModeConfigPanel, undefined)
        closePanel(panel!.id, CloseType.Overwrite)
    }

    return (
        <>
            <Label size="sm" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                {matchModeConfigs.length} Match Mode
                {matchModeConfigs.length === 1 ? "" : "s"}
            </Label>
            <Divider />
            {matchModeConfigElements}
            <Divider />
            <Checkbox
                disabled={!spawnPositionsConfigured}
                tooltip={
                    spawnPositionsConfigured
                        ? "Should robots move to starting positions based on their alliance station"
                        : "Spawn positions are not configured for this field"
                }
                checked={useSpawnPositions}
                label={"Move Robots to Starting Positions"}
                onClick={v => setUseSpawnPositions(v)}
            />
            <Divider />
            <input ref={fileUploadRef} onChange={onInputChanged} type="file" hidden={true} accept=".json" />

            <Box alignSelf={"center"} sx={{ display: "flex", flexDirection: "column", gap: 1, my: 1 }}>
                <Button
                    onClick={() => {
                        createNewMatchModeConfig()
                    }}
                >
                    Create Match Mode Config
                </Button>
                <Button onClick={uploadClicked}>Upload File</Button>
            </Box>
        </>
    )
}

export default MatchModeConfigPanel
