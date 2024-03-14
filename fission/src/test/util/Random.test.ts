import { test, expect, describe } from 'vitest';
import * as Random from '../../util/Random';

describe('Random Number Generator Tests', () => {
    test('Range Compliance', () => {
        Random.SeedRandomGen(67542431);
        for (let i = 0; i < 99; i++) {
            const a = Random.Random();
            expect(a).toBeLessThan(1.0);
            expect(a).toBeGreaterThanOrEqual(0.0);
        }
    });
});