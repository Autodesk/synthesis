import * as THREE from 'three';

class CascadingShadows {
    private _camera: THREE.PerspectiveCamera;
    private _shadowCameras: THREE.OrthographicCamera[] = [];
    private _scene: THREE.Scene;

    private _renderer: THREE.WebGLRenderer;
    private _light: THREE.DirectionalLight;
    private _shadowMaps: THREE.WebGLRenderTarget[] = []; // One shadow map per cascade

    private _numCascades: number;
    private _cascadeSplits: number[];

    constructor(camera: THREE.PerspectiveCamera, scene: THREE.Scene, renderer: THREE.WebGLRenderer, numCascades: number = 5) {
        this._camera = camera;
        this._scene = scene;
        this._renderer = renderer;
        this._numCascades = numCascades;

        // Creating the directional light
        this._light = new THREE.DirectionalLight(0xffffff, 3);
        this._light.position.set(-1, 3, 2);
        this._light.castShadow = true;
        scene.add(this._light);
        
        // Split the camera frustum into numCascades cascades
        this._cascadeSplits = new Array(numCascades).fill(0).map((_, i) => (i + 1) / numCascades);

        // Create the shadow camera and shadow maps
        for (let i = 0; i < numCascades; i++) {
            const shadowCamera = new THREE.OrthographicCamera(-10, 10, 10, -10, 0.5, 500);
            shadowCamera.position.copy(this._light.position);
            shadowCamera.lookAt(0, 0, 0);
            this._shadowCameras.push(shadowCamera);

            const shadowMap = new THREE.WebGLRenderTarget(1024, 1024, {
                minFilter: THREE.NearestFilter,
                magFilter: THREE.NearestFilter,
                format: THREE.RGBAFormat,
            });
            this._shadowMaps.push(shadowMap);
        }
    }

    /** Updates the shadow maps */
    public Update() {
        const frustum = new THREE.Frustum();
        const projScreenMatrix = new THREE.Matrix4();
        projScreenMatrix.multiplyMatrices(this._camera.projectionMatrix, this._camera.matrixWorldInverse);
        frustum.setFromProjectionMatrix(projScreenMatrix);

        for (let i = 0; i < this._numCascades; i++) {
            const near = this._camera.near + this._cascadeSplits[i] * (this._camera.far - this._camera.near);
            const far = this._camera.near + this._cascadeSplits[i + 1] * (this._camera.far - this._camera.near);

            const cascadeFrustum = new THREE.Frustum();
            cascadeFrustum.setFromProjectionMatrix(this.GetCascadeMatrix(near, far));

            const shadowCamera = this._shadowCameras[i];
            shadowCamera.position.copy(this._light.position);
            shadowCamera.lookAt(0, 0, 0);
            shadowCamera.updateMatrixWorld();
            shadowCamera.updateProjectionMatrix();

            this._renderer.setRenderTarget(this._shadowMaps[i]);
            this._renderer.render(this._scene, shadowCamera);
        }

        this._renderer.setRenderTarget(null);
    }

    /** Returns the projection matrix for the cascade */
    private GetCascadeMatrix(near: number, far: number): THREE.Matrix4 {
        const matrix = new THREE.Matrix4();
        const projMatrix = new THREE.Matrix4().makePerspective(
            this._camera.fov,
            this._camera.aspect,
            near,
            far,
            this._camera.near,
            this._camera.far
        );
        const invMatrix = this._camera.matrixWorld.invert();
        matrix.multiplyMatrices(projMatrix, invMatrix);
        return matrix;
    }

    public getShadowMap(index: number): THREE.WebGLRenderTarget {
        return this._shadowMaps[index];
    }
}

export default CascadingShadows;
