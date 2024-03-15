import * as THREE from 'three';
import SceneObject from './SceneObject';

const CLEAR_COLOR = 0x121212;

let nextSceneObjectId = 1;

class SceneRenderer {

    private _mainCamera: THREE.PerspectiveCamera;
    private _scene: THREE.Scene;
    private _renderer: THREE.WebGLRenderer;
    private _clock: THREE.Clock;

    private _sceneObjects: Map<number, SceneObject>;

    public get mainCamera() {
        return this._mainCamera;
    }

    public get scene() {
        return this._scene;
    }

    public get renderer(): THREE.WebGLRenderer {
        return this._renderer;
    }

    public constructor() {
        this._sceneObjects = new Map();
        this._clock = new THREE.Clock();

        this._mainCamera = new THREE.PerspectiveCamera(
            75,
            window.innerWidth / window.innerHeight,
            0.1,
            1000
        );
        this._mainCamera.position.set(-2.5, 2, 2.5);


        this._scene = new THREE.Scene();

        this._renderer = new THREE.WebGLRenderer();
        this._renderer.setClearColor(CLEAR_COLOR);
        this._renderer.setPixelRatio(window.devicePixelRatio);
        this._renderer.shadowMap.enabled = true;
        this._renderer.shadowMap.type = THREE.PCFSoftShadowMap;
        this._renderer.setSize(window.innerWidth, window.innerHeight);

        const directionalLight = new THREE.DirectionalLight(0xffffff, 3.0);
        directionalLight.position.set(-1.0, 3.0, 2.0);
        directionalLight.castShadow = true;
        this._scene.add(directionalLight);

        const shadowMapSize = Math.min(4096, this._renderer.capabilities.maxTextureSize);
        const shadowCamSize = 15;
        console.debug(`Shadow Map Size: ${shadowMapSize}`);

        directionalLight.shadow.camera.top = shadowCamSize;
        directionalLight.shadow.camera.bottom = -shadowCamSize;
        directionalLight.shadow.camera.left = -shadowCamSize;
        directionalLight.shadow.camera.right = shadowCamSize;
        directionalLight.shadow.mapSize = new THREE.Vector2(shadowMapSize, shadowMapSize);
        directionalLight.shadow.blurSamples = 16;
        directionalLight.shadow.normalBias = 0.01;
        directionalLight.shadow.bias = 0.00;

        const ambientLight = new THREE.AmbientLight(0xffffff, 0.1);
        this._scene.add(ambientLight);
    }

    public UpdateCanvasSize() {
        this._renderer.setSize(window.innerWidth, window.innerHeight);
        // No idea why height would be zero, but just incase.
        this._mainCamera.aspect = window.innerWidth / window.innerHeight;
        this._mainCamera.updateProjectionMatrix();
    }

    public Update() {
        // Prevents a problem when rendering at 30hz. Referred to as the spiral of death.
        const deltaTime = this._clock.getDelta();

        this._sceneObjects.forEach(obj => {
            obj.Update();
        });

        // controls.update(deltaTime); // TODO: Add controls?
        this._renderer.render(this._scene, this._mainCamera);
    }

    public RegisterSceneObject<T extends SceneObject>(obj: T): number {
        const id = nextSceneObjectId++;
        obj.id = id;
        this._sceneObjects.set(id, obj);
        obj.Setup();
        return id;
    }

    public RemoveAllSceneObjects() {
        this._sceneObjects.forEach(obj => obj.Dispose());
        this._sceneObjects.clear();
    }
}

let instance: SceneRenderer | null = null;

function GetSceneRenderer() {
    if (!instance) {
        instance = new SceneRenderer();
    }
    return instance;
}

export default GetSceneRenderer;