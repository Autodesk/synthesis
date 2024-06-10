/*
WPILib Web Socket Simulation Spec
https://github.com/wpilibsuite/allwpilib/blob/main/simulation/halsim_ws_core/doc/hardware_ws_api.md
*/

import { Mutex } from 'async-mutex'

const instanceMutex = new Mutex()

class WPILibConnector {

    private static _connector: WPILibConnector | undefined

    private _socket: WebSocket

    private constructor() {
        this._socket = new WebSocket('ws://localhost:3300/wpilibws')

        this._socket.addEventListener('open', () => {
            this._socket.send('hello')
        })

        this._socket.addEventListener('message', (e) => {
            console.debug(`WS MESSAGE: ${e.data}`)
        })
    }

    private teardown() {

    }

    public static async getInstance(): Promise<WPILibConnector> {
        return instanceMutex.runExclusive(() => {
            if (!this._connector) {
                this._connector = new WPILibConnector()
            }
            return this._connector
        })
    }

    public static killInstance() {
        if (this._connector) {
            this._connector.teardown()
        }
    }
}

export default WPILibConnector