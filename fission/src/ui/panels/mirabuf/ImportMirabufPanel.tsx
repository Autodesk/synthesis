import { Accordion, AccordionDetails, AccordionSummary, Box, CircularProgress, Stack, Tooltip } from "@mui/material"
import { Button, ToggleButton, ToggleButtonGroup } from "@/ui/components/StyledComponents"
import type React from "react"
import { type ReactNode, useCallback, useEffect, useLayoutEffect, useMemo, useState } from "react"
import { MdExpandMore } from "react-icons/md"
import {
    type Data,
    getMirabufFiles,
    hasMirabufFiles,
    MirabufFilesStatusUpdateEvent,
    MirabufFilesUpdateEvent,
    requestMirabufFiles,
} from "@/aps/APSDataManagement"
import MirabufCachingService, {
    backUpFields,
    backUpRobots,
    canOPFS,
    type MirabufCacheInfo,
    type MirabufRemoteInfo,
    MiraType,
} from "@/mirabuf/MirabufLoader"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { PAUSE_REF_ASSEMBLY_SPAWNING } from "@/systems/physics/PhysicsTypes"

import World from "@/systems/World"
import { globalOpenPanel } from "@/ui/components/GlobalUIControls"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import { ProgressHandle } from "@/ui/components/ProgressNotificationData"
import {
    DeleteButton,
    PositiveButton,
    PositiveIconButton,
    RefreshButton,
    SynthesisIcons,
} from "@/ui/components/StyledComponents"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import ImportLocalMirabufModal from "@/ui/modals/mirabuf/ImportLocalMirabufModal"
import type TaskStatus from "@/util/TaskStatus"
import type { ConfigurationType } from "../configuring/assembly-config/ConfigTypes"
import InitialConfigPanel from "../configuring/initial-config/InitialConfigPanel"

interface ItemCardProps {
    id: string
    name: string
    primaryButtonNode: ReactNode
    primaryOnClick: () => void
    secondaryOnClick?: () => void
}

const ItemCard: React.FC<ItemCardProps> = ({ id, name, primaryButtonNode, primaryOnClick, secondaryOnClick }) => {
    return (
        <Stack key={id} justifyContent={"space-between"} alignItems={"center"} gap={"1rem"} direction="row">
            <Label size="md" className="text-wrap break-all">
                {name.replace(/.mira$/, "")}
            </Label>
            <Stack
                key={`button-box-${id}`}
                direction="row-reverse"
                gap={"0.25rem"}
                justifyContent={"center"}
                alignItems={"center"}
            >
                {PositiveIconButton({ children: primaryButtonNode, onClick: primaryOnClick })}
                {secondaryOnClick && DeleteButton(secondaryOnClick)}
            </Stack>
        </Stack>
    )
}

export type MiraManifest = {
    robots: MirabufRemoteInfo[]
    fields: MirabufRemoteInfo[]
}

function getCacheInfo(miraType: MiraType): MirabufCacheInfo[] {
    return Object.values(
        canOPFS
            ? MirabufCachingService.getCacheMap(miraType)
            : miraType === MiraType.ROBOT
              ? backUpRobots
              : backUpFields
    )
}

export function spawnCachedMira(info: MirabufCacheInfo, type: MiraType, progressHandle?: ProgressHandle) {
    // If spawning a field, then remove all other fields
    if (type === MiraType.FIELD) {
        World.sceneRenderer.removeAllFields()
    }

    if (!progressHandle) {
        progressHandle = new ProgressHandle(info.name ?? info.cacheKey)
    }

    World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_SPAWNING)
    MirabufCachingService.get(info.id, type)
        .then(assembly => {
            if (assembly) {
                createMirabuf(assembly, progressHandle, info.id).then(mirabufSceneObject => {
                    if (mirabufSceneObject) {
                        World.sceneRenderer.registerSceneObject(mirabufSceneObject)
                        progressHandle.done()

                        if (mirabufSceneObject.miraType == MiraType.ROBOT) {
                            globalOpenPanel(InitialConfigPanel, undefined)
                        }
                    } else {
                        progressHandle.fail()
                    }
                })

                if (!info.name) MirabufCachingService.cacheInfo(info.cacheKey, type, assembly.info?.name ?? undefined)
            } else {
                progressHandle.fail()
                console.error("Failed to spawn robot")
            }
        })
        .catch(() => progressHandle.fail())
        .finally(() => {
            setTimeout(() => World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_SPAWNING), 500)
        })
}

