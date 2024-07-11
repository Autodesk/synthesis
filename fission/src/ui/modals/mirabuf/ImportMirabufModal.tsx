import React, { useEffect, useMemo, useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { FaPlus } from "react-icons/fa6"
import Button from "@/components/Button"
import Label, { LabelSize } from "@/components/Label"
import { Data, Folder, Hub, Project, getHubs, getProjects, searchRootForMira } from "@/aps/APSDataManagement"
import MirabufCachingService, { MirabufCacheInfo, MirabufRemoteInfo, MiraType } from "@/mirabuf/MirabufLoader"
import APS from "@/aps/APS"
import World from "@/systems/World"
import { useModalControlContext } from "@/ui/ModalContext"
import { useTooltipControlContext } from "@/ui/TooltipContext"
import { CreateMirabuf } from "@/mirabuf/MirabufSceneObject"

interface ItemCardProps {
    id: string
    name: string
    buttonText: string
    secondaryButtonText?: string
    onClick: () => void
    secondaryOnClick?: () => void
}

const ItemCard: React.FC<ItemCardProps> = ({
    id,
    name,
    buttonText,
    secondaryButtonText,
    onClick,
    secondaryOnClick,
}) => {
    return (
        <div
            key={id}
            className="flex flex-row align-middle justify-between items-center bg-background rounded-sm p-2 gap-2"
        >
            <Label className="text-wrap break-all">{name}</Label>
            <Button value={buttonText} onClick={onClick} />
            {secondaryButtonText && secondaryOnClick && (
                <Button value={secondaryButtonText} onClick={secondaryOnClick} />
            )}{" "}
        </div>
    )
}

export type MiraManifest = {
    robots: MirabufRemoteInfo[]
    fields: MirabufRemoteInfo[]
}

const ImportMirabufModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { showTooltip } = useTooltipControlContext()
    const { closeModal } = useModalControlContext()

    const [selectedHub, setSelectedHub] = useState<Hub | undefined>(undefined)
    const [selectedProject, setSelectedProject] = useState<Project | undefined>()
    const [selectedFolder, setSelectedFolder] = useState<Folder | undefined>()
    const [manifest, setManifest] = useState<MiraManifest | undefined>()
    const [files, setFiles] = useState<Data[] | undefined>()
    const auth = APS.getAuth()

    useEffect(() => {
        if (auth) {
            getHubs().then(async hubs => {
                if (!hubs) return
                const fileData = []
                for (const hub of hubs) {
                    const projects = await getProjects(hub)
                    if (!projects) continue
                    for (const project of projects) {
                        const data = await searchRootForMira(project)
                        if (data) fileData.push(...data)
                    }
                }
                setFiles(fileData)
            })
        }
    }, [])

    const cachedRobots: MirabufCacheInfo[] = Object.values(MirabufCachingService.GetCacheMap(MiraType.ROBOT))
    const cachedFields: MirabufCacheInfo[] = Object.values(MirabufCachingService.GetCacheMap(MiraType.FIELD))

    useEffect(() => {
        (async () => {
            fetch("/api/mira/manifest.json")
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

    const selectCache = async (info: MirabufCacheInfo, type: MiraType) => {
        const assembly = await MirabufCachingService.Get(info.id, type)

        if (assembly) {
            showTooltip("controls", [
                { control: "WASD", description: "Drive" },
                { control: "E", description: "Intake" },
                { control: "Q", description: "Dispense" },
            ])

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

        closeModal()
    }

    const selectRemote = async (info: MirabufRemoteInfo, type: MiraType) => {
        const cacheInfo = await MirabufCachingService.CacheRemote(info.src, type)

        if (!cacheInfo) {
            console.error("Failed to cache robot")
            closeModal()
        } else {
            selectCache(cacheInfo, type)
        }
    }

    const cachedRobotElements = cachedRobots.map(info =>
        ItemCard({
            name: info.name || info.cacheKey || "Unnamed Robot",
            id: info.id,
            buttonText: "import",
            onClick: () => {
                console.log(`Selecting cached robot: ${info.cacheKey}`)
                selectCache(info, MiraType.ROBOT)
            },
            secondaryButtonText: "delete",
            secondaryOnClick: () => {
                console.log(`Deleting cache of: ${info.cacheKey}`)
                MirabufCachingService.Remove(info.cacheKey, info.id, MiraType.ROBOT)
            },
        })
    )
    const cachedFieldElements = cachedFields.map(info =>
        ItemCard({
            name: info.name || info.cacheKey || "Unnamed Field",
            id: info.id,
            buttonText: "import",
            onClick: () => {
                console.log(`Selecting cached field: ${info.cacheKey}`)
                selectCache(info, MiraType.FIELD)
            },
            secondaryButtonText: "delete",
            secondaryOnClick: () => {
                console.log(`Deleting cache of: ${info.cacheKey}`)
                MirabufCachingService.Remove(info.cacheKey, info.id, MiraType.FIELD)
            },
        })
    )


    const remoteRobotElements = useMemo(() => {
        const remoteRobots = manifest?.robots.filter(path => !cachedRobots.some(info => info.cacheKey.includes(path.src))) ?? []
        return remoteRobots.map(path =>
            ItemCard({
                name: path.displayName,
                id: path.src,
                buttonText: "import",
                onClick: () => {
                    console.log(`Selecting remote: ${path}`)
                    selectRemote(path, MiraType.ROBOT)
                },
            }))
    }, [manifest?.robots, cachedRobots])

    const remoteFieldElements = useMemo(() => {
        const remoteFields = manifest?.fields.filter(path => !cachedFields.some(info => info.cacheKey.includes(path.src))) ?? []
        return remoteFields.map(path =>
            ItemCard({
                name: path.displayName,
                id: path.src,
                buttonText: "import",
                onClick: () => {
                    console.log(`Selecting remote: ${path}`)
                    selectRemote(path, MiraType.FIELD)
                },
            }))
    }, [manifest?.fields, cachedFields])

    const hubElements = useMemo(() => files?.map(file =>
        ItemCard({
            name: file.attributes.displayName!,
            id: file.id,
            buttonText: "APS import",
            onClick: () => console.log(`Selecting APS: ${file.attributes.name}`),
        })
    ), [files])

    return (
        <Modal
            name={"Manage Assemblies"}
            icon={<FaPlus />}
            modalId={modalId}
        // onAccept={() => {}}
        >
            <div className="w-full flex flex-col items-center">
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
            </div>
            <div className="flex overflow-y-auto flex-col gap-2 min-w-[50vw] max-h-[60vh] bg-background-secondary rounded-md p-2">
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {cachedRobotElements
                        ? `${cachedRobotElements.length} Saved Robot${cachedRobotElements.length == 1 ? "" : "s"}`
                        : "Loading Saved Robots"}
                </Label>
                {cachedRobotElements}
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {cachedFieldElements
                        ? `${cachedFieldElements.length} Saved Field${cachedFieldElements.length == 1 ? "" : "s"}`
                        : "Loading Saved Fields"}
                </Label>
                {cachedFieldElements}
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {hubElements
                        ? `${hubElements.length} Remote Asset${hubElements.length == 1 ? "" : "s"}`
                        : "Loading Remote Assets"}
                </Label>
                {hubElements}
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {remoteRobotElements
                        ? `${remoteRobotElements.length} Default Robot${remoteRobotElements.length == 1 ? "" : "s"}`
                        : "Loading Default Robots"}
                </Label>
                {remoteRobotElements}
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {remoteFieldElements
                        ? `${remoteFieldElements.length} Default Field${remoteFieldElements.length == 1 ? "" : "s"}`
                        : "Loading Default Fields"}
                </Label>
                {remoteFieldElements}
            </div>
        </Modal>
    )
}

export default ImportMirabufModal
