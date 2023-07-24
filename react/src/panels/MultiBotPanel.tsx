import React from "react"
import Label, { LabelSize } from "../components/Label"
import Panel from "../components/Panel"
import { IoPeople } from "react-icons/io5"

const MultiBotPanel: React.FC = () => (
    <Panel name={"MultiBot"} icon={<IoPeople />}>
        <Label size={LabelSize.Medium}>MultiBot</Label>
    </Panel>
)

export default MultiBotPanel
