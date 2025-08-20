import type React from "react"
import { useEffect } from "react"
import MatchMode from "@/systems/match_mode/MatchMode"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import type { ModalImplProps } from "../components/Modal"
import { Button } from "../components/StyledComponents"
import { CloseType, useUIContext } from "../helpers/UIProviderHelpers"

type Entry = {
    name: string
    value: number
}

const getMatchWinner = (): { message: string; color: string } => {
    if (SimulationSystem.redScore > SimulationSystem.blueScore) {
        return { message: "Red Team Wins!", color: "#ff0000" }
    } else if (SimulationSystem.blueScore > SimulationSystem.redScore) {
        return { message: "Blue Team Wins!", color: "#1818ff" }
    } else {
        return { message: "It's a Tie!", color: "#ffffff" }
    }
}

const getPerRobotScores = (): {
    redRobotScores: Entry[]
    blueRobotScores: Entry[]
} => {
    const redRobotScores: Entry[] = []
    const blueRobotScores: Entry[] = []
    SimulationSystem.perRobotScore.forEach((score, robot) => {
        if (robot.alliance === "red") {
            redRobotScores.push({
                name: `${robot.nameTag?.text()} (${robot.assemblyName})`,
                value: score,
            })
        } else {
            blueRobotScores.push({
                name: `${robot.nameTag?.text()} (${robot.assemblyName})`,
                value: score,
            })
        }
    })
    return { redRobotScores, blueRobotScores }
}

const MatchResultsModal: React.FC<ModalImplProps<void, void>> = ({ modal }) => {
    const { configureScreen, closeModal } = useUIContext()

    const { message } = getMatchWinner()

    const { redRobotScores, blueRobotScores } = getPerRobotScores()

    useEffect(() => {
        configureScreen(modal!, { hideCancel: true, hideAccept: true, allowClickAway: false }, {})
    }, [])

    // TODO
    // const breakdown: Array<{ label: string; red: number; blue: number }> = [
    //     { label: "LEAVE", red: 0, blue: 0 },
    //     { label: "CORAL", red: 0, blue: 0 },
    //     { label: "ALGAE", red: 0, blue: 0 },
    //     { label: "BARGE", red: 0, blue: 0 },
    //     { label: "PENALTY", red: 0, blue: 0 },
    // ]

    return (
        <div className="w-[90vw] h-[80vh] text-white rounded-lg overflow-hidden select-none">
            <div className="w-full bg-[#0d1b2a] px-4 py-2 text-center text-sm tracking-widest uppercase">
                Match Results
            </div>
            <div className="w-full h-[calc(80vh-2.25rem)] flex flex-row bg-[#0b2233]">
                <div className="w-[20%] h-full bg-[#7a1010] flex flex-col p-3 gap-2">
                    <div className="text-xs font-bold opacity-80">Red Alliance</div>
                    <div className="flex-1 flex flex-col gap-2 overflow-auto">
                        {redRobotScores.length === 0 && <div className="text-sm opacity-70">No robots</div>}
                        {redRobotScores.map(e => (
                            <div
                                key={e.name}
                                className="flex flex-row justify-between items-center bg-[#a41616] rounded px-2 py-1"
                            >
                                <span className="text-sm">{e.name}</span>
                                <span className="text-sm font-bold">{e.value}</span>
                            </div>
                        ))}
                    </div>
                </div>

                <div className="w-[60%] h-full flex flex-col">
                    <div className="w-full flex justify-center mt-3">
                        <div className="bg-[#ffd000] text-black font-extrabold px-4 py-1 rounded-sm text-lg">
                            {message.toUpperCase()}
                        </div>
                    </div>

                    <div className="flex flex-row items-stretch justify-between gap-3 px-4 py-3">
                        <div className="flex-1 bg-[#be1e2d] rounded-md flex items-center justify-center">
                            <span className="text-[6rem] leading-none font-extrabold drop-shadow-lg">
                                {SimulationSystem.redScore}
                            </span>
                        </div>
                        <div className="w-[38%] bg-[#e6e6e6] text-black rounded-md overflow-hidden self-stretch">
                            {/* Breakdown coming soon */}
                            <div className="flex flex-col items-center justify-center h-full py-8">
                                <span className="text-base font-semibold text-gray-600">Breakdown coming soon</span>
                            </div>
                        </div>
                        <div className="flex-1 bg-[#1f4fbf] rounded-md flex items-center justify-center">
                            <span className="text-[6rem] leading-none font-extrabold drop-shadow-lg">
                                {SimulationSystem.blueScore}
                            </span>
                        </div>
                    </div>

                    <div className="mt-auto px-4 pb-4 flex items-center justify-end">
                        <Button
                            onClick={() => {
                                closeModal(CloseType.Accept)
                                MatchMode.getInstance().sandboxModeStart()
                            }}
                        >
                            Back to Sandbox Mode
                        </Button>
                    </div>
                </div>

                <div className="w-[20%] h-full bg-[#10346b] flex flex-col p-3 gap-2">
                    <div className="text-xs font-bold opacity-80">Blue Alliance</div>
                    <div className="flex-1 flex flex-col gap-2 overflow-auto">
                        {blueRobotScores.length === 0 && <div className="text-sm opacity-70">No robots</div>}
                        {blueRobotScores.map(e => (
                            <div
                                key={e.name}
                                className="flex flex-row justify-between items-center bg-[#1f4fbf] rounded px-2 py-1"
                            >
                                <span className="text-sm">{e.name}</span>
                                <span className="text-sm font-bold">{e.value}</span>
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    )
}

export default MatchResultsModal
