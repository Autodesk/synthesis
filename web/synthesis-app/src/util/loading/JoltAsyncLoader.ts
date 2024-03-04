import * as j from '@barclah/jolt-physics/wasm-compat';

var JOLT: typeof j.default | undefined = undefined;
export var JOLT_TYPES = j.default;
export var joltInit = j.default().then(jolt => JOLT = jolt);
export default JOLT;