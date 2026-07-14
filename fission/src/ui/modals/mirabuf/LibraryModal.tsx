import { Box, CircularProgress, Stack, Tab, Tabs, Tooltip } from "@mui/material"
import type React from "react"
import { type ReactNode, useCallback, useEffect, useLayoutEffect, useMemo, useState } from "react"
import { type Data, getMirabufFiles, hasMirabufFiles, requestMirabufFiles } from "@/aps/APSDataManagement"
import DefaultAssetLoader, { type DefaultAssetInfo } from "@/mirabuf/DefaultAssetLoader.ts"
import MirabufCachingService, { type MirabufCacheInfo, MiraType } from "@/mirabuf/MirabufLoader"
import EventSystem from "@/systems/EventSystem.ts"
import { SoundPlayer } from "@/systems/sound/SoundPlayer.ts"
import CommandRegistry from "@/ui/components/CommandRegistry"
import { globalOpenModal } from "@/ui/components/GlobalUIControls"
import Label from "@/ui/components/Label"
import type { ModalImplProps } from "@/ui/components/Modal"
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
    ToggleButton,
    ToggleButtonGroup,
} from "@/ui/components/StyledComponents"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { useTourAnchor } from "@/ui/tour/useTourAnchor"
import ImportLocalMirabufModal from "@/ui/modals/mirabuf/ImportLocalMirabufModal"
import type { ConfigurationType } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import type TaskStatus from "@/util/TaskStatus"
import { downloadAll, spawnAPS, spawnCachedMira, spawnRemote } from "./librarySpawnActions"

const OTHER_YEAR = "Other" as const
type YearKey = number | typeof OTHER_YEAR

const yearOf = (asset: { year?: number }): YearKey => asset.year ?? OTHER_YEAR

interface AssetCardProps {
    name: string
    thumbnail?: string
    miraType: MiraType
    cached: boolean
    onSpawn: () => void
    onDelete?: () => void
}

/** A single robot/field tile: thumbnail (or placeholder), name, spawn button, optional delete. */
const AssetCard: React.FC<AssetCardProps> = ({ name, thumbnail, miraType, cached, onSpawn, onDelete }) => {
    const [thumbFailed, setThumbFailed] = useState(false)
    const showThumb = thumbnail && !thumbFailed
    const PlaceholderIcon = miraType === MiraType.FIELD ? SynthesisIcons.CHESS_BOARD : SynthesisIcons.CAR

    return (
        <Stack
            direction="column"
            gap={0.75}
            sx={{
                p: 1,
                borderRadius: "0.75rem",
                backgroundColor: "#4a4a4a",
                minWidth: 0,
            }}
        >
            <Box
                sx={{
                    position: "relative",
                    aspectRatio: "4 / 3",
                    borderRadius: "0.5rem",
                    overflow: "hidden",
                    backgroundColor: "#2e2e2e",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                }}
            >
                {showThumb ? (
                    <img
                        src={thumbnail}
                        alt={name}
                        onError={() => setThumbFailed(true)}
                        className="w-full h-full object-cover"
                    />
                ) : (
                    <Box sx={{ opacity: 0.4, fontSize: "2.5rem", display: "flex" }}>
                        <PlaceholderIcon />
                    </Box>
                )}
                {cached && (
                    <Box
                        sx={{
                            position: "absolute",
                            top: 4,
                            right: 4,
                            width: 20,
                            height: 20,
                            borderRadius: "50%",
                            backgroundColor: "success.main",
                            color: "#fff",
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "center",
                            fontSize: "0.7rem",
                        }}
                    >
                        <SynthesisIcons.CHECK />
                    </Box>
                )}
            </Box>
            <Stack direction="row" justifyContent="space-between" alignItems="center" gap={0.5} sx={{ minWidth: 0 }}>
                <Label size="sm" className="text-wrap break-words min-w-0">
                    {name.replace(/\.mira$/, "")}
                </Label>
                <Stack direction="row-reverse" gap={0.25} alignItems="center">
                    <PositiveIconButton onClick={onSpawn}>
                        {cached ? <SynthesisIcons.ADD_LARGE /> : <SynthesisIcons.DOWNLOAD_LARGE />}
                    </PositiveIconButton>
                    {cached && onDelete && <DeleteButton onClick={onDelete} />}
                </Stack>
            </Stack>
        </Stack>
    )
}

