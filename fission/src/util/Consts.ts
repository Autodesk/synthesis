const isElectron = window.electronAPI != null
const baseUrl = isElectron ? "https://synthesis.autodesk.com" : ""
export const API_URL = `${baseUrl}/api`
