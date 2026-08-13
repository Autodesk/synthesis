import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { SimulationLayer } from "@/systems/simulation/SimulationSystem"
import {
    AggregateStrategy,
    aggregateValues,
    type SimFlow,
    type SimReceiver,
    type SimSupplier,
} from "@/systems/simulation/wpilib_brain/SimDataFlow"
import World from "@/systems/World"
import { areTypesCompatible, serializeNoraType, type NoraType } from "../Nora"
import SimAccel from "../wpilib_brain/sim/SimAccel"
import SimCANEncoder from "../wpilib_brain/sim/SimCANEncoder"
import SimCANMotor from "../wpilib_brain/sim/SimCANMotor"
import SimGyro from "../wpilib_brain/sim/SimGyro"
import SimPWM from "../wpilib_brain/sim/SimPWM"
import { SimType } from "../wpilib_brain/WPILibTypes"
import { NODE_ID_ROBOT_IO } from "./nodes/RobotIONode"
import { NODE_ID_SIM_IN } from "./nodes/SimInputNode"
import { NODE_ID_SIM_OUT } from "./nodes/SimOutputNode"
import { FuncType, type HandleIdAlias, type NodeInfo, type SimConfigData } from "./SimGraph"

export function compile(config: SimConfigData, assembly: MirabufSceneObject): SimFlow[] | undefined {
    const simLayer = World.simulationSystem.getSimulationLayer(assembly.mechanism)
    if (!simLayer) {
        console.error("No sim layer found")
        return undefined
    }
    try {
        const flows: SimFlow[] = []
        Object.entries(config.handles).forEach(([id, info]) => {
            if (
                !info.isSource &&
                (info.nodeId === NODE_ID_ROBOT_IO || info.nodeId === NODE_ID_SIM_IN) &&
                (Object.entries(config.adjacency[info.id])?.length ?? 0) > 0
            ) {
                const flow = compileTargetHandle(config, simLayer, id, new Set<HandleIdAlias>())
                if (flow) flows.push(flow)
                else throw new Error("Failed to compile flows")
            }
        })
        return flows
    } catch (error) {
        console.error("Error thrown during compilation", error)
        return undefined
    }
}

export function compileTargetHandle(
    config: SimConfigData,
    simLayer: SimulationLayer,
    targetHandleId: HandleIdAlias,
    encountered: Set<HandleIdAlias>
): SimFlow | undefined {
    console.debug("Compiling target handle with ID", targetHandleId)
    const edges = Object.keys(config.adjacency[targetHandleId])
    if (!edges || edges.length < 1) {
        console.warn("No edges found for target handle")
        return undefined
    }

    // Generate receiver
    const targetHandle = config.handles[targetHandleId]
    if (!targetHandle) {
        console.warn("No target handle found")
        return undefined
    }

    const targetNoraType = targetHandle.noraType
    let receiver: SimReceiver<NoraType> | undefined = undefined
    if (targetHandle.nodeId === NODE_ID_ROBOT_IO) {
        switch (targetHandle.originType) {
            case SimType.CAN_ENCODER: {
                receiver = SimCANEncoder.genReceiver(targetHandle.originId)
                break
            }
            case SimType.ACCELEROMETER: {
                receiver = SimAccel.genReceiver(targetHandle.originId)
                break
            }
            case SimType.GYRO: {
                receiver = SimGyro.genReceiver(targetHandle.originId)
                break
            }
        }
    } else if (targetHandle.nodeId === NODE_ID_SIM_IN) {
        receiver = simLayer.getDriver(targetHandle.originId)
    } else {
        receiver = {
            receiverType: targetHandle.noraType,
            setReceiverValue: _ => {
                console.debug("If you're seeing this, that means bad")
            },
        }
    }
    if (!receiver) {
        console.warn("No valid receiver type")
        return undefined
    }

    if (!targetHandle.many && edges.length > 1) {
        console.warn("Edge count doesn't match allowed")
        return undefined
    }

    const suppliers: SimSupplier[] = []
    edges.forEach(edgeId => {
        console.debug(`Creating edge with ID ${edgeId}`)
        const edge = config.edges[edgeId]
        if (!edge) {
            console.warn("Could not find edge with ID", edgeId)
            return
        }
        console.debug(`Edge is between ${edge.sourceId} and ${edge.targetId}`)
        const sourceHandle = config.handles[edge.sourceId]
        if (!sourceHandle || !areTypesCompatible(sourceHandle.noraType!, targetNoraType!)) {
            console.warn(
                !sourceHandle
                    ? "No source handle"
                    : `Source handle type ${serializeNoraType(sourceHandle.noraType!)} doesn't match target type ${serializeNoraType(targetNoraType!)}`
            )
            return
        }
        if (encountered.has(sourceHandle.id)) {
            console.warn("Already encountered source handle with ID", sourceHandle.id)
            return
        }
        encountered.add(sourceHandle.id)
        switch (sourceHandle.nodeId) {
            case NODE_ID_ROBOT_IO: {
                // Get supplier from robot output
                switch (sourceHandle.originType) {
                    case SimType.CAN_MOTOR: {
                        suppliers.push(SimCANMotor.genSupplier(sourceHandle.originId))
                        break
                    }
                    case SimType.PWM: {
                        suppliers.push(SimPWM.genSupplier(sourceHandle.originId))
                        break
                    }
                }
                break
            }
            case NODE_ID_SIM_OUT: {
                // Get supplier from simulation output
                const stim: SimSupplier | undefined = simLayer.getStimuli(sourceHandle.originId)
                if (stim) suppliers.push(stim)
                break
            }
            default: {
                // Figure out function type
                const node = config.nodes[sourceHandle.nodeId]
                if (!node?.funcType) break
                const index = node.sources.indexOf(sourceHandle.id)
                if (index === -1) break
                const funcSuppliers = compileFunctionNode(config, simLayer, node, encountered)
                if (!funcSuppliers || funcSuppliers?.length !== node.sources.length) break
                suppliers.push(funcSuppliers[index])
            }
        }
        encountered.delete(sourceHandle.id)
    })

    if (suppliers.length === 0) {
        console.warn("No suppliers created")
        return undefined
    }

    if (suppliers.length === 1) {
        return {
            supplier: suppliers[0],
            receiver: receiver,
        }
    }
    return {
        supplier: {
            supplierType: targetNoraType,
            // TODO: fix to not always be average
            getSupplierValue: () => {
                const supp = suppliers.map(s => s.getSupplierValue())
                const val = aggregateValues(AggregateStrategy.AVERAGE, targetNoraType, supp)

                return val
            },
        },
        receiver: receiver,
    }
}

