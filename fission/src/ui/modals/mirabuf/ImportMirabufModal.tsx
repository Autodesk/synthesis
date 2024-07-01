import React, { useEffect, useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { FaPlus } from "react-icons/fa6"
import Button from "@/components/Button"
import Label, { LabelSize } from "@/components/Label"
import { Data, Folder, Hub, Item, Project, getFolderData, getHubs, getProjects } from "@/aps/APSDataManagement"
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
            {secondaryButtonText && secondaryOnClick && <Button value={secondaryButtonText} onClick={secondaryOnClick} />}
        </div>
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
    const [auth, setAuth] = useState<APSAuth | undefined>()

    useEffect(() => {
        APS.getAuth().then(setAuth)
    }, [])

    const cachedRobots = Object.entries(GetMap(MiraType.ROBOT) || {})
    const cachedFields = Object.entries(GetMap(MiraType.FIELD) || {})
    console.log(cachedRobots, cachedFields)

    useEffect(() => {
        fetch('/api/mira/manifest.json').then(x => x.json()).then(data => {
            setManifest(data)
        })
    }, [])

    const [hubs, setHubs] = useState<Hub[] | undefined>(undefined)
    useEffect(() => {
        (async () => {
            if (auth) setHubs(await getHubs())
        })()
    }, [auth])

    const [projects, setProjects] = useState<Project[] | undefined>(undefined)
    useEffect(() => {
        (async () => {
            if (auth && selectedHub) {
                setProjects(await getProjects(selectedHub))
            }
        })()
    }, [auth, selectedHub])

    const [folderData, setFolderData] = useState<Data[] | undefined>(undefined)
    useEffect(() => {
        (async () => {
            if (auth && selectedProject) {
                console.log("Project has been selected")
                if (selectedFolder) {
                    console.log(`Selecting folder '${selectedFolder.displayName}'`)

                    setFolderData(await getFolderData(selectedProject, selectedFolder))
                } else {
                    console.log("Defaulting to project root folder")
                    const data = await getFolderData(selectedProject, selectedProject.folder)
                    console.log(`Folder Data:\n${JSON.stringify(data)}`)
                    setFolderData(data)
                }
            }
        })()
    }, [auth, selectedProject, selectedFolder])

    useEffect(() => {
        if (auth && folderData) {
            console.log(`${folderData.length} items in folder data`)
        } else {
            console.log("No folder data")
        }
    }, [auth, folderData])

    let cachedElements;
    if (cachedRobots.length > 0) {
        cachedElements = cachedRobots.map(([key, value]) =>
            ItemCard({
                name: key,
                id: value,
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
                id: value,
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
            buttonText: "import",
            onClick: () => console.log(`Selecting remote: ${path}`)
        })
    );

    let hubElements;

    if (!selectedHub) {
        hubElements = hubs?.map(x =>
            ItemCard({
                name: x.name,
                id: x.id,
                buttonText: ">",
                onClick: () => setSelectedHub(x),
            })
        )
    } else {
        if (!selectedProject) {
            hubElements = projects?.map(x =>
                ItemCard({
                    name: x.name,
                    id: x.id,
                    buttonText: ">",
                    onClick: () => setSelectedProject(x),
                })
            )
        } else {
            hubElements = folderData?.map(x =>
                x instanceof Folder
                    ? ItemCard({
                        name: `DIR: ${x.displayName}`,
                        id: x.id,
                        buttonText: ">",
                        onClick: () => setSelectedFolder(x),
                    })
                    : x instanceof Item
                        ? ItemCard({
                            name: `${x.displayName}`,
                            id: x.id,
                            buttonText: "import",
                            onClick: () => {
                                console.log(`Selecting ${x.displayName} (${x.id})`)
                            },
                        })
                        : ItemCard({
                            name: `${x.type}: ${x.id}`,
                            id: x.id,
                            buttonText: "---",
                            onClick: () => {
                                console.log(`Selecting (${x.id})`)
                            },
                        })
            )
        }
    }
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
