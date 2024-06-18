/**
 * This example will be used to showcase how Jolt physics works.
 */

import * as THREE from "three"
import Stats from "stats.js"
import JOLT from "../util/loading/JoltSyncLoader.ts"
import { OrbitControls } from "three/addons/controls/OrbitControls.js"

import { useEffect, useRef } from "react"
import Jolt from "@barclah/jolt-physics"
import { mirabuf } from "../proto/mirabuf"
import { LoadMirabufRemote } from "../mirabuf/MirabufLoader.ts"
import {
    JoltVec3_ThreeVector3,
    JoltQuat_ThreeQuaternion,
} from "../util/TypeConversions.ts"
import {
    COUNT_OBJECT_LAYERS,
    LAYER_MOVING,
    LAYER_NOT_MOVING,
} from "../util/threejs/MeshCreation.ts"
import MirabufInstance from "../mirabuf/MirabufInstance.ts"
import MirabufParser, { ParseErrorSeverity } from "../mirabuf/MirabufParser.ts"

const clock = new THREE.Clock()
let time = 0

let stats: Stats

let renderer: THREE.WebGLRenderer
let camera: THREE.PerspectiveCamera
let scene: THREE.Scene

let joltInterface: Jolt.JoltInterface
// let physicsSystem: Jolt.PhysicsSystem;
// let bodyInterface: Jolt.BodyInterface;

const dynamicObjects: THREE.Mesh[] = []

const MIRA_FILE = "test_mira/Team_2471_(2018)_v7.mira"
// const MIRA_FILE = "test_mira/Dozer_v2.mira"

let controls: OrbitControls

// vvv Below are the functions required to initialize everything and draw a basic floor with collisions. vvv

function setupCollisionFiltering(settings: Jolt.JoltSettings) {
    const objectFilter = new JOLT.ObjectLayerPairFilterTable(
        COUNT_OBJECT_LAYERS
    )
    objectFilter.EnableCollision(LAYER_NOT_MOVING, LAYER_MOVING)
    objectFilter.EnableCollision(LAYER_MOVING, LAYER_MOVING)

    const BP_LAYER_NOT_MOVING = new JOLT.BroadPhaseLayer(LAYER_NOT_MOVING)
    const BP_LAYER_MOVING = new JOLT.BroadPhaseLayer(LAYER_MOVING)
    const COUNT_BROAD_PHASE_LAYERS = 2

    const bpInterface = new JOLT.BroadPhaseLayerInterfaceTable(
        COUNT_OBJECT_LAYERS,
        COUNT_BROAD_PHASE_LAYERS
    )
    bpInterface.MapObjectToBroadPhaseLayer(
        LAYER_NOT_MOVING,
        BP_LAYER_NOT_MOVING
    )
    bpInterface.MapObjectToBroadPhaseLayer(LAYER_MOVING, BP_LAYER_MOVING)

    settings.mObjectLayerPairFilter = objectFilter
    settings.mBroadPhaseLayerInterface = bpInterface
    settings.mObjectVsBroadPhaseLayerFilter =
        new JOLT.ObjectVsBroadPhaseLayerFilterTable(
            settings.mBroadPhaseLayerInterface,
            COUNT_BROAD_PHASE_LAYERS,
            settings.mObjectLayerPairFilter,
            COUNT_OBJECT_LAYERS
        )
}

function initPhysics() {
    const settings = new JOLT.JoltSettings()
    setupCollisionFiltering(settings)
    joltInterface = new JOLT.JoltInterface(settings)
    JOLT.destroy(settings)

    // physicsSystem = joltInterface.GetPhysicsSystem();
    // bodyInterface = physicsSystem.GetBodyInterface();
}