function compileConstructorNode(
    config: SimConfigData,
    simLayer: SimulationLayer,
    node: NodeInfo,
    encountered: Set<HandleIdAlias>
): SimSupplier<NoraType>[] | undefined {
    if (node.sources.length !== 1 || node.targets.length < 1) {
        return undefined
    }
    const outputType = config.handles[node.sources[0]].noraType
    const inputs = node.targets.map(x => {
        const flow = compileTargetHandle(config, simLayer, x, encountered)
        if (!flow) {
            console.error(`Failed to compile flow. TargetHandleId: ${x}`)
            throw new Error("Failed to compile SimConfig")
        }
        return flow.supplier
    })
    return [
        {
            supplierType: outputType,
            getSupplierValue: () => inputs.flatMap(x => x.getSupplierValue()),
        },
    ]
}

export function compileDeconstructorNode(
    config: SimConfigData,
    simLayer: SimulationLayer,
    node: NodeInfo,
    encountered: Set<HandleIdAlias>
): SimSupplier<NoraType>[] | undefined {
    if (node.sources.length < 1 || node.targets.length !== 1) {
        return undefined
    }
    const inputType = config.handles[node.targets[0]].noraType
    const input = compileTargetHandle(config, simLayer, node.targets[0], encountered)
    if (!input) {
        console.error(`Failed to compile flow. TargetHandleId: ${node.targets[0]}`)
        throw new Error("Failed to compile SimConfig")
    }
    const suppliers: SimSupplier<NoraType>[] = []
    for (let i = 0; i < inputType.length; ++i) {
        suppliers.push({
            supplierType: [inputType[i]],
            getSupplierValue: () => [input.supplier.getSupplierValue()[i]],
        })
    }
    return suppliers
}

export function compileJunctionNode(
    config: SimConfigData,
    simLayer: SimulationLayer,
    node: NodeInfo,
    encountered: Set<HandleIdAlias>
): SimSupplier<NoraType>[] | undefined {
    if (node.sources.length !== 1 || node.targets.length !== 1) {
        return undefined
    }
    const input = compileTargetHandle(config, simLayer, node.targets[0], encountered)
    if (!input) {
        console.error(`Failed to compile flow. TargetHandleId: ${node.targets[0]}`)
        throw new Error("Failed to compile SimConfig")
    }
    return [input.supplier]
}

export function compileFunctionNode(
    config: SimConfigData,
    simLayer: SimulationLayer,
    node: NodeInfo,
    encountered: Set<HandleIdAlias>
): SimSupplier<NoraType>[] | undefined {
    switch (node.funcType) {
        case FuncType.CONSTRUCTOR:
            return compileConstructorNode(config, simLayer, node, encountered)
        case FuncType.DECONSTRUCTOR:
            return compileDeconstructorNode(config, simLayer, node, encountered)
        case FuncType.JUNCTION:
            return compileJunctionNode(config, simLayer, node, encountered)
    }
    return undefined
}
