# Swerve Module Detection & Configuration

This document describes the **developer-facing** implementation details for how swerve modules are
discovered, paired, and configured in Fission (the TypeScript simulator).

## Overview

We treat each swerve pod as two drivers that already exist after a normal mirabuf import:

- a `HingeDriver` for the azimuth (steering) joint
- a `WheelDriver` for the drive (rolling) joint

At runtime, when the drivetrain type is `Swerve`, we:

1. Collect this robot's drivers from its simulation layer.
2. Identify candidate steering (azimuth) hinges by their axis orientation.
3. Collect the drive wheels.
4. Pair each wheel with its nearest azimuth hinge by world-space position.
5. Build a `SwerveDriveBehavior` from those wheel/hinge pairs.

As long as the CAD export yields one steering and one drive joint per pod, the system
self-configures with zero additional work in the exporter.

## Prerequisites & Assumptions

1. **One-to-one pods**: each physical pod exports exactly one steering joint and one rolling joint.
2. **Azimuth axis orientation**: the steering axis is nearly **vertical** (parallel to up). A real
   swerve module pivots about a vertical post, so its hinge axis points along gravity.
3. **Anchor proximity**: a `WheelDriver`'s world anchor and its matching hinge's world anchor are
   spatially close within a pod.
4. **Driver registration**: the import registers both driver types on the robot's simulation layer.

If these do not hold, detection fails and the robot falls back to arcade drive (which needs no
steering joints).

## Detection Algorithm

Detection lives in `SynthesisBrain.detectSwerve()`
(`fission/src/systems/simulation/synthesis_brain/SynthesisBrain.ts`).

### Identifying azimuth hinges

A hinge is an azimuth (steering) hinge when its world-space rotation axis is essentially vertical.
We take the axis, project out the component along world-up, and accept it when the remaining
(perpendicular) magnitude is below `SWERVE_AXIS_TOLERANCE` (`0.05`, ported verbatim from the
original implementation):

```ts
const up = new THREE.Vector3(0, 1, 0)
const swerveHinges: HingeDriver[] = []
hingeDrivers.forEach(h => {
    const a = h.worldAxis
    const axis = new THREE.Vector3(a.GetX(), a.GetY(), a.GetZ()).normalize()
    // Magnitude of the axis component perpendicular to up; near zero means the axis is vertical.
    const perpMag = axis.clone().sub(up.clone().multiplyScalar(up.dot(axis))).length()
    if (perpMag < SynthesisBrain.SWERVE_AXIS_TOLERANCE) swerveHinges.push(h)
})
```

> **Note on the axis vector.** `HingeDriver.worldAxis` uses `Multiply3x3` (rotation only) rather
> than `MulVec3`. The axis is a direction, not a point; `MulVec3` would add the body's world
> position and corrupt it.

Robots spawn upright, so world-up is used directly (the original used the grounded node's up
vector).

### Swerve decision

```ts
const inSwerve = wheelDrivers.length > 0 && swerveHinges.length >= wheelDrivers.length
```

The robot is treated as swerve only when there is at least one wheel and at least as many azimuth
hinges as wheels. If the drivetrain is set to `Swerve` but detection fails, `configure()` logs a
warning and falls back to arcade, leaving every hinge available as an arm joint.

## Pairing Algorithm (nearest-neighbor, consuming)

Pairing lives in `pairNearestHinges()`
(`fission/src/systems/simulation/synthesis_brain/SwervePairing.ts`). Each wheel claims its closest
hinge and that hinge is removed from the pool, so two wheels can never share a hinge:

```ts
export function pairNearestHinges(wheelPositions: Vec3Like[], hingePositions: Vec3Like[]): number[] {
    const available = hingePositions.map((_, i) => i)
    return wheelPositions.map(wheel => {
        let minDistSq = Infinity
        let closestSlot = -1
        available.forEach((hingeIndex, slot) => {
            const hinge = hingePositions[hingeIndex]
            const dx = wheel.x - hinge.x
            const dy = wheel.y - hinge.y
            const dz = wheel.z - hinge.z
            const distSq = dx * dx + dy * dy + dz * dz
            if (distSq < minDistSq) {
                minDistSq = distSq
                closestSlot = slot
            }
        })
        if (closestSlot === -1) return -1
        const [chosen] = available.splice(closestSlot, 1)
        return chosen
    })
}
```

`createSwerveDriveBehavior()` feeds this wheel positions from each wheel's world transform and hinge
positions from each hinge's `worldAnchor`, then builds the `SwerveDriveBehavior` from the paired
drivers. (Distances are compared squared to avoid a square root.)

## Common Pitfalls

- **Axis tilt**: if the azimuth hinge axis tilts away from vertical by more than the tolerance it
  will not be detected.
- **Mismatched anchors**: a pod's steering and drive joints must export with close world anchors, or
  pairing may mismatch.
- **Missing drivers**: both joints must appear in the mirabuf import.

Check the warning logged by `configure()` if a robot set to swerve falls back to arcade.

## Final Notes

The original (Unity / C# v6) implementation this was ported from:

https://github.com/Autodesk/synthesis/blob/636668d534564610eca7e80db856f2eb43fc60e9/engine/Assets/Scripts/SimObjects/RobotSimObject.cs#L540-L579

> *Last updated: 2026-06-24*