function initGraphics() {
    camera = new THREE.PerspectiveCamera(
        75,
        window.innerWidth / window.innerHeight,
        0.1,
        1000
    )

    camera.position.set(-5, 4, 5)

    scene = new THREE.Scene()

    renderer = new THREE.WebGLRenderer()
    renderer.setClearColor(0x121212)
    renderer.setPixelRatio(window.devicePixelRatio)
    renderer.shadowMap.enabled = true
    renderer.shadowMap.type = THREE.PCFSoftShadowMap
    renderer.setSize(window.innerWidth, window.innerHeight)

    controls = new OrbitControls(camera, renderer.domElement)
    controls.update()

    const directionalLight = new THREE.DirectionalLight(0xffffff, 3.0)
    directionalLight.position.set(-1.0, 3.0, 2.0)
    directionalLight.castShadow = true
    scene.add(directionalLight)

    const shadowMapSize = Math.min(4096, renderer.capabilities.maxTextureSize)
    const shadowCamSize = 15
    console.debug(`Shadow Map Size: ${shadowMapSize}`)

    directionalLight.shadow.camera.top = shadowCamSize
    directionalLight.shadow.camera.bottom = -shadowCamSize
    directionalLight.shadow.camera.left = -shadowCamSize
    directionalLight.shadow.camera.right = shadowCamSize
    directionalLight.shadow.mapSize = new THREE.Vector2(
        shadowMapSize,
        shadowMapSize
    )
    directionalLight.shadow.blurSamples = 16
    directionalLight.shadow.normalBias = 0.01
    directionalLight.shadow.bias = 0.0

    const ambientLight = new THREE.AmbientLight(0xffffff, 0.1)
    scene.add(ambientLight)

    // TODO: Add controls.

    // TODO: Add resize event
}

function updatePhysics(deltaTime: number) {
    // If below 55hz run 2 steps. Otherwise things run very slow.
    const numSteps = deltaTime > 1.0 / 55.0 ? 2 : 1
    joltInterface.Step(deltaTime, numSteps)
}

function render() {
    stats.update()
    requestAnimationFrame(render)
    controls.update()

    // Prevents a problem when rendering at 30hz. Referred to as the spiral of death.
    let deltaTime = clock.getDelta()
    deltaTime = Math.min(deltaTime, 1.0 / 30.0)

    // Update transforms.
    for (let i = 0, j = dynamicObjects.length; i < j; i++) {
        const threeObj = dynamicObjects[i]
        const body = threeObj.userData.body
        threeObj.position.copy(JoltVec3_ThreeVector3(body.GetPosition()))
        threeObj.quaternion.copy(JoltQuat_ThreeQuaternion(body.GetRotation()))

        if (body.GetBodyType() === JOLT.EBodyType_SoftBody) {
            // TODO: Special soft body handle.
        }
    }

    time += deltaTime
    updatePhysics(1.0 / 60.0)
    // controls.update(deltaTime); // TODO: Add controls?
    renderer.render(scene, camera)
}

function MyThree() {
    console.log("Running...")

    const refContainer = useRef<HTMLDivElement>(null)
    const urlParams = new URLSearchParams(document.location.search)
    let mira_path = MIRA_FILE

    urlParams.forEach((v, k) => console.debug(`${k}: ${v}`))

    if (urlParams.has("mira")) {
        mira_path = `test_mira/${urlParams.get("mira")!}`
        console.debug(`Selected Mirabuf File: ${mira_path}`)
    }
    console.log(urlParams)

    const addMiraToScene = (assembly: mirabuf.Assembly | undefined) => {
        if (!assembly) {
            console.error("Assembly is undefined")
            return
        }

        const parser = new MirabufParser(assembly)
        if (parser.maxErrorSeverity >= ParseErrorSeverity.Unimportable) {
            console.error(
                `Assembly Parser produced significant errors for '${assembly.info!.name!}'`
            )
            return
        }

        const instance = new MirabufInstance(parser)
        instance.AddToScene(scene)
    }

    useEffect(() => {
        LoadMirabufRemote(mira_path)
            .then((assembly: mirabuf.Assembly | undefined) =>
                addMiraToScene(assembly)
            )
            .catch(_ =>
                LoadMirabufRemote(MIRA_FILE).then(
                    (assembly: mirabuf.Assembly | undefined) =>
                        addMiraToScene(assembly)
                )
            )
            .catch(console.error)

        initGraphics()

        if (refContainer.current) {
            refContainer.current.innerHTML = ""
            refContainer.current.appendChild(renderer.domElement)

            stats = new Stats()
            stats.dom.style.position = "absolute"
            stats.dom.style.top = "0px"
            refContainer.current.appendChild(stats.dom)
        }

        initPhysics()
        render()

        // createFloor();
    }, [])

    return (
        <div>
            <div ref={refContainer}></div>
        </div>
    )
}

export default MyThree
