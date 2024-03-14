import * as THREE from 'three';
import { mirabuf } from "../proto/mirabuf"
import MirabufParser from '../mirabuf/MirabufParser.ts';
import { MirabufTransform_ThreeMatrix4 } from '../util/conversions/MiraThreeConversions.ts';

export const matToString = (mat: THREE.Matrix4) => {
    const arr = mat.toArray();
    return `[\n${arr[0].toFixed(4)}, ${arr[4].toFixed(4)}, ${arr[8].toFixed(4)}, ${arr[12].toFixed(4)},\n`
        + `${arr[1].toFixed(4)}, ${arr[5].toFixed(4)}, ${arr[9].toFixed(4)}, ${arr[13].toFixed(4)},\n`
        + `${arr[2].toFixed(4)}, ${arr[6].toFixed(4)}, ${arr[10].toFixed(4)}, ${arr[14].toFixed(4)},\n`
        + `${arr[3].toFixed(4)}, ${arr[7].toFixed(4)}, ${arr[11].toFixed(4)}, ${arr[15].toFixed(4)},\n]`
}

export const miraMatToString = (mat: mirabuf.ITransform) => {
    const arr = mat.spatialMatrix!;
    return `[\n${arr[0].toFixed(4)}, ${arr[1].toFixed(4)}, ${arr[2].toFixed(4)}, ${arr[3].toFixed(4)},\n`
        + `${arr[4].toFixed(4)}, ${arr[5].toFixed(4)}, ${arr[6].toFixed(4)}, ${arr[7].toFixed(4)},\n`
        + `${arr[8].toFixed(4)}, ${arr[9].toFixed(4)}, ${arr[10].toFixed(4)}, ${arr[11].toFixed(4)},\n`
        + `${arr[12].toFixed(4)}, ${arr[13].toFixed(4)}, ${arr[14].toFixed(4)}, ${arr[15].toFixed(4)},\n]`
}

const transformVerts = (mesh: mirabuf.IMesh) => {
    const newVerts = new Float32Array(mesh.verts!.length);
    for (let i = 0; i < mesh.verts!.length; i += 3) {
        newVerts[i] = mesh.verts!.at(i)! / 100.0;
        newVerts[i + 1] = mesh.verts!.at(i + 1)! / 100.0;
        newVerts[i + 2] = mesh.verts!.at(i + 2)! / 100.0;
    }
    return newVerts;
}

const transformNorms = (mesh: mirabuf.IMesh) => {
    const newNorms = new Float32Array(mesh.normals!.length);
    for (let i = 0; i < mesh.normals!.length; i += 3) {
        const normLength = Math.sqrt(mesh.normals!.at(i)! * mesh.normals!.at(i)! +
            mesh.normals!.at(i + 1)! * mesh.normals!.at(i + 1)! +
            mesh.normals!.at(i + 2)! * mesh.normals!.at(i + 2)!
        );

        newNorms[i] = mesh.normals!.at(i)! / normLength;
        newNorms[i + 1] = mesh.normals!.at(i + 1)! / normLength;
        newNorms[i + 2] = mesh.normals!.at(i + 2)! / normLength;
    }
    return newNorms;
}

const transformGeometry = (geometry: THREE.BufferGeometry, mesh: mirabuf.IMesh) => {
    const newVerts = transformVerts(mesh);
    const newNorms = transformNorms(mesh);

    geometry.setAttribute('position', new THREE.BufferAttribute(new Float32Array(newVerts), 3));
    geometry.setAttribute('normal', new THREE.BufferAttribute(new Float32Array(newNorms), 3));
    geometry.setAttribute('uv', new THREE.BufferAttribute(new Float32Array(mesh.uv!), 2));
    geometry.setIndex(mesh.indices!);
}

const materials = [
    new THREE.MeshToonMaterial({
        color: 0xe32b50
    }),
    new THREE.MeshToonMaterial({
        color: 0x4ccf57
    }),
    new THREE.MeshToonMaterial({
        color: 0xcf4cca
    })
]

