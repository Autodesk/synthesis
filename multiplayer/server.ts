import { setup, teardown } from "../fission/src/test/TestSetup.server.ts";

await setup()

setTimeout(teardown, 5000)