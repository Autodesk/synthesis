import * as THREE from "three"
import SceneObject from "./SceneObject"
import WorldSystem from "../WorldSystem"

import { TransformControls } from "three/examples/jsm/controls/TransformControls.js"
import { OrbitControls } from "three/examples/jsm/controls/OrbitControls.js"

import vertexShader from "@/shaders/vertex.glsl"
import fragmentShader from "@/shaders/fragment.glsl"
import { Theme } from "@/ui/ThemeContext"
import InputSystem from "../input/InputSystem"

const CLEAR_COLOR = 0x121212
const GROUND_COLOR = 0x73937e

let nextSceneObjectId = 1

class SceneRenderer extends WorldSystem {
    private _mainCamera: THREE.PerspectiveCamera
    private _scene: THREE.Scene
    private _renderer: THREE.WebGLRenderer
    private _skybox: THREE.Mesh

    private _sceneObjects: Map<number, SceneObject>

    private _orbitControls: OrbitControls
    private _transformControls: Map<TransformControls, number> // maps all rendered transform controls to their size

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
        this._transformControls = new Map()

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

        const ground = new THREE.Mesh(new THREE.BoxGeometry(10, 1, 10), this.CreateToonMaterial(GROUND_COLOR))
        ground.position.set(0.0, -2.0, 0.0)
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

    public Update(_: number): void {
        this._sceneObjects.forEach(obj => {
            obj.Update()
        })

        // controls.update(deltaTime); // TODO: Add controls?
        this._skybox.position.copy(this._mainCamera.position)

        const mainCameraFovRadians = (Math.PI * (this._mainCamera.fov * 0.5)) / 180
        this._transformControls.forEach((size, tc) => {
            tc.setSize(
                (size / this._mainCamera.position.distanceTo(tc.object!.position)) *
                    Math.tan(mainCameraFovRadians) *
                    1.9
            )
        })

        this._renderer.render(this._scene, this._mainCamera)
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

    /**
     * Attach new transform gizmo to Mesh
     *
     * @param obj Mesh to attach gizmo to
     * @param mode Transform mode (translate, rotate, scale)
     * @param size Size of the gizmo
     * @returns void
     */
    public AddTransformGizmo(obj: THREE.Object3D, mode: "translate" | "rotate" | "scale" = "translate", size: number) {
        const transformControl = new TransformControls(this._mainCamera, this._renderer.domElement)
        transformControl.setMode(mode)
        transformControl.attach(obj)

        // allowing the transform gizmos to rotate with the object
        transformControl.space = "local"

        transformControl.addEventListener(
            "dragging-changed",
            (event: { target: TransformControls; value: unknown }) => {
                const isAnyGizmoDragging = Array.from(this._transformControls.keys()).some(gizmo => gizmo.dragging)
                if (!event.value && !isAnyGizmoDragging) {
                    this._orbitControls.enabled = true // enable orbit controls when not dragging another transform gizmo
                } else if (!event.value && isAnyGizmoDragging) {
                    this._orbitControls.enabled = false // disable orbit controls when dragging another transform gizmo
                } else {
                    this._orbitControls.enabled = !event.value // disable orbit controls when dragging transform gizmo
                }

                if (event.target.mode === "translate") {
                    this._transformControls.forEach((_size, tc) => {
                        // disable other transform gizmos when translating
                        if (tc.object === event.target.object && tc.mode !== "translate") {
                            tc.dragging = false
                            tc.enabled = !event.value
                            return
                        }
                    })
                } else if (
                    event.target.mode === "scale" &&
                    (InputSystem.isKeyPressed("ShiftRight") || InputSystem.isKeyPressed("ShiftLeft"))
                ) {
                    // scale uniformly if shift is pressed
                    transformControl.axis = "XYZE"
                } else if (event.target.mode === "rotate") {
                    // scale on all axes
                    this._transformControls.forEach((_size, tc) => {
                        // disable scale transform gizmo when scaling
                        if (tc.mode === "scale" && tc !== event.target && tc.object === event.target.object) {
                            tc.dragging = false
                            tc.enabled = !event.value
                            return
                        }
                    })
                }
            }
        )

        this._transformControls.set(transformControl, size)
        this._scene.add(transformControl)

        return transformControl
    }

    /**
     * Remove transform gizmos from Mesh
     *
     * @param obj Mesh to remove gizmo from
     * @returns void
     */
    public RemoveTransformGizmos(obj: THREE.Object3D) {
        this._transformControls.forEach((_, tc) => {
            if (tc.object === obj) {
                tc.detach()
                this._scene.remove(tc)
                this._transformControls.delete(tc)
            }
        })
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
}

export default SceneRenderer
