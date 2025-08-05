let seed = Date.now()

/**
 * Set the seed used when randomly generating a number.
 * Default is Date.now() when this module is created.
 *
 * @param   newSeed New seed to use when generating random numbers.
 *                  Must be greater than or equal to 1.0
 */
export function seedRandomGen(newSeed: number) {
    seed = newSeed >= 1.0 ? newSeed : 1.0
}

/**
 * Generate a random number.
 *
 * @returns Gives a random number x where, 0.0 <= x < 1.0
 */
export function random() {
    seed++
    const x = Math.abs(Math.sin(seed + 997) * 1425)
    return x - Math.floor(x)
}

export function randomColor() {
    return `#${(((1 << 24) * Math.random()) | 0).toString(16).padStart(6, "0")}`
}
