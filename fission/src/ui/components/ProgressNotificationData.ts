let nextHandleId = 0

export enum ProgressHandleStatus {
    inProgress = 0,
    Done = 1,
    Error = 2
}

export class ProgressHandle {
    
    private _handleId: number
    private _title: string
    public message: string = ""
    public progress: number = 0.0
    public status: ProgressHandleStatus = ProgressHandleStatus.inProgress

    public get handleId() { return this._handleId }
    public get title() { return this._title }

    public constructor(title: string) {
        this._handleId = nextHandleId++
        this._title = title
    }

    public Update(message: string, progress: number, status?: ProgressHandleStatus) {
        this.message = message
        this.progress = progress
        status && (this.status = status)

        this.Push()
    }

    public Fail(message?: string) {
        this.Update(message ?? "Failed", 1, ProgressHandleStatus.Error)
    }

    public Done(message?: string) {
        this.Update(message ?? "Done", 1, ProgressHandleStatus.Done)
    }

    public Push() {
        ProgressEvent.Dispatch(this)
    }
}

export class ProgressEvent extends Event {
    public static readonly EVENT_KEY = 'ProgressEvent'

    public handle: ProgressHandle

    private constructor(handle: ProgressHandle) {
        super(ProgressEvent.EVENT_KEY)

        this.handle = handle
    }

    public static Dispatch(handle: ProgressHandle) {
        window.dispatchEvent(new ProgressEvent(handle))
    }

    public static AddListener(func: (e: ProgressEvent) => void) {
        window.addEventListener(this.EVENT_KEY, func as (e: Event) => void)
    }

    public static RemoveListener(func: (e: ProgressEvent) => void) {
        window.removeEventListener(this.EVENT_KEY, func as (e: Event) => void)
    }
}