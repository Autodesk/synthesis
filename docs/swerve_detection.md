# Swerve Module Detection & Configuration

This document describes the **developer-facing** implementation details for how swerve modules are discovered, paired,
and configured.

## Overview

We treat each swerve pod as two abstract drivers:

* \`rotator\` for the azimuth (steering) joint
* \`driver\` for the drive (rolling) joint

At runtime, we:

1. Collect all existing drivers for this robot
2. Filter out which are candidate steering joints
3. Collect all drive wheels
4. Pair each wheel with its closest steering joint (by anchor position)
5. Wrap these "driver and rotator" pairs

With this approach, as long as your CAD export yields one steering and one drive joint per pod, the system will self‑configure.
This also comes with the advantage of zero overhead from the perspective of our robot exporter.

## Prerequisites & Assumptions

1. **One-to-one pods**: Each physical wheel pod must export exactly one steering joint and one rolling joint.
2. **Azimuth axis orientation**: Steering axes must be nearly horizontal (perpendicular to gravity).
3. **Anchor proximity**: The world‑space `Anchor` of a `WheelDriver` and its matching `RotationalDriver` must be spatially close (co‑located) within a pod.
4. **Driver registration**: The assembly import process must register both driver types with `SimulationManager.Drivers[robotName]`.

Violating any of these may cause detection to fail; in that case, a fall back occurs to a different drivetrain mode.
One that does not require rotational drivers (e.g., Tank or Arcade).

## Detection Algorithm

### Gathering Drivers

```csharp
var allDrivers = ...
```

This collection contains all joint instances exported from Fusion, created from the imported
mirabuf assembly.

### Filtering Azimuth (Steering) Drivers

```csharp
var potentialAzimuthDrivers = allDrivers
    .OfType<RotationalDriver>()         // only hinge joints
    .Where(d => !d.IsWheel)            // exclude any rotational drivers marked as wheels
    .Where(d => IsHorizontal(d.Axis))  // axis nearly horizontal
    .ToList();
```

* `IsHorizontal()` is closely implemented as:

  ```csharp
  bool IsHorizontal(Vector3 axis) =>
    (axis - Vector3.Dot(Vector3.up, axis) * Vector3.up).magnitude < 0.05f;
  ```

* This selects only steering pivots whose hinge axis lies in the horizontal plane.

### Identifying Wheel Drivers

```csharp
var wheelDrivers = allDrivers.OfType<WheelDriver>();
```

All `WheelDriver` instances correspond to the actual drive wheels. These are the rotational joints
marked as wheels during the robot export process.

### Pairing Algorithm (Nearest‑Neighbor)

```csharp
if (potentialAzimuthDrivers.Count < wheelDrivers.Count)
    return; // not enough pods

var modules = new (RotationalDriver azimuth, WheelDriver drive)[wheelDrivers.Count];
int i = 0;

foreach (var wheel in wheelDrivers) {
    // find the steering joint whose Anchor is closest to this wheel’s Anchor
    var closest = potentialAzimuthDrivers
        .OrderBy(d => (d.Anchor - wheel.Anchor).sqrMagnitude)
        .First();

    modules[i++] = (closest, wheel);
    potentialAzimuthDrivers.Remove(closest);
}
```

## Common Pitfalls

* **Axis tilt**: If the azimuth hinge axis tilts more than \~3°, it may not be detected as horizontal.
* **Mismatched anchors**: CAD pods must export pivot and wheel with matching origin positions.
* **Missing drivers**: Ensure both joints appear in the mirabuf import.

Refer to the debug logs for `Failed to switch to 'Swerve'` messages if detection returns false.

## Final Notes

This implementation was decided upon at the current time to avoid adding additional complexity
to the robot export process. This however does not mean that this automatic swerve module detection
system cannot also exist alongside a more comprehensive system implemented within the exporter.

Such exporter additions (if done correctly) have the advantage of improving clarity for how swerve
is handled within the simulator.

For more information on the original implementation of this swerve module detection system you can here:

https://github.com/BrandonPacewic/synthesis/blob/636668d534564610eca7e80db856f2eb43fc60e9/engine/Assets/Scripts/SimObjects/RobotSimObject.cs#L540-L579

This commit hash represents v6 of Synthesis.

> *Last updated: 2025‑07‑08*
