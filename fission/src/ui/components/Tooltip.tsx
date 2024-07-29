import { TooltipControl, TooltipType } from "@/ui/TooltipContext"
import { FaCircleInfo } from "react-icons/fa6"
import Label, { LabelSize } from "./Label"
import Stack, { StackDirection } from "./Stack"

type TooltipProps = {
    type: TooltipType
    controls?: TooltipControl[]
}

const Tooltip: React.FC<TooltipProps> = ({ type, controls }) => {
    if (type === "controls") {
        return (
            <div className="absolute flex flex-col gap-1 px-8 pt-2 pb-4 rounded-lg left-1/2 -translate-x-1/2 top-2 bg-background">
                <FaCircleInfo className="text-main-text mx-auto pt-1 pb-2 w-8 h-8" />
                {controls?.map(c => (
                    <Stack
                        direction={StackDirection.Horizontal}
                        key={`${c.control}`}
                        spacing={8}
                        justify="around"
                        className="min-w-max"
                    >
                        <Label
                            size={LabelSize.Small}
                            className="bg-background-secondary align-middle px-2 rounded-md min-w-fit"
                        >
                            {c.control}
                        </Label>
                        <Label size={LabelSize.Small} className="min-w-fit align-middle">
                            {c.description}
                        </Label>
                    </Stack>
                ))}
            </div>
        )
    } else {
        return <></>
    }
}

export default Tooltip
