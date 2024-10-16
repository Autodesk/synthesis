import React, { ReactNode, useCallback, useEffect, useMemo, useState } from "react"
import Label, { LabelSize } from "@/components/Label"
import {
    Data,
    GetMirabufFiles,
    HasMirabufFiles,
    MirabufFilesStatusUpdateEvent,
    MirabufFilesUpdateEvent,
    RequestMirabufFiles,
} from "@/aps/APSDataManagement"
import MirabufCachingService, { MirabufCacheInfo, MirabufRemoteInfo, MiraType } from "@/mirabuf/MirabufLoader"
import World from "@/systems/World"
import { useTooltipControlContext } from "@/ui/TooltipContext"
import { CreateMirabuf } from "@/mirabuf/MirabufSceneObject"
import { Box, Divider, styled } from "@mui/material"
import { HiDownload } from "react-icons/hi"
import Button, { ButtonProps, ButtonSize } from "@/ui/components/Button"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import { IoTrashBin } from "react-icons/io5"
import { AiOutlinePlus } from "react-icons/ai"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import { usePanelControlContext } from "@/ui/PanelContext"
import TaskStatus from "@/util/TaskStatus"
import { BiRefresh } from "react-icons/bi"
import { ProgressHandle } from "@/ui/components/ProgressNotificationData"

const DownloadIcon = <HiDownload size={"1.25rem"} />
const AddIcon = <AiOutlinePlus size={"1.25rem"} />
const DeleteIcon = <IoTrashBin size={"1.25rem"} />
const RefreshIcon = <BiRefresh size={"1.25rem"} />

const LabelStyled = styled(Label)({
    fontWeight: 700,
    margin: "0pt",
})

const DividerStyled = styled(Divider)({
    borderColor: "white",
})

const ButtonPrimary: React.FC<ButtonProps> = ({ value, onClick }) => {
    return (
        <Button
            size={ButtonSize.Medium}
            value={value}
            onClick={onClick}
            colorOverrideClass="bg-accept-button hover:brightness-90"
        ></Button>
    )
}

const ButtonSecondary: React.FC<ButtonProps> = ({ value, onClick }) => {
    return (
        <Button
            size={ButtonSize.Medium}
            value={value}
            onClick={onClick}
            colorOverrideClass="bg-cancel-button hover:brightness-90"
        ></Button>
    )
}

const ButtonIcon: React.FC<ButtonProps> = ({ value, onClick }) => {
    return (
        <Button
            value={value}
            onClick={onClick}
            colorOverrideClass="bg-[#00000000] hover:brightness-90"
            sizeOverrideClass="p-[0.25rem]"
        ></Button>
    )
}

interface ItemCardProps {
    id: string
    name: string
    primaryButtonNode: ReactNode
    secondaryButtonNode?: ReactNode
    primaryOnClick: () => void
    secondaryOnClick?: () => void
}

const ItemCard: React.FC<ItemCardProps> = ({
    id,
    name,
    primaryButtonNode,
    secondaryButtonNode,
    primaryOnClick,
    secondaryOnClick,
}) => {
    return (
        <Box
            component={"div"}
            display={"flex"}
            key={id}
            justifyContent={"space-between"}
            alignItems={"center"}
            gap={"1rem"}
        >
            <LabelStyled className="text-wrap break-all">{name.replace(/.mira$/, "")}</LabelStyled>
            <Box
                component={"div"}
                display={"flex"}
                key={`button-box-${id}`}
                flexDirection={"row-reverse"}
                gap={"0.25rem"}
                justifyContent={"center"}
                alignItems={"center"}
            >
                <ButtonPrimary value={primaryButtonNode} onClick={primaryOnClick} />
                {secondaryButtonNode && secondaryOnClick && (
                    <ButtonSecondary value={secondaryButtonNode} onClick={secondaryOnClick} />
                )}
            </Box>
        </Box>
    )
}

export type MiraManifest = {
    robots: MirabufRemoteInfo[]
    fields: MirabufRemoteInfo[]
}

function GetCacheInfo(miraType: MiraType): MirabufCacheInfo[] {
    return Object.values(MirabufCachingService.GetCacheMap(miraType))
}

function SpawnCachedMira(info: MirabufCacheInfo, type: MiraType, progressHandle?: ProgressHandle) {
    if (!progressHandle) {
        progressHandle = new ProgressHandle(info.name ?? info.cacheKey)
    }

    MirabufCachingService.Get(info.id, type)
        .then(assembly => {
            if (assembly) {
                CreateMirabuf(assembly).then(x => {
                    if (x) {
                        World.SceneRenderer.RegisterSceneObject(x)
                        progressHandle.Done()
                    } else {
                        progressHandle.Fail()
                    }
                })

                if (!info.name) MirabufCachingService.CacheInfo(info.cacheKey, type, assembly.info?.name ?? undefined)
            } else {
                progressHandle.Fail()
                console.error("Failed to spawn robot")
            }
        })
        .catch(() => progressHandle.Fail())
}

const ImportMirabufPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { showTooltip } = useTooltipControlContext()
    const { closePanel, openPanel } = usePanelControlContext()

    const [cachedRobots, setCachedRobots] = useState(GetCacheInfo(MiraType.ROBOT))
    const [cachedFields, setCachedFields] = useState(GetCacheInfo(MiraType.FIELD))

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
        if (!HasMirabufFiles()) {
            RequestMirabufFiles()
        } else {
            setFiles(GetMirabufFiles())
        }
    }, [])

    // Get Default Mirabuf Data, Load into manifest.
    useEffect(() => {
        // To remove the prettier warning
        const x = async () => {
            fetch(`/api/mira/manifest.json`)
                .then(x => x.json())
                .then(x => {
                    const map = MirabufCachingService.GetCacheMap(MiraType.ROBOT)
                    const robots: MirabufRemoteInfo[] = []
                    for (const src of x["robots"]) {
                        if (typeof src == "string") {
                            const str = `/api/mira/Robots/${src}`
                            if (!map[str]) robots.push({ displayName: src, src: str })
                        } else {
                            if (!map[src["src"]]) robots.push({ displayName: src["displayName"], src: src["src"] })
                        }
                    }
                    const fields: MirabufRemoteInfo[] = []
                    for (const src of x["fields"]) {
                        if (typeof src == "string") {
                            const str = `/api/mira/Fields/${src}`
                            if (!map[str]) fields.push({ displayName: src, src: str })
                        } else {
                            if (!map[src["src"]]) fields.push({ displayName: src["displayName"], src: src["src"] })
                        }
                    }
                    setManifest({
                        robots,
                        fields,
                    })
                })
        }
        x()
    }, [])

    // Select a mirabuf assembly from the cache.
    const selectCache = useCallback(
        (info: MirabufCacheInfo, type: MiraType) => {
            SpawnCachedMira(info, type)

            showTooltip("controls", [
                { control: "WASD", description: "Drive" },
                { control: "E", description: "Intake" },
                { control: "Q", description: "Dispense" },
            ])

            closePanel(panelId)

            if (type == MiraType.ROBOT) openPanel("choose-scheme")
        },
        [showTooltip, closePanel, panelId, openPanel]
    )

    // Cache a selected remote mirabuf assembly, load from cache.
    const selectRemote = useCallback(
        (info: MirabufRemoteInfo, type: MiraType) => {
            const status = new ProgressHandle(info.displayName)
            status.Update("Downloading from Synthesis...", 0.05)

            MirabufCachingService.CacheRemote(info.src, type)
                .then(cacheInfo => {
                    if (cacheInfo) {
                        SpawnCachedMira(cacheInfo, type, status)
                    } else {
                        status.Fail("Failed to cache")
                    }
                })
                .catch(() => status.Fail())

            closePanel(panelId)

            if (type == MiraType.ROBOT) openPanel("choose-scheme")
        },
        [closePanel, panelId, openPanel]
    )

    const selectAPS = useCallback(
        (data: Data, type: MiraType) => {
            const status = new ProgressHandle(data.attributes.displayName ?? data.id)
            status.Update("Downloading from APS...", 0.05)

            MirabufCachingService.CacheAPS(data, type)
                .then(cacheInfo => {
                    if (cacheInfo) {
                        SpawnCachedMira(cacheInfo, type, status)
                    } else {
                        status.Fail("Failed to cache")
                    }
                })
                .catch(() => status.Fail())

            closePanel(panelId)

            if (type == MiraType.ROBOT) openPanel("choose-scheme")
        },
        [closePanel, panelId, openPanel]
    )

    // Generate Item cards for cached robots.
    const cachedRobotElements = useMemo(
        () =>
            cachedRobots.map(info =>
                ItemCard({
                    name: info.name || info.cacheKey || "Unnamed Robot",
                    id: info.id,
                    primaryButtonNode: AddIcon,
                    primaryOnClick: () => {
                        console.log(`Selecting cached robot: ${info.cacheKey}`)
                        selectCache(info, MiraType.ROBOT)
                    },
                    secondaryButtonNode: DeleteIcon,
                    secondaryOnClick: () => {
                        console.log(`Deleting cache of: ${info.cacheKey}`)
                        MirabufCachingService.Remove(info.cacheKey, info.id, MiraType.ROBOT)

                        setCachedRobots(GetCacheInfo(MiraType.ROBOT))
                    },
                })
            ),
        [cachedRobots, selectCache, setCachedRobots]
    )

    // Generate Item cards for cached fields.
    const cachedFieldElements = useMemo(
        () =>
            cachedFields.map(info =>
                ItemCard({
                    name: info.name || info.cacheKey || "Unnamed Field",
                    id: info.id,
                    primaryButtonNode: AddIcon,
                    primaryOnClick: () => {
                        console.log(`Selecting cached field: ${info.cacheKey}`)
                        selectCache(info, MiraType.FIELD)
                    },
                    secondaryButtonNode: DeleteIcon,
                    secondaryOnClick: () => {
                        console.log(`Deleting cache of: ${info.cacheKey}`)
                        MirabufCachingService.Remove(info.cacheKey, info.id, MiraType.FIELD)

                        setCachedFields(GetCacheInfo(MiraType.FIELD))
                    },
                })
            ),
        [cachedFields, selectCache, setCachedFields]
    )

    // Generate Item cards for remote robots.
    const remoteRobotElements = useMemo(() => {
        const remoteRobots = manifest?.robots.filter(
            path => !cachedRobots.some(info => info.cacheKey.includes(path.src))
        )
        return remoteRobots?.map(path =>
            ItemCard({
                name: path.displayName,
                id: path.src,
                primaryButtonNode: DownloadIcon,
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
        return remoteFields?.map(path =>
            ItemCard({
                name: path.displayName,
                id: path.src,
                primaryButtonNode: DownloadIcon,
                primaryOnClick: () => {
                    console.log(`Selecting remote: ${path}`)
                    selectRemote(path, MiraType.FIELD)
                },
            })
        )
    }, [manifest?.fields, cachedFields, selectRemote])

    // Generate Item cards for APS robots and fields.
    const hubElements = useMemo(
        () =>
            files?.map(file =>
                ItemCard({
                    name: file.attributes.displayName!,
                    id: file.id,
                    primaryButtonNode: DownloadIcon,
                    primaryOnClick: () => {
                        console.debug(file.raw)
                        selectAPS(file, viewType)
                    },
                })
            ),
        [files, selectAPS, viewType]
    )

    return (
        <Panel
            name={"Select Mirabuf"}
            icon={AddIcon}
            panelId={panelId}
            acceptEnabled={false}
            cancelName="Back"
            openLocation="right"
        >
            <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary rounded-md p-2">
                <ToggleButtonGroup
                    value={viewType}
                    exclusive
                    onChange={(_, v) => v != null && setViewType(v)}
                    sx={{
                        alignSelf: "center",
                    }}
                >
                    <ToggleButton value={MiraType.ROBOT}>Robots</ToggleButton>
                    <ToggleButton value={MiraType.FIELD}>Fields</ToggleButton>
                </ToggleButtonGroup>
                {viewType == MiraType.ROBOT ? (
                    <>
                        <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {cachedRobotElements
                                ? `${cachedRobotElements.length} Saved Robot${cachedRobotElements.length == 1 ? "" : "s"}`
                                : "Loading Saved Robots"}
                        </LabelStyled>
                        <DividerStyled />
                        {cachedRobotElements}
                    </>
                ) : (
                    <>
                        <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {cachedFieldElements
                                ? `${cachedFieldElements.length} Saved Field${cachedFieldElements.length == 1 ? "" : "s"}`
                                : "Loading Saved Fields"}
                        </LabelStyled>
                        <DividerStyled />
                        {cachedFieldElements}
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
                    <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                        {hubElements
                            ? `${hubElements.length} Remote Asset${hubElements.length == 1 ? "" : "s"}`
                            : filesStatus.message}
                    </LabelStyled>
                    {hubElements && filesStatus.isDone ? (
                        <ButtonIcon value={RefreshIcon} onClick={() => RequestMirabufFiles()} />
                    ) : (
                        <></>
                    )}
                </Box>
                <DividerStyled />
                {hubElements}
                {viewType == MiraType.ROBOT ? (
                    <>
                        <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {remoteRobotElements
                                ? `${remoteRobotElements.length} Default Robot${remoteRobotElements.length == 1 ? "" : "s"}`
                                : "Loading Default Robots"}
                        </LabelStyled>
                        <DividerStyled />
                        {remoteRobotElements}
                    </>
                ) : (
                    <>
                        <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                            {remoteFieldElements
                                ? `${remoteFieldElements.length} Default Field${remoteFieldElements.length == 1 ? "" : "s"}`
                                : "Loading Default Fields"}
                        </LabelStyled>
                        <DividerStyled />
                        {remoteFieldElements}
                    </>
                )}
            </div>
        </Panel>
    )
}

export default ImportMirabufPanel
