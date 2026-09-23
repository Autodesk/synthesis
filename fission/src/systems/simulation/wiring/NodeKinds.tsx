import type { ComponentType, PropsWithChildren } from "react"
import type { SimulationLayer } from "../SimulationSystem"
import { NodeKindId, nodeTargets, type HandleIdAlias, type HandleInfo, type SimConfigData } from "./SimGraph"
import { Handle, Position, type NodeProps } from "@xyflow/react"
import type { SimReceiver, SimSupplier } from "../wpilib_brain/SimDataFlow"
import { noraTypeToColorStr, type NoraType } from "../Nora"
import { compileSuppliersFor } from "./Compile"
import { RobotIONode } from "./nodes/RobotIONode"
import { SimType } from "../wpilib_brain/WPILibTypes"
import SimCANEncoder from "../wpilib_brain/sim/SimCANEncoder"
import SimAccel from "../wpilib_brain/sim/SimAccel"
import SimGyro from "../wpilib_brain/sim/SimGyro"
import SimCANMotor from "../wpilib_brain/sim/SimCANMotor"
import SimPWM from "../wpilib_brain/sim/SimPWM"
import { FunctionNode } from "./nodes/FunctionNode"

export const NODE_ID_SIM_IN = "sim-input-node"
export const NODE_ID_SIM_OUT = "sim-output-node"
export const NODE_ID_ROBOT_IO = "robot-io-node"

export type CompileCtx = {
    config: SimConfigData
    simLayer: SimulationLayer
    encountered: Set<HandleIdAlias>
}

export type NodeKind = {
    id: NodeKindId
    component: ComponentType<NodeProps>
    makeReceiver?: (handle: HandleInfo, ctx: CompileCtx) => SimReceiver<NoraType> | undefined
    makeSupplier?: (handle: HandleInfo, ctx: CompileCtx) => SimSupplier<NoraType> | undefined
}

const robotIONode: NodeKind = {
    id: NodeKindId.ROBOT_IO,
    component: RobotIONode,
    makeReceiver: (handle, _ctx) => {
        switch (handle.originType) {
            case SimType.CAN_ENCODER:
                return SimCANEncoder.genReceiver(handle.originId)
            case SimType.ACCELEROMETER:
                return SimAccel.genReceiver(handle.originId)
            case SimType.GYRO:
                return SimGyro.genReceiver(handle.originId)
        }
        return undefined
    },
    makeSupplier: (handle, _ctx) => {
        switch (handle.originType) {
            case SimType.CAN_MOTOR:
                return SimCANMotor.genSupplier(handle.originId)
            case SimType.PWM:
                return SimPWM.genSupplier(handle.originId)
        }
        return undefined
    },
}

const simInputNode: NodeKind = {
    id: NodeKindId.SIM_INPUT,
    component: FunctionNode,
    makeReceiver: (handle, ctx) => ctx.simLayer.getDriver(handle.originId),
}

const simOutputNode: NodeKind = {
    id: NodeKindId.SIM_OUTPUT,
    component: FunctionNode,
    makeSupplier: (handle, ctx) => ctx.simLayer.getStimuli(handle.originId),
}

const junctionNode: NodeKind = {
    id: NodeKindId.JUNCTION,
    component: FunctionNode,
    makeSupplier: (handle, ctx) => {
        const [input] = nodeTargets(ctx.config, handle.nodeId)
        return input && compileSuppliersFor(ctx, input.id)
    },
}

const constructorNode: NodeKind = {
    id: NodeKindId.CONSTRUCTOR,
    component: FunctionNode,
    makeSupplier: (handle, ctx) => {
        const inputs = nodeTargets(ctx.config, handle.nodeId).map(t => compileSuppliersFor(ctx, t.id))
        if (!handle.noraType || inputs.some(x => !x)) return undefined
        return {
            supplierType: handle.noraType,
            getSupplierValue: () => inputs.flatMap(x => x!.getSupplierValue()),
        }
    },
}

const deconstructorNode: NodeKind = {
    id: NodeKindId.DECONSTRUCTOR,
    component: FunctionNode,
    makeSupplier: (handle, ctx) => {
        const [input] = nodeTargets(ctx.config, handle.nodeId)
        const supplier = input && compileSuppliersFor(ctx, input.id)
        if (!supplier || handle.index === undefined) return undefined

        return {
            supplierType: [supplier.supplierType[handle.index]],
            getSupplierValue: () => [supplier.getSupplierValue()[handle.index!]],
        }
    },
}

export const NODE_KINDS: Record<NodeKindId, NodeKind> = {
    robotIO: robotIONode,
    simInput: simInputNode,
    simOutput: simOutputNode,
    junction: junctionNode,
    constructor: constructorNode,
    deconstructor: deconstructorNode,
}

export const nodeTypes = Object.fromEntries(Object.values(NODE_KINDS).map(k => [k.id, k.component]))

export const WiringNodeShell: React.FC<PropsWithChildren<{ title: string }>> = ({ title, children }) => (
    <div className="bg-background border-interactive-element-solid border-[0.0625rem] rounded-lg relative flex flex-col gap-4 py-4">
        <div
            style={{ transform: "translateY(-100%) translateX(-50%)" }}
            className="absolute top-0 text-nowrap left-1/2 text-2xl"
        >
            {title}
        </div>
        {children}
    </div>
)

export const HandleRow: React.FC<{ handle: HandleInfo }> = ({ handle }) => {
    const side = handle.isSource ? Position.Right : Position.Left

    return (
        <div className="relative">
            <div className={handle.isSource ? "px-3 text-lg text-right" : "px-3 text-lg"}>{handle.displayName}</div>
            <Handle
                style={{
                    position: "absolute",
                    [handle.isSource ? "right" : "left"]: 0,
                    width: "1rem",
                    height: "1rem",
                    backgroundColor: handle.noraType ? noraTypeToColorStr(handle.noraType) : undefined,
                }}
                type={handle.isSource ? "source" : "target"}
                position={side}
                id={handle.id}
            />
        </div>
    )
}
