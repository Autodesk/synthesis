import React from 'react';
import Label, { LabelSize } from '../components/Label';
import Panel from '../components/Panel';

const MultiBotPanel: React.FC = () => (
    <Panel name={"MultiBot"} icon="https://placeholder.co/512x512">
        <Label size={LabelSize.Medium}>MultiBot</Label>
    </Panel>
)

export default MultiBotPanel;
