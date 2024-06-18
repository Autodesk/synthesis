import React, { useEffect } from "react"
import Label, { LabelSize } from "../components/Label"
import Panel, { PanelPropsImpl } from "../components/Panel"
import { IoPeople } from "react-icons/io5"
import World from "@/systems/World"
import JOLT from "@/util/loading/JoltSyncLoader"
import * as THREE from 'three'

function onClick(e: MouseEvent) {
    const from = new JOLT.Vec3(0, 20, 0)
    const dir = new JOLT.Vec3(0, -1, 0)

    const res = World.PhysicsSystem.RayCast(from, dir)
    
    if (res) {
        const ballMesh = World.SceneRenderer.CreateSphere(0.2, new THREE.MeshStandardMaterial({ emissive: 0xff00ff }))
        const hitPoint = res.point
        ballMesh.position.set(hitPoint.GetX(), hitPoint.GetY(), hitPoint.GetZ())
    }
}

const PokerPanel: React.FC<PanelPropsImpl> = ({ panelId,  }) => {

    useEffect(() => {
        World.SceneRenderer.renderer.domElement.addEventListener('click', onClick)

        return () => {
            World.SceneRenderer.renderer.domElement.removeEventListener('click', onClick)
        }
    }, [])

    return (
        <Panel name={"The Poker"} icon={<IoPeople />} panelId={panelId}>
            <Label size={LabelSize.Medium}>Poker</Label>
            
        </Panel>
    )
}

export default PokerPanel
