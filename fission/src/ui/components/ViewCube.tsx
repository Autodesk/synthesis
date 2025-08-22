import { Box } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useRef, useState } from "react"
import * as THREE from "three"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { CustomOrbitControls } from "@/systems/scene/CameraControls"
import World from "@/systems/World"

interface ViewCubeProps {
    size?: number
    position?: { top?: number; left?: number; right?: number; bottom?: number }
    scaleWithWindow?: boolean
}

const ViewCube: React.FC<ViewCubeProps> = ({
    size = 100,
    position = { top: 20, right: 20 },
    scaleWithWindow = true,
}) => {
    const CLICK_THRESHOLD_PIXELS = 10 // Pixel threshold for distinguishing clicks from drags
    const containerRef = useRef<HTMLDivElement>(null)
    const sceneRef = useRef<THREE.Scene>()
    const rendererRef = useRef<THREE.WebGLRenderer>()
    const cameraRef = useRef<THREE.OrthographicCamera>()
    const cubeRef = useRef<THREE.Group>()
    const axisRef = useRef<THREE.Group>()
    const [hoveredElement, setHoveredElement] = useState<{ type: string; index: number } | null>(null)
    const [isDragging, setIsDragging] = useState(false)
    const [lastMousePos, setLastMousePos] = useState<{ x: number; y: number } | null>(null)
    const [currentMousePos, setCurrentMousePos] = useState<{ x: number; y: number } | null>(null)

    const [dragStartElement, setDragStartElement] = useState<{ type: string; index: number } | null>(null)
    const [windowSize, setWindowSize] = useState({ width: window.innerWidth, height: window.innerHeight })
    const [isPointerLocked, setIsPointerLocked] = useState(false)
    const [totalDragMovement, setTotalDragMovement] = useState<{ x: number; y: number }>({ x: 0, y: 0 })

    const calculateResponsiveSize = () => {
        if (!scaleWithWindow) return size

        const minSize = 40
        const maxSize = 140
        const baseWidth = 1920
        const scale = Math.min(windowSize.width / baseWidth, 1)

        return Math.max(minSize, Math.min(maxSize, minSize + (maxSize - minSize) * scale))
    }

    const responsiveSize = calculateResponsiveSize()
    const containerSize = responsiveSize * 1.4

    useEffect(() => {
        const handleResize = () => {
            setWindowSize({ width: window.innerWidth, height: window.innerHeight })
        }

        window.addEventListener("resize", handleResize)
        return () => window.removeEventListener("resize", handleResize)
    }, [])

    useEffect(() => {
        const handleGlobalMouseUp = () => {
            if (isDragging) {
                setIsDragging(false)
                setLastMousePos(null)

                if (dragStartElement) {
                    const totalDistance = Math.sqrt(
                        totalDragMovement.x * totalDragMovement.x + totalDragMovement.y * totalDragMovement.y
                    )
                    if (totalDistance < CLICK_THRESHOLD_PIXELS) {
                        handleElementClick(dragStartElement)
                    }
                }

                setDragStartElement(null)
                setTotalDragMovement({ x: 0, y: 0 })

                if (document.pointerLockElement) {
                    document.exitPointerLock()
                }
            }
        }

        const handleGlobalMouseMove = (event: MouseEvent) => {
            if (!isDragging) return

            let deltaX = 0
            let deltaY = 0

            if (isPointerLocked) {
                // Use movementX and movementY when pointer is locked
                deltaX = event.movementX ?? 0
                deltaY = event.movementY ?? 0
            } else if (lastMousePos) {
                // Use regular mouse tracking as fallback
                deltaX = event.clientX - lastMousePos.x
                deltaY = event.clientY - lastMousePos.y
                setLastMousePos({ x: event.clientX, y: event.clientY })
            }

            if (deltaX === 0 && deltaY === 0) return

            // Track total movement for click detection
            setTotalDragMovement(prev => ({
                x: prev.x + Math.abs(deltaX),
                y: prev.y + Math.abs(deltaY),
            }))

            const sensitivity = PreferencesSystem.getGlobalPreference("ViewCubeRotationSensitivity")

            const controls = World.sceneRenderer.currentCameraControls
            if (controls instanceof CustomOrbitControls) {
                const currentCoords = controls.getCurrentCoordinates()

                const newTheta = currentCoords.theta - deltaX * sensitivity
                const newPhi = currentCoords.phi - deltaY * sensitivity

                controls.setImmediateCoordinates({ theta: newTheta, phi: newPhi })
            }
        }

        const handleVisibilityChange = () => {
            if (document.hidden && isDragging) {
                setIsDragging(false)
                setLastMousePos(null)
                setDragStartElement(null)
                setTotalDragMovement({ x: 0, y: 0 })

                if (document.pointerLockElement) {
                    document.exitPointerLock()
                }
            }
        }

        const handlePointerLockChange = () => {
            setIsPointerLocked(document.pointerLockElement != null)
        }

        const handlePointerLockError = () => {
            // Fallback to regular mouse tracking if pointer lock fails
            setIsPointerLocked(false)
        }

        const handleKeyDown = (event: KeyboardEvent) => {
            // Allow escape key to exit pointer lock
            if (event.key === "Escape" && isDragging) {
                setIsDragging(false)
                setLastMousePos(null)
                setDragStartElement(null)
                setTotalDragMovement({ x: 0, y: 0 })
                document.exitPointerLock()
            }
        }

        if (isDragging) {
            document.addEventListener("mouseup", handleGlobalMouseUp)
            document.addEventListener("mousemove", handleGlobalMouseMove)
            document.addEventListener("visibilitychange", handleVisibilityChange)
            document.addEventListener("pointerlockchange", handlePointerLockChange)
            document.addEventListener("pointerlockerror", handlePointerLockError)
            document.addEventListener("keydown", handleKeyDown)
        }

        return () => {
            document.removeEventListener("mouseup", handleGlobalMouseUp)
            document.removeEventListener("mousemove", handleGlobalMouseMove)
            document.removeEventListener("visibilitychange", handleVisibilityChange)
            document.removeEventListener("pointerlockchange", handlePointerLockChange)
            document.removeEventListener("pointerlockerror", handlePointerLockError)
            document.removeEventListener("keydown", handleKeyDown)
        }
    }, [isDragging, lastMousePos, isPointerLocked, dragStartElement, totalDragMovement])

    const updateHighlights = useCallback((element: { type: string; index: number } | null) => {
        if (!cubeRef.current) return

        cubeRef.current.children
            .filter(child => child instanceof THREE.Mesh)
            .forEach(child => {
                if (child.userData.type === "visual-face") {
                    if (Array.isArray(child.material)) {
                        child.material
                            .filter(mat => mat instanceof THREE.MeshLambertMaterial)
                            .forEach(mat => {
                                mat.emissive.setHex(0x000000)
                                mat.needsUpdate = true
                            })
                    }
                } else if (child.userData.type === "corner-sphere") {
                    if (child.material instanceof THREE.MeshBasicMaterial) {
                        child.material.transparent = true
                        child.material.opacity = 0
                        child.material.needsUpdate = true
                    }
                } else if (child.userData.type === "edge-highlight") {
                    if (child.material instanceof THREE.MeshBasicMaterial) {
                        child.material.transparent = true
                        child.material.opacity = 0
                        child.material.needsUpdate = true
                    }
                } else if (child.userData.type === "wireframe") {
                    if (child instanceof THREE.LineSegments && child.material instanceof THREE.LineBasicMaterial) {
                        child.material.color.setHex(0x999999)
                        child.material.opacity = 1.0
                        child.material.needsUpdate = true
                    }
                } else if (child.userData.type === "face-highlight") {
                    if (child.material instanceof THREE.MeshBasicMaterial) {
                        child.material.opacity = 0
                        child.material.needsUpdate = true
                    }
                } else if (child.userData.type === "edge-visual-highlight") {
                    if (child.material instanceof THREE.MeshBasicMaterial) {
                        child.material.opacity = 0
                        child.material.needsUpdate = true
                    }
                } else if (child.userData.type === "corner-visual-highlight") {
                    if (child.material instanceof THREE.MeshBasicMaterial) {
                        child.material.opacity = 0
                        child.material.needsUpdate = true
                    }
                }
            })

        if (!element) return

        if (element.type === "face") {
            const faceHighlights = cubeRef.current.children.filter(child => child.userData.type === "face-highlight")
            const targetFace = faceHighlights[element.index]
            if (
                targetFace &&
                targetFace instanceof THREE.Mesh &&
                targetFace.material instanceof THREE.MeshBasicMaterial
            ) {
                targetFace.material.opacity = 0.4
                targetFace.material.needsUpdate = true
            }
        } else if (element.type === "corner") {
            const cornerHighlights = cubeRef.current.children.filter(
                child => child.userData.type === "corner-visual-highlight" && child.userData.index === element.index
            )
            cornerHighlights.forEach(highlight => {
                if (highlight instanceof THREE.Mesh && highlight.material instanceof THREE.MeshBasicMaterial) {
                    highlight.material.opacity = 0.6
                    highlight.material.needsUpdate = true
                }
            })
        } else if (element.type === "edge") {
            const edgeHighlights = cubeRef.current.children.filter(
                child => child.userData.type === "edge-visual-highlight" && child.userData.index === element.index
            )
            edgeHighlights.forEach(highlight => {
                if (highlight instanceof THREE.Mesh && highlight.material instanceof THREE.MeshBasicMaterial) {
                    highlight.material.opacity = 0.6
                    highlight.material.needsUpdate = true
                }
            })
        }
    }, [])

    useEffect(() => {
        const handleGlobalMouseMoveForHighlights = (event: MouseEvent) => {
            if (!containerRef.current || isDragging) return

            const rect = containerRef.current.getBoundingClientRect()
            const isMouseOutside =
                event.clientX < rect.left ||
                event.clientX > rect.right ||
                event.clientY < rect.top ||
                event.clientY > rect.bottom

            if (isMouseOutside && hoveredElement) {
                setHoveredElement(null)
                updateHighlights(null)
            }
        }

        document.addEventListener("mousemove", handleGlobalMouseMoveForHighlights)

        return () => {
            document.removeEventListener("mousemove", handleGlobalMouseMoveForHighlights)
        }
    }, [hoveredElement, isDragging, updateHighlights])

    const getTopBottomOrientation = (isTop: boolean) => {
        if (World && World.sceneRenderer && World.sceneRenderer.currentCameraControls) {
            const controls = World.sceneRenderer.currentCameraControls
            if (controls instanceof CustomOrbitControls) {
                const currentCoords = controls.getCurrentCoordinates()

                const quarterTurn = Math.PI / 2
                const roundedTheta = Math.round(currentCoords.theta / quarterTurn) * quarterTurn

                return {
                    theta: roundedTheta,
                    phi: isTop ? -Math.PI / 2 : Math.PI / 2,
                }
            }
        }

        return {
            theta: 0,
            phi: isTop ? -Math.PI / 2 : Math.PI / 2,
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
        isometricBackRightTop: { theta: (3 * Math.PI) / 4, phi: -Math.PI / 6 },
        isometricBackLeftTop: { theta: (-3 * Math.PI) / 4, phi: -Math.PI / 6 },
        isometricFrontRightBottom: { theta: Math.PI / 4, phi: Math.PI / 6 },
        isometricFrontLeftBottom: { theta: -Math.PI / 4, phi: Math.PI / 6 },
        isometricBackRightBottom: { theta: (3 * Math.PI) / 4, phi: Math.PI / 6 },
        isometricBackLeftBottom: { theta: (-3 * Math.PI) / 4, phi: Math.PI / 6 },
    }

    const createFaceMaterial = useCallback((text: string, color: number): THREE.MeshLambertMaterial => {
        const canvas = document.createElement("canvas")
        const context = canvas.getContext("2d")!
        canvas.width = 256
        canvas.height = 256

        const baseColor = `#${color.toString(16).padStart(6, "0")}`
        context.fillStyle = baseColor
        context.fillRect(0, 0, 256, 256)

        context.strokeStyle = "rgba(0, 0, 0, 0.15)"
        context.lineWidth = 2
        context.strokeRect(1, 1, 254, 254)

        context.strokeStyle = "rgba(255, 255, 255, 0.4)"
        context.lineWidth = 1
        context.strokeRect(2, 2, 252, 252)

        context.fillStyle = "#333333"

        const fontSize = text === "BOTTOM" ? 52 : 58
        context.font = `bold ${fontSize}px 'Segoe UI', Arial, sans-serif`
        context.textAlign = "center"
        context.textBaseline = "middle"

        context.shadowColor = "rgba(255, 255, 255, 0.3)"
        context.shadowBlur = 1
        context.shadowOffsetX = 0
        context.shadowOffsetY = 1

        context.fillText(text, 128, 128)

        const texture = new THREE.CanvasTexture(canvas)
        texture.minFilter = THREE.LinearFilter
        texture.magFilter = THREE.LinearFilter
        return new THREE.MeshLambertMaterial({ map: texture, transparent: true, opacity: 0.7 })
    }, [])
    useEffect(() => {
        if (!containerRef.current) return

        const scene = new THREE.Scene()
        sceneRef.current = scene

        const scaleFactor = containerSize / responsiveSize
        const bound = 2 * scaleFactor
        const camera = new THREE.OrthographicCamera(-bound, bound, bound, -bound, 0.1, 100)
        camera.position.set(5, 5, 5)
        camera.lookAt(0, 0, 0)
        cameraRef.current = camera

        const renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true })
        renderer.setSize(containerSize, containerSize)
        renderer.setClearColor(0x000000, 0)
        renderer.domElement.style.pointerEvents = "none"
        renderer.domElement.style.position = "absolute"
        renderer.domElement.style.top = "0"
        renderer.domElement.style.left = "0"
        renderer.domElement.style.width = "100%"
        renderer.domElement.style.height = "100%"
        rendererRef.current = renderer
        containerRef.current.appendChild(renderer.domElement)

        const cubeGroup = new THREE.Group()
        cubeRef.current = cubeGroup

        const geometry = new THREE.BoxGeometry(2, 2, 2)
        const materials = [
            createFaceMaterial("RIGHT", 0xffffff),
            createFaceMaterial("LEFT", 0xffffff),
            createFaceMaterial("TOP", 0xffffff),
            createFaceMaterial("BOTTOM", 0xffffff),
            createFaceMaterial("FRONT", 0xffffff),
            createFaceMaterial("BACK", 0xffffff),
        ]

        const cube = new THREE.Mesh(geometry, materials)
        cube.userData = { type: "visual-face" }
        cubeGroup.add(cube)

        const faceClickGeometry = new THREE.PlaneGeometry(1.2, 1.2)
        const invisibleMaterial = new THREE.MeshBasicMaterial({
            transparent: true,
            opacity: 0,
            visible: false,
        })

        const faceConfigs = [
            { pos: [1.01, 0, 0], rot: [0, Math.PI / 2, 0], index: 0 },
            { pos: [-1.01, 0, 0], rot: [0, -Math.PI / 2, 0], index: 1 },
            { pos: [0, 1.01, 0], rot: [-Math.PI / 2, 0, 0], index: 2 },
            { pos: [0, -1.01, 0], rot: [Math.PI / 2, 0, 0], index: 3 },
            { pos: [0, 0, 1.01], rot: [0, 0, 0], index: 4 },
            { pos: [0, 0, -1.01], rot: [0, Math.PI, 0], index: 5 },
        ]

        faceConfigs.forEach(config => {
            const faceClickArea = new THREE.Mesh(faceClickGeometry, invisibleMaterial.clone())
            faceClickArea.position.set(config.pos[0], config.pos[1], config.pos[2])
            faceClickArea.rotation.set(config.rot[0], config.rot[1], config.rot[2])
            faceClickArea.userData = { type: "face-click", index: config.index }
            cubeGroup.add(faceClickArea)
        })

        const createEdgeHighlightStrips = () => {
            const edgeStripConfigs = [
                { pos: [0, 1.005, 0.8], rot: [-Math.PI / 2, 0, 0], size: [1.2, 0.4], edgeIndex: 0 },
                { pos: [0, 0.8, 1.005], rot: [0, 0, 0], size: [1.2, 0.4], edgeIndex: 0 },

                { pos: [0, 1.005, -0.8], rot: [-Math.PI / 2, 0, 0], size: [1.2, 0.4], edgeIndex: 1 },
                { pos: [0, 0.8, -1.005], rot: [0, Math.PI, 0], size: [1.2, 0.4], edgeIndex: 1 },

                { pos: [0, -1.005, 0.8], rot: [Math.PI / 2, 0, 0], size: [1.2, 0.4], edgeIndex: 2 },
                { pos: [0, -0.8, 1.005], rot: [0, 0, 0], size: [1.2, 0.4], edgeIndex: 2 },

                { pos: [0, -1.005, -0.8], rot: [Math.PI / 2, 0, 0], size: [1.2, 0.4], edgeIndex: 3 },
                { pos: [0, -0.8, -1.005], rot: [0, Math.PI, 0], size: [1.2, 0.4], edgeIndex: 3 },

                { pos: [0.8, 0, 1.005], rot: [0, 0, 0], size: [0.4, 1.2], edgeIndex: 4 },
                { pos: [1.005, 0, 0.8], rot: [0, Math.PI / 2, 0], size: [0.4, 1.2], edgeIndex: 4 },

                { pos: [0.8, 0, -1.005], rot: [0, Math.PI, 0], size: [0.4, 1.2], edgeIndex: 5 },
                { pos: [1.005, 0, -0.8], rot: [0, Math.PI / 2, 0], size: [0.4, 1.2], edgeIndex: 5 },

                { pos: [-0.8, 0, 1.005], rot: [0, 0, 0], size: [0.4, 1.2], edgeIndex: 6 },
                { pos: [-1.005, 0, 0.8], rot: [0, -Math.PI / 2, 0], size: [0.4, 1.2], edgeIndex: 6 },

                { pos: [-0.8, 0, -1.005], rot: [0, Math.PI, 0], size: [0.4, 1.2], edgeIndex: 7 },
                { pos: [-1.005, 0, -0.8], rot: [0, -Math.PI / 2, 0], size: [0.4, 1.2], edgeIndex: 7 },

                { pos: [1.005, 0.8, 0], rot: [Math.PI / 2, Math.PI / 2, 0], size: [0.4, 1.2], edgeIndex: 8 },
                { pos: [0.8, 1.005, 0], rot: [-Math.PI / 2, 0, 0], size: [0.4, 1.2], edgeIndex: 8 },

                { pos: [1.005, -0.8, 0], rot: [Math.PI / 2, Math.PI / 2, 0], size: [0.4, 1.2], edgeIndex: 9 },
                { pos: [0.8, -1.005, 0], rot: [Math.PI / 2, 0, 0], size: [0.4, 1.2], edgeIndex: 9 },

                { pos: [-1.005, 0.8, 0], rot: [Math.PI / 2, -Math.PI / 2, 0], size: [0.4, 1.2], edgeIndex: 10 },
                { pos: [-0.8, 1.005, 0], rot: [-Math.PI / 2, 0, 0], size: [0.4, 1.2], edgeIndex: 10 },

                { pos: [-1.005, -0.8, 0], rot: [Math.PI / 2, -Math.PI / 2, 0], size: [0.4, 1.2], edgeIndex: 11 },
                { pos: [-0.8, -1.005, 0], rot: [Math.PI / 2, 0, 0], size: [0.4, 1.2], edgeIndex: 11 },
            ]

            edgeStripConfigs.forEach(config => {
                const stripGeometry = new THREE.PlaneGeometry(config.size[0], config.size[1])
                const stripMaterial = new THREE.MeshBasicMaterial({
                    color: 0xdaaf03,
                    transparent: true,
                    opacity: 0,
                    depthTest: true,
                    depthWrite: false,
                })
                const edgeStrip = new THREE.Mesh(stripGeometry, stripMaterial)
                edgeStrip.position.set(config.pos[0], config.pos[1], config.pos[2])
                edgeStrip.rotation.set(config.rot[0], config.rot[1], config.rot[2])
                edgeStrip.userData = { type: "edge-visual-highlight", index: config.edgeIndex }
                cubeGroup.add(edgeStrip)
            })
        }

        const createCornerHighlightSquares = () => {
            const cornerSquareConfigs = [
                { pos: [1.005, 0.8, 0.8], rot: [0, Math.PI / 2, 0], cornerIndex: 0 },
                { pos: [0.8, 1.005, 0.8], rot: [-Math.PI / 2, 0, 0], cornerIndex: 0 },
                { pos: [0.8, 0.8, 1.005], rot: [0, 0, 0], cornerIndex: 0 },

                { pos: [1.005, 0.8, -0.8], rot: [0, Math.PI / 2, 0], cornerIndex: 1 },
                { pos: [0.8, 1.005, -0.8], rot: [-Math.PI / 2, 0, 0], cornerIndex: 1 },
                { pos: [0.8, 0.8, -1.005], rot: [0, Math.PI, 0], cornerIndex: 1 },

                { pos: [1.005, -0.8, 0.8], rot: [0, Math.PI / 2, 0], cornerIndex: 2 },
                { pos: [0.8, -1.005, 0.8], rot: [Math.PI / 2, 0, 0], cornerIndex: 2 },
                { pos: [0.8, -0.8, 1.005], rot: [0, 0, 0], cornerIndex: 2 },

                { pos: [1.005, -0.8, -0.8], rot: [0, Math.PI / 2, 0], cornerIndex: 3 },
                { pos: [0.8, -1.005, -0.8], rot: [Math.PI / 2, 0, 0], cornerIndex: 3 },
                { pos: [0.8, -0.8, -1.005], rot: [0, Math.PI, 0], cornerIndex: 3 },

                { pos: [-1.005, 0.8, 0.8], rot: [0, -Math.PI / 2, 0], cornerIndex: 4 },
                { pos: [-0.8, 1.005, 0.8], rot: [-Math.PI / 2, 0, 0], cornerIndex: 4 },
                { pos: [-0.8, 0.8, 1.005], rot: [0, 0, 0], cornerIndex: 4 },

                { pos: [-1.005, 0.8, -0.8], rot: [0, -Math.PI / 2, 0], cornerIndex: 5 },
                { pos: [-0.8, 1.005, -0.8], rot: [-Math.PI / 2, 0, 0], cornerIndex: 5 },
                { pos: [-0.8, 0.8, -1.005], rot: [0, Math.PI, 0], cornerIndex: 5 },

                { pos: [-1.005, -0.8, 0.8], rot: [0, -Math.PI / 2, 0], cornerIndex: 6 },
                { pos: [-0.8, -1.005, 0.8], rot: [Math.PI / 2, 0, 0], cornerIndex: 6 },
                { pos: [-0.8, -0.8, 1.005], rot: [0, 0, 0], cornerIndex: 6 },

                { pos: [-1.005, -0.8, -0.8], rot: [0, -Math.PI / 2, 0], cornerIndex: 7 },
                { pos: [-0.8, -1.005, -0.8], rot: [Math.PI / 2, 0, 0], cornerIndex: 7 },
                { pos: [-0.8, -0.8, -1.005], rot: [0, Math.PI, 0], cornerIndex: 7 },
            ]

            cornerSquareConfigs.forEach(config => {
                const squareGeometry = new THREE.PlaneGeometry(0.4, 0.4)
                const squareMaterial = new THREE.MeshBasicMaterial({
                    color: 0xdaaf03,
                    transparent: true,
                    opacity: 0,
                    depthTest: false,
                    depthWrite: false,
                })
                const cornerSquare = new THREE.Mesh(squareGeometry, squareMaterial)
                const adjustedPos = config.pos.map(coord => {
                    if (Math.abs(coord) > 1) {
                        return coord > 0 ? 1.02 : -1.02
                    }
                    return coord
                })
                cornerSquare.position.set(adjustedPos[0], adjustedPos[1], adjustedPos[2])
                cornerSquare.rotation.set(config.rot[0], config.rot[1], config.rot[2])
                cornerSquare.userData = { type: "corner-visual-highlight", index: config.cornerIndex }
                cornerSquare.renderOrder = 999
                cubeGroup.add(cornerSquare)
            })
        }

        createEdgeHighlightStrips()
        createCornerHighlightSquares()

        faceConfigs.forEach(config => {
            const faceHighlightGeometry = new THREE.PlaneGeometry(1.2, 1.2)
            const faceHighlightMaterial = new THREE.MeshBasicMaterial({
                color: 0xdaaf03,
                transparent: true,
                opacity: 0,
                depthTest: true,
                depthWrite: false,
            })
            const faceHighlight = new THREE.Mesh(faceHighlightGeometry, faceHighlightMaterial)
            faceHighlight.position.set(config.pos[0] * 1.002, config.pos[1] * 1.002, config.pos[2] * 1.002)
            faceHighlight.rotation.set(config.rot[0], config.rot[1], config.rot[2])
            faceHighlight.userData = { type: "face-highlight", index: config.index }
            cubeGroup.add(faceHighlight)
        })

        const edges = new THREE.EdgesGeometry(geometry)
        const edgeLineMaterial = new THREE.LineBasicMaterial({
            color: 0x777777,
            linewidth: 1,
            transparent: true,
            opacity: 1.0,
        })
        const wireframe = new THREE.LineSegments(edges, edgeLineMaterial)
        wireframe.userData = { type: "wireframe" }
        cubeGroup.add(wireframe)

        const cornerGeometry = new THREE.SphereGeometry(0.25)
        const cornerMaterial = new THREE.MeshBasicMaterial({
            color: 0xbbbbbb,
            transparent: true,
            opacity: 0,
            depthTest: false,
            depthWrite: false,
        })

        const cornerPositions = [
            [1, 1, 1],
            [1, 1, -1],
            [1, -1, 1],
            [1, -1, -1],
            [-1, 1, 1],
            [-1, 1, -1],
            [-1, -1, 1],
            [-1, -1, -1],
        ]

        cornerPositions.forEach((pos, i) => {
            const cornerSphere = new THREE.Mesh(cornerGeometry, cornerMaterial.clone())
            cornerSphere.position.set(pos[0], pos[1], pos[2])
            cornerSphere.userData = { type: "corner-sphere", index: i }
            cubeGroup.add(cornerSphere)
        })

        const edgeGeometry = new THREE.CylinderGeometry(0.04, 0.04, 2.2)
        const edgeMaterial = new THREE.MeshBasicMaterial({
            color: 0xff8800,
            transparent: true,
            opacity: 0,
            depthTest: false,
            depthWrite: false,
        })

        const edgeConfigs = [
            // X-axis edges
            { pos: [0, 1, 1], rot: [0, 0, Math.PI / 2] },
            { pos: [0, 1, -1], rot: [0, 0, Math.PI / 2] },
            { pos: [0, -1, 1], rot: [0, 0, Math.PI / 2] },
            { pos: [0, -1, -1], rot: [0, 0, Math.PI / 2] },
            // Y-axis edges
            { pos: [1, 0, 1], rot: [0, 0, 0] },
            { pos: [1, 0, -1], rot: [0, 0, 0] },
            { pos: [-1, 0, 1], rot: [0, 0, 0] },
            { pos: [-1, 0, -1], rot: [0, 0, 0] },
            // Z-axis edges
            { pos: [1, 1, 0], rot: [Math.PI / 2, 0, 0] },
            { pos: [1, -1, 0], rot: [Math.PI / 2, 0, 0] },
            { pos: [-1, 1, 0], rot: [Math.PI / 2, 0, 0] },
            { pos: [-1, -1, 0], rot: [Math.PI / 2, 0, 0] },
        ]

        edgeConfigs.forEach((config, i) => {
            const edgeCylinder = new THREE.Mesh(edgeGeometry, edgeMaterial.clone())
            edgeCylinder.position.set(config.pos[0], config.pos[1], config.pos[2])
            edgeCylinder.rotation.set(config.rot[0], config.rot[1], config.rot[2])
            edgeCylinder.userData = { type: "edge-highlight", index: i }
            cubeGroup.add(edgeCylinder)

            const edgeHitGeometry = new THREE.CylinderGeometry(0.2, 0.2, 1.4)
            const edgeHitMaterial = new THREE.MeshBasicMaterial({
                transparent: true,
                opacity: 0,
                visible: false,
            })
            const edgeHitArea = new THREE.Mesh(edgeHitGeometry, edgeHitMaterial)
            edgeHitArea.position.set(config.pos[0], config.pos[1], config.pos[2])
            edgeHitArea.rotation.set(config.rot[0], config.rot[1], config.rot[2])
            edgeHitArea.userData = { type: "edge-hit", index: i }
            cubeGroup.add(edgeHitArea)
        })

        scene.add(cubeGroup)

        const createAxisIndicators = () => {
            const axisGroup = new THREE.Group()

            const createAxisLine = (
                color: number,
                direction: THREE.Vector3,
                position: THREE.Vector3,
                label: string
            ) => {
                const group = new THREE.Group()

                const lineLength = 2.5
                const lineRadius = 0.015

                const lineGeometry = new THREE.CylinderGeometry(lineRadius, lineRadius, lineLength)
                const lineMaterial = new THREE.MeshBasicMaterial({ color })
                const line = new THREE.Mesh(lineGeometry, lineMaterial)

                if (Math.abs(direction.y) > 0.99) {
                    if (direction.y < 0) {
                        line.rotateX(Math.PI)
                    }
                } else {
                    const up = new THREE.Vector3(0, 1, 0)
                    const quaternion = new THREE.Quaternion()
                    quaternion.setFromUnitVectors(up, direction)
                    line.setRotationFromQuaternion(quaternion)
                }

                const canvas = document.createElement("canvas")
                const context = canvas.getContext("2d")!
                canvas.width = 256
                canvas.height = 256

                context.fillStyle = `#${color.toString(16).padStart(6, "0")}`
                context.font = "bold 192px Arial"
                context.textAlign = "center"
                context.textBaseline = "middle"
                context.fillText(label, 128, 128)

                const texture = new THREE.CanvasTexture(canvas)
                const labelMaterial = new THREE.SpriteMaterial({ map: texture })
                const labelSprite = new THREE.Sprite(labelMaterial)
                labelSprite.scale.set(0.8, 0.8, 1)
                labelSprite.position.copy(direction.clone().multiplyScalar(lineLength / 2 + 0.4))

                group.add(line)
                group.add(labelSprite)
                group.position.copy(position)

                return group
            }

            const lineShift = 0.25
            const lineInset = 1.01

            const xAxis = createAxisLine(
                0xff0000,
                new THREE.Vector3(-1, 0, 0),
                new THREE.Vector3(-lineShift, -lineInset, -lineInset),
                "X"
            )
            axisGroup.add(xAxis)

            const yAxis = createAxisLine(
                0x00ff00,
                new THREE.Vector3(0, 1, 0),
                new THREE.Vector3(lineInset, lineShift, -lineInset),
                "Y"
            )
            axisGroup.add(yAxis)

            const zAxis = createAxisLine(
                0x0000ff,
                new THREE.Vector3(0, 0, 1),
                new THREE.Vector3(lineInset, -lineInset, lineShift),
                "Z"
            )
            axisGroup.add(zAxis)

            return axisGroup
        }

        const axisIndicators = createAxisIndicators()
        scene.add(axisIndicators)
        axisRef.current = axisIndicators

        const ambientLight = new THREE.AmbientLight(0xffffff, 1.8)
        scene.add(ambientLight)

        const directionalLight = new THREE.DirectionalLight(0xffffff, 0.9)
        directionalLight.position.set(5, 5, 5)
        scene.add(directionalLight)

        const directionalLight2 = new THREE.DirectionalLight(0xffffff, 0.9)
        directionalLight2.position.set(-2, -2, -2)
        scene.add(directionalLight2)

        let animationFrameId: number

        const animate = () => {
            if (rendererRef.current && sceneRef.current && cameraRef.current) {
                const mainCamera = World.sceneRenderer.mainCamera
                const controls = World.sceneRenderer.currentCameraControls

                if (mainCamera && cubeRef.current && controls instanceof CustomOrbitControls) {
                    const coords = controls.getCurrentCoordinates()

                    const camEuler = new THREE.Euler(coords.phi + Math.asin(1 / Math.sqrt(3)), coords.theta, 0, "YXZ")
                    const camQuat = new THREE.Quaternion().setFromEuler(camEuler).invert()

                    const offsetQuat = new THREE.Quaternion().setFromEuler(new THREE.Euler(0, Math.PI / 4, 0))

                    cubeRef.current.quaternion.copy(offsetQuat).multiply(camQuat)

                    if (axisRef.current) {
                        axisRef.current.quaternion.copy(cubeRef.current.quaternion)
                    }
                }

                rendererRef.current.render(sceneRef.current, cameraRef.current)
            }
            animationFrameId = requestAnimationFrame(animate)
        }

        animate()

        return () => {
            if (animationFrameId) {
                cancelAnimationFrame(animationFrameId)
            }
            if (containerRef.current && renderer.domElement) {
                containerRef.current.removeChild(renderer.domElement)
            }
            renderer.dispose()
        }
    }, [responsiveSize, containerSize, createFaceMaterial])

    const getClickedElement = (event: React.MouseEvent) => {
        if (!rendererRef.current || !cameraRef.current || !sceneRef.current || !cubeRef.current) return null

        const rect = event.currentTarget.getBoundingClientRect()

        const overlayX = (event.clientX - rect.left) / rect.width
        const overlayY = (event.clientY - rect.top) / rect.height

        const offsetRatio = (containerSize - responsiveSize) / (2 * containerSize)
        const scaleRatio = responsiveSize / containerSize

        const rendererX = offsetRatio + overlayX * scaleRatio
        const rendererY = offsetRatio + overlayY * scaleRatio

        const x = rendererX * 2 - 1
        const y = -(rendererY * 2 - 1)

        const raycaster = new THREE.Raycaster()
        raycaster.setFromCamera(new THREE.Vector2(x, y), cameraRef.current)

        const cornerHighlights = cubeRef.current.children.filter(
            child => child.userData.type === "corner-visual-highlight"
        )

        const cornerIntersects = raycaster.intersectObjects(cornerHighlights, false)
        if (cornerIntersects.length > 0) {
            const cornerIndex = cornerIntersects[0].object.userData.index
            return { type: "corner", index: cornerIndex }
        }

        const edgeHighlights = cubeRef.current.children.filter(child => child.userData.type === "edge-visual-highlight")
        const edgeIntersects = raycaster.intersectObjects(edgeHighlights, false)
        if (edgeIntersects.length > 0) {
            const edgeIndex = edgeIntersects[0].object.userData.index
            return { type: "edge", index: edgeIndex }
        }

        const faceHighlights = cubeRef.current.children.filter(child => child.userData.type === "face-highlight")
        const faceIntersects = raycaster.intersectObjects(faceHighlights, false)
        if (faceIntersects.length > 0) {
            const faceIndex = faceIntersects[0].object.userData.index
            return { type: "face", index: faceIndex }
        }

        return null
    }

    const updateHighlightsAtCurrentPosition = () => {
        if (currentMousePos && containerRef.current) {
            const mockEvent = {
                currentTarget: containerRef.current.children[0],
                clientX: currentMousePos.x,
                clientY: currentMousePos.y,
                preventDefault: () => {},
            } as React.MouseEvent

            const element = getClickedElement(mockEvent)
            setHoveredElement(element)
            updateHighlights(element)
        } else {
            setHoveredElement(null)
            updateHighlights(null)
        }
    }

    const normalizeTheta = (theta: number): number => {
        while (theta > Math.PI) theta -= 2 * Math.PI
        while (theta < -Math.PI) theta += 2 * Math.PI
        return theta
    }

    const snapToOrientation = (orientation: { theta: number; phi: number }) => {
        const controls = World.sceneRenderer.currentCameraControls
        if (controls instanceof CustomOrbitControls) {
            const currentCoords = controls.getCurrentCoordinates()

            const normalizedCurrentTheta = normalizeTheta(currentCoords.theta)
            const normalizedTargetTheta = normalizeTheta(orientation.theta)

            let diff = normalizedTargetTheta - normalizedCurrentTheta
            if (diff > Math.PI) {
                diff -= 2 * Math.PI
            } else if (diff < -Math.PI) {
                diff += 2 * Math.PI
            }

            const finalTargetTheta = currentCoords.theta + diff

            controls.animateToOrientation(finalTargetTheta, orientation.phi, 280)

            setTimeout(() => {
                updateHighlightsAtCurrentPosition()
            }, 250)
        }
    }

    const handleMouseMove = (event: React.MouseEvent) => {
        // Only update mouse position and highlights when not dragging
        if (!isDragging) {
            setCurrentMousePos({ x: event.clientX, y: event.clientY })
            const element = getClickedElement(event)
            setHoveredElement(element)
            updateHighlights(element)
        }
    }

    const handleMouseDown = (event: React.MouseEvent) => {
        if (isMouseOverCube(event)) {
            setIsDragging(true)
            const mousePos = { x: event.clientX, y: event.clientY }
            setLastMousePos(mousePos)
            setDragStartElement(getClickedElement(event))
            setTotalDragMovement({ x: 0, y: 0 })

            // Request pointer lock for better cursor control
            if (containerRef.current) {
                containerRef.current.requestPointerLock()
            }

            event.preventDefault()
        }
    }

    const handleMouseEnter = (event: React.MouseEvent) => {
        setCurrentMousePos({ x: event.clientX, y: event.clientY })
        const element = getClickedElement(event)
        setHoveredElement(element)
        updateHighlights(element)
    }

    const handleMouseLeave = () => {
        setCurrentMousePos(null)
        setHoveredElement(null)
        updateHighlights(null)
    }

    const handleElementClick = (element: { type: string; index: number }) => {
        if (element.type === "face") {
            const faceOrientations = ["right", "left", "top", "bottom", "front", "back"]
            const orientationKey = faceOrientations[element.index]

            let targetOrientation
            if (orientationKey === "top") {
                targetOrientation = getTopBottomOrientation(true)
            } else if (orientationKey === "bottom") {
                targetOrientation = getTopBottomOrientation(false)
            } else if (orientationKey && orientations[orientationKey as keyof typeof orientations]) {
                targetOrientation = orientations[orientationKey as keyof typeof orientations]
            }

            if (targetOrientation) {
                snapToOrientation(targetOrientation)
            }
        } else if (element.type === "corner") {
            const cornerOrientations = [
                "isometricFrontRightTop",
                "isometricBackRightTop",
                "isometricFrontRightBottom",
                "isometricBackRightBottom",
                "isometricFrontLeftTop",
                "isometricBackLeftTop",
                "isometricFrontLeftBottom",
                "isometricBackLeftBottom",
            ]
            const orientationKey = cornerOrientations[element.index]

            if (orientationKey && orientations[orientationKey as keyof typeof orientations]) {
                snapToOrientation(orientations[orientationKey as keyof typeof orientations])
            }
        } else if (element.type === "edge") {
            const edgeOrientations = [
                // X-axis edges
                { theta: 0, phi: -Math.PI / 4 },
                { theta: Math.PI, phi: -Math.PI / 4 },
                { theta: 0, phi: Math.PI / 4 },
                { theta: Math.PI, phi: Math.PI / 4 },
                // Y-axis edges
                { theta: Math.PI / 4, phi: 0 },
                { theta: (3 * Math.PI) / 4, phi: 0 },
                { theta: -Math.PI / 4, phi: 0 },
                { theta: (-3 * Math.PI) / 4, phi: 0 },
                // Z-axis edges
                { theta: Math.PI / 2, phi: -Math.PI / 4 },
                { theta: Math.PI / 2, phi: Math.PI / 4 },
                { theta: -Math.PI / 2, phi: -Math.PI / 4 },
                { theta: -Math.PI / 2, phi: Math.PI / 4 },
            ]

            if (element.index < edgeOrientations.length) {
                snapToOrientation(edgeOrientations[element.index])
            }
        }
    }

    const getCursor = () => {
        if (isDragging) return "grabbing"
        if (!hoveredElement) return "default"
        return "pointer"
    }

    const isMouseOverCube = (event: React.MouseEvent) => {
        if (!rendererRef.current || !cameraRef.current || !sceneRef.current || !cubeRef.current) return false

        const rect = event.currentTarget.getBoundingClientRect()

        const overlayX = (event.clientX - rect.left) / rect.width
        const overlayY = (event.clientY - rect.top) / rect.height

        const offsetRatio = (containerSize - responsiveSize) / (2 * containerSize)
        const scaleRatio = responsiveSize / containerSize

        const rendererX = offsetRatio + overlayX * scaleRatio
        const rendererY = offsetRatio + overlayY * scaleRatio

        const x = rendererX * 2 - 1
        const y = -(rendererY * 2 - 1)

        const raycaster = new THREE.Raycaster()
        raycaster.setFromCamera(new THREE.Vector2(x, y), cameraRef.current)

        const allCubeElements = cubeRef.current.children.filter(child => {
            const type = child.userData.type
            return (
                type === "visual-face" ||
                type === "face-click" ||
                type === "edge-visual-highlight" ||
                type === "corner-visual-highlight" ||
                type === "face-highlight"
            )
        })

        const intersects = raycaster.intersectObjects(allCubeElements, false)
        return intersects.length > 0
    }

    return (
        <Box
            ref={containerRef}
            sx={{
                position: "absolute",
                width: containerSize,
                height: containerSize,
                pointerEvents: "none",
                ...position,
            }}
        >
            <Box
                sx={{
                    position: "absolute",
                    width: responsiveSize,
                    height: responsiveSize,
                    top: (containerSize - responsiveSize) / 2,
                    left: (containerSize - responsiveSize) / 2,
                    pointerEvents: "auto",
                    cursor: getCursor(),
                    userSelect: "none",
                }}
                onMouseMove={handleMouseMove}
                onMouseDown={handleMouseDown}
                onMouseLeave={handleMouseLeave}
                onMouseEnter={handleMouseEnter}
            />
        </Box>
    )
}

export default ViewCube
