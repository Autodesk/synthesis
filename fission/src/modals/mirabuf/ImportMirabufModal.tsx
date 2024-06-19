import React, { useEffect, useState } from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaPlus } from "react-icons/fa6"
import Button from "@/components/Button"
import Label, { LabelSize } from "@/components/Label"
import { Data, Folder, Hub, Item, Project, getFolderData, getHubs, getProjects } from "@/aps/APSDataManagement"

interface ItemCardProps {
    id: string
    name: string
    buttonText: string
    onClick: () => void
}

const ItemCard: React.FC<ItemCardProps> = ({ id, name, buttonText, onClick }) => {
    return (
        <div
            key={id}
            className="flex flex-row align-middle justify-between items-center bg-background rounded-sm p-2 gap-2"
        >
            <Label className="text-wrap break-all">{name}</Label>
            <Button value={buttonText} onClick={onClick} />
        </div>
    )
}

const ImportMirabufModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    // update tooltip based on type of drivetrain, receive message from Synthesis
    // const { showTooltip } = useTooltipControlContext()

    const [selectedHub, setSelectedHub] = useState<Hub | undefined>(undefined)
    const [selectedProject, setSelectedProject] = useState<Project | undefined>(undefined)
    const [selectedFolder, setSelectedFolder] = useState<Folder | undefined>(undefined)

    const [hubs, setHubs] = useState<Hub[] | undefined>(undefined)
    useEffect(() => {
        (async () => {
            setHubs(await getHubs())
        })()
    }, [])

    const [projects, setProjects] = useState<Project[] | undefined>(undefined)
    useEffect(() => {
        (async () => {
            if (selectedHub) {
                setProjects(await getProjects(selectedHub))
            }
        })()
    }, [selectedHub])

    const [folderData, setFolderData] = useState<Data[] | undefined>(undefined)
    useEffect(() => {
        (async () => {
            if (selectedProject) {
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
    }, [selectedProject, selectedFolder])

    useEffect(() => {
        if (folderData) {
            console.log(`${folderData.length} items in folder data`)
        } else {
            console.log("No folder data")
        }
    }, [folderData])

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
                ) : (
                    <></>
                )}
            </div>
            <div className="flex overflow-y-auto flex-col gap-2 min-w-[50vw] max-h-[60vh] bg-background-secondary rounded-md p-2">
                {!selectedHub
                    ? hubs?.map(x =>
                          ItemCard({
                              name: x.name,
                              id: x.id,
                              buttonText: ">",
                              onClick: () => setSelectedHub(x),
                          })
                      )
                    : !selectedProject
                      ? projects?.map(x =>
                            ItemCard({
                                name: x.name,
                                id: x.id,
                                buttonText: ">",
                                onClick: () => setSelectedProject(x),
                            })
                        )
                      : folderData?.map(x =>
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
                        )}
            </div>
        </Modal>
    )
}

export default ImportMirabufModal
