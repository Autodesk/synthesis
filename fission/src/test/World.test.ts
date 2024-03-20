import World from "@/systems/World";
import { describe, test, expect, beforeEach, vi } from "vitest";

describe('World Tests', () => {

    beforeEach(() => {
        vi.resetAllMocks();
    });

    test('World Sanity Check', () => {
        expect(World.isAlive).toBeFalsy();

        // TODO: Find a way to mock window global
        // World.InitWorld();
        // expect(World.isAlive).toBeTruthy();
        // expect(World.SceneRenderer).toBeTruthy();
        // expect(World.DestroyWorld).toBeTruthy();
    });
})