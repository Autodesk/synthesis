import { describe, expect, test } from "vitest"

import Queue from "../../util/Queue"
import { random } from "../../util/Random"

describe("Queue Tests", () => {
    test("Create Empty", () => {
        const q = new Queue<number>()
        expect(q.dequeue()).toBeUndefined()
        expect(q.size).toBe(0)
    })

    test("Single Element", () => {
        const q = new Queue<number>()
        const element = 5
        q.enqueue(element)

        expect(q.size).toBe(1)
        expect(q.dequeue()).toBe(element)
        expect(q.size).toBe(0)
    })

    test("Five Elements", () => {
        const q = new Queue<number>()
        const elements = [1, 4, 5, 2, 6]
        q.enqueue(...elements)

        let expectedSize = elements.length
        for (const element of elements) {
            expect(q.size).toBe(expectedSize)
            expect(q.dequeue()).toBe(element)
            expectedSize--
        }
        expect(q.size).toBe(0)
        expect(q.dequeue()).toBeUndefined()
    })

    test("Add 5, Remove 3, Add 8, Remove All", () => {
        const q = new Queue<number>()
        const elementsA = [1, 4, 5, 2, 6]
        const elementsB = [1, 4, 5, 2, 6, 9, 10, 54]

        q.enqueue(...elementsA)
        expect(q.size).toBe(5)
        for (let i = 0; i < 3; i++) {
            expect(q.dequeue()).toBe(elementsA[i])
            expect(q.size).toBe(elementsA.length - (i + 1))
        }

        q.enqueue(...elementsB)
        expect(q.size).toBe(10)
        for (let i = 0; i < 2; i++) {
            expect(q.dequeue()).toBe(elementsA[i + 3])
            expect(q.size).toBe(9 - i)
        }
        for (let i = 0; i < 8; i++) {
            expect(q.dequeue()).toBe(elementsB[i])
            expect(q.size).toBe(7 - i)
        }
        expect(q.size).toBe(0)
        expect(q.dequeue()).toBeUndefined()
    })

    test("Queue Clone (100 Elements)", () => {
        const q1 = new Queue<number>()
        for (let i = 0; i < 100; i++) {
            q1.enqueue(Math.floor(random() * 400))
        }

        const q2 = q1.clone()
        expect(q2.size).toBe(q1.size)
        for (let i = 0; i < q1.size; i++) {
            expect(q2.dequeue()).toBe(q1.dequeue())
        }
    })
})
