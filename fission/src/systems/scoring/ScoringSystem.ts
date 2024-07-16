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

        const onContactAdded = (e: Event) => {
            const event = e as OnContactAddedEvent
            const body1 = event.message.body1
            const body2 = event.message.body2

            if (body1.GetID().GetIndex() == zone.GetID().GetIndex()) {
                console.log(`${body1.GetID().GetIndex()} collided with ${body2.GetID().GetIndex()}`)

                const fields = [...World.SceneRenderer.sceneObjects.entries()]
                    .filter(x => {
                        if (!(x[1] instanceof MirabufSceneObject)) {
                            return false
                        } else {
                            return x[1].miraType == MiraType.FIELD
                        }
                    })
                    .map(x => x[1]) as MirabufSceneObject[]

                fields.forEach(x => {
                    x.mechanism.nodeToBody.forEach( (bodyID, rigidNodeID) => {
                        const associate = World.PhysicsSystem.GetBodyAssociation(bodyID)
                        if (associate instanceof RigidNodeAssociate) {
                            if (associate.isGamePiece) {
                                const body2ID = body2.GetID().GetIndex()
                                const gamepieceID = +associate.rigidNodeId.replace("_gp", "") // Gets number without gamepiece tag

                                if (body2ID == gamepieceID) {
                                    this.points++
                                    console.log(this.points)
                                }
                            }
                        }
                    })
                })
            }

        }

        OnContactAddedEvent.AddListener(onContactAdded)
    }

    public Update(_: number): void {
        
    }

    public Destroy(): void {
        
    }
}

export default ScoringSystem