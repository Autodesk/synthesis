import { Box, CircularProgress, Stack, Tab, Tabs, Tooltip } from "@mui/material"
import type React from "react"
import { type ReactNode, useCallback, useEffect, useMemo, useState } from "react"
import { type Data, getMirabufFiles, hasMirabufFiles, requestMirabufFiles } from "@/aps/APSDataManagement"
import DefaultAssetLoader, { type DefaultAssetInfo } from "@/mirabuf/DefaultAssetLoader.ts"
import MirabufCachingService, { type MirabufCacheInfo, MiraType } from "@/mirabuf/MirabufLoader"
import { getCachedThumbnail } from "@/mirabuf/MirabufThumbnail"
import EventSystem from "@/systems/EventSystem.ts"
import { SoundPlayer } from "@/systems/sound/SoundPlayer.ts"
import CommandRegistry from "@/ui/components/CommandRegistry"
import { globalOpenModal } from "@/ui/components/GlobalUIControls"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import Label from "@/ui/components/Label"
import type { ModalImplProps } from "@/ui/components/Modal"
import {
    Accordion,
    AccordionDetails,
    AccordionSummary,
    Button,
    DeleteButton,
    IconButton,
    PositiveButton,
    PositiveIconButton,
    RefreshButton,
    SynthesisIcons,
    ToggleButton,
    ToggleButtonGroup,
} from "@/ui/components/StyledComponents"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { tourTarget } from "@/ui/tour/TourSteps"
import { useTourAnchor } from "@/ui/tour/TourProviderHelpers"
import ImportLocalMirabufModal from "@/ui/modals/mirabuf/ImportLocalMirabufModal"
import type TaskStatus from "@/util/TaskStatus"
import { downloadAll, spawnAPS, spawnCachedMira, spawnRemote } from "./LibrarySpawnActions"

const OTHER_YEAR = "Other" as const
const FAVORITES_YEAR = "Favorites" as const
type YearKey = number | typeof OTHER_YEAR | typeof FAVORITES_YEAR

const yearOf = (asset: { year?: number }): number | typeof OTHER_YEAR => asset.year ?? OTHER_YEAR

interface AssetCardProps {
    name: string
    thumbnail?: string
    embeddedThumbnailHash?: string
    miraType: MiraType
    cached: boolean
    isFavorite: boolean
    onToggleFavorite: () => void
    onSpawn: () => void
    onDelete?: () => void
}

function useEmbeddedThumbnail(hash: string | undefined): string | undefined {
    const [url, setUrl] = useState<string | undefined>(undefined)

    useEffect(() => {
        setUrl(undefined)
        if (!hash) return

        let objectUrl: string | undefined
        let stale = false
        getCachedThumbnail(hash)
            .then(blob => {
                if (stale || !blob) return
                objectUrl = URL.createObjectURL(blob)
                setUrl(objectUrl)
            })
            .catch(console.error)
        return () => {
            stale = true
            if (objectUrl) URL.revokeObjectURL(objectUrl)
        }
    }, [hash])

    return url
}

const AssetCard: React.FC<AssetCardProps> = ({
    name,
    thumbnail,
    embeddedThumbnailHash,
    miraType,
    cached,
    isFavorite,
    onToggleFavorite,
    onSpawn,
    onDelete,
}) => {
    const [failedSrc, setFailedSrc] = useState<string | undefined>(undefined)
    const embedded = useEmbeddedThumbnail(thumbnail ? undefined : embeddedThumbnailHash)
    const thumbnailSrc = thumbnail ?? embedded
    const showThumb = thumbnailSrc && thumbnailSrc !== failedSrc
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
                        src={thumbnailSrc}
                        alt={name}
                        onError={() => setFailedSrc(thumbnailSrc)}
                        className="w-full h-full object-cover"
                    />
                ) : (
                    <Box sx={{ opacity: 0.4, fontSize: "2.5rem", display: "flex" }}>
                        <PlaceholderIcon />
                    </Box>
                )}
                <Tooltip title={isFavorite ? "Remove from favorites" : "Add to favorites"}>
                    <IconButton
                        onClick={onToggleFavorite}
                        size="small"
                        sx={{
                            position: "absolute",
                            top: 2,
                            left: 2,
                            color: isFavorite ? "#f5c518" : "rgba(255, 255, 255, 0.75)",
                            backgroundColor: "rgba(0, 0, 0, 0.35)",
                            "&:hover": { backgroundColor: "rgba(0, 0, 0, 0.55)" },
                        }}
                    >
                        {isFavorite ? <SynthesisIcons.STAR /> : <SynthesisIcons.STAR_OUTLINE />}
                    </IconButton>
                </Tooltip>
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

