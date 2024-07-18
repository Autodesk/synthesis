// import * as THREE from "three";
// import World from "../World";
// import WorldSystem from "../WorldSystem";
// import JOLT from "@/util/loading/JoltSyncLoader";
// import Jolt from "@barclah/jolt-physics";
// import MirabufSceneObject, { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject";
// import { OnContactAddedEvent } from "../physics/ContactEvents";
// import { FieldPreferencesKey } from "../preferences/PreferenceTypes";

// class ScoringSystem extends WorldSystem {
//     private points = 0

//     constructor() {
//         super()

//         // // Create and render a zone sensor
//         // const geometry = new THREE.BoxGeometry( 1, 1, 1 );
//         // const material = new THREE.MeshBasicMaterial( { color: 0x0000ff } );
//         // const cube = new THREE.Mesh( geometry, material );
//         // World.SceneRenderer.scene.add( cube );

//         // const zone = World.PhysicsSystem.CreateBox(
//         //     new THREE.Vector3(0.5, 0.5, 0.5),
//         //     undefined,
//         //     undefined,
//         //     undefined,
//         //     true
//         // )

//         // World.PhysicsSystem.JoltBodyInterface.AddBody(zone.GetID(), JOLT.EActivation_Activate)

//         // const field = ([...World.SceneRenderer.sceneObjects.entries()][0][1] as MirabufSceneObject).fieldPreferences?.scoringZones[0]

//         // // When zone collides with gamepiece, adds point
//         // const onContactAdded = (event: OnContactAddedEvent) => {
//         //     const body1 = event.message.body1
//         //     const body2 = event.message.body2

//         //     if (body1.GetIndexAndSequenceNumber() == zone.GetID().GetIndexAndSequenceNumber()) {
//         //         this.ZoneCollision(body2)
//         //     } else if (body2.GetIndexAndSequenceNumber() == zone.GetID().GetIndexAndSequenceNumber()) {
//         //         this.ZoneCollision(body1)
//         //     }
//         // }

//         // OnContactAddedEvent.AddListener(onContactAdded)


//         // ContactListenerTests
//         // OnContactPersistedEvent.AddListener((event: OnContactPersistedEvent) => console.log(`persisted: ${event.message.body1.GetIndexAndSequenceNumber()}`))
//         // OnContactRemovedEvent.AddListener((event: OnContactRemovedEvent) => console.log(`removed: ${event.message.GetSubShapeID1()}`))
//         // OnContactValidateEvent.AddListener((event: OnContactValidateEvent) => console.log(`validate: ${event.message.body1.GetID().GetIndexAndSequenceNumber()}`))
//     }

//     public Update(_: number): void {
        
//     }

//     public Destroy(): void {
        
//     }

//     private ZoneCollision(gpID: Jolt.BodyID) {
//         console.log(`Scoring zone collided with ${gpID.GetIndex()}`)

//         const associate = <RigidNodeAssociate>World.PhysicsSystem.GetBodyAssociation(gpID)
//         if (associate?.isGamePiece) {
//             this.points++
//             console.log(`points ${this.points}`)
//         }
//     }
// }

// export default ScoringSystem