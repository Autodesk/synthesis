export class UICallback<T extends unknown[], U> extends Function {
    private _funcs: Set<(...args: T) => U>
    private _maxSize: number

    constructor(maxSize: number = -1) {
        super("...args", "return this.__self__.__call__(...args)")
        const self = this.bind(this)
        this.__self__ = self
        self._funcs = new Set()
        self._maxSize = maxSize
        return self
    }

    addFunc(f: (...args: T) => U) {
        if (this._maxSize !== -1 && this._funcs.size >= this._maxSize) {
            throw new Error(`Cannot add another function to UICallback! Already at max size of ${this._maxSize}`)
        }

        this._funcs.add(f)
        console.log(`TRYING ADDING FUNC ${f} FOR ${this._funcs.size} FUNCS`)
    }

    setFunc(f: (...args: T) => U) {
        this._funcs.clear()
        this._funcs.add(f)
    }

    setFuncs(f: ((...args: T) => U)[]) {
        this._funcs.clear()
        f.forEach(this._funcs.add)
    }

    removeFunc(f: (...args: T) => U) {
        this._funcs.delete(f)
    }

    __call__(...args: T): U {
        console.log("CALLING UI CALLBACK", args)
        return [...this._funcs].reduce((_prev, f, _i, _arr) => f(...args), undefined as U)
    }
}