/** Responsive card grid shared by the year view and the Saved section. */
const AssetCardGrid: React.FC<{ children: ReactNode }> = ({ children }) => (
    <Box
        sx={{
            display: "grid",
            gridTemplateColumns: { xs: "repeat(2, 1fr)", sm: "repeat(3, 1fr)", lg: "repeat(4, 1fr)" },
            gap: 2,
        }}
    >
        {children}
    </Box>
)

const LibraryModal: React.FC<ModalImplProps<void, void>> = ({ modal }) => {
    const { addToast, closeModal, openModal, configureScreen } = useUIContext()
    const { unconfirmedImport } = useStateContext()
    const libraryRef = useTourAnchor("spawn-panel")

    // Default library (remote manifest). DefaultAssetLoader loads async ~1s after boot,
    // so snapshot into state and refresh if it wasn't ready yet.
    const [manifestRobots, setManifestRobots] = useState<DefaultAssetInfo[]>(DefaultAssetLoader.robots)
    const [manifestFields, setManifestFields] = useState<DefaultAssetInfo[]>(DefaultAssetLoader.fields)
    useEffect(() => {
        if (DefaultAssetLoader.robots.length === 0 && DefaultAssetLoader.fields.length === 0) {
            DefaultAssetLoader.refresh()
                .then(() => {
                    setManifestRobots(DefaultAssetLoader.robots)
                    setManifestFields(DefaultAssetLoader.fields)
                })
                .catch(console.error)
        }
    }, [])

    // Cached assets (drives the "downloaded" state on library cards + the Saved section).
    const [cachedInfos, setCachedInfos] = useState<MirabufCacheInfo[]>(() => MirabufCachingService.getAll())
    const refreshCached = useCallback(() => setCachedInfos(MirabufCachingService.getAll()), [])
    const cachedByHash = useMemo(() => new Map(cachedInfos.map(c => [c.hash, c])), [cachedInfos])

    // APS (Autodesk Hub) files.
    const [apsType, setApsType] = useState<MiraType>(MiraType.ROBOT)
    const [filesStatus, setFilesStatus] = useState<TaskStatus>({
        isDone: false,
        message: "Waiting on APS...",
        progress: 0,
    })
    const [files, setFiles] = useState<Data[] | undefined>(undefined)

    // Merge robots + fields, deduped by hash (the manifest can contain multiple entries
    // that resolve to the same content, which would otherwise collide as React keys).
    const manifestAssets = useMemo(() => {
        const seen = new Set<string>()
        return [...manifestRobots, ...manifestFields].filter(asset => {
            if (seen.has(asset.hash)) return false
            seen.add(asset.hash)
            return true
        })
    }, [manifestRobots, manifestFields])

    // Year tabs: union of manifest years, descending, with an "Other" tab pinned last.
    const years = useMemo<YearKey[]>(() => {
        const set = new Set<YearKey>()
        for (const asset of manifestAssets) set.add(yearOf(asset))
        const numeric = [...set].filter((y): y is number => typeof y === "number").sort((a, b) => b - a)
        return set.has(OTHER_YEAR) ? [...numeric, OTHER_YEAR] : numeric
    }, [manifestAssets])

    const [activeYear, setActiveYear] = useState<YearKey | undefined>(undefined)
    useEffect(() => {
        if (activeYear === undefined && years.length > 0) {
            setActiveYear(years[0])
        }
    }, [years, activeYear])

    // Robots + field(s) for the selected year, robots first.
    const assetsForYear = useMemo(
        () => (activeYear === undefined ? [] : manifestAssets.filter(asset => yearOf(asset) === activeYear)),
        [manifestAssets, activeYear]
    )

    // Cached assets not present in the default library (imports / APS downloads).
    const manifestHashes = useMemo(() => new Set(manifestAssets.map(a => a.hash)), [manifestAssets])
    const savedExtra = useMemo(
        () => cachedInfos.filter(c => !manifestHashes.has(c.hash)),
        [cachedInfos, manifestHashes]
    )

    useEffect(() => {
        configureScreen(modal!, { title: "Library", hideAccept: true, cancelText: "Close", allowClickAway: true }, {})
    }, [])

    useEffect(() => {
        const unsubscribeStatus = EventSystem.listen("MirabufFilesStatusUpdateEvent", v => setFilesStatus(v))
        const unsubscribeUpdate = EventSystem.listen("MirabufFilesUpdateEvent", v => setFiles(v))
        return () => {
            unsubscribeStatus()
            unsubscribeUpdate()
        }
    }, [])

    useEffect(() => {
        if (!hasMirabufFiles()) {
            requestMirabufFiles().catch(console.error)
        } else {
            setFiles(getMirabufFiles())
        }
    }, [])

    // biome-ignore lint: must run only on mount; the closeModal dep would re-run it and re-close
    useLayoutEffect(() => {
        if (unconfirmedImport) {
            addToast("warning", "You're already importing a model!", "Confirm that one before importing another.")
            closeModal(CloseType.Cancel)
        }
    }, [])

    const spawnLibraryAsset = useCallback(
        (asset: DefaultAssetInfo) => {
            const cached = cachedByHash.get(asset.hash)
            if (cached) {
                spawnCachedMira(cached).catch(console.error)
            } else {
                spawnRemote(asset)
            }
            closeModal(CloseType.Cancel)
        },
        [cachedByHash, closeModal]
    )

    const spawnSaved = useCallback(
        (info: MirabufCacheInfo) => {
            spawnCachedMira(info).catch(console.error)
            closeModal(CloseType.Cancel)
        },
        [closeModal]
    )

    const deleteCached = useCallback(
        async (hash: string) => {
            await MirabufCachingService.remove(hash)
            refreshCached()
        },
        [refreshCached]
    )

    const spawnAPSFile = useCallback(
        (file: Data) => {
            spawnAPS(file, apsType)
            closeModal(CloseType.Cancel)
        },
        [apsType, closeModal]
    )

    const downloadAllForYear = useCallback(() => {
        downloadAll(assetsForYear, cachedInfos).catch(console.error)
        closeModal(CloseType.Cancel)
    }, [assetsForYear, cachedInfos, closeModal])

    const importFromFile = useCallback(() => {
        // openModal auto-closes this Library modal (fires its onClose(Overwrite)).
        openModal(ImportLocalMirabufModal, { configurationType: "ROBOTS" as ConfigurationType })
    }, [openModal])

    const hasRemoteInYear = assetsForYear.some(asset => !cachedByHash.has(asset.hash))

    return (
        <Stack
            direction="column"
            ref={libraryRef}
            sx={{
                width: { xs: "88vw", lg: "min(90vw, 1120px)" },
                height: { xs: "76vh", lg: "70vh" },
            }}
        >
            <Tabs
                value={activeYear ?? false}
                onChange={(_, newValue) => setActiveYear(newValue)}
                textColor="inherit"
                indicatorColor="primary"
                variant="scrollable"
                scrollButtons="auto"
                allowScrollButtonsMobile
                {...SoundPlayer.getInstance().buttonSoundEffects()}
            >
                {years.map(year => (
                    <Tab key={String(year)} value={year} label={String(year)} />
                ))}
            </Tabs>

            <Box sx={{ flex: 1, minHeight: 0, overflowY: "auto", pt: 2 }}>
                {activeYear === undefined ? (
                    <Label size="sm">Loading Library...</Label>
                ) : assetsForYear.length > 0 ? (
                    <AssetCardGrid>
                        {assetsForYear.map(asset => (
                            <AssetCard
                                key={asset.hash}
                                name={asset.name}
                                thumbnail={asset.thumbnail}
                                miraType={asset.miraType}
                                cached={cachedByHash.has(asset.hash)}
                                onSpawn={() => spawnLibraryAsset(asset)}
                                onDelete={cachedByHash.has(asset.hash) ? () => deleteCached(asset.hash) : undefined}
                            />
                        ))}
                    </AssetCardGrid>
                ) : (
                    <Label size="sm">No Assets Found</Label>
                )}

                {hasRemoteInYear && (
                    <Stack alignItems="center" mt={2}>
                        <PositiveButton onClick={downloadAllForYear}>Download All</PositiveButton>
                    </Stack>
                )}

                <Stack direction="column" gap={1} mt={2}>
                    <Accordion>
                        <AccordionSummary expandIcon={<SynthesisIcons.EXPAND_MORE_LARGE />}>
                            <Label size="md">
                                {`${savedExtra.length} Saved Asset${savedExtra.length === 1 ? "" : "s"} (not in Library)`}
                            </Label>
                        </AccordionSummary>
                        <AccordionDetails>
                            {savedExtra.length > 0 ? (
                                <AssetCardGrid>
                                    {savedExtra.map(info => (
                                        <AssetCard
                                            key={info.hash}
                                            name={info.name || "Unnamed"}
                                            thumbnail={info.thumbnail}
                                            miraType={info.miraType}
                                            cached
                                            onSpawn={() => spawnSaved(info)}
                                            onDelete={() => deleteCached(info.hash)}
                                        />
                                    ))}
                                </AssetCardGrid>
                            ) : (
                                <Label size="sm">No Saved Assets</Label>
                            )}
                        </AccordionDetails>
                    </Accordion>

                    <Accordion>
                        <AccordionSummary expandIcon={<SynthesisIcons.EXPAND_MORE_LARGE />}>
                            <Stack direction="row" gap={0.5} justifyContent="center" alignItems="center">
                                <Label size="md">
                                    {files ? (
                                        `${files.length} Autodesk Hub Asset${files.length === 1 ? "" : "s"}`
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
                                {files && <RefreshButton onClick={() => requestMirabufFiles()} />}
                            </Stack>
                        </AccordionSummary>
                        <AccordionDetails>
                            <Stack direction="column" gap={1}>
                                <ToggleButtonGroup
                                    value={apsType}
                                    exclusive
                                    onChange={(_, v) => v != null && setApsType(v)}
                                    sx={{ alignSelf: "center" }}
                                >
                                    <ToggleButton value={MiraType.ROBOT}>Robot</ToggleButton>
                                    <ToggleButton value={MiraType.FIELD}>Field</ToggleButton>
                                </ToggleButtonGroup>
                                {files && files.length > 0 ? (
                                    files
                                        .slice()
                                        .sort((a, b) =>
                                            a.attributes.displayName!.localeCompare(b.attributes.displayName!)
                                        )
                                        .map(file => (
                                            <Stack
                                                key={file.id}
                                                direction="row"
                                                justifyContent="space-between"
                                                alignItems="center"
                                                gap={1}
                                            >
                                                <Label size="sm" className="text-wrap break-all">
                                                    {`${file.attributes.displayName!.replace(".mira", "")}${file.attributes.versionNumber !== undefined ? ` (v${file.attributes.versionNumber})` : ""}`}
                                                </Label>
                                                <PositiveIconButton
                                                    onClick={() => spawnAPSFile(file)}
                                                    children={<SynthesisIcons.DOWNLOAD_LARGE />}
                                                />
                                            </Stack>
                                        ))
                                ) : filesStatus.isDone ? (
                                    <Label size="sm">No Assets Found</Label>
                                ) : (
                                    <Label size="sm">Loading from APS...</Label>
                                )}
                            </Stack>
                        </AccordionDetails>
                    </Accordion>
                </Stack>

                <Stack alignItems="center" mt={2}>
                    <Button onClick={importFromFile}>Import from File</Button>
                </Stack>
            </Box>
        </Stack>
    )
}

export default LibraryModal

// Command palette entry (module side effect). Registered after definition so the
// closure references the fully-initialized component.
CommandRegistry.get().registerCommands([
    {
        id: "spawn-asset",
        label: "Spawn Asset",
        description: "Open the asset Library.",
        keywords: ["spawn", "asset", "robot", "field", "import", "mirabuf", "library"],
        perform: () => {
            globalOpenModal(LibraryModal, undefined)
        },
    },
])
