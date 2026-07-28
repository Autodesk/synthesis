import { Box, CircularProgress, Stack, Tab, Tabs, Tooltip } from "@mui/material"
import type React from "react"
import { type ReactNode, useCallback, useEffect, useMemo, useState } from "react"
import { type Data, getMirabufFiles, hasMirabufFiles, requestMirabufFiles } from "@/aps/APSDataManagement"
import DefaultAssetLoader, { type DefaultAssetInfo } from "@/mirabuf/DefaultAssetLoader.ts"
import MirabufCachingService, { type MirabufCacheInfo, MiraType, spawnCachedMira } from "@/mirabuf/MirabufLoader"
import EventSystem from "@/systems/EventSystem.ts"
import { globalOpenPanel } from "@/ui/components/GlobalUIControls"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import { ProgressHandle } from "@/ui/components/ProgressNotificationData"
import {
    Accordion,
    AccordionDetails,
    AccordionSummary,
    Button,
    DeleteButton,
    PositiveButton,
    PositiveIconButton,
    RefreshButton,
    SynthesisIcons,
} from "@/ui/components/StyledComponents"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import ImportLocalMirabufModal from "@/ui/modals/mirabuf/ImportLocalMirabufModal"
import type TaskStatus from "@/util/TaskStatus"
import {
    configTypeToMiraType,
    type ConfigurationType,
    miraTypeToConfigType,
} from "../configuring/assembly-config/ConfigTypes"
import CommandRegistry from "@/ui/components/CommandRegistry"
import { SoundPlayer } from "@/systems/sound/SoundPlayer.ts"

// Register commands: Open import panel scoped to robots/fields (module-scope side effect)
CommandRegistry.get().registerCommands([
    {
        id: "spawn-asset-robots",
        label: "Spawn Asset (Robots)",
        description: "Open asset spawn panel scoped to robots.",
        keywords: ["spawn", "asset", "robot", "import", "mirabuf"],
        perform: () => {
            globalOpenPanel<void, ImportMirabufPanelCustomProps>(ImportMirabufPanel, { configurationType: "ROBOTS" })
        },
    },
    {
        id: "spawn-asset-fields",
        label: "Spawn Asset (Fields)",
        description: "Open asset spawn panel scoped to fields.",
        keywords: ["spawn", "asset", "field", "import", "mirabuf"],
        perform: () => {
            globalOpenPanel<void, ImportMirabufPanelCustomProps>(ImportMirabufPanel, { configurationType: "FIELDS" })
        },
    },
])

interface ItemCardProps {
    name: string
    primaryButtonNode: ReactNode
    primaryOnClick: () => void
    secondaryOnClick?: () => void
}

const ItemCard: React.FC<ItemCardProps> = ({ name, primaryButtonNode, primaryOnClick, secondaryOnClick }) => {
    return (
        <Stack justifyContent={"space-between"} alignItems={"center"} gap={"1rem"} direction="row">
            <Label size="md" className="text-wrap break-all">
                {name.replace(/.mira$/, "")}
            </Label>
            <Stack direction="row-reverse" gap={"0.25rem"} justifyContent={"center"} alignItems={"center"}>
                <PositiveIconButton children={primaryButtonNode} onClick={primaryOnClick} />
                {secondaryOnClick && <DeleteButton onClick={secondaryOnClick} />}
            </Stack>
        </Stack>
    )
}

interface ImportMirabufPanelCustomProps {
    configurationType: ConfigurationType
}

