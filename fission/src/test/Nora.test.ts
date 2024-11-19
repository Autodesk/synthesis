import { Receiver, Supplier } from "@/systems/simulation/Nora"
import { beforeEach, describe, expect, test, vi } from "vitest"

describe("Nora Tests", () => {
    beforeEach(() => {
        vi.resetAllMocks()
    })

    test("Parameter Assertion", () => {
        const testSupplier: Supplier = {
            getSupplierValue() {
                return "0.5"
            }
        }

        const testReceiver: Receiver = {
            setReceiverValue(val: [ number ]) {
                console.debug(`Received Value: ${val[0]}`)
            }
        }

        expect(() => testReceiver.setReceiverValue(testSupplier.getSupplierValue())).toThrowError()
    })
})
