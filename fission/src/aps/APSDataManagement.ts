import { Mutex } from "async-mutex"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import type TaskStatus from "@/util/TaskStatus"
import APS from "./APS"

export const FOLDER_DATA_TYPE = "folders"
export const ITEM_DATA_TYPE = "items"

let mirabufFiles: Data[] | undefined
const mirabufFilesMutex: Mutex = new Mutex()

export class APSDataError extends Error {
    errorCode: string
    title: string
    detail: string

    constructor(errorCode: string, title: string, detail: string) {
        super(title)
        this.name = "APSDataError"
        this.errorCode = errorCode
        this.title = title
        this.detail = detail
    }
}

export type APSHubError = {
    Id: string | null
    HttpStatusCode: string
    ErrorCode: string
    Title: string
    Detail: string
    AboutLink: string | null
    Source: string | null
    meta: object | null
}

export type Filter = {
    fieldName: string
    matchValue: string
}

export interface Hub {
    id: string
    name: string
}

export interface Project {
    id: string
    name: string
    folder: Folder
}

export type DataAttributes = {
    name: string
    displayName?: string
    versionNumber?: number
    fileType?: string
}

export type Relationships = {
    storage: { meta: { link: { href?: string } } }
    parent: { data: { id?: string } }
    rootFolder: { data: RawData }
}

export type RawData = Omit<{ [key in keyof Data]: Data[key] }, "raw" | "href"> & { relationships: Relationships }

export class Data {
    id: string
    type: string
    attributes: DataAttributes
    href: string | undefined

    raw: { [k: string]: unknown }

    constructor(x: RawData) {
        this.id = x.id
        this.type = x.type
        this.attributes = x.attributes

        this.raw = x

        this.href = x.relationships?.storage?.meta?.link?.href
    }
}

export class Folder extends Data {
    displayName: string | undefined
    parentId: string | undefined

    constructor(x: RawData) {
        super(x)
        if (x.attributes) {
            if (x.attributes.displayName) {
                this.displayName = x.attributes.displayName
            }
        }
        if (x.relationships) {
            if (x.relationships.parent) {
                this.parentId = x.relationships.parent.data.id
            }
        }
    }
}

export class Item extends Data {
    displayName: string | undefined

    constructor(x: RawData) {
        super(x)

        if (x.attributes) {
            if (x.attributes.displayName) {
                this.displayName = x.attributes.displayName
            }
        }
    }
}

export async function getHubs(): Promise<Hub[] | undefined> {
    const auth = await APS.getAuth()
    if (!auth) {
        return undefined
    }

    try {
        APS.incApsCalls("project-v1-hubs")
        return await fetch("https://developer.api.autodesk.com/project/v1/hubs", {
            method: "GET",
            headers: {
                Authorization: `Bearer ${auth.access_token}`,
            },
        })
            .then(x => x.json())
            .then(x => {
                if ((x.data as RawData[] | undefined)?.length ?? 0 > 0) {
                    return (x.data as RawData[]).map<Hub>(y => {
                        return { id: y.id, name: y.attributes.name }
                    })
                } else {
                    return undefined
                }
            })
    } catch (e) {
        console.error("Failed to get hubs")
        console.error(e)
        console.log(auth)
        console.log(APS.userInfo)
        if (e instanceof APSDataError) {
            globalAddToast("error", e.title, e.detail)
        } else if (e instanceof Error) {
            globalAddToast("error", "Failed to get hubs.", e.message)
        }
        return undefined
    }
}

export async function getProjects(hub: Hub): Promise<Project[] | undefined> {
    const auth = await APS.getAuth()
    if (!auth) {
        return undefined
    }

    try {
        APS.incApsCalls("project-v1-hubs-x-projects")
        return await fetch(`https://developer.api.autodesk.com/project/v1/hubs/${hub.id}/projects/`, {
            method: "GET",
            headers: {
                Authorization: `Bearer ${auth.access_token}`,
            },
        })
            .then(x => x.json())
            .then(x => {
                if ((x.data as RawData[]).length > 0) {
                    return (x.data as RawData[]).map<Project>(y => {
                        return {
                            id: y.id,
                            name: y.attributes.name,
                            folder: new Folder(y.relationships.rootFolder.data),
                        }
                    })
                } else {
                    return undefined
                }
            })
    } catch (e) {
        console.error("Failed to get hubs")
        if (e instanceof Error) {
            globalAddToast("error", "Failed to get hubs.", e.message)
        }
        return undefined
    }
}

