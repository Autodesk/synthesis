import Button from "@/ui/components/Button"
import { useRef, useState } from "react"

function TouchControls() {
    const inputRef = useRef<HTMLInputElement>(null)

    const showPlace = useState(false)
    const showTouchControls = useState(false)

    return (
        <>
            <input ref={inputRef} />
            {showPlace ? (
                <Button
                    className="fixed bottom-4 right-4"
                    value="Touch Controls"
                    onClick={() => {
                        if (inputRef.current) {
                            const pressEvent = new KeyboardEvent("keydown", {
                                key: "Enter",
                                code: "Enter",
                                bubbles: true,
                            })
                            inputRef.current.dispatchEvent(pressEvent)

                            setTimeout(() => {
                                const releaseEvent = new KeyboardEvent("keyup", {
                                    key: "Enter",
                                    code: "Enter",
                                    bubbles: true,
                                })
                                inputRef.current?.dispatchEvent(releaseEvent)
                            }, 100)
                        }
                    }}
                />
            ) : (
                <></>
            )}
            {showTouchControls ? (
                <div className="fixed bottom-4 left-4">
                    <Button
                        value="↑"
                        onClick={() => {
                            if (inputRef.current) {
                                const pressEvent = new KeyboardEvent("keydown", {
                                    key: "ArrowUp",
                                    code: "ArrowUp",
                                    bubbles: true,
                                })
                                inputRef.current.dispatchEvent(pressEvent)

                                setTimeout(() => {
                                    const releaseEvent = new KeyboardEvent("keyup", {
                                        key: "ArrowUp",
                                        code: "ArrowUp",
                                        bubbles: true,
                                    })
                                    inputRef.current?.dispatchEvent(releaseEvent)
                                }, 100)
                            }
                        }}
                    />
                    <Button
                        value="←"
                        onClick={() => {
                            if (inputRef.current) {
                                const pressEvent = new KeyboardEvent("keydown", {
                                    key: "ArrowLeft",
                                    code: "ArrowLeft",
                                    bubbles: true,
                                })
                                inputRef.current.dispatchEvent(pressEvent)

                                setTimeout(() => {
                                    const releaseEvent = new KeyboardEvent("keyup", {
                                        key: "ArrowLeft",
                                        code: "ArrowLeft",
                                        bubbles: true,
                                    })
                                    inputRef.current?.dispatchEvent(releaseEvent)
                                }, 100)
                            }
                        }}
                    />
                    <Button
                        value="↓"
                        onClick={() => {
                            if (inputRef.current) {
                                const pressEvent = new KeyboardEvent("keydown", {
                                    key: "ArrowDown",
                                    code: "ArrowDown",
                                    bubbles: true,
                                })
                                inputRef.current.dispatchEvent(pressEvent)

                                setTimeout(() => {
                                    const releaseEvent = new KeyboardEvent("keyup", {
                                        key: "ArrowDown",
                                        code: "ArrowDown",
                                        bubbles: true,
                                    })
                                    inputRef.current?.dispatchEvent(releaseEvent)
                                }, 100)
                            }
                        }}
                    />
                    <Button
                        value="→"
                        onClick={() => {
                            if (inputRef.current) {
                                const pressEvent = new KeyboardEvent("keydown", {
                                    key: "ArrowRight",
                                    code: "ArrowRight",
                                    bubbles: true,
                                })
                                inputRef.current.dispatchEvent(pressEvent)

                                setTimeout(() => {
                                    const releaseEvent = new KeyboardEvent("keyup", {
                                        key: "ArrowRight",
                                        code: "ArrowRight",
                                        bubbles: true,
                                    })
                                    inputRef.current?.dispatchEvent(releaseEvent)
                                }, 100)
                            }
                        }}
                    />
                </div>
            ) : (
                <></>
            )}
        </>
    )
}

export default TouchControls