interface ImportMirabufPanelCustomProps {
    configurationType: ConfigurationType
}

const ImportMirabufPanel: React.FC<PanelImplProps<void, ImportMirabufPanelCustomProps>> = ({ panel, parent }) => {
    const { addToast, closePanel, openModal, configureScreen } = useUIContext()
    const { unconfirmedImport } = useStateContext()

    const { configurationType } = panel!.props.custom

    const [cachedRobots, setCachedRobots] = useState(getCacheInfo(MiraType.ROBOT))
    const [cachedFields, setCachedFields] = useState(getCacheInfo(MiraType.FIELD))

    const [manifest, setManifest] = useState<MiraManifest | undefined>()
    const [viewType, setViewType] = useState<MiraType>(MiraType.ROBOT)

    const [filesStatus, setFilesStatus] = useState<TaskStatus>({
        isDone: false,
        message: "Waiting on APS...",
        progress: 0,
    })
    const [files, setFiles] = useState<Data[] | undefined>(undefined)

    useEffect(() => {
        configureScreen(panel!, { title: "Spawn Asset", hideAccept: true, cancelText: "Back" }, {})
    }, [])

    useEffect(() => {
        const updateFilesStatus = (e: Event) => {
            setFilesStatus((e as MirabufFilesStatusUpdateEvent).status)
        }

        const updateFiles = (e: Event) => {
            setFiles((e as MirabufFilesUpdateEvent).data)
        }

        window.addEventListener(MirabufFilesStatusUpdateEvent.EVENT_KEY, updateFilesStatus)
        window.addEventListener(MirabufFilesUpdateEvent.EVENT_KEY, updateFiles)

        return () => {
            window.removeEventListener(MirabufFilesStatusUpdateEvent.EVENT_KEY, updateFilesStatus)
            window.removeEventListener(MirabufFilesUpdateEvent.EVENT_KEY, updateFiles)
        }
    })

    useEffect(() => {
        if (!hasMirabufFiles()) {
            requestMirabufFiles()
        } else {
            setFiles(getMirabufFiles())
        }
    }, [])

    // biome-ignore lint: things break if we don't add the closePanel dep
    useLayoutEffect(() => {
        if (unconfirmedImport) {
            addToast("warning", "You're already importing a model!", "Confirm that one before importing another.")
            closePanel(panel!.id, CloseType.Cancel)
            return
        }

        if (parent) closePanel(parent.id, CloseType.Cancel)
    }, [])

    // Get Default Mirabuf Data, Load into manifest.
    useEffect(() => {
        // To remove the prettier warning
        const x = async () => {
            // Detect if we're running in electron and use direct remote URL
            const isElectron = window.electronAPI != null
            const baseUrl = isElectron ? "https://synthesis.autodesk.com" : ""

            fetch(`${baseUrl}/api/mira/manifest.json`)
                .then(x => x.json())
                .then(x => {
                    const map = MirabufCachingService.getCacheMap(MiraType.ROBOT)
                    const robots: MirabufRemoteInfo[] = []
                    for (const src of x["robots"]) {
                        if (typeof src == "string") {
                            const str = `${baseUrl}/api/mira/robots/${src}`
                            if (!map[str]) robots.push({ displayName: src, src: str })
                        } else {
                            if (!map[src.src]) robots.push({ displayName: src.displayName, src: src.src })
                        }
                    }
                    const fields: MirabufRemoteInfo[] = []
                    for (const src of x["fields"]) {
                        if (typeof src == "string") {
                            const str = `${baseUrl}/api/mira/fields/${src}`
                            if (!map[str]) fields.push({ displayName: src, src: str })
                        } else {
                            if (!map[src.src]) fields.push({ displayName: src.displayName, src: src.src })
                        }
                    }
                    setManifest({
                        robots,
                        fields,
                    })
                })
                .catch(error => {
                    console.error("Failed to fetch manifest:", error)
                })
        }
        x()
    }, [])

    // Select a mirabuf assembly from the cache.
    const selectCache = useCallback(
        (info: MirabufCacheInfo, type: MiraType) => {
            spawnCachedMira(info, type)

            if (panel) closePanel(panel.id, CloseType.Cancel)
        },
        [closePanel, panel]
    )

    // Cache a selected remote mirabuf assembly, load from cache.
    const selectRemote = useCallback(
        (info: MirabufRemoteInfo, type: MiraType) => {
            const status = new ProgressHandle(info.displayName)
            status.update("Downloading from Synthesis...", 0.05)

            MirabufCachingService.cacheRemote(info.src, type, info.displayName)
                .then(cacheInfo => {
                    if (cacheInfo) {
                        spawnCachedMira(cacheInfo, type, status)
                    } else {
                        status.fail("Failed to cache")
                    }
                })
                .catch(() => status.fail())

            if (panel) closePanel(panel.id, CloseType.Cancel)
        },
        [closePanel, panel]
    )

    // Cache a selected remote mirabuf assembly, without load.
    const cacheRemoteOnly = useCallback((info: MirabufRemoteInfo, type: MiraType) => {
        const status = new ProgressHandle(info.displayName)
        status.update("Downloading from Synthesis...", 0.05)

        MirabufCachingService.cacheRemote(info.src, type, info.displayName)
            .then(cacheInfo => {
                if (cacheInfo) {
                    status.done()
                } else {
                    status.fail("Failed to cache")
                }
            })
            .catch(() => status.fail())
    }, [])

    const selectAPS = useCallback(
        (data: Data, type: MiraType) => {
            const status = new ProgressHandle(data.attributes.displayName ?? data.id)
            status.update("Downloading from APS...", 0.05)

            MirabufCachingService.cacheAPS(data, type)
                .then(cacheInfo => {
                    if (cacheInfo) {
                        spawnCachedMira(cacheInfo, type, status)
                    } else {
                        status.fail("Failed to cache")
                    }
                })
                .catch(() => status.fail())

            if (panel) closePanel(panel.id, CloseType.Cancel)
        },
        [closePanel, panel]
    )

    // Generate Item cards for cached robots.
    const cachedRobotElements = useMemo(
        () =>
            cachedRobots
                .sort((a, b) => a.name?.localeCompare(b.name ?? "") ?? -1)
                .map(info =>
                    ItemCard({
                        name: info.name || info.cacheKey || "Unnamed Robot",
                        id: info.id,
                        primaryButtonNode: SynthesisIcons.ADD_LARGE,
                        primaryOnClick: () => {
                            console.log(`Selecting cached robot: ${info.cacheKey}`)
                            selectCache(info, MiraType.ROBOT)
                        },
                        secondaryOnClick: () => {
                            console.log(`Deleting cache of: ${info.cacheKey}`)
                            MirabufCachingService.remove(info.cacheKey, info.id, MiraType.ROBOT)

                            setCachedRobots(getCacheInfo(MiraType.ROBOT))
                        },
                    })
                ),
        [cachedRobots, selectCache]
    )

    // Generate Item cards for cached fields.
    const cachedFieldElements = useMemo(
        () =>
            cachedFields
                .sort((a, b) => a.name?.localeCompare(b.name ?? "") ?? -1)
                .map(info =>
                    ItemCard({
                        name: info.name || info.cacheKey || "Unnamed Field",
                        id: info.id,
                        primaryButtonNode: SynthesisIcons.ADD_LARGE,
                        primaryOnClick: () => {
                            console.log(`Selecting cached field: ${info.cacheKey}`)
                            selectCache(info, MiraType.FIELD)
                        },
                        secondaryOnClick: () => {
                            console.log(`Deleting cache of: ${info.cacheKey}`)
                            MirabufCachingService.remove(info.cacheKey, info.id, MiraType.FIELD)

                            setCachedFields(getCacheInfo(MiraType.FIELD))
                        },
                    })
                ),
        [cachedFields, selectCache]
    )

    // Generate Item cards for remote robots.
    const remoteRobotElements = useMemo(() => {
        const remoteRobots = manifest?.robots.filter(
            path => !cachedRobots.some(info => info.cacheKey.includes(path.src))
        )
        return remoteRobots
            ?.sort((a, b) => a.displayName.localeCompare(b.displayName))
            .map(path =>
                ItemCard({
                    name: path.displayName,
                    id: path.src,
                    primaryButtonNode: SynthesisIcons.DOWNLOAD_LARGE,
                    primaryOnClick: () => {
                        console.log(`Selecting remote: ${path}`)
                        selectRemote(path, MiraType.ROBOT)
                    },
                })
            )
    }, [manifest?.robots, cachedRobots, selectRemote])

    // Generate Item cards for remote fields.
    const remoteFieldElements = useMemo(() => {
        const remoteFields = manifest?.fields.filter(
            path => !cachedFields.some(info => info.cacheKey.includes(path.src))
        )
        return remoteFields
            ?.sort((a, b) => a.displayName.localeCompare(b.displayName))
            .map(path =>
                ItemCard({
                    name: path.displayName,
                    id: path.src,
                    primaryButtonNode: SynthesisIcons.DOWNLOAD_LARGE,
                    primaryOnClick: () => {
                        console.log(`Selecting remote: ${path}`)
                        selectRemote(path, MiraType.FIELD)
                    },
                })
            )
    }, [manifest?.fields, cachedFields, selectRemote])

    function downloadAllRemote(cached: MirabufCacheInfo[]): () => void {
        // biome-ignore lint: Returning a callback is fine to avoid repeating ourselves
        return useCallback(() => {
            const miraType: MiraType | undefined = cached[0]?.miraType
            const property = miraType === MiraType.ROBOT ? "robots" : "fields"
            const remotes = manifest ? manifest[property] : []

            remotes
                .filter(path => !cached.some(info => info.cacheKey.includes(path.src)))
                .forEach(path => cacheRemoteOnly(path, miraType))

            if (panel) closePanel(panel.id, CloseType.Cancel)
        }, [manifest, cached, cacheRemoteOnly, closePanel, panel])
    }

    const downloadAllRemoteRobots = downloadAllRemote(cachedRobots)
    const downloadAllRemoteFields = downloadAllRemote(cachedFields)

    // Generate Item cards for APS robots and fields.
    const hubElements = useMemo(
        () =>
            files
                ?.sort((a, b) => a.attributes.displayName!.localeCompare(b.attributes.displayName!))
                .map(file =>
                    ItemCard({
                        name: `${file.attributes.displayName!.replace(".mira", "")}${file.attributes.versionNumber !== undefined ? ` (v${file.attributes.versionNumber})` : ""}`,
                        id: file.id,
                        primaryButtonNode: SynthesisIcons.DOWNLOAD_LARGE,
                        primaryOnClick: () => {
                            console.debug(file.raw)
                            selectAPS(file, viewType)
                        },
                    })
                ),
        [files, selectAPS, viewType]
    )
    useEffect(() => {
        setViewType(configurationType === "ROBOTS" ? MiraType.ROBOT : MiraType.FIELD)
    }, [])
    return (
        <Stack direction="column" gap={2} className="overflow-y-auto">
            <ToggleButtonGroup
                value={viewType}
                exclusive
                onChange={(_, v) => {
                    if (v != null) {
                        setViewType(v)
                    }
                }}
                sx={{
                    alignSelf: "center",
                }}
            >
                <ToggleButton value={MiraType.ROBOT}>Robots</ToggleButton>
                <ToggleButton value={MiraType.FIELD}>Fields</ToggleButton>
            </ToggleButtonGroup>
            <Accordion defaultExpanded>
                <AccordionSummary expandIcon={<MdExpandMore size={24} />}>
                    {viewType === MiraType.ROBOT ? (
                        <Label size="md" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {cachedRobotElements
                                ? `${cachedRobotElements.length} Saved Robot${cachedRobotElements.length === 1 ? "" : "s"}`
                                : "Loading Saved Robots"}
                        </Label>
                    ) : (
                        <Label size="md" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {cachedFieldElements
                                ? `${cachedFieldElements.length} Saved Field${cachedFieldElements.length == 1 ? "" : "s"}`
                                : "Loading Saved Fields"}
                        </Label>
                    )}
                </AccordionSummary>
                <AccordionDetails>
                    {viewType === MiraType.ROBOT ? cachedRobotElements : cachedFieldElements}
                </AccordionDetails>
            </Accordion>
            <Accordion>
                <AccordionSummary expandIcon={<MdExpandMore size={24} />}>
                    <Stack
                        direction="row"
                        key={`remote-label-container`}
                        gap={"0.25rem"}
                        justifyContent={"center"}
                        alignItems={"center"}
                    >
                        <Label size="md" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {hubElements ? (
                                `${hubElements.length} Remote Asset${hubElements.length === 1 ? "" : "s"}`
                            ) : (
                                <Tooltip title={filesStatus.message}>
                                    <Stack direction="row" gap={1}>
                                        <Label size="md">Loading from APS...</Label>
                                        <CircularProgress
                                            variant="determinate"
                                            value={filesStatus.isDone ? 100 : filesStatus.progress * 100}
                                        />
                                    </Stack>
                                </Tooltip>
                            )}
                        </Label>
                        {hubElements && filesStatus.isDone && RefreshButton(() => requestMirabufFiles())}
                    </Stack>
                </AccordionSummary>
                <AccordionDetails>{hubElements}</AccordionDetails>
            </Accordion>
            <Accordion>
                <AccordionSummary expandIcon={<MdExpandMore size={24} />}>
                    {viewType === MiraType.ROBOT ? (
                        <Label size="md" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {remoteRobotElements
                                ? `${remoteRobotElements.length} Default Robot${remoteRobotElements.length === 1 ? "" : "s"}`
                                : "Loading Default Robots"}
                        </Label>
                    ) : (
                        <Label size="md" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {remoteFieldElements
                                ? `${remoteFieldElements.length} Default Field${remoteFieldElements.length === 1 ? "" : "s"}`
                                : "Loading Default Fields"}
                        </Label>
                    )}
                </AccordionSummary>
                <AccordionDetails>
                    {viewType === MiraType.ROBOT ? remoteRobotElements : remoteFieldElements}
                    <Stack justifyContent="center" mt={1}>
                        <PositiveButton
                            onClick={viewType === MiraType.ROBOT ? downloadAllRemoteRobots : downloadAllRemoteFields}
                        >
                            Download All
                        </PositiveButton>
                    </Stack>
                </AccordionDetails>
            </Accordion>
            <Box alignSelf={"center"}>
                <Button
                    onClick={() => {
                        openModal(ImportLocalMirabufModal, undefined)
                        closePanel(panel!.id, CloseType.Overwrite)
                    }}
                >
                    Import from File
                </Button>
            </Box>
        </Stack>
    )
}

export default ImportMirabufPanel
