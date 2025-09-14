import type Jolt from "@azaleacolburn/jolt-physics"
import { EdgeDetectionMode, EffectComposer, EffectPass, RenderPass, SMAAEffect } from "postprocessing"
import * as THREE from "three"
import { CSM } from "three/examples/jsm/csm/CSM.js"
import autodeskLogo from "@/assets/autodesk_symbol.png"
import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject, { type RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import fragmentShader from "@/shaders/fragment.glsl"
import vertexShader from "@/shaders/vertex.glsl"
import { type CameraControls, type CameraControlsType, CustomOrbitControls } from "@/systems/scene/CameraControls"
import { type ContextData, ContextSupplierEvent } from "@/ui/components/ContextMenuData"
import { globalOpenPanel } from "@/ui/components/GlobalUIControls"
import { type PixelSpaceCoord, SceneOverlayEvent, SceneOverlayEventKey } from "@/ui/components/SceneOverlayEvents"
import { TouchControlsEvent, TouchControlsEventKeys } from "@/ui/components/TouchControls"
import type { ConfigurationType } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import ImportMirabufPanel from "@/ui/panels/mirabuf/ImportMirabufPanel"
import { convertThreeVector3ToJoltVec3 } from "@/util/TypeConversions"
import PreferencesSystem from "../preferences/PreferencesSystem"
import type { GraphicsPreferences } from "../preferences/PreferenceTypes"
import World from "../World"
import WorldSystem from "../WorldSystem"
import GizmoSceneObject from "./GizmoSceneObject"
import type SceneObject from "./SceneObject"
import ScreenInteractionHandler, { type InteractionEnd } from "./ScreenInteractionHandler"
import type { LocalSceneObjectId, RemoteSceneObjectId } from "@/systems/multiplayer/types.ts"

const CLEAR_COLOR = 0x121212
const GROUND_COLOR = 0xfffef0

const STANDARD_ASPECT = 16.0 / 9.0
export const STANDARD_CAMERA_FOV_X = 110.0
export const STANDARD_CAMERA_FOV_Y = STANDARD_CAMERA_FOV_X / STANDARD_ASPECT

const textureLoader = new THREE.TextureLoader()

let nextSceneObjectId = 1

class SceneRenderer extends WorldSystem {
    private _mainCamera: THREE.PerspectiveCamera
    private _scene: THREE.Scene
    private _renderer: THREE.WebGLRenderer
    private _skybox: THREE.Mesh
    private _composer: EffectComposer

    private _sceneObjects: Map<number, SceneObject>
    private _gizmosOnMirabuf: Map<number, GizmoSceneObject> // maps of all the gizmos that are attached to a mirabuf scene object

    private _cameraControls: CameraControls

    private _isPlacingAssembly: boolean = false

    private _light: THREE.DirectionalLight | CSM | undefined
    private _screenInteractionHandler: ScreenInteractionHandler

    public get sceneObjects() {
        return this._sceneObjects
    }
    public set sceneObjects(objects: Map<number, SceneObject>) {
        this._sceneObjects = objects
    }

    public filterSceneObjects<T extends SceneObject>(predicate: (obj: SceneObject) => obj is T): T[] {
        return [...this._sceneObjects.values()].filter(predicate)
    }

    public readonly mirabufSceneObjects = {
        getAll: () => this.filterSceneObjects(obj => obj instanceof MirabufSceneObject),
        findWhere: (predicate: (obj: MirabufSceneObject) => boolean) =>
            this.mirabufSceneObjects.getAll().find(predicate),
        getField: () => this.mirabufSceneObjects.findWhere(obj => obj.miraType == MiraType.FIELD),
        getRobots: () => this.mirabufSceneObjects.getAll().filter(obj => obj.miraType == MiraType.ROBOT),
    } as const

    public get mainCamera() {
        return this._mainCamera
    }

    public get scene() {
        return this._scene
    }

    public get renderer(): THREE.WebGLRenderer {
        return this._renderer
    }

    public get isPlacingAssembly() {
        return this._isPlacingAssembly
    }

    public set isPlacingAssembly(value: boolean) {
        new TouchControlsEvent(TouchControlsEventKeys.PLACE_BUTTON, value)
        this._isPlacingAssembly = value
    }

    public get currentCameraControls(): CameraControls {
        return this._cameraControls
    }

    public get screenInteractionHandler(): ScreenInteractionHandler {
        return this._screenInteractionHandler
    }

    /**
     * Collection that maps Mirabuf objects to active GizmoSceneObjects
     */
    public get gizmosOnMirabuf() {
        return this._gizmosOnMirabuf
    }

    public constructor() {
        super()

        this._sceneObjects = new Map()
        this._gizmosOnMirabuf = new Map()

        const aspect = window.innerWidth / window.innerHeight
        this._mainCamera = new THREE.PerspectiveCamera(STANDARD_CAMERA_FOV_Y, aspect, 0.1, 1000)
        this._mainCamera.position.set(-2.5, 2, 2.5)

        this._scene = new THREE.Scene()

        this._renderer = new THREE.WebGLRenderer({
            powerPreference: "high-performance",
            antialias: false,
            stencil: false,
            depth: !PreferencesSystem.getGraphicsPreferences().antiAliasing,
        })
        this._renderer.setClearColor(CLEAR_COLOR)
        this._renderer.setPixelRatio(window.devicePixelRatio)
        this._renderer.shadowMap.enabled = true
        this._renderer.shadowMap.type = THREE.PCFSoftShadowMap
        this._renderer.setSize(window.innerWidth, window.innerHeight)

        this.changeLighting(PreferencesSystem.getGraphicsPreferences().fancyShadows)

        const ambientLight = new THREE.AmbientLight(0xffffff, 0.3)
        this._scene.add(ambientLight)

        const groundGeometry = new THREE.BoxGeometry(15, 0.2, 15)

        const logoTexture = textureLoader.load(autodeskLogo)
        logoTexture.wrapS = THREE.ClampToEdgeWrapping
        logoTexture.wrapT = THREE.ClampToEdgeWrapping
        logoTexture.center.set(0.5, 0.5) // Size Adjustment
        logoTexture.repeat.set(2, 2)

        const logoMaterial = new THREE.MeshToonMaterial({
            map: logoTexture,
            color: GROUND_COLOR,
            shadowSide: THREE.DoubleSide,
        })
        if (this._light instanceof CSM) this._light.setupMaterial(logoMaterial)

        const solidMaterial = this.createToonMaterial(GROUND_COLOR)

        // Define each face individually
        const materials = [
            solidMaterial,
            solidMaterial,
            logoMaterial, // Logo on top face only
            solidMaterial,
            solidMaterial,
            solidMaterial,
        ]

        const ground = new THREE.Mesh(groundGeometry, materials)
        ground.position.set(0.0, -0.09, 0.0)
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

        if (PreferencesSystem.getGraphicsPreferences().antiAliasing) {
            const antiAliasEffect = new SMAAEffect({
                edgeDetectionMode: EdgeDetectionMode.COLOR,
            })
            const antiAliasPass = new EffectPass(this._mainCamera, antiAliasEffect)
            this._composer.addPass(antiAliasPass)
        }

        // Orbit controls
        this._screenInteractionHandler = new ScreenInteractionHandler(this._renderer.domElement)
        this._screenInteractionHandler.contextMenu = e => this.onContextMenu(e)

        this._cameraControls = new CustomOrbitControls(this._mainCamera, this._screenInteractionHandler)
    }

    public setCameraControls(controlsType: CameraControlsType) {
        this._cameraControls.dispose()
        switch (controlsType) {
            case "Orbit":
                this._cameraControls = new CustomOrbitControls(this._mainCamera, this._screenInteractionHandler)
                break
        }
    }

    public updateCanvasSize() {
        this._renderer.setSize(window.innerWidth, window.innerHeight, true)

        const vec = new THREE.Vector2(0, 0)
        this._renderer.getSize(vec)
        // No idea why height would be zero, but just incase.
        this._mainCamera.aspect = window.innerHeight > 0 ? window.innerWidth / window.innerHeight : 1.0

        if (this._mainCamera.aspect < STANDARD_ASPECT) {
            this._mainCamera.fov = STANDARD_CAMERA_FOV_Y
        } else {
            this._mainCamera.fov = STANDARD_CAMERA_FOV_X / this._mainCamera.aspect
        }

        this._mainCamera.updateProjectionMatrix()
    }

    /** Function to disable or enable the antiAliasingPass */
    public update(deltaT: number): void {
        this._sceneObjects.forEach(obj => {
            obj.update()
        })

        this._mainCamera.updateMatrixWorld()

        // updating the CSM light if it is enabled
        if (this._light instanceof CSM) this._light.update()

        this._skybox.position.copy(this._mainCamera.position)

        // Update the tags each frame if they are enabled in preferences
        if (PreferencesSystem.getGlobalPreference("RenderSceneTags")) new SceneOverlayEvent(SceneOverlayEventKey.UPDATE)

        this._screenInteractionHandler.update(deltaT)
        this._cameraControls.update(deltaT)

        this._composer.render(deltaT)
        // this._renderer.render(this._scene, this._mainCamera)
    }

    public destroy(): void {
        this.removeAllSceneObjects()
        this._screenInteractionHandler.dispose()
    }

    /**
     * Changes the quality of lighting between cascading shadows and directional lights
     *
     * @param quality: string representing the quality of lighting - "Low", "Medium", "High"
     */
    public changeLighting(fancyShadows: boolean): void {
        // removing the previous lighting method
        if (this._light instanceof THREE.DirectionalLight) {
            this._scene.remove(this._light)
        } else if (this._light instanceof CSM) {
            this._light.dispose()
            this._light.remove()
        }

        // setting the shadow map size
        const graphicsSettings = PreferencesSystem.getGraphicsPreferences()
        const shadowMapSize = Math.min(graphicsSettings.shadowMapSize, this._renderer.capabilities.maxTextureSize)

        // setting the light to a basic directional light
        if (!fancyShadows) {
            const shadowCamSize = 15

            this._light = new THREE.DirectionalLight(0xffffff, graphicsSettings.lightIntensity)
            const lightDirection = new THREE.Vector3(1.0, -3.0, -2.0).normalize()
            this._light.position.copy(lightDirection.clone().multiplyScalar(-20))
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
        } else {
            // setting the light to a cascading shadow map
            this.createCSM(graphicsSettings)

            // setting up all the materials
            this.setupCSMMaterials()
        }
    }

    public createCSM(settings: GraphicsPreferences) {
        this._light = new CSM({
            parent: this._scene,
            camera: this._mainCamera,
            cascades: settings.cascades,
            lightDirection: new THREE.Vector3(1.0, -3.0, -2.0).normalize(),
            lightIntensity: settings.lightIntensity,
            shadowMapSize: settings.shadowMapSize,
            mode: "custom",
            maxFar: settings.maxFar,
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
        this._light.fade = true
    }

    private setupCSMMaterials() {
        this._scene.children.forEach(child => {
            if (child instanceof THREE.Mesh) {
                if (this._light instanceof CSM) this._light.setupMaterial(child.material)
            }
        })
    }

    /** Sets the light intensity for both directional light and csm */
    public setLightIntensity(intensity: number) {
        if (this._light instanceof THREE.DirectionalLight) {
            this._light.intensity = intensity
        } else if (this._light instanceof CSM) {
            this._light.dispose()
            this._light.remove()

            this.createCSM({
                ...PreferencesSystem.getGraphicsPreferences(),
                lightIntensity: intensity,
            })
            this.setupCSMMaterials()
        }
    }

    /** Changes the settings of the cascading shadows from the Quality Settings Panel */
    public changeCSMSettings(settings: GraphicsPreferences) {
        if (!(this._light instanceof CSM)) return

        this._light.dispose()
        this._light.remove()

        this.createCSM(settings)
        this.setupCSMMaterials()
    }

    public registerSceneObject<T extends SceneObject>(obj: T, idOverride?: number): LocalSceneObjectId {
        const id = idOverride ?? nextSceneObjectId++
        if (nextSceneObjectId <= id) {
            nextSceneObjectId = id + 1
        }
        if (this._sceneObjects.has(id)) {
            console.error("Trying to add with existing ID!", obj, idOverride)
            return -1 as LocalSceneObjectId
        }
        obj.id = id
        this._sceneObjects.set(id, obj)
        obj.setup()
        return id as LocalSceneObjectId
    }

    /** Registers gizmos that are attached to a parent mirabufsceneobject  */
    public registerGizmoSceneObject(obj: GizmoSceneObject): number {
        if (obj.hasParent()) this._gizmosOnMirabuf.set(obj.parentObjectId!, obj)
        return this.registerSceneObject(obj)
    }

    public removeAllSceneObjects() {
        this._sceneObjects.forEach(obj => obj.dispose())
        this._gizmosOnMirabuf.clear()
        this._sceneObjects.clear()
    }

    public removeSceneObject(id: number) {
        const obj = this._sceneObjects.get(id)

        // If the object is a mirabuf object, remove the gizmo as well
        if (obj instanceof MirabufSceneObject) {
            const objGizmo = this._gizmosOnMirabuf.get(id)
            if (this._gizmosOnMirabuf.delete(id)) objGizmo!.dispose()
            World?.multiplayerSystem?.broadcast({
                type: "deleteObject",
                data: id as RemoteSceneObjectId,
            })
        } else if (obj instanceof GizmoSceneObject && obj.hasParent()) {
            this._gizmosOnMirabuf.delete(obj.parentObjectId!)
        }

        if (this._sceneObjects.delete(id)) {
            obj!.dispose()
        }
    }

    public removeAllFields() {
        for (const [id, obj] of this._sceneObjects) {
            if (obj instanceof MirabufSceneObject && obj.miraType == MiraType.FIELD) {
                this.removeSceneObject(id)
            }
        }
    }

    public createSphere(radius: number, material?: THREE.Material | undefined): THREE.Mesh {
        const geo = new THREE.SphereGeometry(radius)
        if (material) {
            if (this._light instanceof CSM) this._light.setupMaterial(material)
            return new THREE.Mesh(geo, material)
        } else {
            return new THREE.Mesh(geo, this.createToonMaterial())
        }
    }

    public createBox(halfExtent: Jolt.Vec3, material?: THREE.Material | undefined): THREE.Mesh {
        const geo = new THREE.BoxGeometry(halfExtent.GetX(), halfExtent.GetY(), halfExtent.GetZ())
        if (material) {
            return new THREE.Mesh(geo, material)
        } else {
            return new THREE.Mesh(geo, this.createToonMaterial())
        }
    }

    public createToonMaterial(color: THREE.ColorRepresentation = 0xff00aa, steps: number = 5): THREE.MeshToonMaterial {
        const format = THREE.RedFormat
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
    public pixelToWorldSpace(mouseX: number, mouseY: number, z: number = 0.5): THREE.Vector3 {
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
    public worldToPixelSpace(world: THREE.Vector3): PixelSpaceCoord {
        this._mainCamera.updateMatrixWorld()
        const screenSpace = world.project(this._mainCamera)
        return [(window.innerWidth * (screenSpace.x + 1.0)) / 2.0, (window.innerHeight * (1.0 - screenSpace.y)) / 2.0]
    }

    /**
     * TODO: remove
     * Updates the skybox colors based on the current theme

     * @param currentTheme: current theme from ThemeContext.useTheme()
     */
    // public updateSkyboxColors(currentTheme: Theme) {
    //     if (!this._skybox) return
    //     if (this._skybox.material instanceof THREE.ShaderMaterial) {
    //         this._skybox.material.uniforms.rColor.value = currentTheme["Background"]["color"]["r"]
    //         this._skybox.material.uniforms.gColor.value = currentTheme["Background"]["color"]["g"]
    //         this._skybox.material.uniforms.bColor.value = currentTheme["Background"]["color"]["b"]
    //     }
    // }

    /** returns whether any gizmos are being currently dragged */
    public isAnyGizmoDragging(): boolean {
        return [...this._gizmosOnMirabuf.values()].some(obj => obj.gizmo.dragging)
    }

    /**
     * Adding object to scene
     *
     * @param obj Object to add
     */
    public addObject(obj: THREE.Object3D) {
        this._scene.add(obj)
    }

    /**
     * Removing object from scene
     *
     * @param obj Object to remove
     */
    public removeObject(obj: THREE.Object3D) {
        this._scene.remove(obj)
    }

    /**
     * Sets up the threejs material for cascading shadows if the CSM is enabled
     *
     * @param material
     */
    public setupMaterial(material: THREE.Material) {
        if (this._light instanceof CSM) this._light.setupMaterial(material)
    }

    /**
     * Context Menu handler for the scene canvas.
     *
     * @param e Mouse event data.
     */
    public onContextMenu(e: InteractionEnd) {
        // Cast ray into physics scene.
        const origin = this.mainCamera.position

        const worldSpace = this.pixelToWorldSpace(e.position[0], e.position[1])
        const dir = worldSpace.sub(origin).normalize().multiplyScalar(40.0)

        const res = World.physicsSystem.rayCast(
            convertThreeVector3ToJoltVec3(origin),
            convertThreeVector3ToJoltVec3(dir)
        )

        // Use any associations to determine ContextData.
        let miraSupplierData: ContextData | undefined
        if (res) {
            const assoc = World.physicsSystem.getBodyAssociation(res.data.mBodyID) as RigidNodeAssociate
            const sceneObject = assoc?.sceneObject
            if (sceneObject) {
                if (
                    !World.multiplayerSystem ||
                    (sceneObject.miraType === MiraType.ROBOT &&
                        World.multiplayerSystem
                            ?.getOwnRobots()
                            .map(obj => obj.id)
                            .includes(sceneObject.id))
                ) {
                    miraSupplierData = assoc.sceneObject.getSupplierData()
                }
            }
        }
        // All else fails, present default options.
        if (!miraSupplierData) {
            miraSupplierData = { title: "The Scene", items: [] }
            miraSupplierData.items.push({
                name: "Add",
                func: () => {
                    globalOpenPanel(ImportMirabufPanel, {
                        configurationType: "ROBOTS" as ConfigurationType,
                    })
                },
            })
        }

        ContextSupplierEvent.dispatch(miraSupplierData, e.position)
    }
}

export default SceneRenderer
