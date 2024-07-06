import React, { useEffect, useState } from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaPlus } from "react-icons/fa6"
import Stack, { StackDirection } from "../../components/Stack"
import Button, { ButtonSize } from "../../components/Button"
import { useModalControlContext } from "@/ui/ModalContext"
import Label, { LabelSize } from "@/components/Label"
import World from "@/systems/World"
import { useTooltipControlContext } from "@/ui/TooltipContext"
import MirabufCachingService, { MirabufCacheInfo, MiraType } from "@/mirabuf/MirabufLoader"
import { CreateMirabuf } from "@/mirabuf/MirabufSceneObject"

interface MirabufRemoteInfo {
    displayName: string
    src: string
}

interface MirabufRemoteCardProps {
    info: MirabufRemoteInfo
    select: (info: MirabufRemoteInfo) => void
}

const MirabufRemoteCard: React.FC<MirabufRemoteCardProps> = ({ info, select }) => {
    return (
        <div
            key={info.src}
            className="flex flex-row align-middle justify-between items-center bg-background rounded-sm p-2 gap-2"
        >
            <Label className="text-wrap break-all">{info.displayName}</Label>
            <Button value="Spawn" onClick={() => select(info)} />
        </div>
    )
}

interface MirabufCacheCardProps {
    info: MirabufCacheInfo
    select: (info: MirabufCacheInfo) => void
}

const MirabufCacheCard: React.FC<MirabufCacheCardProps> = ({ info, select }) => {
    return (
        <div
            key={info.id}
            className="flex flex-row align-middle justify-between items-center bg-background rounded-sm p-2 gap-2"
        >
            <Label className="text-wrap break-all">{info.name ?? info.cacheKey}</Label>
            <Button value="Spawn" onClick={() => select(info)} />
        </div>
    )
}

export const AddRobotsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { showTooltip } = useTooltipControlContext()
    const { closeModal } = useModalControlContext()

    const [cachedRobots, setCachedRobots] = useState<MirabufCacheInfo[] | undefined>(undefined)

    // prettier-ignore
    useEffect(() => {
        (async () => {
            const map = MirabufCachingService.GetCacheMap(MiraType.ROBOT)
            setCachedRobots(Object.values(map))
        })()
    }, [])

    const [remoteRobots, setRemoteRobots] = useState<MirabufRemoteInfo[] | undefined>(undefined)

    // prettier-ignore
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
                    setRemoteRobots(robots)
                })
        })()
    }, [])

    const selectCache = async (info: MirabufCacheInfo) => {
        const assembly = await MirabufCachingService.Get(info.id, MiraType.ROBOT)

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
                MirabufCachingService.CacheInfo(info.cacheKey, MiraType.ROBOT, assembly.info?.name ?? undefined)
        } else {
            console.error("Failed to spawn robot")
        }

        closeModal()
    }

    const selectRemote = async (info: MirabufRemoteInfo) => {
        const cacheInfo = await MirabufCachingService.CacheRemote(info.src, MiraType.ROBOT)

        if (!cacheInfo) {
            console.error("Failed to cache robot")
            closeModal()
        } else {
            selectCache(cacheInfo)
        }
    }

    return (
        <Modal name={"Robot Selection"} icon={<FaPlus />} modalId={modalId} acceptEnabled={false}>
            <div className="flex overflow-y-auto flex-col gap-2 min-w-[50vw] max-h-[60vh] bg-background-secondary rounded-md p-2">
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {cachedRobots ? `${cachedRobots.length} Saved Robots` : "No Saved Robots"}
                </Label>
                {cachedRobots ? cachedRobots!.map(x => MirabufCacheCard({ info: x, select: selectCache })) : <></>}
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {remoteRobots ? `${remoteRobots.length} Default Robots` : "No Default Robots"}
                </Label>
                {remoteRobots ? remoteRobots!.map(x => MirabufRemoteCard({ info: x, select: selectRemote })) : <></>}
            </div>
        </Modal>
    )
}

export const AddFieldsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { closeModal } = useModalControlContext()

    const [cachedFields, setCachedFields] = useState<MirabufCacheInfo[] | undefined>(undefined)

    // prettier-ignore
    useEffect(() => {
        (async () => {
            const map = MirabufCachingService.GetCacheMap(MiraType.FIELD)
            setCachedFields(Object.values(map))
        })()
    }, [])

    const [remoteFields, setRemoteFields] = useState<MirabufRemoteInfo[] | undefined>(undefined)

    // prettier-ignore
    useEffect(() => {
        (async () => {
            fetch("/api/mira/manifest.json")
                .then(x => x.json())
                .then(x => {
                    // TODO: Skip already cached fields
                    const map = MirabufCachingService.GetCacheMap(MiraType.FIELD)
                    const fields: MirabufRemoteInfo[] = []
                    for (const src of x["fields"]) {
                        if (typeof src == "string") {
                            const newSrc = `/api/mira/Fields/${src}`
                            if (!map[newSrc]) fields.push({ displayName: src, src: newSrc })
                        } else {
                            if (!map[src["src"]])
                            fields.push({ displayName: src["displayName"], src: src["src"] })
                        }
                    }
                    setRemoteFields(fields)
                })
        })()
    }, [])

    const selectCache = async (info: MirabufCacheInfo) => {
        const assembly = await MirabufCachingService.Get(info.id, MiraType.FIELD)

        if (assembly) {
            CreateMirabuf(assembly).then(x => {
                if (x) {
                    World.SceneRenderer.RegisterSceneObject(x)
                }
            })

            if (!info.name)
                MirabufCachingService.CacheInfo(info.cacheKey, MiraType.FIELD, assembly.info?.name ?? undefined)
        } else {
            console.error("Failed to spawn field")
        }

        closeModal()
    }

    const selectRemote = async (info: MirabufRemoteInfo) => {
        const cacheInfo = await MirabufCachingService.CacheRemote(info.src, MiraType.FIELD)

        if (!cacheInfo) {
            console.error("Failed to cache field")
            closeModal()
        } else {
            selectCache(cacheInfo)
        }
    }

    return (
        <Modal name={"Field Selection"} icon={<FaPlus />} modalId={modalId} acceptEnabled={false}>
            <div className="flex overflow-y-auto flex-col gap-2 min-w-[50vw] max-h-[60vh] bg-background-secondary rounded-md p-2">
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {cachedFields ? `${cachedFields.length} Saved Fields` : "No Saved Fields"}
                </Label>
                {cachedFields ? cachedFields!.map(x => MirabufCacheCard({ info: x, select: selectCache })) : <></>}
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {remoteFields ? `${remoteFields.length} Default Fields` : "No Default Fields"}
                </Label>
                {remoteFields ? remoteFields!.map(x => MirabufRemoteCard({ info: x, select: selectRemote })) : <></>}
            </div>
        </Modal>
    )
}

export const SpawningModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()

    return (
        <Modal name={"Spawning"} icon={<FaPlus />} modalId={modalId}>
            <Stack direction={StackDirection.Vertical}>
                <Button
                    value={"Robots"}
                    onClick={() => openModal("add-robot")}
                    size={ButtonSize.Large}
                    className="m-auto"
                />
                <Button
                    value={"Fields"}
                    onClick={() => openModal("add-field")}
                    size={ButtonSize.Large}
                    className="m-auto"
                />
            </Stack>
        </Modal>
    )
}
