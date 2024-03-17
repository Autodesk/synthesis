import { mirabuf } from "@/proto/mirabuf";
import SceneObject from "../systems/scene/SceneObject";
import MirabufInstance from "./MirabufInstance";
import { LoadMirabufRemote } from "./MirabufLoader";
import MirabufParser, { ParseErrorSeverity } from "./MirabufParser";
import World from "@/systems/World";
import Jolt from '@barclah/jolt-physics';
import { JoltMat44_ThreeMatrix4 } from "@/util/TypeConversions";
import * as THREE from 'three';

class MirabufSceneObject extends SceneObject {

    private _mirabufInstance: MirabufInstance;
    private _bodies: Map<string, Jolt.Body>;

    public constructor(mirabufInstance: MirabufInstance) {
        super();

        this._mirabufInstance = mirabufInstance;
        this._bodies = World.PhysicsSystem.CreateBodiesFromParser(mirabufInstance.parser);
    }

    public Setup(): void {
        this._mirabufInstance.AddToScene(World.SceneRenderer.scene);
    }

    public Update(): void {
        this._mirabufInstance.parser.rigidNodes.forEach(rn => {
            const body = this._bodies.get(rn.name);
            const transform = JoltMat44_ThreeMatrix4(body!.GetWorldTransform())
            rn.parts.forEach(part => {
                this._mirabufInstance.meshes.get(part)!.forEach(mesh => {
                    mesh.position.setFromMatrixPosition(transform);
                    mesh.rotation.setFromRotationMatrix(transform);
                });
            });
        });
    }

    public Dispose(): void {
        World.PhysicsSystem.DestroyBodies(...this._bodies.values());
        this._mirabufInstance.Dispose(World.SceneRenderer.scene);
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