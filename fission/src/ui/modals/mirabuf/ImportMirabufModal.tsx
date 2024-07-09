import React, { useEffect, useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { FaPlus } from "react-icons/fa6"
import Button from "@/components/Button"
import Label, { LabelSize } from "@/components/Label"
import { Data, Folder, Hub, Project, getHubs, getProjects, searchRootForMira } from "@/aps/APSDataManagement"
import MirabufCachingService, { MirabufCacheInfo, MiraType } from "@/mirabuf/MirabufLoader"
import APS from "@/aps/APS"

interface ItemCardProps {
    id: string
    name: string
    buttonText: string
    secondaryButtonText?: string
    onClick: () => void
    secondaryOnClick?: () => void
}

const ItemCard: React.FC<ItemCardProps> = ({ id, name, buttonText, secondaryButtonText, onClick, secondaryOnClick }) => {
    return (
        <div
            key={id}
            className="flex flex-row align-middle justify-between items-center bg-background rounded-sm p-2 gap-2"
        >
            <Label className="text-wrap break-all">{name}</Label>
            <Button value={buttonText} onClick={onClick} />
            {secondaryButtonText && secondaryOnClick && <Button value={secondaryButtonText} onClick={secondaryOnClick} />} </div>
    )
}

export type MiraManifest = {
    robots: string[]
    fields: string[]
}

const ImportMirabufModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    // update tooltip based on type of drivetrain, receive message from Synthesis
    // const { showTooltip } = useTooltipControlContext()

    const [selectedHub, setSelectedHub] = useState<Hub | undefined>(undefined)
    const [selectedProject, setSelectedProject] = useState<Project | undefined>()
    const [selectedFolder, setSelectedFolder] = useState<Folder | undefined>()
    const [manifest, setManifest] = useState<MiraManifest | undefined>()
    const [files, setFiles] = useState<Data[] | undefined>()
    const auth = APS.getAuth();

    useEffect(() => {
        if (auth) {
            getHubs().then(async (hubs) => {
                if (!hubs) return;
                const fileData = []
                for (const hub of hubs) {
                    const projects = await getProjects(hub)
                    if (!projects) continue;
                    for (const project of projects) {
                        const data = await searchRootForMira(project)
                        if (data)
                            fileData.push(...data)
                    }
                }
                setFiles(fileData)
            })
        }
    }, [])

    const cachedRobots: MirabufCacheInfo[] = Object.values(MirabufCachingService.GetCacheMap(MiraType.ROBOT))
    const cachedFields: MirabufCacheInfo[] = Object.values(MirabufCachingService.GetCacheMap(MiraType.FIELD))

    useEffect(() => {
        fetch('/api/mira/manifest.json').then(x => x.json()).then(data => {
            setManifest(data)
        })
    }, [])

    let cachedRobotElements;
    if (cachedRobots.length > 0) {
        cachedRobotElements = cachedRobots.map(info =>
            ItemCard({
                name: info.name || info.cacheKey || "Unnamed Robot",
                id: info.id,
                buttonText: "import",
                onClick: () => console.log(`Selecting cached robot: ${info.cacheKey}`),
                secondaryButtonText: "delete",
                secondaryOnClick: () => {
                    console.log(`Deleting cache of: ${info.cacheKey}`)
                    MirabufCachingService.Remove(info.cacheKey, info.id, MiraType.ROBOT)
                }
            })
        )
    }
    let cachedFieldElements;
    if (cachedFields.length > 0) {
        cachedFieldElements = cachedFields.map(info =>
            ItemCard({
                name: info.name || info.cacheKey || "Unnamed Field",
                id: info.id,
                buttonText: "import",
                onClick: () => console.log(`Selecting cached field: ${info.cacheKey}`),
                secondaryButtonText: "delete",
                secondaryOnClick: () => {
                    console.log(`Deleting cache of: ${info.cacheKey}`)
                    MirabufCachingService.Remove(info.cacheKey, info.id, MiraType.FIELD)
                }
            })
        );
    }

    let remoteRobots = manifest?.robots || [];
    let remoteFields = manifest?.fields || [];
    if (cachedRobots.length > 0) {
        remoteRobots = remoteRobots.filter(path => !cachedRobots.some(info => info.cacheKey.includes(path)))
    }
    if (cachedFields.length > 0) {
        remoteFields = remoteFields.filter(path => !cachedFields.some(info => info.cacheKey.includes(path)))
    }
    const remoteRobotElements = remoteRobots.map(path =>
        ItemCard({
            name: path,
            id: path,
            buttonText: "download",
            onClick: () => console.log(`Selecting remote: ${path}`)
        })
    );

    const remoteFieldElements = remoteFields.map(path =>
        ItemCard({
            name: path,
            id: path,
            buttonText: "download",
            onClick: () => console.log(`Selecting remote: ${path}`)
        })
    );

    const hubElements = files?.map(file => (
        ItemCard({
            name: file.attributes.displayName!,
            id: file.id,
            buttonText: "APS import",
            onClick: () => console.log(`Selecting APS: ${file.attributes.name}`)
        })
    ));

    return (
        <Modal
            name={"Manage Assemblies"}
            icon={<FaPlus />}
            modalId={modalId}
            onAccept={() => {
                // showTooltip("controls", [
                //     { control: "WASD", description: "Drive" },
                //     { control: "E", description: "Intake" },
                //     { control: "Q", description: "Dispense" },
                // ]);
            }}
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
                    {cachedRobotElements ? `${cachedRobotElements.length} Saved Robots${cachedRobotElements.length > 1 ? 's' : ''}` : "No Saved Robots"}
                </Label>
                {cachedRobotElements}
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {cachedFieldElements ? `${cachedFieldElements.length} Saved Fields${cachedFieldElements.length > 1 ? 's' : ''}` : "No Saved Fields"}
                </Label>
                {cachedFieldElements}
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {hubElements ? `${hubElements.length} Remote Asset${hubElements.length > 1 ? 's' : ''}` : "No Remote Assets"}
                </Label>
                {hubElements}
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {remoteRobotElements ? `${remoteRobotElements.length} Default Robots${remoteRobotElements.length > 1 ? 's' : ''}` : "No Default Robots"}
                </Label>
                {remoteRobotElements}
                <Label size={LabelSize.Medium} className="text-center border-b-[1pt] mt-[4pt] mb-[2pt] mx-[5%]">
                    {remoteFieldElements ? `${remoteFieldElements.length} Default Field${remoteFieldElements.length > 1 ? 's' : ''}` : "No Default Fields"}
                </Label>
                {remoteFieldElements}
            </div>
        </Modal>
    )
}

export default ImportMirabufModal
