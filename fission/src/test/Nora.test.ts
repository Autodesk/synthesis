import { Receiver, Supplier } from "@/systems/simulation/Nora"
import { beforeEach, describe, expect, test, vi } from "vitest"

describe("Nora Tests", () => {
    beforeEach(() => {
        vi.resetAllMocks()
    })

    test("Hashmap Testing", () => {
        type KeyType = {
            id: number
            src: string
            objs: number[]
        }

        const map = new Map<KeyType, number>()

        const keyA: KeyType = {
            id: 0,
            src: "a",
            objs: [ 1, 2 ]
        }
        
        const keyB: KeyType = {
            id: 1,
            src: "b",
            objs: [ 3, 4 ]
        }

        const keyACopy = JSON.parse(JSON.stringify(keyA))

        map.set(keyA, 5)
        map.set(keyB, 10)

        expect(map.get(keyA)).toBe(5)
        expect(map.get(keyB)).toBe(10)
        expect(map.get(keyACopy)).toBe(5)
    })
})
