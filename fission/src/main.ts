import path from "node:path"
import { app, BrowserWindow } from "electron"
import started from "electron-squirrel-startup"

// Handle creating/removing shortcuts on Windows when installing/uninstalling.
if (started) {
    app.quit()
}

const createWindow = () => {
    // Create the browser window.
    const mainWindow = new BrowserWindow({
        // In production, the app is fullscreen by default, and in development, it's windowed by default.
        width: MAIN_WINDOW_VITE_DEV_SERVER_URL ? 1200 : undefined,
        height: MAIN_WINDOW_VITE_DEV_SERVER_URL ? 800 : undefined,
        fullscreen: !MAIN_WINDOW_VITE_DEV_SERVER_URL,
        webPreferences: {
            preload: path.join(__dirname, "preload.js"),
            nodeIntegration: false,
            contextIsolation: true,
        },
        icon: path.resolve(__dirname, "assets/icons/synthesis-logo.png"),
    })

    // and load the index.html of the app.
    if (MAIN_WINDOW_VITE_DEV_SERVER_URL) {
        mainWindow.loadURL(MAIN_WINDOW_VITE_DEV_SERVER_URL)
    } else {
        mainWindow.loadFile(path.join(__dirname, `../renderer/${MAIN_WINDOW_VITE_NAME}/index.html`))
    }

    // Open the DevTools in development
    if (MAIN_WINDOW_VITE_DEV_SERVER_URL) {
        mainWindow.webContents.openDevTools()
    }
}

// This method will be called when Electron has finished
app.whenReady().then(() => {
    createWindow()

    app.on("activate", () => {
        if (BrowserWindow.getAllWindows().length === 0) {
            createWindow()
        }
    })
})

/**
 * Quit when all windows are closed, except on macOS. There, it's common
 * for applications and their menu bar to stay active until the user quits
 * explicitly with Cmd + Q.
 */
app.on("window-all-closed", () => {
    if (process.platform !== "darwin") {
        app.quit()
    }
})

/**
 * In this file you can include the rest of your app's specific main process
 * code. You can also put them in separate files and import them here.
 */
