import * as j from "@azaleacolburn/jolt-physics/wasm-compat"

// biome-ignore lint/style/useNamingConvention: Jolt is special
let JOLT: typeof j.default | undefined = undefined
export const JOLT_TYPES = j.default
export const joltInit = j.default().then(jolt => {
    JOLT = jolt
    return jolt
})
export default JOLT
