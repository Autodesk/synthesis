import React, { ReactNode, useCallback, useEffect, useMemo, useState } from "react"
import Label, { LabelSize } from "@/components/Label"
import { Data, getHubs, getProjects, searchRootForMira } from "@/aps/APSDataManagement"
import MirabufCachingService, { MirabufCacheInfo, MirabufRemoteInfo, MiraType } from "@/mirabuf/MirabufLoader"
import APS from "@/aps/APS"
import World from "@/systems/World"
import { useModalControlContext } from "@/ui/ModalContext"
import { useTooltipControlContext } from "@/ui/TooltipContext"
import { CreateMirabuf } from "@/mirabuf/MirabufSceneObject"
import { Box, styled } from "@mui/material"
import { HiDownload } from "react-icons/hi"
import Button, { ButtonProps, ButtonSize } from "@/ui/components/Button"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import { IoTrashBin } from "react-icons/io5"
import { AiOutlinePlus } from "react-icons/ai"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import { usePanelControlContext } from "@/ui/PanelContext"

const DownloadIcon = (<HiDownload size={"1.25rem"} />)
const AddIcon = (<AiOutlinePlus size={"1.25rem"} />)
const DeleteIcon = (<IoTrashBin size={"1.25rem"} />)

interface TaskStatus {
    isDone: boolean,
    message: string,
}

