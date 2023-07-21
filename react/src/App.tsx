import Panel from './components/Panel';
import Button from './components/Button'
import Label, { LabelSize } from './components/Label'
import Slider from './components/Slider'
import Checkbox from './components/Checkbox';
import Dropdown from './components/Dropdown'

function App() {
    return (
        <div className="flex w-full h-full">
            <Panel name="Configuration" icon="https://placehold.co/128x128">
                <Label size={LabelSize.Medium}>Manipulate Gamepiece</Label>
                <Slider min={1} max={100} defaultValue={50} />
                <Button value="Test" />
                <Checkbox label={"Label"} defaultState={false} />
                <Dropdown label={"Dropdown"} options={["Option 1", "Option 2", "Option 3"]} />
            </Panel>
        </div>
    )
}

export default App
