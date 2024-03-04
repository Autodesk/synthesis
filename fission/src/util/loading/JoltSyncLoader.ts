/**
 * This loader still exists within Synthesis instead of the npm re-export because we want to have
 * customizable control over when it is initialized.
 */

import * as JOLT from '@barclah/jolt-physics/wasm-compat';

var Jolt = await JOLT.default();
export default Jolt;