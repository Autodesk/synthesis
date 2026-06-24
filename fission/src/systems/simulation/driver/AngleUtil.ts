/**
 * Moves `current` toward `target`, changing it by at most `maxDelta`.
 *
 * Used to rate-limit a position-controlled hinge's commanded setpoint so the motor is never
 * handed a target far from where the joint actually is. Bounding the per-frame setpoint jump
 * bounds the motor torque (and therefore the reaction torque on the parent body), which is what
 * keeps a swerve azimuth module from slamming to a new angle and kicking the chassis.
 *
 * `maxDelta` is treated as a magnitude; negative values are clamped to 0 (no movement). When the
 * remaining distance is within `maxDelta`, `target` is returned exactly so the setpoint settles.
 */
export function slewTowards(current: number, target: number, maxDelta: number): number {
    const limit = Math.max(0, maxDelta)
    const delta = target - current
    if (delta > limit) return current + limit
    if (delta < -limit) return current - limit
    return target
}

/**
 * Smallest signed rotation (in radians, within [-π, π]) that takes angle `from` to angle `to`.
 *
 * Used to drive a continuously-rotating azimuth hinge along the shortest path: because the hinge
 * angle wraps at ±π, the naive difference `to - from` can be up to ~2π and would send the module
 * the long way around. This returns the wrapped, shortest-path error instead.
 */
export function shortestAngleDelta(from: number, to: number): number {
    const twoPi = 2 * Math.PI
    let d = (to - from) % twoPi
    if (d > Math.PI) d -= twoPi
    if (d < -Math.PI) d += twoPi
    return d
}
