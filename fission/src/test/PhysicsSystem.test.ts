import { test, expect, describe, assert } from 'vitest';
import PhysicsSystem from '../systems/physics/PhysicsSystem';

describe('Physics System Tests', () => {
    test('Convex Hull Shape (Cube)', () => {
        const points: Float32Array = new Float32Array(
            [
                 0.5, -0.5,  0.5,
                -0.5, -0.5,  0.5,
                -0.5, -0.5, -0.5,
                 0.5, -0.5, -0.5,
                 0.5,  0.5,  0.5,
                -0.5,  0.5,  0.5,
                -0.5,  0.5, -0.5,
                 0.5,  0.5, -0.5,
            ]
        );

        const system = new PhysicsSystem();
        const shapeResult = system.CreateConvexHull(points);

        assert(shapeResult.HasError() == false, shapeResult.GetError().c_str());
        expect(shapeResult.IsValid()).toBe(true);
        
        const shape = shapeResult.Get();

        expect(shape.GetVolume() - 1.0).toBeLessThan(0.001);
        expect(shape.GetCenterOfMass().Length()).toBe(0.0);

        shape.Release();
        system.Destroy();

    });
    test('Convex Hull Shape (Tetrahedron)', () => {
        const points: Float32Array = new Float32Array(
            [
                0.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                1.0, 0.0, 0.0,
                0.0, 0.0, 1.0
            ]
        );

        const system = new PhysicsSystem();
        const shapeResult = system.CreateConvexHull(points);

        assert(shapeResult.HasError() == false, shapeResult.GetError().c_str());
        expect(shapeResult.IsValid()).toBe(true);
        
        const shape = shapeResult.Get();
        const bounds = shape.GetLocalBounds();
        const boxSize = bounds.mMax.Sub(bounds.mMin);
        
        expect(boxSize.GetX() - 1.0).toBeLessThan(0.001);
        expect(boxSize.GetY() - 1.0).toBeLessThan(0.001);
        expect(boxSize.GetZ() - 1.0).toBeLessThan(0.001);
        expect(shape.GetVolume() - (1.0 / 6.0)).toBeLessThan(0.001);
        expect(shape.GetMassProperties().mMass - 6.0).toBeLessThan(0.001);

        shape.Release();
        system.Destroy();
    });
});