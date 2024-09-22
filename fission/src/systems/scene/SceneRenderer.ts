import * as THREE from "three"
import WorldSystem from "../WorldSystem"
import SceneObject from "./SceneObject"
import GizmoSceneObject from "./GizmoSceneObject"

import { EdgeDetectionMode, EffectComposer, EffectPass, RenderPass, SMAAEffect } from "postprocessing"
import { OrbitControls } from "three/examples/jsm/controls/OrbitControls.js"

import fragmentShader from "@/shaders/fragment.glsl"
import vertexShader from "@/shaders/vertex.glsl"
import { Theme } from "@/ui/ThemeContext"
import Jolt from "@barclah/jolt-physics"

import { PixelSpaceCoord, SceneOverlayEvent, SceneOverlayEventKey } from "@/ui/components/SceneOverlayEvents"
import PreferencesSystem from "../preferences/PreferencesSystem"
import { CSM } from "three/examples/jsm/csm/CSM.js"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"

const CLEAR_COLOR = 0x121212
const GROUND_COLOR = 0x4066c7

let nextSceneObjectId = 1

class SceneRenderer extends WorldSystem {
    private _mainCamera: THREE.PerspectiveCamera
    private _scene: THREE.Scene
    private _renderer: THREE.WebGLRenderer
    private _skybox: THREE.Mesh
    private _composer: EffectComposer

    private _antiAliasPass: EffectPass

    private _sceneObjects: Map<number, SceneObject>
    private _gizmosOnMirabuf: Map<number, GizmoSceneObject> // maps of all the gizmos that are attached to a mirabuf scene object

    private _orbitControls: OrbitControls

    private _light: THREE.DirectionalLight | CSM | undefined

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

    public get orbitControls() {
        return this._orbitControls
    }

    public constructor() {
        super()

        this._sceneObjects = new Map()
        this._gizmosOnMirabuf = new Map()

        this._mainCamera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000)
        this._mainCamera.position.set(-2.5, 2, 2.5)

        this._scene = new THREE.Scene()

        this._renderer = new THREE.WebGLRenderer({
            // Following parameters are used to optimize post-processing
            powerPreference: "high-performance",
            antialias: false,
            stencil: false,
            depth: false,
        })
        this._renderer.setClearColor(CLEAR_COLOR)
        this._renderer.setPixelRatio(window.devicePixelRatio)
        this._renderer.shadowMap.enabled = true
        this._renderer.shadowMap.type = THREE.PCFSoftShadowMap
        this._renderer.setSize(window.innerWidth, window.innerHeight)

        // Adding the lighting uisng quality preferences
        this.ChangeLighting(PreferencesSystem.getGlobalPreference<string>("QualitySettings"))

        const ambientLight = new THREE.AmbientLight(0xffffff, 0.3)
        this._scene.add(ambientLight)

        const ground = new THREE.Mesh(new THREE.BoxGeometry(10, 1, 10), this.CreateToonMaterial(GROUND_COLOR))
        ground.position.set(0.0, -0.5, 0.0)
        ground.receiveShadow = true
        ground.castShadow = true
        this._scene.add(ground)

        // Adding spherical skybox mesh
        const geometry = new THREE.SphereGeometry(1000)
        const material = new THREE.ShaderMaterial({
            vertexShader: vertexShader,
            fragmentShader: fragmentShader,
            side: THREE.BackSide,
            uniforms: {
                rColor: { value: 1.0 },
                gColor: { value: 1.0 },
                bColor: { value: 1.0 },
            },
        })

        this._skybox = new THREE.Mesh(geometry, material)
        this._skybox.receiveShadow = false
        this._skybox.castShadow = false
        this.scene.add(this._skybox)

        // POST PROCESSING: https://github.com/pmndrs/postprocessing
        this._composer = new EffectComposer(this._renderer)
        this._composer.addPass(new RenderPass(this._scene, this._mainCamera))

        const antiAliasEffect = new SMAAEffect({ edgeDetectionMode: EdgeDetectionMode.COLOR })
        this._antiAliasPass = new EffectPass(this._mainCamera, antiAliasEffect)
        this._composer.addPass(this._antiAliasPass)

