export function generateId(root: string): string {
  return `${root}-${Math.random().toString(36).substring(2, 9)}`;
}
