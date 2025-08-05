let nextHandleId = 0

export enum ProgressHandleStatus {
    IN_PROGRESS = 0,
    DONE = 1,
    ERROR = 2,
}

export class ProgressHandle {
    private _handleId: number
    private _title: string
    public message: string = ""
    public progress: number = 0.0
    public status: ProgressHandleStatus = ProgressHandleStatus.IN_PROGRESS

    public get handleId() {
        return this._handleId
    }
    public get title() {
        return this._title
    }

    public constructor(title: string) {
        this._handleId = nextHandleId++
        this._title = title

        this.push()
    }

    public update(message: string, progress: number, status?: ProgressHandleStatus) {
        this.message = message
        this.progress = progress
        if (status) {
            this.status = status
        }

        this.push()
    }

    public fail(message?: string) {
        this.update(message ?? "Failed", 1, ProgressHandleStatus.ERROR)
    }

    public done(message?: string) {
        this.update(message ?? "Done", 1, ProgressHandleStatus.DONE)
    }

    public push() {
        ProgressEvent.dispatch(this)
    }
}

export class ProgressEvent extends Event {
    public static readonly EVENT_KEY = "ProgressEvent"

    public handle: ProgressHandle

    private constructor(handle: ProgressHandle) {
        super(ProgressEvent.EVENT_KEY)

        this.handle = handle
    }

    public static dispatch(handle: ProgressHandle) {
        window.dispatchEvent(new ProgressEvent(handle))
    }

    public static addListener(func: (e: ProgressEvent) => void) {
        window.addEventListener(this.EVENT_KEY, func as (e: Event) => void)
    }

    public static removeListener(func: (e: ProgressEvent) => void) {
        window.removeEventListener(this.EVENT_KEY, func as (e: Event) => void)
    }
}
