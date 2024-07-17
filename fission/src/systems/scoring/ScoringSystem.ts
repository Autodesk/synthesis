import * as THREE from "three";
import World from "../World";
import WorldSystem from "../WorldSystem";
import JOLT from "@/util/loading/JoltSyncLoader";
import Jolt from "@barclah/jolt-physics";
import MirabufSceneObject, { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject";
import { MiraType } from "@/mirabuf/MirabufLoader";
import { OnContactAddedEvent } from "../physics/ContactEvents";

class ScoringSystem extends WorldSystem {
    private zone: Jolt.Body;

    private points = 0

    constructor() {
        super()

        // Create and render a zone sensor
        const geometry = new THREE.BoxGeometry( 1, 1, 1 );
        const material = new THREE.MeshBasicMaterial( { color: 0x0000ff } );
        const cube = new THREE.Mesh( geometry, material );
        World.SceneRenderer.scene.add( cube );

        const zone = World.PhysicsSystem.CreateBox(
            new THREE.Vector3(0.5, 0.5, 0.5),
            undefined,
            undefined,
            undefined,
            true
        )

        this.zone = zone

        World.PhysicsSystem.JoltBodyInterface.AddBody(zone.GetID(), JOLT.EActivation_Activate)

        // When zone collides with gamepiece, adds point
        const onContactAdded = (event: OnContactAddedEvent) => {
            const body1 = event.message.body1
            const body2 = event.message.body2

            if (body1.GetIndexAndSequenceNumber() == zone.GetID().GetIndexAndSequenceNumber()) {
                this.ZoneCollision(body2)
            } else if (body2.GetIndexAndSequenceNumber() == zone.GetID().GetIndexAndSequenceNumber()) {
                this.ZoneCollision(body1)
            }
        }

        OnContactAddedEvent.AddListener(onContactAdded)
    }

    public Update(_: number): void {
        
    }

    public Destroy(): void {
        
    }

    private ZoneCollision(gpID: Jolt.BodyID) {
        console.log(`Scoring zone collided with ${gpID.GetIndex()}`)

        const associate = <RigidNodeAssociate>World.PhysicsSystem.GetBodyAssociation(gpID)
        if (associate?.isGamePiece) {
            this.points++
            console.log(`points ${this.points}`)
        }
    }
}

export default ScoringSystem