        // Orbit controls
        this._orbitControls = new OrbitControls(this._mainCamera, this._renderer.domElement)
        this._orbitControls.update()
    }

    public UpdateCanvasSize() {
        this._renderer.setSize(window.innerWidth, window.innerHeight)
        // No idea why height would be zero, but just incase.
        this._mainCamera.aspect = window.innerWidth / window.innerHeight
        this._mainCamera.updateProjectionMatrix()
    }

    public Update(deltaT: number): void {
        this._sceneObjects.forEach(obj => {
            obj.Update()
        })

        this._mainCamera.updateMatrixWorld()

        // updating the CSM light if it is enabled
        if (this._light instanceof CSM) this._light.update()

        this._skybox.position.copy(this._mainCamera.position)

        // Update the tags each frame if they are enabled in preferences
        if (PreferencesSystem.getGlobalPreference<boolean>("RenderSceneTags"))
            new SceneOverlayEvent(SceneOverlayEventKey.UPDATE)

        this._composer.render(deltaT)
    }

    public Destroy(): void {
        this.RemoveAllSceneObjects()
    }

    /**
     * Changes the quality of lighting between cascading shadows and directional lights
     *
     * @param quality: string representing the quality of lighting - "Low", "Medium", "High"
     */
    public ChangeLighting(quality: string): void {
        // removing the previous lighting method
        if (this._light instanceof THREE.DirectionalLight) {
            this._scene.remove(this._light)
        } else if (this._light instanceof CSM) {
            this._light.dispose()
            this._light.remove()
        }

        // setting the shadow map size
        const shadowMapSize = Math.min(4096, this._renderer.capabilities.maxTextureSize)

        // setting the light to a basic directional light
        if (quality === "Low" || quality === "Medium") {
            const shadowCamSize = 15

            this._light = new THREE.DirectionalLight(0xffffff, 5.0)
            this._light.position.set(-1.0, 3.0, 2.0)
            this._light.castShadow = true
            this._light.shadow.camera.top = shadowCamSize
            this._light.shadow.camera.bottom = -shadowCamSize
            this._light.shadow.camera.left = -shadowCamSize
            this._light.shadow.camera.right = shadowCamSize
            this._light.shadow.mapSize = new THREE.Vector2(shadowMapSize, shadowMapSize)
            this._light.shadow.blurSamples = 16
            this._light.shadow.bias = 0.0
            this._light.shadow.normalBias = 0.01
            this._scene.add(this._light)
        } else if (quality === "High") {
            // setting light to cascading shadows
            this._light = new CSM({
                parent: this._scene,
                camera: this._mainCamera,
                cascades: 4,
                lightDirection: new THREE.Vector3(1.0, -3.0, -2.0).normalize(),
                lightIntensity: 5,
                shadowMapSize: shadowMapSize,
                mode: "custom",
                maxFar: 30,
                shadowBias: -0.00001,
                customSplitsCallback: (cascades: number, near: number, far: number, breaks: number[]) => {
                    const blend = 0.7
                    for (let i = 1; i < cascades; i++) {
                        const uniformFactor = (near + ((far - near) * i) / cascades) / far
                        const logarithmicFactor = (near * (far / near) ** (i / cascades)) / far
                        const combinedFactor = uniformFactor * (1 - blend) + logarithmicFactor * blend

                        breaks.push(combinedFactor)
                    }

                    breaks.push(1)
                },
            })

            // setting up the materials for all objects in the scene
            this._light.fade = true
            this._scene.children.forEach(child => {
                if (child instanceof THREE.Mesh) {
                    if (this._light instanceof CSM) this._light.setupMaterial(child.material)
                }
            })
        }
    }

    public RegisterSceneObject<T extends SceneObject>(obj: T): number {
        const id = nextSceneObjectId++
        obj.id = id
        this._sceneObjects.set(id, obj)
        obj.Setup()
        return id
    }

    /** Registers gizmos that are attached to a parent mirabufsceneobject  */
    public RegisterGizmoSceneObject(obj: GizmoSceneObject): number {
        if (obj.HasParent()) this._gizmosOnMirabuf.set(obj.parentObjectId!, obj)
        return this.RegisterSceneObject(obj)
    }

    public RemoveAllSceneObjects() {
        this._sceneObjects.forEach(obj => obj.Dispose())
        this._gizmosOnMirabuf.clear()
        this._sceneObjects.clear()
    }

    public RemoveSceneObject(id: number) {
        const obj = this._sceneObjects.get(id)

        // If the object is a mirabuf object, remove the gizmo as well
        if (obj instanceof MirabufSceneObject) {
            const objGizmo = this._gizmosOnMirabuf.get(id)
            if (this._gizmosOnMirabuf.delete(id)) objGizmo!.Dispose()
        } else if (obj instanceof GizmoSceneObject && obj.HasParent()) {
            this._gizmosOnMirabuf.delete(obj.parentObjectId!)
        }

        if (this._sceneObjects.delete(id)) {
            obj!.Dispose()
        }
    }

    public CreateSphere(radius: number, material?: THREE.Material | undefined): THREE.Mesh {
        const geo = new THREE.SphereGeometry(radius)
        if (material) {
            if (this._light instanceof CSM) this._light.setupMaterial(material)
            return new THREE.Mesh(geo, material)
        } else {
            return new THREE.Mesh(geo, this.CreateToonMaterial())
        }
    }

    public CreateBox(halfExtent: Jolt.Vec3, material?: THREE.Material | undefined): THREE.Mesh {
        const geo = new THREE.BoxGeometry(halfExtent.GetX(), halfExtent.GetY(), halfExtent.GetZ())
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
        const material = new THREE.MeshToonMaterial({
            color: color,
            shadowSide: THREE.DoubleSide,
            gradientMap: gradientMap,
        })
        if (this._light instanceof CSM) this._light.setupMaterial(material)
        return material
    }

    /**
     * Convert pixel coordinates to a world space vector
     *
     * @param mouseX X pixel position of the mouse (MouseEvent.clientX)
     * @param mouseY Y pixel position of the mouse (MouseEvent.clientY)
     * @param z Travel from the near to far plane of the camera frustum. Default is 0.5, range is [0.0, 1.0]
     * @returns World space point within the frustum given the parameters.
     */
    public PixelToWorldSpace(mouseX: number, mouseY: number, z: number = 0.5): THREE.Vector3 {
        const screenSpace = new THREE.Vector3(
            (mouseX / window.innerWidth) * 2 - 1,
            ((window.innerHeight - mouseY) / window.innerHeight) * 2 - 1,
            Math.min(1.0, Math.max(0.0, z))
        )

        return screenSpace.unproject(this.mainCamera)
    }

    /**
     * Convert world space coordinates to screen space coordinates
     *
     * @param world World space coordinates
     * @returns Pixel space coordinates
     */
    public WorldToPixelSpace(world: THREE.Vector3): PixelSpaceCoord {
        this._mainCamera.updateMatrixWorld()
        const screenSpace = world.project(this._mainCamera)
        return [(window.innerWidth * (screenSpace.x + 1.0)) / 2.0, (window.innerHeight * (1.0 - screenSpace.y)) / 2.0]
    }

    /**
     * Updates the skybox colors based on the current theme

     * @param currentTheme: current theme from ThemeContext.useTheme()
     */
    public UpdateSkyboxColors(currentTheme: Theme) {
        if (!this._skybox) return
        if (this._skybox.material instanceof THREE.ShaderMaterial) {
            this._skybox.material.uniforms.rColor.value = currentTheme["Background"]["color"]["r"]
            this._skybox.material.uniforms.gColor.value = currentTheme["Background"]["color"]["g"]
            this._skybox.material.uniforms.bColor.value = currentTheme["Background"]["color"]["b"]
        }
    }

    /** returns whether any gizmos are being currently dragged */
    public IsAnyGizmoDragging(): boolean {
        return Array.from(this._sceneObjects.values())
            .filter(obj => obj instanceof GizmoSceneObject)
            .some(obj => obj.gizmo.dragging)
    }

    /**
     * Adding object to scene
     *
     * @param obj Object to add
     */
    public AddObject(obj: THREE.Object3D) {
        this._scene.add(obj)
    }

    /**
     * Removing object from scene
     *
     * @param obj Object to remove
     */
    public RemoveObject(obj: THREE.Object3D) {
        this._scene.remove(obj)
    }

    /**
     * Sets up the threejs material for cascading shadows if the CSM is enabled
     *
     * @param material
     */
    public SetupMaterial(material: THREE.Material) {
        if (this._light instanceof CSM) this._light.setupMaterial(material)
    }
}

export default SceneRenderer
