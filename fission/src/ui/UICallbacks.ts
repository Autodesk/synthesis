export class UICallback<T extends unknown[], U> extends Function {
    private _userDefinedFunc?: (...args: T) => U
    private _defaultFunc?: (...args: T) => U
    // biome-ignore lint/style/useNamingConvention: used in the code returned in the constructor
    // @ts-expect-error allow ignored
    private __self__: UICallback<T, U>

    constructor() {
        super("...args", "return this.__self__.__call__(...args)")
        const self = this.bind(this)
        this.__self__ = self
        return self
    }

    setUserDefinedFunc(f: (...args: T) => U) {
        this._userDefinedFunc = f
    }

    setDefaultFunc(f: (...args: T) => U) {
        this._defaultFunc = f
    }

    __call__(...args: T): U | undefined {
        const userDefinedRet = this._userDefinedFunc?.(...args)
        const defaultRet = this._defaultFunc?.(...args)

        return userDefinedRet ?? defaultRet
    }
}
