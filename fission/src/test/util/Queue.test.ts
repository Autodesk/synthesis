import { test, expect, describe } from 'vitest';

import Queue from '../../util/Queue';

describe('Queue Tests', () => {
    test('Create Empty', () => {
        const q = new Queue<number>();
        expect(q.Dequeue()).toBeUndefined();
        expect(q.size).toBe(0);
    });

    test('Single Element', () => {
        const q = new Queue<number>();
        const element = 5;
        q.Enqueue(element);

        expect(q.size).toBe(1);
        expect(q.Dequeue()).toBe(element);
        expect(q.size).toBe(0);
    });

    test('Five Elements', () => {
        const q = new Queue<number>();
        const elements = [ 1, 4, 5, 2, 6 ];
        q.Enqueue(...elements);

        let expectedSize = elements.length;
        for (const element of elements) {
            expect(q.size).toBe(expectedSize);
            expect(q.Dequeue()).toBe(element);
            expectedSize--;
        }
        expect(q.size).toBe(0);
        expect(q.Dequeue()).toBeUndefined();
    });

    test('Add 5, Remove 3, Add 8, Remove All', () => {
        const q = new Queue<number>();
        const elementsA = [ 1, 4, 5, 2, 6 ];
        const elementsB = [ 1, 4, 5, 2, 6, 9, 10, 54 ];

        q.Enqueue(...elementsA);
        expect(q.size).toBe(5);
        for (let i = 0; i < 3; i++) {
            expect(q.Dequeue()).toBe(elementsA[i]);
            expect(q.size).toBe(elementsA.length - (i + 1));
        }

        q.Enqueue(...elementsB);
        expect(q.size).toBe(10);
        for (let i = 0; i < 2; i++) {
            expect(q.Dequeue()).toBe(elementsA[i + 3]);
            expect(q.size).toBe(9 - i);
        }
        for (let i = 0; i < 8; i++) {
            expect(q.Dequeue()).toBe(elementsB[i]);
            expect(q.size).toBe(7 - i);
        }
        expect(q.size).toBe(0);
        expect(q.Dequeue()).toBeUndefined();
    });
});