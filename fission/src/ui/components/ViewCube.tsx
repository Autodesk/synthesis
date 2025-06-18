import React, { useEffect, useRef, useState } from "react"
import * as THREE from "three"
import { Box } from "@mui/material"
import World from "@/systems/World"
import { CustomOrbitControls } from "@/systems/scene/CameraControls"

interface ViewCubeProps {
    size?: number
    position?: { top?: number; left?: number; right?: number; bottom?: number }
}

const ViewCube: React.FC<ViewCubeProps> = ({ size = 100, position = { top: 20, right: 20 } }) => {
    const containerRef = useRef<HTMLDivElement>(null)
    const sceneRef = useRef<THREE.Scene>()
    const rendererRef = useRef<THREE.WebGLRenderer>()
    const cameraRef = useRef<THREE.OrthographicCamera>()
    const cubeRef = useRef<THREE.Group>()
    const [hoveredElement, setHoveredElement] = useState<{ type: string, index: number } | null>(null)

    const getTopBottomOrientation = (isTop: boolean) => {
        if (World && World.SceneRenderer && World.SceneRenderer.currentCameraControls) {
            const controls = World.SceneRenderer.currentCameraControls
            if (controls instanceof CustomOrbitControls) {
                const currentCoords = controls.getCurrentCoordinates()
                
                const quarterTurn = Math.PI / 2
                const roundedTheta = Math.round(currentCoords.theta / quarterTurn) * quarterTurn
                
                return {
                    theta: roundedTheta,
                    phi: isTop ? -Math.PI / 2 : Math.PI / 2
                }
            }
        }
        
        return {
            theta: 0,
            phi: isTop ? -Math.PI / 2 : Math.PI / 2
        }
    }

    const orientations = {
        front: { theta: 0, phi: 0 },
        back: { theta: Math.PI, phi: 0 },
        right: { theta: Math.PI / 2, phi: 0 },
        left: { theta: -Math.PI / 2, phi: 0 },
        top: { theta: 0, phi: -Math.PI / 2 },
        bottom: { theta: 0, phi: Math.PI / 2 },

        isometricFrontRightTop: { theta: Math.PI / 4, phi: -Math.PI / 6 },
        isometricFrontLeftTop: { theta: -Math.PI / 4, phi: -Math.PI / 6 },
        isometricBackRightTop: { theta: 3 * Math.PI / 4, phi: -Math.PI / 6 },
        isometricBackLeftTop: { theta: -3 * Math.PI / 4, phi: -Math.PI / 6 },
        isometricFrontRightBottom: { theta: Math.PI / 4, phi: Math.PI / 6 },
        isometricFrontLeftBottom: { theta: -Math.PI / 4, phi: Math.PI / 6 },
        isometricBackRightBottom: { theta: 3 * Math.PI / 4, phi: Math.PI / 6 },
        isometricBackLeftBottom: { theta: -3 * Math.PI / 4, phi: Math.PI / 6 },
    }

    useEffect(() => {
        if (!containerRef.current) return

        const scene = new THREE.Scene()
        sceneRef.current = scene

        const camera = new THREE.OrthographicCamera(-2, 2, 2, -2, 0.1, 100)
        camera.position.set(5, 5, 5)
        camera.lookAt(0, 0, 0)
        cameraRef.current = camera

        const renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true })
        renderer.setSize(size, size)
        renderer.setClearColor(0x000000, 0)
        renderer.domElement.style.pointerEvents = 'none'
        renderer.domElement.style.position = 'absolute'
        renderer.domElement.style.top = '0'
        renderer.domElement.style.left = '0'
        renderer.domElement.style.width = '100%'
        renderer.domElement.style.height = '100%'
        rendererRef.current = renderer
        containerRef.current.appendChild(renderer.domElement)

        const cubeGroup = new THREE.Group()
        cubeRef.current = cubeGroup

        const geometry = new THREE.BoxGeometry(2, 2, 2)
        const materials = [
            createFaceMaterial("R", 0xa6a6a6),
            createFaceMaterial("L", 0xa6a6a6),
            createFaceMaterial("T", 0xa6a6a6),
            createFaceMaterial("B", 0xa6a6a6),
            createFaceMaterial("F", 0xa6a6a6),
            createFaceMaterial("K", 0xa6a6a6),
        ]

        const cube = new THREE.Mesh(geometry, materials)
        cube.userData = { type: 'face' }
        cubeGroup.add(cube)

        const edges = new THREE.EdgesGeometry(geometry)
        const edgeLineMaterial = new THREE.LineBasicMaterial({
            color: 0x666666,
            linewidth: 2,
            transparent: true,
            opacity: 0.8
        })
        const wireframe = new THREE.LineSegments(edges, edgeLineMaterial)
        wireframe.userData = { type: 'wireframe' }
        cubeGroup.add(wireframe)

        const cornerGeometry = new THREE.SphereGeometry(0.15)
        const cornerMaterial = new THREE.MeshBasicMaterial({
            color: 0x888888,
            transparent: true,
            opacity: 0,
            depthTest: false,
            depthWrite: false 
        })
        
        const cornerPositions = [
            [1, 1, 1], [1, 1, -1], [1, -1, 1], [1, -1, -1],
            [-1, 1, 1], [-1, 1, -1], [-1, -1, 1], [-1, -1, -1]
        ]
        
        cornerPositions.forEach((pos, i) => {
            const cornerSphere = new THREE.Mesh(cornerGeometry, cornerMaterial.clone())
            cornerSphere.position.set(pos[0], pos[1], pos[2])
            cornerSphere.userData = { type: 'corner-sphere', index: i }
            cubeGroup.add(cornerSphere)
        })

        const edgeGeometry = new THREE.CylinderGeometry(0.04, 0.04, 2.2)
        const edgeMaterial = new THREE.MeshBasicMaterial({
            color: 0xff8800,
            transparent: true,
            opacity: 0,
            depthTest: false,
            depthWrite: false
        })

        const edgeConfigs = [
            // X-axis edges
            { pos: [0, 1, 1], rot: [0, 0, Math.PI/2] },
            { pos: [0, 1, -1], rot: [0, 0, Math.PI/2] },
            { pos: [0, -1, 1], rot: [0, 0, Math.PI/2] },
            { pos: [0, -1, -1], rot: [0, 0, Math.PI/2] },
            
            // Y-axis edges
            { pos: [1, 0, 1], rot: [0, 0, 0] },
            { pos: [1, 0, -1], rot: [0, 0, 0] },
            { pos: [-1, 0, 1], rot: [0, 0, 0] },
            { pos: [-1, 0, -1], rot: [0, 0, 0] },
            
            // Z-axis edges
            { pos: [1, 1, 0], rot: [Math.PI/2, 0, 0] },
            { pos: [1, -1, 0], rot: [Math.PI/2, 0, 0] },
            { pos: [-1, 1, 0], rot: [Math.PI/2, 0, 0] },
            { pos: [-1, -1, 0], rot: [Math.PI/2, 0, 0] }
        ]

        edgeConfigs.forEach((config, i) => {
            const edgeCylinder = new THREE.Mesh(edgeGeometry, edgeMaterial.clone())
            edgeCylinder.position.set(config.pos[0], config.pos[1], config.pos[2])
            edgeCylinder.rotation.set(config.rot[0], config.rot[1], config.rot[2])
            edgeCylinder.userData = { type: 'edge-highlight', index: i }
            cubeGroup.add(edgeCylinder)

            const edgeHitGeometry = new THREE.CylinderGeometry(0.25, 0.25, 2.2)
            const edgeHitMaterial = new THREE.MeshBasicMaterial({
                transparent: true,
                opacity: 0,
                visible: false
            })
            const edgeHitArea = new THREE.Mesh(edgeHitGeometry, edgeHitMaterial)
            edgeHitArea.position.set(config.pos[0], config.pos[1], config.pos[2])
            edgeHitArea.rotation.set(config.rot[0], config.rot[1], config.rot[2])
            edgeHitArea.userData = { type: 'edge-hit', index: i }
            cubeGroup.add(edgeHitArea)
        })

        scene.add(cubeGroup)

        const ambientLight = new THREE.AmbientLight(0xffffff, 0.6)
        scene.add(ambientLight)

        const directionalLight = new THREE.DirectionalLight(0xffffff, 0.4)
        directionalLight.position.set(5, 5, 5)
        scene.add(directionalLight)

        const animate = () => {
            if (rendererRef.current && sceneRef.current && cameraRef.current) {
                const mainCamera = World.SceneRenderer.mainCamera;
                const controls = World.SceneRenderer.currentCameraControls;

                if (mainCamera && cubeRef.current && controls instanceof CustomOrbitControls) {
                    const coords = controls.getCurrentCoordinates();

                    const camEuler = new THREE.Euler(coords.phi + Math.asin(1/Math.sqrt(3)), coords.theta, 0, 'YXZ');
                    const camQuat = new THREE.Quaternion().setFromEuler(camEuler).invert();

                    const offsetQuat = new THREE.Quaternion().setFromEuler(
                        new THREE.Euler(0, Math.PI / 4, 0)
                    );

                    cubeRef.current.quaternion.copy(offsetQuat).multiply(camQuat);
                }

                rendererRef.current.render(sceneRef.current, cameraRef.current);
            }
            requestAnimationFrame(animate);
        };

        animate()

        return () => {
            if (containerRef.current && renderer.domElement) {
                containerRef.current.removeChild(renderer.domElement)
            }
            renderer.dispose()
        }
    }, [size])

    const createFaceMaterial = (text: string, color: number) => {
        const canvas = document.createElement('canvas')
        const context = canvas.getContext('2d')!
        canvas.width = 128
        canvas.height = 128

        const gradient = context.createLinearGradient(0, 0, 128, 128)
        const baseColor = `#${color.toString(16).padStart(6, '0')}`

        const r = (color >> 16) & 255
        const g = (color >> 8) & 255
        const b = color & 255
        const darkColor = `#${Math.floor(r * 0.7).toString(16).padStart(2, '0')}${Math.floor(g * 0.7).toString(16).padStart(2, '0')}${Math.floor(b * 0.7).toString(16).padStart(2, '0')}`

        gradient.addColorStop(0, baseColor)
        gradient.addColorStop(1, darkColor)

        context.fillStyle = gradient
        context.fillRect(0, 0, 128, 128)

        context.strokeStyle = 'rgba(255, 255, 255, 0.3)'
        context.lineWidth = 2
        context.strokeRect(1, 1, 126, 126)

        context.strokeStyle = 'rgba(0, 0, 0, 0.3)'
        context.lineWidth = 1
        context.strokeRect(4, 4, 120, 120)

        context.shadowColor = 'rgba(0, 0, 0, 0.5)'
        context.shadowBlur = 2
        context.shadowOffsetX = 1
        context.shadowOffsetY = 1

        context.fillStyle = 'white'
        context.font = 'bold 42px Arial, sans-serif'
        context.textAlign = 'center'
        context.textBaseline = 'middle'
        context.fillText(text, 64, 64)

        const texture = new THREE.CanvasTexture(canvas)
        texture.minFilter = THREE.LinearFilter
        texture.magFilter = THREE.LinearFilter
        return new THREE.MeshLambertMaterial({ map: texture, transparent: true })
    }

    const getClickedElement = (event: React.MouseEvent) => {
        if (!rendererRef.current || !cameraRef.current || !sceneRef.current || !cubeRef.current) return null

        const rect = event.currentTarget.getBoundingClientRect()
        const x = ((event.clientX - rect.left) / rect.width) * 2 - 1
        const y = -((event.clientY - rect.top) / rect.height) * 2 + 1

        const raycaster = new THREE.Raycaster()
        raycaster.setFromCamera(new THREE.Vector2(x, y), cameraRef.current)

        const cornerSpheres = cubeRef.current.children.filter(child => 
            child.userData.type === 'corner-sphere'
        )
        const cornerIntersects = raycaster.intersectObjects(cornerSpheres, false)
        if (cornerIntersects.length > 0) {
            const cornerIndex = cornerIntersects[0].object.userData.index
            return { type: 'corner', index: cornerIndex }
        }

        const edgeHitAreas = cubeRef.current.children.filter(child => 
            child.userData.type === 'edge-hit'
        )
        const edgeIntersects = raycaster.intersectObjects(edgeHitAreas, false)
        if (edgeIntersects.length > 0) {
            const edgeIndex = edgeIntersects[0].object.userData.index
            return { type: 'edge', index: edgeIndex }
        }

        const mainCube = cubeRef.current.children.find(child => child.userData.type === 'face')
        if (mainCube) {
            const faceIntersects = raycaster.intersectObject(mainCube, false)
            if (faceIntersects.length > 0) {
                const faceIndex = faceIntersects[0].face?.materialIndex || 0
                return { type: 'face', index: faceIndex }
            }
        }

        return null
    }

    const updateHighlights = (element: { type: string, index: number } | null) => {
        if (!cubeRef.current) return

        cubeRef.current.children.forEach(child => {
            if (child instanceof THREE.Mesh) {
                if (child.userData.type === 'face') {
                    if (Array.isArray(child.material)) {
                        child.material.forEach(mat => {
                            if (mat instanceof THREE.MeshLambertMaterial) {
                                mat.emissive.setHex(0x000000)
                                mat.needsUpdate = true
                            }
                        })
                    }
                } else if (child.userData.type === 'corner-sphere') {
                    if (child.material instanceof THREE.MeshBasicMaterial) {
                        child.material.transparent = true
                        child.material.opacity = 0
                        child.material.needsUpdate = true
                    }
                } else if (child.userData.type === 'edge-highlight') {
                    if (child.material instanceof THREE.MeshBasicMaterial) {
                        child.material.transparent = true
                        child.material.opacity = 0
                        child.material.needsUpdate = true
                    }
                } else if (child.userData.type === 'wireframe') {
                    if (child instanceof THREE.LineSegments && child.material instanceof THREE.LineBasicMaterial) {
                        child.material.color.setHex(0x666666)
                        child.material.opacity = 0.8
                        child.material.needsUpdate = true
                    }
                }
            }
        })

        if (!element) return

        if (element.type === 'face') {
            const mainCube = cubeRef.current.children.find(child => child.userData.type === 'face')
            if (mainCube instanceof THREE.Mesh && Array.isArray(mainCube.material)) {
                const material = mainCube.material[element.index]
                if (material instanceof THREE.MeshLambertMaterial) {
                    material.emissive.setHex(0x0066ff) // Blue
                    material.needsUpdate = true
                }
            }
        } else if (element.type === 'corner') {
            const cornerSpheres = cubeRef.current.children.filter(child => 
                child.userData.type === 'corner-sphere'
            )
            const targetCorner = cornerSpheres[element.index]
            if (targetCorner && targetCorner instanceof THREE.Mesh && targetCorner.material instanceof THREE.MeshBasicMaterial) {
                targetCorner.material.color.setHex(0x00ff00) // Green
                targetCorner.material.transparent = true
                targetCorner.material.opacity = 0.6
                targetCorner.material.needsUpdate = true
            }
        } else if (element.type === 'edge') {
            const edgeHighlights = cubeRef.current.children.filter(child => 
                child.userData.type === 'edge-highlight'
            )
            const targetEdge = edgeHighlights[element.index]
            if (targetEdge && targetEdge instanceof THREE.Mesh && targetEdge.material instanceof THREE.MeshBasicMaterial) {
                targetEdge.material.color.setHex(0xff8800) // Orange
                targetEdge.material.transparent = true
                targetEdge.material.opacity = 0.6
                targetEdge.material.needsUpdate = true
            }
        }
    }

    const snapToOrientation = (orientation: { theta: number; phi: number }) => {
        const controls = World.SceneRenderer.currentCameraControls
        if (controls instanceof CustomOrbitControls) {

            const currentCoords = controls.getCurrentCoordinates()
            let targetTheta = orientation.theta
            
            const currentTheta = currentCoords.theta
            const diff = targetTheta - currentTheta
            
            if (diff > Math.PI) {
                targetTheta -= 2 * Math.PI
            } else if (diff < -Math.PI) {
                targetTheta += 2 * Math.PI
            }

            controls.animateToOrientation(targetTheta, orientation.phi, 500)
        }
    }

    const handleMouseMove = (event: React.MouseEvent) => {
        const element = getClickedElement(event)
        setHoveredElement(element)
        updateHighlights(element)
    }

    const handleMouseEnter = (event: React.MouseEvent) => {
        const element = getClickedElement(event)
        setHoveredElement(element)
        updateHighlights(element)
    }

    const handleMouseLeave = () => {
        setHoveredElement(null)
        updateHighlights(null)
    }

    const handleClick = (event: React.MouseEvent) => {
        const element = getClickedElement(event)
        if (!element) return

        if (element.type === 'face') {
            const faceOrientations = ["right", "left", "top", "bottom", "front", "back"]
            const orientationKey = faceOrientations[element.index]
            
            let targetOrientation
            if (orientationKey === 'top') {
                targetOrientation = getTopBottomOrientation(true)
            } else if (orientationKey === 'bottom') {
                targetOrientation = getTopBottomOrientation(false)
            } else if (orientationKey && orientations[orientationKey as keyof typeof orientations]) {
                targetOrientation = orientations[orientationKey as keyof typeof orientations]
            }
            
            if (targetOrientation) {
                snapToOrientation(targetOrientation)
            }
        } else if (element.type === 'corner') {
            const cornerOrientations = [
                "isometricFrontRightTop",
                "isometricBackRightTop",
                "isometricFrontRightBottom",
                "isometricBackRightBottom",
                "isometricFrontLeftTop",
                "isometricBackLeftTop",
                "isometricFrontLeftBottom",
                "isometricBackLeftBottom"
            ]
            const orientationKey = cornerOrientations[element.index]
            
            if (orientationKey && orientations[orientationKey as keyof typeof orientations]) {
                snapToOrientation(orientations[orientationKey as keyof typeof orientations])
            }
        } else if (element.type === 'edge') {
            const edgeOrientations = [
                // X-axis edges
                { theta: 0, phi: -Math.PI / 4 },
                { theta: Math.PI, phi: -Math.PI / 4 },
                { theta: 0, phi: Math.PI / 4 }, 
                { theta: Math.PI, phi: Math.PI / 4 },
                // Y-axis edges
                { theta: Math.PI / 4, phi: 0 },
                { theta: 3 * Math.PI / 4, phi: 0 },
                { theta: -Math.PI / 4, phi: 0 },
                { theta: -3 * Math.PI / 4, phi: 0 },
                // Z-axis edges
                { theta: Math.PI / 2, phi: -Math.PI / 4 },
                { theta: Math.PI / 2, phi: Math.PI / 4 },
                { theta: -Math.PI / 2, phi: -Math.PI / 4 },
                { theta: -Math.PI / 2, phi: Math.PI / 4 }
            ]
            
            if (element.index < edgeOrientations.length) {
                snapToOrientation(edgeOrientations[element.index])
            }
        }
    }

    const getCursor = () => {
        if (!hoveredElement) return "default"
        
        switch (hoveredElement.type) {
            case 'face': return "pointer"
            case 'edge': return "pointer"  
            case 'corner': return "pointer"
            default: return "default"
        }
    }

    return (
        <Box
            ref={containerRef}
            sx={{
                position: "absolute",
                width: size,
                height: size,
                cursor: getCursor(),
                userSelect: "none",
                pointerEvents: "auto",
                ...position,
            }}
            onMouseMove={handleMouseMove}
            onMouseLeave={handleMouseLeave}
            onMouseEnter={handleMouseEnter}
            onClick={handleClick}
        />
    )
}

export default ViewCube
