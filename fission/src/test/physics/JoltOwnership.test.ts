import { describe, expect, test } from "vitest"
import { getPerpendicular } from "@/systems/physics/ConstraintSettingsUtilities"
import JOLT from "@/util/loading/JoltSyncLoader"

/**
 * what you have to destroy and why
 *
 * jolt classes are c++ structs behind emscripten bindings. assigning into one
 * (`settings.mPoint1 = v`) memcpys the bytes in, so the struct never owns your vector.
 * destroying the settings won't free it
 *
 * so ownership is all about where the vector came from
 *
 *   - `new JOLT.Vec3(...)` (and the `convert*` helpers) is a real malloc, it's yours, destroy it
 *     or it leaks
 *   - anything a bound method hands back (`Normalized()`, `AddRVec3()`) is NOT allocated. the
 *     binding just has one static per method:
 *
 *         Vec3* emscripten_bind_Vec3_Normalized_0(Vec3* self) {
 *             static Vec3 temp;             // one slot, reused forever
 *             temp = self->Normalized();    // 16 bytes overwritten, no malloc
 *             return &temp;
 *         }
 *
 *     nothing was allocated so nothing leaks and nothing can be freed. destroying one hands free()
 *     a data segment address malloc never gave out
 *
 * rule: destroy what you allocated, not what you were handed
 *
 * you can't see which is which from a signature, so the helpers in `ConstraintSettingsUtilities`
 * never let a static escape - they copy it out first. `createAnchorPoint()` and
 * `getPerpendicular()` give you heap vectors that are yours to destroy
 */

const ptr = (obj: object) => JOLT.getPointer(obj)

describe("Jolt Physics Memory Ownership", () => {
    describe("a vector from `new JOLT.Vec3` is heap memory that you own", () => {
        test("every allocation gets its own distinct address", () => {
            const a = new JOLT.Vec3(1, 2, 3)
            const b = new JOLT.Vec3(1, 2, 3)

            // fresh address every time
            expect(ptr(a)).not.toBe(ptr(b))

            JOLT.destroy(a)
            JOLT.destroy(b)
        })

        test("destroy() returns the block to the allocator, which hands the address straight back", () => {
            const original = new JOLT.Vec3(1, 2, 3)
            const originalPtr = ptr(original)

            JOLT.destroy(original)

            // freed block goes back to the allocator, next alloc gets it right back
            const reused = new JOLT.Vec3(4, 5, 6)
            expect(ptr(reused)).toBe(originalPtr)

            JOLT.destroy(reused)
        })
    })

    describe("a normalized vector is a shared static, so it cannot leak", () => {
        test("Normalized() returns the same fixed address on every call", () => {
            const axis = new JOLT.Vec3(3, 0, 0)

            const first = ptr(axis.Normalized())
            const second = ptr(axis.Normalized())
            console.log(`Normalized() -> ${first}, then -> ${second} (one fixed slot)`)

            // same slot both times
            expect(second).toBe(first)

            JOLT.destroy(axis)
        })

        test("the address lives below the heap, and malloc never hands it out", () => {
            const axis = new JOLT.Vec3(3, 0, 0)
            const staticPtr = ptr(axis.Normalized())

            // hacky way to find where malloc's block starts - just alloc a bunch and take the lowest
            const heapVectors = Array.from({ length: 5_000 }, (_, i) => new JOLT.Vec3(i, i, i))
            const lowestHeapPtr = Math.min(...heapVectors.map(ptr))
            console.log(`static temp at ${staticPtr}; malloc's block starts at ${lowestHeapPtr}`)

            // static sits below all of it, so malloc never owned that address
            expect(staticPtr).toBeLessThan(lowestHeapPtr)
            expect(heapVectors.every(v => ptr(v) !== staticPtr)).toBe(true)

            heapVectors.forEach(v => JOLT.destroy(v))
            JOLT.destroy(axis)
        })

        test("repeated calls accumulate nothing, which is what 'does not leak' means", () => {
            const axis = new JOLT.Vec3(3, 0, 0)

            const staticPtrs = new Set<number>()
            const heapPtrs = new Set<number>()
            const heapVectors: (typeof axis)[] = []

            for (let i = 0; i < 2_000; i++) {
                staticPtrs.add(ptr(axis.Normalized()))

                const allocated = new JOLT.Vec3(1, 0, 0)
                heapVectors.push(allocated)
                heapPtrs.add(ptr(allocated))
            }

            // 2000 calls, one address. nothing piles up so there's nothing to reclaim and no
            // destroy to forget. you can't leak what you never accumulated
            expect(staticPtrs.size).toBe(1)
            // same loop with real allocs grows forever, that's the thing that actually leaks
            expect(heapPtrs.size).toBe(2_000)

            heapVectors.forEach(v => JOLT.destroy(v))
            JOLT.destroy(axis)
        })

        test("the shared slot is clobbered by the next call, so results must be copied out", () => {
            const x = new JOLT.Vec3(3, 0, 0).Normalized()
            expect([x.GetX(), x.GetY(), x.GetZ()]).toEqual([1, 0, 0])

            // normalize anything else and the one slot x points at is gone
            const y = new JOLT.Vec3(0, 7, 0).Normalized()

            expect(ptr(x)).toBe(ptr(y))
            expect([x.GetX(), x.GetY(), x.GetZ()]).toEqual([0, 1, 0])
        })

        test("getPerpendicular() copies out of the static, so callers get a heap vector they own", () => {
            const axis = new JOLT.Vec3(1, 0, 0)
            const staticPtr = ptr(axis.Normalized())

            // tryGetPerpendicular copies out of the shared slot before returning, so what you get
            // back is yours to destroy like everything else these helpers hand out
            const first = getPerpendicular(axis)
            const second = getPerpendicular(axis)
            console.log(`getPerpendicular() -> ${ptr(first)}, then -> ${ptr(second)}; static at ${staticPtr}`)

            expect(ptr(first)).not.toBe(staticPtr)
            expect(ptr(first)).toBeGreaterThan(staticPtr)

            // separate allocs, so one call can't stomp a result you're still holding
            expect(ptr(second)).not.toBe(ptr(first))
            expect([first.GetX(), first.GetY(), first.GetZ()]).toEqual([0, 1, 0])

            JOLT.destroy(first)
            JOLT.destroy(second)
            JOLT.destroy(axis)
        })
    })

    describe("settings structs copy the bytes in and never take ownership", () => {
        test("the member lives inside the struct, so mutating the source cannot reach it", () => {
            const settings = new JOLT.HingeConstraintSettings()
            const anchor = new JOLT.RVec3(1.5, 2.5, 3.5)

            settings.mPoint1 = anchor

            // mPoint1 is by value (`RVec3 mPoint1;`) so the struct already has 16 bytes for it. the
            // setter can only memcpy in, there's nowhere to stash a pointer. that's why the getter
            // hands back an address inside the struct
            expect(ptr(settings.mPoint1)).not.toBe(ptr(anchor))
            expect(ptr(settings.mPoint1)).toBeGreaterThan(ptr(settings))

            anchor.SetX(99)

            // two separate spots, the copy already happened
            expect(anchor.GetX()).toBe(99)
            expect(settings.mPoint1.GetX()).toBe(1.5)

            // struct never kept the pointer so its destructor can't free anchor. heap vector you
            // assigned in is still yours and still leaks if you don't destroy it
            JOLT.destroy(settings)
            expect(anchor.GetX()).toBe(99)
            JOLT.destroy(anchor)
        })
    })
})
