let lastCall = Date.now()

const delay = 1000

class APS {
    static requestAuth() {
        if (Date.now() - lastCall > delay) {
            lastCall = Date.now()
            window.open(`https://developer.api.autodesk.com/authentication/v2/authorize?response_type=code&client_id=hy2qmHs0LJbY6sKdx5bTrNUITGbWM5ys7Ixlrs9xbFxvAeTC&redirect_uri=${encodeURI('http://localhost:3000/h1dd3n/dist/')}&scope=data:read`)
        }
    }
}

export default APS