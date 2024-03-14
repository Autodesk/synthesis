class Queue<T> {
    
    private _head?: LinkedNode<T>;
    private _tail?: LinkedNode<T>;
    private _size: number = 0;

    public get size() { return this._size; }

    constructor() { }

    public Enqueue(...items: T[]) {
        for (const item of items) {
            const node = new LinkedNode<T>(item);
            if (this._head) {
                this._tail!.next = node;
            } else {
                this._head = node;
            }
            this._tail = node;

            this._size++;
        }
    }

    public Dequeue(): T | undefined {
        let retVal: T | undefined;
        if (this._head) {
            retVal = this._head.value;
            this._head = this._head.next;
            !this._head && (this._tail = undefined);
            this._size--;
        }
        return retVal;
    }
}

class LinkedNode<T> {
    public value: T;
    public next?: LinkedNode<T>;

    constructor(value: T, next?: LinkedNode<T>) {
        this.value = value;
        this.next = next;
    }
}

export default Queue;