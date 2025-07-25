import React, { ReactNode, useCallback, useEffect, useLayoutEffect, useMemo, useState } from "react"
import { LabelSize } from "@/components/Label"
import {
    Data,
    getMirabufFiles,
    hasMirabufFiles,
    MirabufFilesStatusUpdateEvent,
    MirabufFilesUpdateEvent,
    requestMirabufFiles,
} from "@/aps/APSDataManagement"
import MirabufCachingService, {
    canOPFS,
    MirabufCacheInfo,
    MirabufRemoteInfo,
    MiraType,
    backUpMap,
} from "@/mirabuf/MirabufLoader"
import World from "@/systems/World"
import { useTooltipControlContext } from "@/ui/TooltipContext"
import MirabufSceneObject, { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { Box } from "@mui/material"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import { usePanelControlContext } from "@/ui/helpers/UsePanelManager"
import { useModalControlContext } from "@/ui/helpers/UseModalManager"
import TaskStatus from "@/util/TaskStatus"
import {
    DeleteButton,
    PositiveButton,
    RefreshButton,
    SectionDivider,
    SectionLabel,
    SynthesisIcons,
} from "@/ui/components/StyledComponents"
import { ProgressHandle } from "@/ui/components/ProgressNotificationData"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import Button from "@/ui/components/Button"
import { globalAddToast, globalOpenPanel } from "@/ui/components/GlobalUIControls"
import { PAUSE_REF_ASSEMBLY_SPAWNING } from "@/systems/physics/PhysicsSystem"
import { mirabufPanelState } from "@/panels/mirabuf/MirabufState.tsx"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import { mirabuf } from "@/proto/mirabuf"

interface ItemCardProps {
    id: string
    name: string
    primaryButtonNode: ReactNode
    primaryOnClick: () => void
    secondaryOnClick?: () => void
}

const ItemCard: React.FC<ItemCardProps> = ({ id, name, primaryButtonNode, primaryOnClick, secondaryOnClick }) => {
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
                <PositiveButton value={primaryButtonNode} onClick={primaryOnClick} />
                {secondaryOnClick && DeleteButton(secondaryOnClick)}
            </Box>
        </Box>
    )
}

export type MiraManifest = {
    robots: MirabufRemoteInfo[]
    fields: MirabufRemoteInfo[]
    pieces: MirabufRemoteInfo[]
}

function getCacheInfo(miraType: MiraType): MirabufCacheInfo[] {
    return Object.values(canOPFS ? MirabufCachingService.getCacheMap(miraType) : backUpMap.get(miraType)!)
}

function spawnCachedMira(info: MirabufCacheInfo, type: MiraType, progressHandle?: ProgressHandle) {
    if (type == MiraType.FIELD) {
        World.sceneRenderer.removeAllFields()
        World.sceneRenderer.removeAllGamePieces()
    }

    progressHandle ??= new ProgressHandle(info.name ?? info.cacheKey)

    World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_SPAWNING)
    MirabufCachingService.get(info.id, type)
        .then(assembly => {
            if (assembly) {
                const { mainSceneObject, gamePieces } = createMirabuf(assembly, info.id, type, progressHandle) ?? {}

                if (mainSceneObject) {
                    World.sceneRenderer.registerSceneObject(mainSceneObject)

                    // The point of this code is to prevent the caching of game pieces of the same type
                    // This might actually be the worst code I've ever written
                    // It essentially keeps a list of all the game piece name prefixes as delimited by a few characters I noticed were being used for that purpose
                    // It also tracks which prefixes have already had a game piece of that type cached

                    // There are certainly better ways of checking prefixes, although the correct way of handling this problem would be to traverse the mesh and check that it's identical
                    // However, that solution would be slow and more difficult to implement and this solution will work for now
                    const pieceNames: [string, boolean][] = []
                    gamePieces
                        ?.map(gp => gp.parser.assembly?.info?.name)
                        .filter(name => name != undefined)
                        .forEach(name => {
                            // Which prefixes are checked should be updated whenever someone finds a new one
                            if (name.includes(":")) {
                                pieceNames.push([name.split(":")[0], false])
                            } else if (name.includes(" ")) {
                                pieceNames.push([name.split(" ")[0], false])
                            }
                        })

                    gamePieces?.forEach(async instance => {
                        const assembly = instance.parser.assembly
                        if (
                            pieceNames.some(([name, hasCached], i, arr) => {
                                // If a piece has a prefix and another game piece of this type has been cached, this pieces should not be
                                const hasPrefix = assembly?.info?.name?.includes(name)
                                const noCache = hasPrefix && hasCached
                                // If a piece has the prefix but there hasn't been a cache, there will be so we should mark it as such
                                // This has to be done here, since we loose information about what prefix this game piece has when this predicate is resolved
                                if (hasPrefix && !hasCached) {
                                    arr[i][1] = true
                                }
                                return noCache
                            })
                        ) {
                            const sceneObject = new MirabufSceneObject(instance, assembly.info?.name!, "")
                            World.sceneRenderer.registerSceneObject(sceneObject)
                        } else {
                            const buffer = mirabuf.Assembly.encode(assembly).finish()

                            const cacheInfo = await MirabufCachingService.cacheLocal(buffer, MiraType.PIECE)
                            if (!cacheInfo) return

                            if (!cacheInfo.name) {
                                MirabufCachingService.cacheInfo(
                                    cacheInfo.cacheKey,
                                    MiraType.PIECE,
                                    assembly.info?.name ?? undefined
                                )
                            }
                            const sceneObject = new MirabufSceneObject(instance, assembly.info?.name!, cacheInfo.id)
                            World.sceneRenderer.registerSceneObject(sceneObject)
                        }
                    })
                    progressHandle.done()

                    globalOpenPanel("initial-config")
                } else {
                    progressHandle.fail()
                }

                if (!info.name) MirabufCachingService.cacheInfo(info.cacheKey, type, assembly.info?.name ?? undefined)
            } else {
                progressHandle.fail()
                console.error("Failed to spawn assembly")
            }
        })
        .catch(() => progressHandle.fail())
        .finally(() => {
            setTimeout(() => World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_SPAWNING), 500)
        })
}

const ImportMirabufPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { showTooltip } = useTooltipControlContext()
    const { closePanel } = usePanelControlContext()
    const { openModal } = useModalControlContext()

    const [cachedRobots, setCachedRobots] = useState(getCacheInfo(MiraType.ROBOT))
    const [cachedFields, setCachedFields] = useState(getCacheInfo(MiraType.FIELD))
    const [cachedPieces, setCachedPieces] = useState(getCacheInfo(MiraType.PIECE))

    const [manifest, setManifest] = useState<MiraManifest | undefined>()
    const [viewType, setViewType] = useState<MiraType>(MiraType.ROBOT)

    const [filesStatus, setFilesStatus] = useState<TaskStatus>({ isDone: false, message: "Waiting on APS..." })
    const [files, setFiles] = useState<Data[] | undefined>(undefined)

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
        if (mirabufPanelState.hasUnconfirmedImport) {
            closePanel("import-mirabuf")
            globalAddToast("warning", "You're already importing a model!", "Confirm that one before importing another.")
            return
        }
        closePanel("configure")
    }, [])

    // Get Default Mirabuf Data, Load into manifest.
    useEffect(() => {
        // To remove the prettier warning
        const x = async () => {
            fetch(`/api/mira/manifest.json`)
                .then(x => x.json())
                .then(x => {
                    const map = MirabufCachingService.getCacheMap(MiraType.ROBOT)
                    const robots: MirabufRemoteInfo[] = []
                    for (const src of x["robots"]) {
                        if (typeof src == "string") {
                            const str = `/api/mira/robots/${src}`
                            if (!map[str]) robots.push({ displayName: src, src: str })
                        } else {
                            if (!map[src["src"]]) robots.push({ displayName: src["displayName"], src: src["src"] })
                        }
                    }
                    const fields: MirabufRemoteInfo[] = []
                    for (const src of x["fields"]) {
                        if (typeof src == "string") {
                            const str = `/api/mira/fields/${src}`
                            if (!map[str]) fields.push({ displayName: src, src: str })
                        } else {
                            if (!map[src["src"]]) fields.push({ displayName: src["displayName"], src: src["src"] })
                        }
                    }
                    const pieces: MirabufRemoteInfo[] = []
                    for (const src of x["pieces"] ?? []) {
                        if (typeof src == "string") {
                            const str = `/api/mira/pieces/${src}`
                            if (!map[str]) pieces.push({ displayName: src, src: str })
                        } else {
                            if (!map[src["src"]]) pieces.push({ displayName: src["displayName"], src: src["src"] })
                        }
                    }

                    setManifest({
                        robots,
                        fields,
                        pieces,
                    })
                })
        }
        x()
    }, [])

    // Select a mirabuf assembly from the cache.
    const selectCache = useCallback(
        (info: MirabufCacheInfo, type: MiraType) => {
            spawnCachedMira(info, type)

            showTooltip("controls", [
                { control: "WASD", description: "Drive" },
                { control: "E", description: "Intake" },
                { control: "Q", description: "Dispense" },
            ])

            closePanel(panelId)
        },
        [showTooltip, closePanel, panelId]
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

            closePanel(panelId)
        },
        [closePanel, panelId]
    )

    // Cache a selected remote mirabuf assembly, without load.
    const cacheRemoteOnly = useCallback((info: MirabufRemoteInfo, type: MiraType) => {
        const status = new ProgressHandle(info.displayName)
        status.update("Downloading from Synthesis...", 0.05)

        MirabufCachingService.cacheRemote(info.src, type)
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

            closePanel(panelId)
        },
        [closePanel, panelId]
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

    const cachedGamePieces = useMemo(
        () =>
            cachedPieces
                .sort((a, b) => a.name?.localeCompare(b.name ?? "") ?? -1)
                .map(info =>
                    ItemCard({
                        name: info.name || info.cacheKey || "Unnamed Piece",
                        id: info.id,
                        primaryButtonNode: SynthesisIcons.ADD_LARGE,
                        primaryOnClick: () => {
                            console.log(`Selecting cached game pieces: ${info.cacheKey}`)
                            selectCache(info, MiraType.PIECE)
                        },
                        secondaryOnClick: () => {
                            console.log(`Deleting cache of: ${info.cacheKey}`)
                            MirabufCachingService.remove(info.cacheKey, info.id, MiraType.PIECE)

                            setCachedPieces(getCacheInfo(MiraType.PIECE))
                        },
                    })
                ),
        [cachedPieces, selectCache]
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

    // Generate Item cards for remote game pieces.
    const remoteGamePieces = useMemo(() => {
        const remotePieces = manifest?.pieces.filter(
            path => !cachedPieces.some(info => info.cacheKey.includes(path.src))
        )
        return remotePieces
            ?.sort((a, b) => a.displayName.localeCompare(b.displayName))
            .map(path =>
                ItemCard({
                    name: path.displayName,
                    id: path.src,
                    primaryButtonNode: SynthesisIcons.DOWNLOAD_LARGE,
                    primaryOnClick: () => {
                        console.log(`Selecting remote: ${path.displayName}`)
                        selectRemote(path, MiraType.PIECE)
                    },
                })
            )
    }, [manifest?.pieces, cachedPieces, selectRemote])

    function downloadAllRemote(cached: MirabufCacheInfo[]): () => void {
        // biome-ignore lint: Returning a callback is fine to avoid repeating ourselves
        return useCallback(() => {
            const miraType: MiraType | undefined = cached[0]?.miraType
            const property = miraType === MiraType.ROBOT ? "robots" : miraType === MiraType.FIELD ? "fields" : "pieces"
            const remotes = manifest ? manifest[property] : []

            remotes
                .filter(path => !cached.some(info => info.cacheKey.includes(path.src)))
                .forEach(path => cacheRemoteOnly(path, miraType))

            closePanel(panelId)
        }, [manifest, cached, cacheRemoteOnly, closePanel, panelId])
    }

    const downloadAllRemoteRobots = downloadAllRemote(cachedRobots)
    const downloadAllRemoteFields = downloadAllRemote(cachedFields)
    const downloadAllRemotePieces = downloadAllRemote(cachedPieces)

    // Generate Item cards for APS robots and fields.
    const hubElements = useMemo(
        () =>
            files
                ?.sort((a, b) => a.attributes.displayName!.localeCompare(b.attributes.displayName!))
                .map(file =>
                    ItemCard({
                        name: `${file.attributes.displayName!.replace(".mira", "")}${file.attributes.versionNumber != undefined ? ` (v${file.attributes.versionNumber})` : ""}`,
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
        setViewType(mirabufPanelState.currentMode)
        mirabufPanelState.currentMode = mirabufPanelState.defaultMode
    }, [])
    return (
        <Panel
            name={"Spawn Asset"}
            icon={SynthesisIcons.ADD_LARGE}
            panelId={panelId}
            acceptEnabled={false}
            cancelName="Back"
            openLocation="right"
        >
            <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary rounded-md p-2">
                <ToggleButtonGroup
                    value={viewType}
                    exclusive
                    onChange={(_, v) => {
                        if (v != null) {
                            setViewType(v)
                        }
                    }}
                    {...SoundPlayer.buttonSoundEffects()}
                    sx={{
                        alignSelf: "center",
                    }}
                >
                    <ToggleButton value={MiraType.ROBOT}>Robots</ToggleButton>
                    <ToggleButton value={MiraType.FIELD}>Fields</ToggleButton>
                    <ToggleButton value={MiraType.PIECE}>Game Pieces</ToggleButton>
                </ToggleButtonGroup>
                {viewType == MiraType.ROBOT ? (
                    <>
                        <SectionLabel size={LabelSize.MEDIUM} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {cachedRobotElements
                                ? `${cachedRobotElements.length} Saved Robot${cachedRobotElements.length == 1 ? "" : "s"}`
                                : "Loading Saved Robots"}
                        </SectionLabel>
                        <SectionDivider />
                        {cachedRobotElements}
                    </>
                ) : viewType == MiraType.FIELD ? (
                    <>
                        <SectionLabel size={LabelSize.MEDIUM} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {cachedFieldElements
                                ? `${cachedFieldElements.length} Saved Field${cachedFieldElements.length == 1 ? "" : "s"}`
                                : "Loading Saved Fields"}
                        </SectionLabel>
                        <SectionDivider />
                        {cachedFieldElements}
                    </>
                ) : (
                    <>
                        <SectionLabel size={LabelSize.MEDIUM} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {cachedGamePieces
                                ? `${cachedGamePieces.length} Saved Game Piece${cachedGamePieces.length == 1 ? "" : "s"}`
                                : "Loading Saved Game Pieces"}
                        </SectionLabel>
                        <SectionDivider />
                        {cachedGamePieces}
                    </>
                )}
                <Box
                    component={"div"}
                    display={"flex"}
                    key={`remote-label-container`}
                    flexDirection={"row"}
                    gap={"0.25rem"}
                    justifyContent={"center"}
                    alignItems={"center"}
                >
                    <SectionLabel size={LabelSize.MEDIUM} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                        {hubElements
                            ? `${hubElements.length} Remote Asset${hubElements.length == 1 ? "" : "s"}`
                            : filesStatus.message}
                    </SectionLabel>
                    {hubElements && filesStatus.isDone ? RefreshButton(() => requestMirabufFiles()) : <></>}
                </Box>
                <SectionDivider />
                {hubElements}
                {viewType == MiraType.ROBOT ? (
                    <>
                        <SectionLabel size={LabelSize.MEDIUM} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {remoteRobotElements
                                ? `${remoteRobotElements.length} Default Robot${remoteRobotElements.length == 1 ? "" : "s"}`
                                : "Loading Default Robots"}
                        </SectionLabel>
                        <SectionDivider />
                        {remoteRobotElements}
                        <Box display="flex" justifyContent="center" mt={1}>
                            <PositiveButton value="Download All" onClick={downloadAllRemoteRobots} />
                        </Box>
                    </>
                ) : viewType == MiraType.FIELD ? (
                    <>
                        <SectionLabel size={LabelSize.MEDIUM} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {remoteFieldElements
                                ? `${remoteFieldElements.length} Default Field${remoteFieldElements.length == 1 ? "" : "s"}`
                                : "Loading Default Fields"}
                        </SectionLabel>
                        <SectionDivider />
                        {remoteFieldElements}
                        <Box display="flex" justifyContent="center" mt={1}>
                            <PositiveButton value="Download All" onClick={downloadAllRemoteFields} />
                        </Box>
                    </>
                ) : (
                    <>
                        <SectionLabel size={LabelSize.MEDIUM} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {remoteGamePieces
                                ? `${remoteGamePieces.length} Default Game Piece${remoteGamePieces.length == 1 ? "" : "s"}`
                                : "Loading Default Game Pieces"}
                        </SectionLabel>
                        <SectionDivider />
                        {remoteGamePieces}
                        <Box display="flex" justifyContent="center" mt={1}>
                            <PositiveButton value="Download All" onClick={downloadAllRemotePieces} />
                        </Box>
                    </>
                )}
                <Box alignSelf={"center"}>
                    <Button value="Import from File" onClick={() => openModal("import-local-mirabuf")} />
                </Box>
            </div>
        </Panel>
    )
}

export default ImportMirabufPanel
