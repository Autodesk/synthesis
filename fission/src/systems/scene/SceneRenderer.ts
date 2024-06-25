import * as THREE from 'three';
import SceneObject from './SceneObject';
import WorldSystem from '../WorldSystem';

import vertexShader from '@/shaders/vertex.glsl';
import fragmentShader from '@/shaders/fragment.glsl';

const CLEAR_COLOR = 0x121212
const GROUND_COLOR = 0x73937e

let nextSceneObjectId = 1

class SceneRenderer extends WorldSystem {

    private _mainCamera: THREE.PerspectiveCamera;
    private _scene: THREE.Scene;
    private _renderer: THREE.WebGLRenderer;
    private _skybox: THREE.Mesh;

    private _sceneObjects: Map<number, SceneObject>

    public get sceneObjects() {
        return this._sceneObjects
    }

    public get mainCamera() {
        return this._mainCamera
    }

    public get scene() {
        return this._scene
    }

    public get renderer(): THREE.WebGLRenderer {
        return this._renderer
    }

    public constructor() {
        super()

        this._sceneObjects = new Map()

        this._mainCamera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000)
        this._mainCamera.position.set(-2.5, 2, 2.5)

        this._scene = new THREE.Scene()

        this._renderer = new THREE.WebGLRenderer()
        this._renderer.setClearColor(CLEAR_COLOR)
        this._renderer.setPixelRatio(window.devicePixelRatio)
        this._renderer.shadowMap.enabled = true
        this._renderer.shadowMap.type = THREE.PCFSoftShadowMap
        this._renderer.setSize(window.innerWidth, window.innerHeight)

        const directionalLight = new THREE.DirectionalLight(0xffffff, 3.0)
        directionalLight.position.set(-1.0, 3.0, 2.0)
        directionalLight.castShadow = true
        this._scene.add(directionalLight)

        const shadowMapSize = Math.min(4096, this._renderer.capabilities.maxTextureSize)
        const shadowCamSize = 15
        console.debug(`Shadow Map Size: ${shadowMapSize}`)

        directionalLight.shadow.camera.top = shadowCamSize
        directionalLight.shadow.camera.bottom = -shadowCamSize
        directionalLight.shadow.camera.left = -shadowCamSize
        directionalLight.shadow.camera.right = shadowCamSize
        directionalLight.shadow.mapSize = new THREE.Vector2(shadowMapSize, shadowMapSize)
        directionalLight.shadow.blurSamples = 16
        directionalLight.shadow.normalBias = 0.01
        directionalLight.shadow.bias = 0.0

        const ambientLight = new THREE.AmbientLight(0xffffff, 0.1)
        this._scene.add(ambientLight)

        const ground = new THREE.Mesh(new THREE.BoxGeometry(10, 1, 10), this.CreateToonMaterial(GROUND_COLOR));
        ground.position.set(0.0, -2.0, 0.0);
        ground.receiveShadow = true;
        ground.castShadow = true;
        this._scene.add(ground);

        // skybox
        const currentTheme = (window as any).getTheme();
        console.log('Current Theme:', currentTheme['Background']['color']['r']);
        
        const geometry = new THREE.SphereGeometry(1000);
        const material = new THREE.ShaderMaterial({
            vertexShader: vertexShader,
            fragmentShader: fragmentShader,
            side: THREE.BackSide,
            uniforms: {
                rColor: { value: currentTheme['Background']['color']['r']},
                gColor: { value: currentTheme['Background']['color']['g']},
                bColor: { value: currentTheme['Background']['color']['b'] },
            }
        });

        this._skybox = new THREE.Mesh(geometry, material);
        this._skybox.receiveShadow = false;
        this._skybox.castShadow = false;
        this.scene.add(this._skybox); 


    }

    public UpdateCanvasSize() {
        this._renderer.setSize(window.innerWidth, window.innerHeight)
        // No idea why height would be zero, but just incase.
        this._mainCamera.aspect = window.innerWidth / window.innerHeight
        this._mainCamera.updateProjectionMatrix()
    }

    public Update(_: number): void {
        this._sceneObjects.forEach(obj => {
            obj.Update()
        })

        // controls.update(deltaTime); // TODO: Add controls?
        this._skybox.position.copy(this._mainCamera.position);
        this._renderer.render(this._scene, this._mainCamera);
    }

    public Destroy(): void {
        this.RemoveAllSceneObjects()
    }

    public RegisterSceneObject<T extends SceneObject>(obj: T): number {
        const id = nextSceneObjectId++
        obj.id = id
        this._sceneObjects.set(id, obj)
        obj.Setup()
        return id
    }

    public RemoveAllSceneObjects() {
        this._sceneObjects.forEach(obj => obj.Dispose())
        this._sceneObjects.clear()
    }

    public RemoveSceneObject(id: number) {
        const obj = this._sceneObjects.get(id)
        if (this._sceneObjects.delete(id)) {
            obj!.Dispose()
        }
    }

    public CreateSphere(radius: number, material?: THREE.Material | undefined): THREE.Mesh {
        const geo = new THREE.SphereGeometry(radius)
        if (material) {
            return new THREE.Mesh(geo, material)
        } else {
            return new THREE.Mesh(geo, this.CreateToonMaterial())
        }
    }

    public CreateToonMaterial(color: THREE.ColorRepresentation = 0xff00aa, steps: number = 5): THREE.MeshToonMaterial {
        const format = this._renderer.capabilities.isWebGL2 ? THREE.RedFormat : THREE.LuminanceFormat
        const colors = new Uint8Array(steps)
        for (let c = 0; c < colors.length; c++) {
            colors[c] = 128 + (c / colors.length) * 128
        }
        const gradientMap = new THREE.DataTexture(colors, colors.length, 1, format)
        gradientMap.needsUpdate = true
        return new THREE.MeshToonMaterial({
            color: color,
            gradientMap: gradientMap,
        })
    }

}

export default SceneRenderer
