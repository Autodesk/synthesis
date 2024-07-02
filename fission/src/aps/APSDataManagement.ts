/* eslint-disable @typescript-eslint/no-explicit-any */
import { MainHUD_AddToast } from "@/ui/components/MainHUD";
import APS from "./APS"

export const FOLDER_DATA_TYPE = "folders"
export const ITEM_DATA_TYPE = "items"

export class APSDataError extends Error {
    error_code: string;
    title: string;
    detail: string;

    constructor(error_code: string, title: string, detail: string) {
        super(title);
        this.name = "APSDataError";
        this.error_code = error_code;
        this.title = title;
        this.detail = detail;
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

export interface Hub {
    id: string
    name: string
}

export interface Project {
    id: string
    name: string
    folder: Folder
}

export class Data {
    id: string
    type: string

    constructor(x: any) {
        this.id = x.id
        this.type = x.type
    }
}

export class Folder extends Data {
    displayName: string | undefined
    parentId: string | undefined

    constructor(x: any) {
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

    constructor(x: any) {
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
        return await fetch("https://developer.api.autodesk.com/project/v1/hubs", {
            method: "GET",
            headers: {
                Authorization: `Bearer ${auth.access_token}`,
            },
        })
            .then(x => x.json())
            .then(x => {
                if ((x.data as any[]).length > 0) {
                    return (x.data as any[]).map<Hub>(y => {
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
            MainHUD_AddToast('error', e.title, e.detail);
        } else if (e instanceof Error) {
            MainHUD_AddToast('error', 'Failed to get hubs.', e.message);
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
        return await fetch(`https://developer.api.autodesk.com/project/v1/hubs/${hub.id}/projects/`, {
            method: "GET",
            headers: {
                Authorization: `Bearer ${auth.access_token}`,
            },
        })
            .then(x => x.json())
            .then(x => {
                if ((x.data as any[]).length > 0) {
                    return (x.data as any[]).map<Project>(y => {
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
            MainHUD_AddToast('error', 'Failed to get hubs.', e.message);
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
                if ((x.data as any[]).length > 0) {
                    return (x.data as any[]).map<Data>(y => {
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
            MainHUD_AddToast('error', 'Failed to get folder data.', e.message);
        }
        return undefined
    }
}
