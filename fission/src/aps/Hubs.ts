/* eslint-disable @typescript-eslint/no-explicit-any */
import APS from "./APS";

export interface Hub {
    id: string;
    name: string;
}

export default async function getHubs(): Promise<Hub[] | undefined> {
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