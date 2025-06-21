/**
 * This loader still exists within Synthesis instead of the npm re-export because we want to have
 * customizable control over when it is initialized.
 */

import * as j from "@azaleacolburn/jolt-physics/wasm-compat"

const JOLT = await j.default()
export default JOLT
