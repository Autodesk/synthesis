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
    id: string
    name: string
    autonomousTime: number
    teleopTime: number
    endgameTime: number
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
                const indexRes = await fetch("public/match-mode-config/index.json")
                const fileNames: string[] = await indexRes.json()

                const allConfigs = await Promise.all(
                    fileNames.map(async fileName => {
                        const res = await fetch(`public/match-mode-config/${fileName}`)
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
        return (
            typeof config === "object" &&
            config !== null &&
            typeof (config as Record<string, unknown>).id === "string" &&
            typeof (config as Record<string, unknown>).name === "string" &&
            typeof (config as Record<string, unknown>).autonomousTime === "number" &&
            typeof (config as Record<string, unknown>).teleopTime === "number" &&
            typeof (config as Record<string, unknown>).endgameTime === "number"
        )
    }

    const onInputChanged = async (e: ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const file = e.target.files[0]

            // Check if it's a JSON file
            if (!file.name.toLowerCase().endsWith(".json")) {
                Global_AddToast?.("error", "Invalid File Type", "Please select a JSON file")
                return
            }

            try {
                // Read file content
                const fileContent = await file.text()

                // Parse JSON
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
                    Global_AddToast?.(
                        "error",
                        "Match Mode Config Already Exists",
                        "There is already a match mode config with this ID"
                    )
                    return
                }

                // If validation passes, add to the list
                setMatchModeConfigs(prev => [...prev, parsedConfig])

                Global_AddToast?.("info", "Match Mode Config Added", `Successfully added "${parsedConfig.name}"`)
            } catch (error) {
                Global_AddToast?.("error", "Invalid JSON File", "The file is not valid JSON or could not be read")
            }
        }
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
