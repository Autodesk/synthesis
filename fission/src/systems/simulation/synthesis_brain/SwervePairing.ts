export interface Vec3Like {
    x: number
    y: number
    z: number
}

/**
 * Pairs each wheel with its nearest azimuth hinge by Euclidean distance, consuming each
 * hinge as it is assigned so two wheels can never share the same hinge.
 *
 * Faithful to the original Unity implementation, which selected the closest azimuth for
 * each wheel and then removed it from the candidate pool (`potentialAzimuthDrivers.Remove`).
 * (The original had a latent bug where its running-minimum distance was never updated; this
 * port implements the clearly-intended nearest-with-consumption behavior.)
 *
 * Distances are compared squared, which preserves ordering and avoids the sqrt.
 *
 * @returns for each wheel index, the index of its assigned hinge, or -1 if no hinge remains.
 */
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
