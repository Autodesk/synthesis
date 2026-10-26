export class UICallback<T extends unknown[], U> extends Function {
    private _userDefinedFunc?: (...args: T) => U
    private _defaultFunc?: (...args: T) => U
    // @ts-expect-error allow ignored
    // biome-ignore lint/style/useNamingConvention: this name has special meaning here
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

    // biome-ignore lint/style/useNamingConvention: this name has special meaning here
    __call__(...args: T): U | undefined {
        const userDefinedRet = this._userDefinedFunc?.(...args)
        const defaultRet = this._defaultFunc?.(...args)

        return userDefinedRet ?? defaultRet
    }
}
