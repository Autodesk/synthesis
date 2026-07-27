/**
 * This loader still exists within Synthesis instead of the npm re-export because we want to have
 * customizable control over when it is initialized.
 */

import * as J from "@synthesis.adsk/jolt-physics/wasm-compat"

const JOLT = await J.default()
export default JOLT
