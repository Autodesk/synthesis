import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import React, { ChangeEvent, useEffect, useMemo, useRef, useState } from "react"
import { SectionLabel, SynthesisIcons, PositiveButton, SectionDivider } from "@/ui/components/StyledComponents"
import { LabelSize } from "@/components/Label"
import { Box } from "@mui/material"
import { usePanelControlContext } from "@/ui/helpers/UsePanelManager"
import MatchMode from "@/systems/MatchMode"
import { Global_AddToast } from "@/ui/components/GlobalUIControls"
import { useModalControlContext } from "@/ui/helpers/UseModalManager"
import Button from "@/ui/components/Button"

export interface MatchModeConfig {
    id: string // Required
    name: string // Required
    autonomousTime: number // Optional, defaults to 15
    teleopTime: number // Optional, defaults to 135
    endgameTime: number // Optional, defaults to 20
}

function MatchConfigSelected(config: MatchModeConfig, openModal: (modalName: string) => void) {
    if (MatchMode.getInstance().isMatchEnabled()) {
        Global_AddToast?.(
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
}

const ItemCard: React.FC<ItemCardProps> = ({ id, name, primaryOnClick }) => {
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
                <PositiveButton value={"Select"} onClick={primaryOnClick} />
            </Box>
        </Box>
    )
}

const MatchModeConfigPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { closePanel } = usePanelControlContext()
    const { openModal } = useModalControlContext()

    const [matchModeConfigs, setMatchModeConfigs] = useState<MatchModeConfig[]>([])

    useEffect(() => {
        const loadAllJsonFiles = async () => {
            try {
                const indexRes = await fetch("match-mode-config/index.json")
                const fileNames: string[] = await indexRes.json()

                const allConfigs = await Promise.all(
                    fileNames.map(async fileName => {
                        const res = await fetch(`match-mode-config/${fileName}`)
                        return res.json()
                    })
                )

                setMatchModeConfigs(allConfigs)
            } catch (err) {
                console.error("Error loading JSON files:", err)
                Global_AddToast?.(
                    "error",
                    "Error Loading Match Mode Configs",
                    "Please check the console for more details"
                )
            }
        }

        loadAllJsonFiles()
    }, [])

    const matchModeConfigElements = useMemo(
        () =>
            matchModeConfigs.map(config =>
                ItemCard({
                    id: config.id,
                    name: config.name || config.id || "Unnamed Match Mode",
                    primaryOnClick: () => {
                        MatchConfigSelected(config, openModal)
                        closePanel("match-mode-config")
                    },
                })
            ),
        [matchModeConfigs, openModal, closePanel]
    )

    const fileUploadRef = useRef<HTMLInputElement>(null)

    const uploadClicked = () => {
        if (fileUploadRef.current) {
            fileUploadRef.current.click()
        }
    }

    const validateMatchModeConfig = (config: unknown): config is MatchModeConfig => {
        if (typeof config !== "object" || config === null) {
            return false
        }

        const configObj = config as Record<string, unknown>

        // Check required fields
        if (typeof configObj.id !== "string") {
            console.error("Match mode config validation failed: 'id' field is required and must be a string")
            Global_AddToast?.("error", "Invalid Match Mode Config", "The 'id' field is required and must be a string")
            return false
        }

        if (typeof configObj.name !== "string") {
            console.error("Match mode config validation failed: 'name' field is required and must be a string")
            Global_AddToast?.("error", "Invalid Match Mode Config", "The 'name' field is required and must be a string")
            return false
        }

        // Check optional fields and provide defaults/warnings
        const expectedFields = new Set(["id", "name", "autonomousTime", "teleopTime", "endgameTime"])
        const actualFields = new Set(Object.keys(configObj))

        // Check for missing optional fields
        const missingFields = ["autonomousTime", "teleopTime", "endgameTime"].filter(
            field => !(field in configObj) || typeof configObj[field] !== "number"
        )
        if (missingFields.length > 0) {
            console.warn(
                `Match mode config '${configObj.name}' is missing or has invalid optional fields: ${missingFields.join(", ")}. Default values will be used.`
            )
            Global_AddToast?.(
                "warning",
                "Invalid Match Mode Config",
                `The following optional fields are missing or invalid: ${missingFields.join(", ")}. Default values will be used.`
            )
        }

        // Check for extra fields
        const extraFields = [...actualFields].filter(field => !expectedFields.has(field))
        if (extraFields.length > 0) {
            console.warn(
                `Match mode config '${configObj.name}' contains unexpected fields: ${extraFields.join(", ")}. These will be ignored.`
            )
            Global_AddToast?.(
                "warning",
                "Unexpected Fields in Match Mode Config",
                `The following fields are unexpected and will be ignored: ${extraFields.join(", ")}`
            )
        }

        return true
    }

    const handleFileUpload = async (file: File) => {
        // Check if it's a JSON file
        if (!file.name.toLowerCase().endsWith(".json")) {
            Global_AddToast?.("error", "Invalid File Type", "Please select a JSON file")
            return
        }

        try {
            // Read file content
            const fileContent = await file.text()
            const parsedConfig = JSON.parse(fileContent)

            // Validate structure
            if (!validateMatchModeConfig(parsedConfig)) {
                Global_AddToast?.(
                    "error",
                    "Invalid Match Mode Config",
                    "The JSON file does not match the required MatchModeConfig structure"
                )
                return
            }

            // Ensures that the config id is unique
            if (matchModeConfigs.find(config => config.id === parsedConfig.id)) {
                console.error(
                    `Match mode config validation failed: A config with id '${parsedConfig.id}' already exists`
                )
                Global_AddToast?.(
                    "error",
                    "Match Mode Config Already Exists",
                    "There is already a match mode config with this ID"
                )
                return
            }

            // If validation passes, normalize the config with defaults for missing fields
            const normalizedConfig: MatchModeConfig = {
                id: parsedConfig.id,
                name: parsedConfig.name,
                autonomousTime: typeof parsedConfig.autonomousTime === "number" ? parsedConfig.autonomousTime : 15,
                teleopTime: typeof parsedConfig.teleopTime === "number" ? parsedConfig.teleopTime : 135,
                endgameTime: typeof parsedConfig.endgameTime === "number" ? parsedConfig.endgameTime : 20,
            }

            setMatchModeConfigs(prev => [...prev, normalizedConfig])

            Global_AddToast?.("info", "Match Mode Config Added", `Successfully added "${normalizedConfig.name}"`)
        } catch (error) {
            Global_AddToast?.("error", "Invalid JSON File", "The file is not valid JSON or could not be read")
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
            icon={SynthesisIcons.Import}
            panelId={panelId}
            acceptEnabled={false}
            cancelName="Back"
            openLocation="center"
            onCancel={() => {
                closePanel("match-mode-config")
            }}
        >
            <SectionLabel size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
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
