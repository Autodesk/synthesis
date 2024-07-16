import * as THREE from "three";
import World from "../World";
import WorldSystem from "../WorldSystem";
import JOLT from "@/util/loading/JoltSyncLoader";
import Jolt from "@barclah/jolt-physics";
import MirabufSceneObject, { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject";
import { MiraType } from "@/mirabuf/MirabufLoader";

class ScoringSystem extends WorldSystem {
    private zone: Jolt.Body;

    private _contactListener: Jolt.ContactListenerJS;

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

        this._contactListener = new JOLT.ContactListenerJS()

        this._contactListener.OnContactAdded = (bodyPtr1, bodyPtr2, manifoldPtr, settingsPtr) => {
            const body1 = JOLT.wrapPointer(bodyPtr1, JOLT.Body) as Jolt.Body;
            const body2 = JOLT.wrapPointer(bodyPtr2, JOLT.Body) as Jolt.Body;
            const manifold = JOLT.wrapPointer(manifoldPtr, Jolt.ContactManifold);
            const settings = JOLT.wrapPointer(settingsPtr, Jolt.ContactSettings);

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

                                console.log(`${(gamepieceID)} and ${body2ID}`)
                                if (body2ID == gamepieceID) {
                                    this.points++
                                    console.log(this.points)
                                }
                            }
                        }
                    })
                })
            }
        };

        this._contactListener.OnContactPersisted = (bodyPtr1, bodyPtr2, manifoldPtr, settingsPtr) => {

        }

        this._contactListener.OnContactRemoved = (subShapePairPtr) => {
            const shapePair = JOLT.wrapPointer(subShapePairPtr, JOLT.SubShapeIDPair) as Jolt.SubShapeIDPair
            const body1ID = shapePair.GetBody1ID()
            const body2ID = shapePair.GetBody2ID()

            if (body1ID.GetIndex() == zone.GetID().GetIndex()) 
                console.log(`${body1ID.GetIndex()} removed from ${body2ID.GetIndex()}`)

        }

        this._contactListener.OnContactValidate = (bodyPtr1, bodyPtr2, inBaseOffsetPtr, inCollisionResultPtr) => {
            const body1 = JOLT.wrapPointer(bodyPtr1, Jolt.Body);
            const body2 = JOLT.wrapPointer(bodyPtr2, Jolt.Body);
            const collideShapeResult = JOLT.wrapPointer(inCollisionResultPtr, Jolt.CollideShapeResult);
            return JOLT.ValidateResult_AcceptAllContactsForThisBodyPair
        }

        World.PhysicsSystem.JoltPhysicsSystem.SetContactListener(this._contactListener)
    }

    public Update(_: number): void {
        
    }

    public Destroy(): void {
        
    }
}

export default ScoringSystem