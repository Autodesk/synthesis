export async function getBinaryFile(src) {
    const get_file_array = file => {
        return new Promise((acc, _) => {
            const reader = new FileReader()
            reader.onload = event => {
                acc(event.target.result)
            }
            reader.onerror = err => {
                err(err)
            }
            reader.readAsArrayBuffer(file)
        })
    }
    const miraFile = await fetch(src, { cache: "no-store" }).then(x => x.blob())
    const temp = await get_file_array(miraFile)
    return new Uint8Array(temp)
}