const getMaterial = (data: mirabuf.IAssemblyData, appearanceOverride: string | undefined | null) => {
    let appearances;
    if (appearanceOverride && (appearances = data.materials?.appearances) && appearances[appearanceOverride]) {
        const miraMaterial = data.materials.appearances[appearanceOverride];
        let hex = 0xe32b50;
        if (miraMaterial.albedo) {
            const {A, B, G, R} = miraMaterial.albedo;
            if (A && B && G && R)
                hex = A << 24 | R << 16 | G << 8  | B;
        }

        return new THREE.MeshPhongMaterial({
            color: hex,
            shininess: 0.5,
        });
    }
}

export const applyTransforms = (assembly: mirabuf.Assembly | undefined, scene: THREE.Scene) => {
            if (!assembly) return;
            const data = assembly.data;
            if (!data) return;
            const parts = data.parts;
            if (!parts) return;
            const partInstances = new Map<string, mirabuf.IPartInstance>();
            for (const partInstance of Object.values(parts.partInstances!)) {
                partInstances.set(partInstance.info!.GUID!, partInstance);
            }

            const parser = new MirabufParser(assembly);
            const root = parser.designHierarchyRoot;
            
            const transforms = new Map<string, THREE.Matrix4>();
            const getTransforms = (node: mirabuf.INode, parent: THREE.Matrix4) => {
                for (const child of node.children!) {
                    if (!partInstances.has(child.value!)) {
                        continue;
                    }
                    const partInstance = partInstances.get(child.value!)!;
                    
                    if (transforms.has(child.value!)) continue;
                    const mat = MirabufTransform_ThreeMatrix4(partInstance.transform!)!;

                    // console.log(`[${partInstance.info!.name!}] -> ${matToString(mat)}`);

                    transforms.set(child.value!, mat.premultiply(parent));
                    getTransforms(child, mat);
                }
            }

            for (const child of root.children!) {
                const partInstance = partInstances.get(child.value!)!;
                let mat;
                if (!partInstance.transform) {
                    const def = parts.partDefinitions![partInstances.get(child.value!)!.partDefinitionReference!];
                    if (!def.baseTransform) {
                        mat = new THREE.Matrix4().identity();
                    } else {
                        mat = MirabufTransform_ThreeMatrix4(def.baseTransform);
                    }
                } else {
                    mat = MirabufTransform_ThreeMatrix4(partInstance.transform);
                }

                // console.log(`[${partInstance.info!.name!}] -> ${matToString(mat!)}`);

                transforms.set(partInstance.info!.GUID!, mat!);
                getTransforms(child, mat!);
            }

            let i = 0;


            const instances = parts.partInstances;
            if (!instances) return;
            for (const instance of Object.values(instances)/* .filter(x => x.info!.name!.startsWith('EyeBall')) */) {
                const definition = assembly.data!.parts!.partDefinitions![instance.partDefinitionReference!]!;
                const bodies = definition.bodies;
                if (!bodies) continue;
                for (const body of bodies) {
                    if (!body) continue;
                    const mesh = body.triangleMesh;
                    const geometry = new THREE.BufferGeometry();
                    if (mesh && mesh.mesh && mesh.mesh.verts && mesh.mesh.normals && mesh.mesh.uv && mesh.mesh.indices) {
                        transformGeometry(geometry, mesh.mesh!);

                        const appearanceOverride = body.appearanceOverride;
                        const material: THREE.Material = getMaterial(data, appearanceOverride) || materials[i++ % materials.length];


                        const threeMesh = new THREE.Mesh( geometry, material );
                        // threeMesh.receiveShadow = true;
                        // threeMesh.castShadow = true;
                        scene.add(threeMesh);
                        
                        const mat = transforms.get(instance.info!.GUID!)!;
                        
                        // console.log(`RENDER [${instance.info!.name!}] -> ${matToString(mat)}`);

                        threeMesh.position.setFromMatrixPosition(mat);
                        threeMesh.rotation.setFromRotationMatrix(mat);
                    }
                }
            }
        }
