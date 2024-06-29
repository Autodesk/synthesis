import * as j from "@barclah/jolt-physics/wasm-compat"

let JOLT: typeof j.default | undefined = undefined
export const JOLT_TYPES = j.default
export const joltInit = j.default().then(jolt => (JOLT = jolt))
export default JOLT
