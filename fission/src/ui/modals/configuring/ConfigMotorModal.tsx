import { FaGear } from "react-icons/fa6"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Slider from "@/components/Slider"
import Label, { LabelSize } from "@/components/Label"
import { useState } from "react"

type Motor = {
    name: string
    defaultVelocity: number
    minVelocity: number
    maxVelocity: number
    unit: "RPM" | "M/S"
}

// Synthesis should send all of this because we already have logic to generate these entries
// rather than calculating them here
const sampleMotors: Motor[] = [
    {
        name: "Drive",
        defaultVelocity: 100,
        minVelocity: 0,
        maxVelocity: 150,
        unit: "RPM",
    },
]

const ConfigMotorModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const [motors, setMotors] = useState<Motor[]>(sampleMotors)
    const [initialData] = useState<Motor[]>([...motors])

    return (
        <Modal
            name="Motor Configuration"
            icon={<FaGear />}
            modalId={modalId}
            middleName="Session Save"
            middleEnabled={true}
            acceptName="Save"
            onCancel={() => {
                setMotors(initialData)
                // send cancel
            }}
            onMiddle={() => {
                // send session save
            }}
            onAccept={() => {
                // send save
            }}
        >
            <table>
                <tr className="text-left">
                    <th>
                        <Label size={LabelSize.Medium}>Motor</Label>
                    </th>
                    <th>
                        <Label size={LabelSize.Medium}>Target Velocity</Label>
                    </th>
                </tr>
                {motors.map((m: Motor) => (
                    <tr>
                        <td className="w-32">
                            <Label size={LabelSize.Medium}>{m.name}</Label>
                        </td>
                        <td className="w-48">
                            <Slider value={m.defaultVelocity} min={m.minVelocity} max={m.maxVelocity} label={m.unit} />
                        </td>
                    </tr>
                ))}
            </table>
        </Modal>
    )
}

export default ConfigMotorModal
