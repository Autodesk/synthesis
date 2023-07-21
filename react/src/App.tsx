import Panel from './components/Panel';
import Button from './components/Button'
import Label, { LabelSize } from './components/Label'
import Slider from './components/Slider'

function App() {
    return (
        <>
            <Panel name="Configuration" icon="https://placehold.co/128x128">
                <Label size={LabelSize.Medium}>Manipulate Gamepiece</Label>
                <Slider min={1} max={100} defaultValue={50} />
                <Button value="Test" />
            </Panel>
        </>
    )
}

export default App
