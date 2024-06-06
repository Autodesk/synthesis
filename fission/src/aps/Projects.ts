/* eslint-disable @typescript-eslint/no-explicit-any */
import APS from "./APS";
import { Hub } from "./Hubs";

export interface Project {
    type: string;
    id: string;
    name: string;
    rootFolderId: string;
}

export default async function getProjects(hub: Hub): Promise<Project[] | undefined> {
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
            if ((x.data as any[]).length > 0) {
                return (x.data as any[]).map<Project>(y => { return { id: y.id, name: y.attributes.name, type: y.type, rootFolderId: y.relationships.rootFolder.data.id }})
            } else {
                return undefined
            }
        })
    } catch (e) {
        console.error('Failed to get hubs')
        return undefined
    }
}