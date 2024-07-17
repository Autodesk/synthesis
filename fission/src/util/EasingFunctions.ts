/**
 * Source: https://easings.net/#easeOutQuad
 * 
 * @param x 
 * @returns 
 */
export function easeOutQuad(x: number): number {
    return 1 - (1 - x) * (1 - x);
}