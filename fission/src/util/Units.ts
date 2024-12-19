import { getFontSize } from "./Utility"

export class DOMUnitExpression {
    public exprA: DOMUnitExpression | DOMUnit
    public exprB?: DOMUnitExpression | DOMUnit
    public op?: (a: number, b: number) => number

    private constructor(
        exprA: DOMUnitExpression | DOMUnit,
        exprB?: DOMUnitExpression | DOMUnit,
        op?: (a: number, b: number) => number
    ) {
        this.exprA = exprA
        this.exprB = exprB
        this.op = op
    }

    public static fromUnit(value: number, type?: DOMUnitTypes): DOMUnitExpression {
        return new DOMUnitExpression(new DOMUnit(value, type ?? "px"))
    }

    public evaluate(element: Element): number {
        if (this.op && this.exprB) {
            return this.op(this.exprA.evaluate(element), this.exprB.evaluate(element))
        } else {
            return this.exprA.evaluate(element)
        }
    }

    public add(b: DOMUnit | DOMUnitExpression): DOMUnitExpression {
        return new DOMUnitExpression(this, b, (x, y) => x + y)
    }

    public sub(b: DOMUnit | DOMUnitExpression): DOMUnitExpression {
        return new DOMUnitExpression(this, b, (x, y) => x - y)
    }

    public mul(b: DOMUnit | DOMUnitExpression): DOMUnitExpression {
        return new DOMUnitExpression(this, b, (x, y) => x * y)
    }

    public div(b: DOMUnit | DOMUnitExpression): DOMUnitExpression {
        return new DOMUnitExpression(this, b, (x, y) => x / y)
    }
}

type DOMUnitTypes = "px" | "rem" | "w" | "h"

export class DOMUnit {
    public type: DOMUnitTypes
    public value: number

    public constructor(value: number, type: DOMUnitTypes) {
        this.value = value
        this.type = type
    }

    public evaluate(element: Element, verbose: boolean = false): number {
        if (verbose) console.debug(`${this.value} ${this.type} END UNIT`)
        switch (this.type) {
            case "px":
                return this.value
            case "rem":
                return this.value * getFontSize(element)
            case "w":
                return this.value * element.clientWidth
            case "h":
                return this.value * element.clientHeight
            default:
                return 0
        }
    }
}
