import Panel from './components/Panel';
import Button from './components/Button'
import Label, { LabelSize } from './components/Label'

function App() {
    return (
        <>
            <Panel name="Configuration" icon="https://placehold.co/128x128">
                <Label size={LabelSize.Medium}>Manipulate Gamepiece</Label>
                <Button value="Test" onClick={() => alert("test")} />
            </Panel>
        </>
    )
}

export default App