const ImportMirabufPanel: React.FC<PanelImplProps<void, ImportMirabufPanelCustomProps>> = ({ panel }) => {
    const { closePanel, openModal, configureScreen } = useUIContext()

    const { configurationType } = panel!.props.custom

    const [cachedRobots, setCachedRobots] = useState(MirabufCachingService.getAll(MiraType.ROBOT))
    const [cachedFields, setCachedFields] = useState(MirabufCachingService.getAll(MiraType.FIELD))
    const [cachedPieces, setCachedPieces] = useState(MirabufCachingService.getAll(MiraType.PIECE))

    const manifestRobots = useMemo(() => DefaultAssetLoader.robots, [])
    const manifestFields = useMemo(() => DefaultAssetLoader.fields, [])
    const [viewType, setViewType] = useState<MiraType>(MiraType.ROBOT)

    const [filesStatus, setFilesStatus] = useState<TaskStatus>({
        isDone: false,
        message: "Waiting on APS...",
        progress: 0,
    })
    const [files, setFiles] = useState<Data[] | undefined>(undefined)

    useEffect(() => {
        configureScreen(panel!, { title: "Spawn Asset", hideAccept: true, cancelText: "Back" }, {})
    }, [configureScreen, panel])

    useEffect(() => {
        const unsubscribeStatus = EventSystem.listen("MirabufFilesStatusUpdateEvent", v => setFilesStatus(v))
        const unsubscribeUpdate = EventSystem.listen("MirabufFilesUpdateEvent", v => setFiles(v))

        return () => {
            unsubscribeStatus()
            unsubscribeUpdate()
        }
    })

    useEffect(() => {
        if (!hasMirabufFiles()) {
            requestMirabufFiles().catch(console.error)
        } else {
            setFiles(getMirabufFiles())
        }
    }, [])

    // Select a mirabuf assembly from the cache.
    const selectCache = useCallback(
        async (info: MirabufCacheInfo) => {
            await spawnCachedMira(info)
            if (panel) closePanel(panel.id, CloseType.CANCEL)
        },
        [closePanel, panel]
    )

    // Cache a selected remote mirabuf assembly, load from cache.
    const selectRemote = useCallback(
        (info: DefaultAssetInfo) => {
            const status = new ProgressHandle(info.name)
            status.update("Downloading from Synthesis...", 0.05)

            MirabufCachingService.cacheRemote(info.remotePath, info.miraType, info.name, info.hash)
                .then(async cacheInfo => {
                    if (cacheInfo) {
                        await spawnCachedMira(cacheInfo, status)
                    } else {
                        status.fail("Failed to cache")
                    }
                })
                .catch(e => {
                    console.error(e)
                    status.fail()
                })

            if (panel) closePanel(panel.id, CloseType.CANCEL)
        },
        [closePanel, panel]
    )

    const selectAPS = useCallback(
        (data: Data, type: MiraType) => {
            const status = new ProgressHandle(data.attributes.displayName ?? data.id)
            status.update("Downloading from APS...", 0.05)

            MirabufCachingService.cacheAPS(data, type)
                .then(async cacheInfo => {
                    if (cacheInfo) {
                        await spawnCachedMira(cacheInfo, status)
                    } else {
                        status.fail("Failed to cache")
                    }
                })
                .catch(e => {
                    console.error(e)
                    status.fail()
                })

            if (panel) closePanel(panel.id, CloseType.CANCEL)
        },
        [closePanel, panel]
    )
    const createCachedAssetElements = useCallback(
        (items: MirabufCacheInfo[]) => {
            return items
                .sort((a, b) => a.name?.localeCompare(b.name ?? "") ?? -1)
                .map(info => (
                    <ItemCard
                        name={info.name || "Unnamed"}
                        key={info.hash}
                        primaryButtonNode={<SynthesisIcons.ADD_LARGE />}
                        primaryOnClick={async () => {
                            console.log(`Selecting cached: ${info.name}`)
                            await selectCache(info)
                        }}
                        secondaryOnClick={async () => {
                            console.log(`Deleting cache of: ${info.name}`)
                            await MirabufCachingService.remove(info.hash)
                            if (info.miraType == MiraType.ROBOT) {
                                setCachedRobots(MirabufCachingService.getAll(MiraType.ROBOT))
                            } else {
                                setCachedFields(MirabufCachingService.getAll(MiraType.FIELD))
                            }
                        }}
                    />
                ))
        },
        [selectCache]
    )
    // Generate Item cards for cached robots.
    const cachedRobotElements = useMemo(
        () => createCachedAssetElements(cachedRobots),
        [cachedRobots, createCachedAssetElements]
    )

    // Generate Item cards for cached fields.
    const cachedFieldElements = useMemo(
        () => createCachedAssetElements(cachedFields),
        [cachedFields, createCachedAssetElements]
    )

    const cachedGamePieces = useMemo(
        () =>
            cachedPieces
                .sort((a, b) => a.name?.localeCompare(b.name ?? "") ?? -1)
                .map(info =>
                    ItemCard({
                        name: info.name || "Unnamed Piece",
                        key: info.hash,
                        primaryButtonNode: <SynthesisIcons.ADD_LARGE />,
                        primaryOnClick: async () => {
                            console.log(`Selecting cached game pieces: ${info.name}`)
                            await selectCache(info)
                        },
                        secondaryOnClick: async () => {
                            console.log(`Deleting cache of: ${info.name}`)
                            await MirabufCachingService.remove(info.hash)
                            setCachedPieces(MirabufCachingService.getAll(MiraType.PIECE))
                        },
                    })
                ),
        [cachedPieces, selectCache]
    )

    // Generate Item cards for remote robots.
    const remoteRobotElements = useMemo(() => {
        const remoteRobots = manifestRobots.filter(
            remoteRobot => !cachedRobots.some(info => info.hash == remoteRobot.hash)
        )
        return remoteRobots
            ?.sort((a, b) => a.name.localeCompare(b.name))
            .map(item => (
                <ItemCard
                    name={item.name}
                    key={item.hash}
                    primaryButtonNode={<SynthesisIcons.DOWNLOAD_LARGE />}
                    primaryOnClick={() => {
                        console.log(`Selecting remote: ${item.remotePath}`)
                        selectRemote(item)
                    }}
                />
            ))
    }, [manifestRobots, cachedRobots, selectRemote])

    // Generate Item cards for remote fields.
    const remoteFieldElements = useMemo(() => {
        const remoteFields = manifestFields.filter(
            remoteField => !cachedFields.some(info => info.hash == remoteField.hash)
        )
        return remoteFields
            ?.sort((a, b) => a.name.localeCompare(b.name))
            .map(asset => (
                <ItemCard
                    name={asset.name}
                    key={asset.hash}
                    primaryButtonNode={<SynthesisIcons.DOWNLOAD_LARGE />}
                    primaryOnClick={() => {
                        console.log(`Selecting remote: ${asset.remotePath}`)
                        selectRemote(asset)
                    }}
                />
            ))
    }, [manifestFields, cachedFields, selectRemote])

    const downloadAllRemote = useCallback(
        (manifestAssets: DefaultAssetInfo[], cachedAssets: MirabufCacheInfo[]) => {
            const status = new ProgressHandle("Caching Remote Assets")

            const toCache = manifestAssets.filter(asset => !cachedAssets.some(info => info.hash === asset.hash))

            let completeCount = 0
            const totalCount = toCache.length
            status.update(`Downloading... (0/${totalCount})`, 0.05)

            toCache.forEach(asset => {
                MirabufCachingService.cacheRemote(asset.remotePath, asset.miraType, asset.name, asset.hash)
                    .then(cacheInfo => {
                        if (cacheInfo) {
                            completeCount++
                            if (completeCount == totalCount) {
                                status.done()
                            } else {
                                status.update(
                                    `Downloading... (${completeCount}/${totalCount})`,
                                    completeCount / totalCount
                                )
                            }
                        } else {
                            status.fail("Failed to cache")
                        }
                    })
                    .catch(e => {
                        console.error(e)
                        status.fail()
                    })
            })

            if (panel) closePanel(panel.id, CloseType.CANCEL)
        },
        [closePanel, panel]
    )

    const downloadAllRemoteRobots = useCallback(
        () => downloadAllRemote(manifestRobots, cachedRobots),
        [manifestRobots, cachedRobots, downloadAllRemote]
    )
    const downloadAllRemoteFields = useCallback(
        () => downloadAllRemote(manifestFields, cachedFields),
        [manifestFields, cachedFields, downloadAllRemote]
    )
    const downloadAllRemotePieces = useCallback(
        () => downloadAllRemote([], cachedPieces),
        [cachedPieces, downloadAllRemote]
    )

    // Generate Item cards for APS robots and fields.
    const hubElements = useMemo(
        () =>
            files
                ?.sort((a, b) => a.attributes.displayName!.localeCompare(b.attributes.displayName!))
                .map(file => (
                    <ItemCard
                        name={`${file.attributes.displayName!.replace(".mira", "")}${file.attributes.versionNumber !== undefined ? ` (v${file.attributes.versionNumber})` : ""}`}
                        key={file.id}
                        primaryButtonNode={<SynthesisIcons.DOWNLOAD_LARGE />}
                        primaryOnClick={() => {
                            console.debug(file.raw)
                            selectAPS(file, viewType)
                        }}
                    />
                )),
        [files, selectAPS, viewType]
    )
    useEffect(() => {
        setViewType(configTypeToMiraType(configurationType) ?? MiraType.ROBOT)
    }, [configurationType])
    return (
        <Stack direction="column" gap={2} className="overflow-y-auto">
            <Tabs
                value={viewType}
                onChange={(_, newValue) => setViewType(newValue)}
                textColor="inherit"
                indicatorColor="primary"
                centered
                {...SoundPlayer.getInstance().buttonSoundEffects()}
            >
                <Tab key="robots" value={MiraType.ROBOT} label="ROBOTS" />
                <Tab key="fields" value={MiraType.FIELD} label="FIELDS" />
                <Tab key="pieces" value={MiraType.PIECE} label="PIECES" />
            </Tabs>
            <Accordion defaultExpanded>
                <AccordionSummary expandIcon={<SynthesisIcons.EXPAND_MORE_LARGE />}>
                    {viewType === MiraType.ROBOT ? (
                        <Label size="md" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {cachedRobotElements
                                ? `${cachedRobotElements.length} Saved Robot${cachedRobotElements.length === 1 ? "" : "s"}`
                                : "Loading Saved Robots"}
                        </Label>
                    ) : viewType === MiraType.FIELD ? (
                        <Label size="md" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {cachedFieldElements
                                ? `${cachedFieldElements.length} Saved Field${cachedFieldElements.length == 1 ? "" : "s"}`
                                : "Loading Saved Fields"}
                        </Label>
                    ) : (
                        <Label size="md" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {cachedGamePieces
                                ? `${cachedGamePieces.length} Saved Piece${cachedGamePieces.length == 1 ? "" : "s"}`
                                : "Loading Saved Pieces"}
                        </Label>
                    )}
                </AccordionSummary>
                <AccordionDetails>
                    {viewType === MiraType.ROBOT ? (
                        cachedRobotElements && cachedRobotElements.length > 0 ? (
                            cachedRobotElements
                        ) : (
                            <Label size="sm">No Saved Assets</Label>
                        )
                    ) : viewType === MiraType.FIELD ? (
                        cachedFieldElements && cachedFieldElements.length > 0 ? (
                            cachedFieldElements
                        ) : (
                            <Label size="sm">No Saved Assets</Label>
                        )
                    ) : cachedGamePieces && cachedGamePieces.length > 0 ? (
                        cachedGamePieces
                    ) : (
                        <Label size="sm">No Saved Assets</Label>
                    )}
                </AccordionDetails>
            </Accordion>
            <Accordion>
                <AccordionSummary expandIcon={<SynthesisIcons.EXPAND_MORE_LARGE />}>
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
                                    <Stack direction="row" gap={1} alignItems="center">
                                        <Label size="md">Loading from APS...</Label>
                                        <CircularProgress
                                            size="1em"
                                            variant="determinate"
                                            value={filesStatus.isDone ? 100 : filesStatus.progress * 100}
                                        />
                                    </Stack>
                                </Tooltip>
                            )}
                        </Label>
                        {hubElements && <RefreshButton onClick={() => requestMirabufFiles()} />}
                    </Stack>
                </AccordionSummary>
                <AccordionDetails>
                    {hubElements && hubElements.length > 0 ? (
                        hubElements
                    ) : filesStatus.isDone ? (
                        <Label size="sm">No Assets Found</Label>
                    ) : (
                        <Label size="sm">Loading from APS...</Label>
                    )}
                </AccordionDetails>
            </Accordion>
            <Accordion>
                <AccordionSummary expandIcon={<SynthesisIcons.EXPAND_MORE_LARGE />}>
                    {viewType === MiraType.ROBOT ? (
                        <Label size="md" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {remoteRobotElements
                                ? `${remoteRobotElements.length} Default Robot${remoteRobotElements.length === 1 ? "" : "s"}`
                                : "Loading Default Robots"}
                        </Label>
                    ) : viewType === MiraType.FIELD ? (
                        <Label size="md" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {remoteFieldElements
                                ? `${remoteFieldElements.length} Default Field${remoteFieldElements.length === 1 ? "" : "s"}`
                                : "Loading Default Fields"}
                        </Label>
                    ) : (
                        <Label size="md" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            Default Game Pieces
                        </Label>
                    )}
                </AccordionSummary>
                <AccordionDetails>
                    {viewType === MiraType.ROBOT ? (
                        remoteRobotElements && remoteRobotElements.length > 0 ? (
                            remoteRobotElements
                        ) : (
                            <Label size="sm">No Assets Found</Label>
                        )
                    ) : viewType === MiraType.FIELD ? (
                        remoteFieldElements && remoteFieldElements.length > 0 ? (
                            remoteFieldElements
                        ) : (
                            <Label size="sm">No Assets Found</Label>
                        )
                    ) : (
                        <Label size="sm">No Assets Found</Label>
                    )}
                    <Stack justifyContent="center" mt={1}>
                        <PositiveButton
                            onClick={
                                viewType === MiraType.ROBOT
                                    ? downloadAllRemoteRobots
                                    : viewType === MiraType.FIELD
                                      ? downloadAllRemoteFields
                                      : downloadAllRemotePieces
                            }
                        >
                            Download All
                        </PositiveButton>
                    </Stack>
                </AccordionDetails>
            </Accordion>
            <Box alignSelf={"center"}>
                <Button
                    onClick={() => {
                        openModal(ImportLocalMirabufModal, {
                            configurationType: miraTypeToConfigType(viewType ?? MiraType.ROBOT),
                        })
                        closePanel(panel!.id, CloseType.OVERWRITE)
                    }}
                >
                    Import from File
                </Button>
            </Box>
        </Stack>
    )
}

export default ImportMirabufPanel
