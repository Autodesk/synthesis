import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader.ts"
import MirabufInstance from "@/mirabuf/MirabufInstance.ts"
import MirabufParser from "@/mirabuf/MirabufParser.ts"

export const ROBOT_MODELS = {
    DOZER: "/api/mira/robots/Dozer v11.mira",
    MULTI_JOINT: "/api/mira/private/Multi-Joint Wheels v0.mira",
} satisfies Record<string, string>

export const FIELD_MODELS = {
    2018: "/api/mira/fields/FRC Field 2018 v13.mira",
    2023: "/api/mira/fields/FRC Field 2023 v8.mira",
} satisfies Record<number, string>

export async function getMiraAssembly(name: keyof typeof ROBOT_MODELS | keyof typeof FIELD_MODELS) {
    if (name in ROBOT_MODELS) {
        return await MirabufCachingService.cacheRemoteAndReturn(
            ROBOT_MODELS[name as keyof typeof ROBOT_MODELS],
            MiraType.ROBOT
        )
    } else {
        return await MirabufCachingService.cacheRemoteAndReturn(
            FIELD_MODELS[name as keyof typeof FIELD_MODELS],
            MiraType.FIELD
        )
    }
}
export async function getMiraInstance(name: keyof typeof ROBOT_MODELS | keyof typeof FIELD_MODELS) {
    const assembly = await getMiraAssembly(name)
    if (assembly) {
        return new MirabufInstance(new MirabufParser(assembly))
    }
}