export async function getFolderData(project: Project, folder: Folder): Promise<Data[] | undefined> {
    const auth = await APS.getAuth()
    if (!auth) {
        return undefined
    }

    try {
        APS.incApsCalls("data-v1-projects-x-folders-x-contents")
        return await fetch(
            `https://developer.api.autodesk.com/data/v1/projects/${project.id}/folders/${folder.id}/contents`,
            {
                method: "GET",
                headers: {
                    Authorization: `Bearer ${auth.access_token}`,
                },
            }
        )
            .then(x => x.json())
            .then(x => {
                console.log("Raw Folder Data")
                console.log(x)
                if ((x.data as RawData[]).length > 0) {
                    return (x.data as RawData[]).map<Data>(y => {
                        if (y.type == ITEM_DATA_TYPE) {
                            return new Item(y)
                        } else if (y.type == FOLDER_DATA_TYPE) {
                            return new Folder(y)
                        } else {
                            return new Data(y)
                        }
                    })
                } else {
                    console.log("No data in folder")
                    return undefined
                }
            })
    } catch (e) {
        console.error("Failed to get folder data")
        if (e instanceof Error) {
            globalAddToast("error", "Failed to get folder data.", e.message)
        }
        return undefined
    }
}

function filterToQuery(filters: Filter[]): string {
    return filters.map(filter => encodeURIComponent(`filter[${filter.fieldName}]`) + `=${filter.matchValue}`).join("&")
}

export async function searchFolder(project: Project, folder: Folder, filters?: Filter[]): Promise<Data[] | undefined> {
    const auth = await APS.getAuth()
    if (!auth) return undefined
    let endpoint = `https://developer.api.autodesk.com/data/v1/projects/${project.id}/folders/${folder.id}/search`
    if (filters && filters.length > 0) {
        endpoint += `?${filterToQuery(filters)}`
    }

    APS.incApsCalls("data-v1-projects-x-folders-x-search")
    const res = await fetch(endpoint, {
        method: "GET",
        headers: {
            Authorization: `Bearer ${auth.access_token}`,
        },
    })
    if (!res.ok) {
        globalAddToast("error", "Error getting cloud files.", "Please sign in again.")
        return []
    }
    const json = await res.json()
    return json.data.map((data: RawData) => new Data(data))
}

export async function searchRootForMira(project: Project): Promise<Data[] | undefined> {
    return searchFolder(project, project.folder, [{ fieldName: "fileType", matchValue: "mira" }])
}

export async function downloadData(data: Data): Promise<ArrayBuffer | undefined> {
    if (!data.href) {
        return undefined
    }

    const auth = await APS.getAuth()
    if (!auth) {
        return undefined
    }

    APS.incApsCalls("object_bucket")
    return await fetch(data.href, {
        method: "GET",
        headers: {
            Authorization: `Bearer ${auth.access_token}`,
        },
    }).then(x => x.arrayBuffer())
}

export function hasMirabufFiles(): boolean {
    return mirabufFiles != undefined
}

export async function requestMirabufFiles() {
    if (mirabufFilesMutex.isLocked()) {
        return
    }

    mirabufFilesMutex.runExclusive(async () => {
        const auth = await APS.getAuth()
        if (auth) {
            getHubs().then(async hubs => {
                if (!hubs) {
                    window.dispatchEvent(
                        new MirabufFilesStatusUpdateEvent({
                            isDone: true,
                            message: "Failed to get Hubs",
                            progress: 1,
                        })
                    )
                    return
                }
                const fileData: Data[] = []
                let i = 0

                const projects = (
                    await Promise.all(
                        hubs.map(async hub => {
                            const projects = await getProjects(hub)
                            return projects ?? []
                        })
                    )
                ).flat()

                if (!projects.length) return
                for (const project of projects) {
                    window.dispatchEvent(
                        new MirabufFilesStatusUpdateEvent({
                            isDone: false,
                            message: `Searching Project '${project.name}'`,
                            progress: i++ / projects.length,
                        })
                    )
                    const data = await searchRootForMira(project)
                    if (data) fileData.push(...data)
                }
                window.dispatchEvent(
                    new MirabufFilesStatusUpdateEvent({
                        isDone: true,
                        message: `Found ${fileData.length} file${fileData.length == 1 ? "" : "s"}`,
                        progress: 1,
                    })
                )
                mirabufFiles = fileData
                window.dispatchEvent(new MirabufFilesUpdateEvent(mirabufFiles))
            })
        }
    })
}

export function getMirabufFiles(): Data[] | undefined {
    return mirabufFiles
}

export class MirabufFilesUpdateEvent extends Event {
    public static readonly EVENT_KEY: string = "MirabufFilesUpdateEvent"

    public data: Data[]

    public constructor(data: Data[]) {
        super(MirabufFilesUpdateEvent.EVENT_KEY)

        this.data = data
    }
}

export class MirabufFilesStatusUpdateEvent extends Event {
    public static readonly EVENT_KEY: string = "MirabufFilesStatusUpdateEvent"

    public status: TaskStatus

    public constructor(status: TaskStatus) {
        super(MirabufFilesStatusUpdateEvent.EVENT_KEY)

        this.status = status
    }
}
