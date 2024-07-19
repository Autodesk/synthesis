/**
 * Delay instantiation.
 */
class Lazy<T> {
    private _value: T | undefined
    private _initValue: () => T

    public constructor(initValue: () => T) {
        this._initValue = initValue
    }

    public getValue(): T {
        if (!this._value) {
            this._value = this._initValue()
        }
        return this._value
    }
}

export default Lazy
