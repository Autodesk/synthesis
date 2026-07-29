/**
 * Conversion between a swerve module's commanded heading and its azimuth hinge angle.
 *
 * A Jolt hinge reads zero in the pose its constraint was created in, which is the robot's CAD rest
 * pose -- not "wheel pointing forward". A swerve robot parked in an X has every module at 45 degrees
 * there, and each module's tread can point anywhere at all, so a heading is not a hinge angle.
 *
 * Both quantities below are measured in the chassis frame, where a module's rest orientation is the
 * identity, so they stay valid however the robot is later posed.
 */

/** Reference basis for headings: forward is assembly +Z, right is assembly +X. */
export interface ModuleForward {
    x: number
    y: number
    z: number
}

/**
 * Heading a module's tread points at when its hinge reads zero.
 *
 * @param localForward the wheel's rolling direction in its vehicle body's (the pod's) local space.
 */
export function moduleRestHeading(localForward: ModuleForward): number {
    return Math.atan2(localForward.x, localForward.z)
}

/**
 * Whether a module's heading advances with its hinge angle (+1) or against it (-1).
 *
 * A positive hinge angle is a right-handed rotation of the pod about the hinge axis, and heading
 * increases with a right-handed rotation about up, so the two agree only when the axis points up.
 *
 * @param localHingeAxisUpComponent the up component of the hinge axis in body 1's (the chassis') space.
 */
export function moduleHingeSign(localHingeAxisUpComponent: number): number {
    return localHingeAxisUpComponent >= 0 ? 1 : -1
}

export function wrapToPi(radians: number): number {
    let wrapped = radians
    while (wrapped > Math.PI) wrapped -= 2 * Math.PI
    while (wrapped < -Math.PI) wrapped += 2 * Math.PI
    return wrapped
}

/** Hinge angle that aims a module's tread at `heading`. */
export function hingeAngleForHeading(heading: number, restHeading: number, hingeSign: number): number {
    return wrapToPi(hingeSign * (heading - restHeading))
}
