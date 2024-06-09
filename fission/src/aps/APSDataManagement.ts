/* eslint-disable @typescript-eslint/no-explicit-any */
import APS from "./APS";

export const FOLDER_DATA_TYPE = 'folders'
export const ITEM_DATA_TYPE = 'items'

export interface Hub {
    id: string;
    name: string;
}

export interface Project {
    id: string;
    name: string;
    folder: Folder;
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
    const auth = APS.auth
    if (!auth) {
        return undefined
    }

    try {
        return await fetch('https://developer.api.autodesk.com/project/v1/hubs', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${auth.access_token}`
            }
        }).then(x => x.json()).then(x => {
            if ((x.data as any[]).length > 0) {
                return (x.data as any[]).map<Hub>(y => { return { id: y.id, name: y.attributes.name }})
            } else {
                return undefined
            }
        })
    } catch (e) {
        console.error('Failed to get hubs')
        return undefined
    }
}

export async function getProjects(hub: Hub): Promise<Project[] | undefined> {
    const auth = APS.auth
    if (!auth) {
        return undefined
    }

    try {
        return await fetch(`https://developer.api.autodesk.com/project/v1/hubs/${hub.id}/projects/`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${auth.access_token}`
            }
        }).then(x => x.json()).then(x => {
            // console.log('Project Data')
            // console.log(x)
            if ((x.data as any[]).length > 0) {
                return (x.data as any[]).map<Project>(y => {
                    return {
                        id: y.id,
                        name: y.attributes.name,
                        folder: new Folder(y.relationships.rootFolder.data)
                    }
                })
            } else {
                return undefined
            }
        })
    } catch (e) {
        console.error('Failed to get hubs')
        return undefined
    }
}

export async function getFolderData(project: Project, folder: Folder): Promise<Data[] | undefined> {
    const auth = APS.auth
    if (!auth) {
        return undefined
    }

    try {
        return await fetch(`https://developer.api.autodesk.com/data/v1/projects/${project.id}/folders/${folder.id}/contents`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${auth.access_token}`
            }
        }).then(x => x.json()).then(x => {
            console.log('Raw Folder Data')
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
                console.log('No data in folder')
                return undefined
            }
        })
    } catch (e) {
        console.error('Failed to get folder data')
        return undefined
    }
}

// export async function downloadData(project: Project, data: Data) {

// }