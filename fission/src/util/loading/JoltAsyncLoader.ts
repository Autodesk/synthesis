/* eslint-disable @typescript-eslint/naming-convention */

import * as j from "@azaleacolburn/jolt-physics/wasm-compat"

let JOLT: typeof j.default | undefined = undefined
export const JOLT_TYPES = j.default
export const joltInit = j.default().then(jolt => (JOLT = jolt))
export default JOLT
