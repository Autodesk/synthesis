import { MAP_BOUNDARY_Y } from "@/systems/physics/PhysicsSystem"
import World from "../World"

class GamePiecePositionTracker {
    public static update(): void {
        const field = World.sceneRenderer.mirabufSceneObjects.getField()
        if (!field || field.gamePieces.length === 0) return

        field.gamePieces.forEach(({ bodyId }) => {
            const body = World.physicsSystem.getBody(bodyId)
            if (body && body.GetPosition().GetY() <= MAP_BOUNDARY_Y) {
                field.destroyGamePiece(bodyId)
            }
        })
    }
}

export default GamePiecePositionTracker