const AutodeskHubAccordion: React.FC<{ onSpawned: () => void }> = ({ onSpawned }) => {
    const [apsType, setApsType] = useState<MiraType>(MiraType.ROBOT)
    const [filesStatus, setFilesStatus] = useState<TaskStatus>({
        isDone: false,
        message: "Waiting on APS...",
        progress: 0,
    })
    const [files, setFiles] = useState<Data[] | undefined>(undefined)

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

    const sortedFiles = useMemo(
        () => files?.slice().sort((a, b) => a.attributes.displayName!.localeCompare(b.attributes.displayName!)),
        [files]
    )

    const spawnFile = (file: Data) => {
        spawnAPS(file, apsType)
        onSpawned()
    }

    return (
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
                    {sortedFiles && sortedFiles.length > 0 ? (
                        sortedFiles.map(file => (
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
                                <PositiveIconButton onClick={() => spawnFile(file)}>
                                    <SynthesisIcons.DOWNLOAD_LARGE />
                                </PositiveIconButton>
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
    )
}

const LibraryModal: React.FC<ModalImplProps<void, void>> = ({ modal }) => {
    const { closeModal, openModal, configureScreen } = useUIContext()
    const libraryRef = useTourAnchor("spawn-panel")

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

    const [cachedInfos, setCachedInfos] = useState<MirabufCacheInfo[]>(() => MirabufCachingService.getAll())
    const refreshCached = useCallback(() => setCachedInfos(MirabufCachingService.getAll()), [])
    const cachedByHash = useMemo(() => new Map(cachedInfos.map(c => [c.hash, c])), [cachedInfos])

    // merging fields and robots (field -> robot order) & deduping by hash
    const manifestAssets = useMemo(() => {
        const seen = new Set<string>()
        return [...manifestFields, ...manifestRobots].filter(asset => {
            if (seen.has(asset.hash)) return false
            seen.add(asset.hash)
            return true
        })
    }, [manifestRobots, manifestFields])

    // cached assets not in default library
    const manifestHashes = useMemo(() => new Set(manifestAssets.map(a => a.hash)), [manifestAssets])
    const savedExtra = useMemo(
        () => cachedInfos.filter(c => !manifestHashes.has(c.hash)),
        [cachedInfos, manifestHashes]
    )

     const [favoriteStatus, setFavoriteStatus] = useState(() => ({
        ...PreferencesSystem.getUserPreference("AssemblyFavoriteStatus"),
    }))
    const isFavorite = useCallback(
        (hash: string, defaultFavorite = false) => {
            const status = favoriteStatus[hash]
            if (status === "favorited") return true
            if (status === "unfavorited") return false
            return defaultFavorite
        },
        [favoriteStatus]
    )
    const toggleFavorite = useCallback(
        (hash: string, defaultFavorite = false) => {
            PreferencesSystem.setFavoriteAsset(hash, !isFavorite(hash, defaultFavorite), defaultFavorite)
            setFavoriteStatus({ ...PreferencesSystem.getUserPreference("AssemblyFavoriteStatus") })
        },
        [isFavorite]
    )
    const favoriteManifest = useMemo(
        () => manifestAssets.filter(asset => isFavorite(asset.hash, asset.defaultFavorite)),
        [manifestAssets, isFavorite]
    )
    const favoriteSaved = useMemo(() => savedExtra.filter(info => isFavorite(info.hash)), [savedExtra, isFavorite])

    const hasFavorites = favoriteManifest.length > 0 || favoriteSaved.length > 0

    // Favorites is always the first tab, even when empty

    const years = useMemo<YearKey[]>(() => {
        const set = new Set<YearKey>()
        for (const asset of manifestAssets) set.add(yearOf(asset))
        if (savedExtra.length > 0) set.add(OTHER_YEAR)
        const numeric = [...set].filter((y): y is number => typeof y === "number").sort((a, b) => b - a)
        const result: YearKey[] = [FAVORITES_YEAR, ...numeric]
        if (set.has(OTHER_YEAR)) result.push(OTHER_YEAR)
        return result
    }, [manifestAssets, savedExtra])

    const [activeYear, setActiveYear] = useState<YearKey | undefined>(undefined)
    useEffect(() => {
        if (activeYear !== undefined && years.includes(activeYear)) return
        const fallback = years.find(y => y !== FAVORITES_YEAR) ?? FAVORITES_YEAR
        setActiveYear(hasFavorites ? FAVORITES_YEAR : fallback)
    }, [years, activeYear, hasFavorites])


    const showSaved = activeYear === OTHER_YEAR
    const showFavorites = activeYear === FAVORITES_YEAR

    const assetsForYear = useMemo(() => {
        if (activeYear === undefined) return []
        if (showFavorites) return favoriteManifest
        return manifestAssets.filter(asset => yearOf(asset) === activeYear)
    }, [activeYear, showFavorites, favoriteManifest, manifestAssets])

    const savedForTab = showFavorites ? favoriteSaved : showSaved ? savedExtra : []

    const hasAssets = assetsForYear.length > 0 || savedForTab.length > 0

    

    useEffect(() => {
        configureScreen(modal!, { title: "Library", hideAccept: true, cancelText: "Close", allowClickAway: true }, {})
    }, [])

    const spawnLibraryAsset = useCallback(
        (asset: DefaultAssetInfo) => {
            const cached = cachedByHash.get(asset.hash)
            if (cached) {
                spawnCachedMira(cached).catch(console.error)
            } else {
                spawnRemote(asset)
            }
            closeModal(CloseType.CANCEL)
        },
        [cachedByHash, closeModal]
    )

    const spawnSaved = useCallback(
        (info: MirabufCacheInfo) => {
            spawnCachedMira(info).catch(console.error)
            closeModal(CloseType.CANCEL)
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

    const downloadAllForYear = useCallback(() => {
        downloadAll(assetsForYear, cachedInfos).catch(console.error)
        closeModal(CloseType.CANCEL)
    }, [assetsForYear, cachedInfos, closeModal])

    const importFromFile = useCallback(() => {
        // openModal auto-closes this Library modal (fires its onClose(Overwrite)).
        openModal(ImportLocalMirabufModal, { configurationType: "ROBOTS" })
    }, [openModal])

    const hasRemoteInYear = assetsForYear.some(asset => !cachedByHash.has(asset.hash))

    return (
        // tour anchor
        <Stack
            direction="column"
            ref={libraryRef}
            sx={{
                width: { xs: "88vw", lg: "min(90vw, 1120px)" },
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
                // pin the year tabs while the single (modal) scroll container scrolls
                sx={{ position: "sticky", top: 0, zIndex: 2, backgroundColor: "#2e2e2e" }}
                {...SoundPlayer.getInstance().buttonSoundEffects()}
            >
                {years.map(year => (
                    <Tab key={String(year)} value={year} label={String(year)} />
                ))}
            </Tabs>

            <Box sx={{ pt: 2 }}>
                {activeYear === undefined ? (
                    <Label size="sm">Loading Library...</Label>
                ) : hasAssets ? (
                    <AssetCardGrid>
                        {assetsForYear.map(asset => (
                            <AssetCard
                                key={asset.hash}
                                name={asset.name}
                                thumbnail={asset.thumbnail}
                                embeddedThumbnailHash={cachedByHash.has(asset.hash) ? asset.hash : undefined}
                                miraType={asset.miraType}
                                cached={cachedByHash.has(asset.hash)}
                                isFavorite={isFavorite(asset.hash, asset.defaultFavorite)}
                                onToggleFavorite={() => toggleFavorite(asset.hash, asset.defaultFavorite)}
                                onSpawn={() => spawnLibraryAsset(asset)}
                                onDelete={cachedByHash.has(asset.hash) ? () => deleteCached(asset.hash) : undefined}
                            />
                        ))}
                        {savedForTab.map(info => (
                            <AssetCard
                                key={info.hash}
                                name={info.name || "Unnamed"}
                                thumbnail={info.thumbnail}
                                miraType={info.miraType}
                                cached
                                isFavorite={isFavorite(info.hash)}
                                onToggleFavorite={() => toggleFavorite(info.hash)}
                                onSpawn={() => spawnSaved(info)}
                                onDelete={() => deleteCached(info.hash)}
                            />
                        ))}
                    </AssetCardGrid>
                ) : (
                    <Label size="sm">{showFavorites ? "No favorited assets yet!" : "No Assets Found"}</Label>
                )}

                {hasRemoteInYear && (
                    <Stack alignItems="center" mt={2}>
                        <PositiveButton onClick={downloadAllForYear}>Download All</PositiveButton>
                    </Stack>
                )}

                <Stack direction="column" gap={1} mt={2}>
                    <AutodeskHubAccordion onSpawned={() => closeModal(CloseType.CANCEL)} />
                </Stack>

                <Stack alignItems="center" mt={2}>
                    <Button onClick={importFromFile}>Import from File</Button>
                </Stack>
            </Box>
        </Stack>
    )
}

// tagging onboarding target to allow for auto-advancing despite minification
tourTarget(LibraryModal, "LibraryModal")

export default LibraryModal

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
