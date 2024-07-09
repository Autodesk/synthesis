import React, { useEffect, useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { FaPlus } from "react-icons/fa6"
import Button from "@/components/Button"
import Label, { LabelSize } from "@/components/Label"
import { Data, Folder, Hub, Item, Project, getFolderData, getHubs, getProjects, searchRootForMira } from "@/aps/APSDataManagement"
import { DeleteCached, GetMap, MiraType } from "@/mirabuf/MirabufLoader"
import APS, { APSAuth } from "@/aps/APS"

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

    useEffect(() => {
        APS.getAuth().then(auth => {
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
        })
    }, [])

    const cachedRobots = Object.entries(GetMap(MiraType.ROBOT) || {})
    const cachedFields = Object.entries(GetMap(MiraType.FIELD) || {})

    useEffect(() => {
        fetch('/api/mira/manifest.json').then(x => x.json()).then(data => {
            setManifest(data)
        })
    }, [])

    let cachedElements;
    if (cachedRobots.length > 0) {
        cachedElements = cachedRobots.map(([key, value]) =>
            ItemCard({
                name: key,
                id: key,
                buttonText: "import",
                onClick: () => console.log(`Selecting cached robot: ${key}`),
                secondaryButtonText: "delete",
                secondaryOnClick: () => {
                    console.log(`Deleting cache of: ${key}, ${value}`)
                    DeleteCached(MiraType.ROBOT, value);
                }
            })
        )
    }
    if (cachedFields.length > 0) {
        const fieldElements = cachedFields.map(([key, value]) =>
            ItemCard({
                name: key,
                id: key,
                buttonText: "import",
                onClick: () => console.log(`Selecting cached field: ${key}`),
                secondaryButtonText: "delete",
                secondaryOnClick: () => {
                    console.log(`Deleting cache of: ${key}`)
                    DeleteCached(MiraType.FIELD, value);
                }
            })
        );
        cachedElements = cachedElements ? cachedElements.concat(fieldElements) : fieldElements
    }

    let remotePaths = (manifest ? manifest.robots.concat(manifest.fields) : []);
    if (cachedRobots.length > 0) {
        remotePaths = remotePaths.filter(path => !cachedRobots.some(([key, _value]) => key.includes(path)))
    }
    if (cachedFields.length > 0) {
        remotePaths = remotePaths.filter(path => !cachedFields.some(([key, _value]) => key.includes(path)))
    }
    const remoteElements = remotePaths.map(path =>
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

    console.log('HUB ELEMENTS', hubElements)
    const displayElements = (cachedElements || []).concat(hubElements, remoteElements)
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
                {displayElements && displayElements.length > 0 ?
                    displayElements : <p>Nothing to display.</p>}
            </div>
        </Modal>
    )
}

export default ImportMirabufModal
