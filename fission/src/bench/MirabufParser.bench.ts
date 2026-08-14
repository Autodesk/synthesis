import { bench, describe } from "vitest"
import type { mirabuf } from "@/proto/mirabuf"
import { getMiraAssembly } from "@/test/GetAssets"
import MirabufParser from "@/mirabuf/MirabufParser"

// Top-level await: runs once at module load before any bench executes.
// beforeAll does not fire in Vitest browser bench mode, so this is the workaround.
const [dozer, multiJoint, field2018] = (await Promise.all([
    getMiraAssembly("DOZER"),
    getMiraAssembly("MULTI_JOINT"),
    getMiraAssembly(2018),
])) as [mirabuf.Assembly, mirabuf.Assembly, mirabuf.Assembly]

describe("MirabufParser", () => {
    bench("Parse Dozer", () => {
        new MirabufParser(dozer)
    })

    bench("Parse Multi-Joint Wheels", () => {
        new MirabufParser(multiJoint)
    })

    bench("Parse FRC Field 2018", () => {
        new MirabufParser(field2018)
    })
})