const LabelStyled = styled(Label)({
    fontWeight: 700
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
            <LabelStyled className="text-wrap break-all">{name.replace(/.mira$/, '')}</LabelStyled>
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

function SpawnCachedMira(info: MirabufCacheInfo, type: MiraType) {
    MirabufCachingService.Get(info.id, type).then(assembly => {
        if (assembly) {
            CreateMirabuf(assembly).then(x => {
                if (x) {
                    World.SceneRenderer.RegisterSceneObject(x)
                }
            })

            if (!info.name)
                MirabufCachingService.CacheInfo(info.cacheKey, type, assembly.info?.name ?? undefined)
        } else {
            console.error("Failed to spawn robot")
        }
    })
}

const ImportMirabufPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { showTooltip } = useTooltipControlContext()
    const { closePanel } = usePanelControlContext()

    const [cachedRobots, setCachedRobots] = useState(GetCacheInfo(MiraType.ROBOT))
    const [cachedFields, setCachedFields] = useState(GetCacheInfo(MiraType.FIELD))


    const [manifest, setManifest] = useState<MiraManifest | undefined>()
    const [files, setFiles] = useState<Data[] | undefined>()
    const [apsFilesState, setApsFilesState] = useState<TaskStatus>({ isDone: false, message: 'Retrieving APS Hubs...' })
    const auth = useMemo(async () => await APS.getAuth(), [])

    const [viewType, setViewType] = useState<MiraType>(MiraType.ROBOT)

    // Get APS Mirabuf Data, Load into files.
    useEffect(() => {
        (async () => {
            if (await auth) {
                getHubs().then(async hubs => {
                    if (!hubs) {
                        setApsFilesState({ isDone: true, message: 'Failed to load APS Hubs' })
                        return
                    }
                    const fileData: Data[] = []
                    for (const hub of hubs) {
                        const projects = await getProjects(hub)
                        if (!projects) continue
                        for (const project of projects) {
                            setApsFilesState({ isDone: false, message: `Searching Project '${project.name}'` })
                            const data = await searchRootForMira(project)
                            if (data) fileData.push(...data)
                        }
                    }
                    setApsFilesState({ isDone: true, message: `Found ${fileData.length} file${fileData.length == 1 ? '' : 's'}` })
                    setFiles(fileData)
                })
            }
        })()
    }, [auth])

    // Get Default Mirabuf Data, Load into manifest.
    useEffect(() => {
        (async () => {
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
                        fields
                    })
                })
        })()
    }, [])

    // Select a mirabuf assembly from the cache.
    const selectCache = useCallback((info: MirabufCacheInfo, type: MiraType) => {
        SpawnCachedMira(info, type)

        showTooltip("controls", [
            { control: "WASD", description: "Drive" },
            { control: "E", description: "Intake" },
            { control: "Q", description: "Dispense" },
        ])

        closePanel(panelId)
    }, [showTooltip, closePanel, panelId])

    // Cache a selected remote mirabuf assembly, load from cache.
    const selectRemote = useCallback((info: MirabufRemoteInfo, type: MiraType) => {
        MirabufCachingService.CacheRemote(info.src, type).then(cacheInfo => {
            cacheInfo && SpawnCachedMira(cacheInfo, type)
        })

        closePanel(panelId)
    }, [closePanel, panelId])

    // Generate Item cards for cached robots.
    const cachedRobotElements = useMemo(() => cachedRobots.map(info =>
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
    ), [cachedRobots, selectCache, setCachedRobots])

    // Generate Item cards for cached fields.
    const cachedFieldElements = useMemo(() => cachedFields.map(info =>
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
    ), [cachedFields, selectCache, setCachedFields])

    // Generate Item cards for remote robots.
    const remoteRobotElements = useMemo(() => {
        const remoteRobots = manifest?.robots.filter(path => !cachedRobots.some(info => info.cacheKey.includes(path.src)))
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
        const remoteFields = manifest?.fields.filter(path => !cachedFields.some(info => info.cacheKey.includes(path.src)))
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
    const hubElements = useMemo(() => files?.map(file =>
        ItemCard({
            name: file.attributes.displayName!,
            id: file.id,
            primaryButtonNode: DownloadIcon,
            primaryOnClick: () => {
                if (file.href) {
                    selectRemote({ src: file.href, displayName: file.attributes.displayName ?? file.id }, viewType)
                } else {
                    console.error('No href for file')
                    console.debug(file.raw)
                }
            }
        })
    ), [files, selectRemote, viewType])

    return (
        <Panel
            name={"Select Mirabuf"}
            icon={AddIcon}
            panelId={panelId}
            acceptEnabled={false}
            cancelName="Back"
            openLocation="right"
        >
            {/* <div className="w-full flex flex-col items-center">
                {selectedHub ? (
                    selectedFolder ? (
                        <>
                            <Label size={LabelSize.Medium}>Folder: {selectedFolder.displayName}</Label>
                            <Button value="back to project root" onClick={() => setSelectedFolder(undefined)} />
                        </>
                    ) : selectedProject ? (
                        <>
                            <Label size={LabelSize.Medium}>Project: {selectedProject.name}</Label>
                            <Button value="back to projects" onClick={() => setSelectedProject(undefined)} />
                        </>
                    ) : (
                        <>
                            <Label size={LabelSize.Medium}>Hub: {selectedHub.name}</Label>
                            <Button value="back to hubs" onClick={() => setSelectedHub(undefined)} />
                        </>
                    )
                ) : APS.userInfo ? (
                    <></>
                ) : (
                    <Button value={"Sign In to APS"} onClick={() => APS.requestAuthCode()} />
                )}
            </div> */}
            <div className="flex flex-col gap-2 bg-background-secondary rounded-md p-2">
                <ToggleButtonGroup
                    value={viewType}
                    exclusive
                    onChange={(_, v) => v != null && setViewType(v)}
                    sx={{
                        alignSelf: "center"
                    }}
                >
                    <ToggleButton value={MiraType.ROBOT}>Robots</ToggleButton>
                    <ToggleButton value={MiraType.FIELD}>Fields</ToggleButton>
                </ToggleButtonGroup>
                {
                    viewType == MiraType.ROBOT
                        ?
                        (<>
                            <LabelStyled size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                                {cachedRobotElements
                                    ? `${cachedRobotElements.length} Saved Robot${cachedRobotElements.length == 1 ? "" : "s"}`
                                    : "Loading Saved Robots"}
                            </LabelStyled>
                            {cachedRobotElements}
                        </>)
                        :
                        (<>
                            <LabelStyled size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                                {cachedFieldElements
                                    ? `${cachedFieldElements.length} Saved Field${cachedFieldElements.length == 1 ? "" : "s"}`
                                    : "Loading Saved Fields"}
                            </LabelStyled>
                            {cachedFieldElements}
                        </>)
                }
                <LabelStyled size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {hubElements
                        ? `${hubElements.length} Remote Asset${hubElements.length == 1 ? "" : "s"}`
                        : apsFilesState.message}
                </LabelStyled>
                {hubElements}
                {
                    viewType == MiraType.ROBOT
                        ?
                        (<>
                            <LabelStyled size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                                {remoteRobotElements
                                    ? `${remoteRobotElements.length} Default Robot${remoteRobotElements.length == 1 ? "" : "s"}`
                                    : "Loading Default Robots"}
                            </LabelStyled>
                            {remoteRobotElements}
                        </>)
                        :
                        (<>
                            <LabelStyled size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                                {remoteFieldElements
                                    ? `${remoteFieldElements.length} Default Field${remoteFieldElements.length == 1 ? "" : "s"}`
                                    : "Loading Default Fields"}
                            </LabelStyled>
                            {remoteFieldElements}
                        </>)
                }
            </div>
        </Panel>
    )
}

export default ImportMirabufPanel
