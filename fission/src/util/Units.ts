import { getFontSize } from "./Utility"

export class DOMUnitExpression {
    public value: DOMUnit
    public op?: (a: number, b: number) => number
    public expr?: DOMUnitExpression

    private constructor(value: DOMUnit, op?: (a: number, b: number) => number, expr?: DOMUnitExpression) {
        this.value = value
        this.op = op
        this.expr = expr
    }

    public static fromUnit(value: number, type?: DOMUnitTypes): DOMUnitExpression {
        return new DOMUnitExpression(new DOMUnit(value, type ?? "px"))
    }

    public evaluate(element: Element): number {
        if (this.op && this.expr) {
            return this.op(this.value.evaluate(element), this.expr.evaluate(element))
        } else {
            return this.value.evaluate(element)
        }
    }

    public add(b: DOMUnit | DOMUnitExpression): DOMUnitExpression {
        if (this.op && this.expr) {
            this.expr.add(b)
        } else {
            this.op = (a, b) => a + b
            this.expr = b instanceof DOMUnitExpression ? b : new DOMUnitExpression(b)    
        }

        return this;
    }

    public sub(b: DOMUnit | DOMUnitExpression): DOMUnitExpression {
        if (this.op && this.expr) {
            this.expr.sub(b)
        } else {
            this.op = (a, b) => a - b
            this.expr = b instanceof DOMUnitExpression ? b : new DOMUnitExpression(b)
        }

        return this
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

    public evaluate(element: Element): number {
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