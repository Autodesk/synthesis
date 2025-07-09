import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import React, { ChangeEvent, useEffect, useMemo, useRef, useState } from "react"
import {
    SectionLabel,
    SynthesisIcons,
    PositiveButton,
    SectionDivider,
    NegativeButton,
} from "@/ui/components/StyledComponents"
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
                <NegativeButton value={SynthesisIcons.DeleteLarge} onClick={secondaryOnClick} />
                <PositiveButton value={SynthesisIcons.SelectLarge} onClick={primaryOnClick} />
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

                const defaultConfigs = await Promise.all(
                    fileNames.map(async fileName => {
                        const res = await fetch(`match-mode-config/${fileName}`)
                        return res.json()
                    })
                )
                const localConfigs = JSON.parse(window.localStorage.getItem("match-mode-configs") || "[]")

                const combinedConfigs = [...defaultConfigs, ...localConfigs]
                const uniqueConfigsById = Array.from(new Map(combinedConfigs.map(item => [item.id, item])).values())

                setMatchModeConfigs(uniqueConfigsById)
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
                    secondaryOnClick: () => {
                        // Delete the config from the local storage
                        setMatchModeConfigs(prev => prev.filter(c => c.id !== config.id))
                        window.localStorage.setItem(
                            "match-mode-configs",
                            JSON.stringify(matchModeConfigs.filter(c => c.id !== config.id))
                        )
                        matchModeConfigs.filter(c => c.id !== config.id)
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

    // eslint-disable-next-line
    const validateMatchModeConfig = (config: any): config is MatchModeConfig => {
        let valid = true

        const props: { id: string; expected_type: string; required: boolean }[] = [
            { id: "id", expected_type: "string", required: true },
            { id: "name", expected_type: "string", required: true },
            { id: "autonomousTime", expected_type: "number", required: false },
            { id: "teleopTime", expected_type: "number", required: false },
            { id: "endgameTime", expected_type: "number", required: false },
        ]

        const typeError = (id: string, expected_type?: string) => {
            const error_message = expected_type ? `must be a ${expected_type}` : "is required"
            console.error(`Match mode config validation failed: the '${id}' field ${error_message}`)
            Global_AddToast?.("error", "Invalid Match Mode Config", `The '${id}' field ${error_message}`)
        }

        for (const prop of props) {
            if (config[prop.id] == undefined) {
                if (prop.required) {
                    typeError(prop.id)
                    valid = false
                }
            } else if (typeof config[prop.id] != prop.expected_type) {
                if (prop.required) {
                    typeError(prop.id, prop.expected_type)
                    valid = false
                } else {
                    Global_AddToast?.(
                        "warning",
                        "Invalid Match Mode Config",
                        `The '${prop.id}' field must be a ${prop.expected_type}, ignoring ${prop.id} field`
                    )
                }
            }
        }

        return valid
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
            const parsedConfig = JSON.parse(fileContent) // ?? {}

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
            window.localStorage.setItem("match-mode-configs", JSON.stringify([...matchModeConfigs, normalizedConfig]))

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
