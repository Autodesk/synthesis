import MirabufParser from "@/mirabuf/MirabufParser"
import type { mirabuf } from "@/proto/mirabuf"
import { getMiraAssembly } from "@/test/GetAssets"
import { bench, beforeAll, describe } from "vitest"

describe("Mirabuf Parsing", () => {
    describe("2018", () => {
        let assembly: mirabuf.Assembly | undefined
        beforeAll(async () => {
            assembly = await getMiraAssembly(2018)
        })
        bench(
            "parse",
            () => {
                new MirabufParser(assembly!)
            },
            { time: 100 }
        )
    })

    describe("Dozer", () => {
        let assembly: mirabuf.Assembly | undefined
        beforeAll(async () => {
            assembly = await getMiraAssembly("DOZER")
        })
        bench(
            "parse",
            () => {
                new MirabufParser(assembly!)
            },
            { time: 100 }
        )
    })
})
