/**
 * DEBUG-ONLY. The chassis command the mecanum harness' mocked InputSystem hands back.
 *
 * Deliberately dependency-free: the `vi.mock("@/systems/input/InputSystem")` factory imports this,
 * and anything it pulls in would be loaded before the mock is installed.
 */
export interface Command {
    forward: number
    strafe: number
    turn: number
}

export const harnessCommand: Command = { forward: 0, strafe: 0, turn: 0 }

export function setHarnessCommand(command: Command): void {
    harnessCommand.forward = command.forward
    harnessCommand.strafe = command.strafe
    harnessCommand.turn = command.turn
}
