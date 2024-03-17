import { mirabuf } from "@/proto/mirabuf";
import SceneObject from "../systems/scene/SceneObject";
import MirabufInstance from "./MirabufInstance";
import { LoadMirabufRemote } from "./MirabufLoader";
import MirabufParser, { ParseErrorSeverity } from "./MirabufParser";
import World from "@/systems/World";
import Jolt from '@barclah/jolt-physics';
import { JoltMat44_ThreeMatrix4 } from "@/util/TypeConversions";
import * as THREE from 'three';
import JOLT from "@/util/loading/JoltSyncLoader";

const DEBUG_BODIES = true;

class MirabufSceneObject extends SceneObject {

    private _mirabufInstance: MirabufInstance;
    private _bodies: Map<string, Jolt.Body>;
    private _debugBodies: Map<string, THREE.Mesh> | null;

    public constructor(mirabufInstance: MirabufInstance) {
        super();

        this._mirabufInstance = mirabufInstance;
        this._bodies = World.PhysicsSystem.CreateBodiesFromParser(mirabufInstance.parser);
        this._debugBodies = null;
    }

    public Setup(): void {
        this._mirabufInstance.AddToScene(World.SceneRenderer.scene);

        if (DEBUG_BODIES) {
            this._debugBodies = new Map();
            this._bodies.forEach((body, rnName) => {
                const mesh = this.CreateMeshForShape(body.GetShape());
                World.SceneRenderer.scene.add(mesh);
                this._debugBodies!.set(rnName, mesh);
            });
        }
    }

    public Update(): void {
        this._mirabufInstance.parser.rigidNodes.forEach(rn => {
            const body = this._bodies.get(rn.name);
            const transform = JoltMat44_ThreeMatrix4(body!.GetWorldTransform());
            rn.parts.forEach(part => {
                const partTransform = this._mirabufInstance.parser.globalTransforms.get(part)!.clone().premultiply(transform);
                this._mirabufInstance.meshes.get(part)!.forEach(mesh => {
                    mesh.position.setFromMatrixPosition(partTransform);
                    mesh.rotation.setFromRotationMatrix(partTransform);
                });
            });

            if (this._debugBodies) {
                const mesh = this._debugBodies.get(rn.name)!;
                mesh.position.setFromMatrixPosition(transform);
                mesh.rotation.setFromRotationMatrix(transform);
            }
        });
    }

    public Dispose(): void {
        World.PhysicsSystem.DestroyBodies(...this._bodies.values());
        this._mirabufInstance.Dispose(World.SceneRenderer.scene);
        this._debugBodies?.forEach(x => {
            World.SceneRenderer.scene.remove(x);
            x.geometry.dispose();
            (x.material as THREE.Material).dispose();
        });
        this._debugBodies?.clear();
    }

    private CreateMeshForShape(shape: Jolt.Shape): THREE.Mesh {
        const scale = new JOLT.Vec3(1, 1, 1);
        const triangleContext = new JOLT.ShapeGetTriangles(shape, JOLT.AABox.prototype.sBiggest(), shape.GetCenterOfMass(), JOLT.Quat.prototype.sIdentity(), scale);
        JOLT.destroy(scale);
    
        const vertices = new Float32Array(JOLT.HEAP32.buffer, triangleContext.GetVerticesData(), triangleContext.GetVerticesSize() / Float32Array.BYTES_PER_ELEMENT);
        const buffer = new THREE.BufferAttribute(vertices, 3).clone();
        JOLT.destroy(triangleContext);
    
        const geometry = new THREE.BufferGeometry();
        geometry.setAttribute('position', buffer);
        geometry.computeVertexNormals();
    
        const material = new THREE.MeshStandardMaterial({ color: 0x33ff33, wireframe: true })
        const mesh = new THREE.Mesh(geometry, material);

        return mesh;
    }
}

export async function CreateMirabufFromUrl(path: string): Promise<MirabufSceneObject | null | undefined> {
    const miraAssembly = await LoadMirabufRemote(path)
        .catch(console.error);

    if (!miraAssembly || !(miraAssembly instanceof mirabuf.Assembly)) {
        return;
    }

    const parser = new MirabufParser(miraAssembly);
    if (parser.maxErrorSeverity >= ParseErrorSeverity.Unimportable) {
        console.error(`Assembly Parser produced significant errors for '${miraAssembly.info!.name!}'`);
        return;
    }
    
    return new MirabufSceneObject(new MirabufInstance(parser));
}

export default MirabufSceneObject;