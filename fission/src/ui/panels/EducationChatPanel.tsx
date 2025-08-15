import type React from "react"
import { useState, useRef, useEffect } from "react"
import { Stack, TextField, IconButton, Chip, Box, Divider } from "@mui/material"
import { Button } from "../components/StyledComponents"
import type { PanelImplProps } from "../components/Panel"
import { useUIContext } from "../helpers/UIProviderHelpers"
import Label from "../components/Label"
import ScrollView from "../components/ScrollView"
import { FaPaperPlane, FaRobot, FaUser, FaBook, FaCode, FaWrench, FaCircleQuestion } from "react-icons/fa6"

interface Message {
    id: string
    text: string
    sender: "user" | "bot"
    timestamp: Date
    category?: string
}

interface SuggestedQuestion {
    text: string
    category: string
}

const SUGGESTED_QUESTIONS: SuggestedQuestion[] = [
    { text: "How do I spawn a robot?", category: "basics" },
    { text: "What is WPILib?", category: "wpilib" },
    { text: "How do I configure robot controls?", category: "controls" },
    { text: "Explain the wiring panel", category: "simulation" },
    { text: "How to import a Mirabuf file?", category: "files" },
    { text: "What are the keyboard shortcuts?", category: "basics" },
]

const CATEGORIES = [
    { label: "All Topics", value: "all", icon: <FaBook /> },
    { label: "FRC Basics", value: "frc", icon: <FaRobot /> },
    { label: "WPILib", value: "wpilib", icon: <FaCode /> },
    { label: "Synthesis", value: "synthesis", icon: <FaWrench /> },
]

const EducationChatPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen, addToast } = useUIContext()
    const [messages, setMessages] = useState<Message[]>([
        {
            id: "welcome",
            text: "Hello! I'm your Synthesis education assistant. I can help you learn about FRC robotics, WPILib, and how to use Synthesis. What would you like to know?",
            sender: "bot",
            timestamp: new Date(),
        },
    ])
    const [inputText, setInputText] = useState("")
    const [selectedCategory, setSelectedCategory] = useState("all")
    const [isTyping, setIsTyping] = useState(false)
    const messagesEndRef = useRef<HTMLDivElement>(null)

    useEffect(() => {
        configureScreen(
            panel!,
            {
                title: "Education Assistant",
                hideAccept: true,
                cancelText: "Close",
            },
            {}
        )
    }, [])

    useEffect(() => {
        scrollToBottom()
    }, [messages])

    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: "smooth" })
    }

    const handleSend = () => {
        if (!inputText.trim()) return

        const userMessage: Message = {
            id: `msg-${Date.now()}`,
            text: inputText,
            sender: "user",
            timestamp: new Date(),
        }

        setMessages(prev => [...prev, userMessage])
        setInputText("")
        setIsTyping(true)

        // Simulate bot response (replace with actual AI integration later)
        setTimeout(() => {
            const botResponse: Message = {
                id: `msg-${Date.now()}-bot`,
                text: `I understand you're asking about "${inputText}". This feature will connect to an AI model trained on the Synthesis codebase and FRC documentation. For now, try exploring the UI components and panels available in Synthesis!`,
                sender: "bot",
                timestamp: new Date(),
                category: detectCategory(inputText),
            }
            setMessages(prev => [...prev, botResponse])
            setIsTyping(false)
        }, 1000)
    }

    const detectCategory = (text: string): string => {
        const lowerText = text.toLowerCase()
        if (lowerText.includes("wpilib") || lowerText.includes("wpi")) return "wpilib"
        if (lowerText.includes("frc") || lowerText.includes("first")) return "frc"
        if (lowerText.includes("synthesis") || lowerText.includes("simulation")) return "synthesis"
        return "general"
    }

    const handleSuggestedQuestion = (question: string) => {
        setInputText(question)
        handleSend()
    }

    const handleKeyPress = (e: React.KeyboardEvent) => {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault()
            handleSend()
        }
    }

    return (
        <Stack
            direction="column"
            spacing={2}
            sx={{
                height: "60vh",
                width: "500px",
                display: "flex",
                flexDirection: "column",
            }}
        >
            {/* Category Selector */}
            <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", gap: 1 }}>
                {CATEGORIES.map(cat => (
                    <Chip
                        key={cat.value}
                        label={cat.label}
                        icon={cat.icon}
                        onClick={() => setSelectedCategory(cat.value)}
                        color={selectedCategory === cat.value ? "primary" : "default"}
                        variant={selectedCategory === cat.value ? "filled" : "outlined"}
                        sx={{ cursor: "pointer" }}
                    />
                ))}
            </Stack>

            <Divider />

            {/* Messages Area */}
            <Box
                sx={{
                    flex: 1,
                    overflowY: "auto",
                    display: "flex",
                    flexDirection: "column",
                    gap: 2,
                    pr: 1,
                    "&::-webkit-scrollbar": {
                        width: "8px",
                    },
                    "&::-webkit-scrollbar-track": {
                        background: "transparent",
                    },
                    "&::-webkit-scrollbar-thumb": {
                        background: "rgba(255, 255, 255, 0.2)",
                        borderRadius: "4px",
                    },
                    "&::-webkit-scrollbar-thumb:hover": {
                        background: "rgba(255, 255, 255, 0.3)",
                    },
                }}
            >
                {messages.map(message => (
                    <Stack
                        key={message.id}
                        direction="row"
                        spacing={1}
                        sx={{
                            alignSelf: message.sender === "user" ? "flex-end" : "flex-start",
                            maxWidth: "80%",
                        }}
                    >
                        {message.sender === "bot" && (
                            <Box
                                sx={{
                                    width: 32,
                                    height: 32,
                                    borderRadius: "50%",
                                    display: "flex",
                                    alignItems: "center",
                                    justifyContent: "center",
                                    bgcolor: "primary.main",
                                    color: "white",
                                    flexShrink: 0,
                                }}
                            >
                                <FaRobot size={16} />
                            </Box>
                        )}
                        <Box
                            sx={{
                                p: 1.5,
                                borderRadius: 2,
                                bgcolor: message.sender === "user" ? "primary.main" : "background.paper",
                                color: message.sender === "user" ? "white" : "text.primary",
                                boxShadow: 1,
                            }}
                        >
                            <Box sx={{ fontSize: "0.9rem", lineHeight: 1.5 }}>{message.text}</Box>
                            <Box
                                sx={{
                                    fontSize: "0.7rem",
                                    opacity: 0.7,
                                    mt: 0.5,
                                }}
                            >
                                {message.timestamp.toLocaleTimeString([], {
                                    hour: "2-digit",
                                    minute: "2-digit",
                                })}
                            </Box>
                        </Box>
                        {message.sender === "user" && (
                            <Box
                                sx={{
                                    width: 32,
                                    height: 32,
                                    borderRadius: "50%",
                                    display: "flex",
                                    alignItems: "center",
                                    justifyContent: "center",
                                    bgcolor: "secondary.main",
                                    color: "white",
                                    flexShrink: 0,
                                }}
                            >
                                <FaUser size={16} />
                            </Box>
                        )}
                    </Stack>
                ))}
                {isTyping && (
                    <Stack direction="row" spacing={1} sx={{ alignSelf: "flex-start" }}>
                        <Box
                            sx={{
                                width: 32,
                                height: 32,
                                borderRadius: "50%",
                                display: "flex",
                                alignItems: "center",
                                justifyContent: "center",
                                bgcolor: "primary.main",
                                color: "white",
                            }}
                        >
                            <FaRobot size={16} />
                        </Box>
                        <Box
                            sx={{
                                p: 1.5,
                                borderRadius: 2,
                                bgcolor: "background.paper",
                                display: "flex",
                                gap: 0.5,
                            }}
                        >
                            <Box
                                className="typing-dot"
                                sx={{
                                    width: 8,
                                    height: 8,
                                    borderRadius: "50%",
                                    bgcolor: "text.secondary",
                                    animation: "typing 1.4s infinite",
                                    animationDelay: "0s",
                                }}
                            />
                            <Box
                                className="typing-dot"
                                sx={{
                                    width: 8,
                                    height: 8,
                                    borderRadius: "50%",
                                    bgcolor: "text.secondary",
                                    animation: "typing 1.4s infinite",
                                    animationDelay: "0.2s",
                                }}
                            />
                            <Box
                                className="typing-dot"
                                sx={{
                                    width: 8,
                                    height: 8,
                                    borderRadius: "50%",
                                    bgcolor: "text.secondary",
                                    animation: "typing 1.4s infinite",
                                    animationDelay: "0.4s",
                                }}
                            />
                        </Box>
                    </Stack>
                )}
                <div ref={messagesEndRef} />
            </Box>

            <Divider />

            {/* Suggested Questions */}
            {messages.length === 1 && (
                <Box>
                    <Label size="sm" sx={{ mb: 1 }}>
                        Suggested Questions:
                    </Label>
                    <Stack
                        direction="row"
                        spacing={1}
                        sx={{
                            flexWrap: "wrap",
                            gap: 0.5,
                        }}
                    >
                        {SUGGESTED_QUESTIONS.map((q, idx) => (
                            <Chip
                                key={idx}
                                label={q.text}
                                size="small"
                                variant="outlined"
                                onClick={() => handleSuggestedQuestion(q.text)}
                                sx={{
                                    cursor: "pointer",
                                    "&:hover": {
                                        bgcolor: "action.hover",
                                    },
                                }}
                            />
                        ))}
                    </Stack>
                </Box>
            )}

            {/* Input Area */}
            <Stack direction="row" spacing={1}>
                <TextField
                    fullWidth
                    multiline
                    maxRows={3}
                    value={inputText}
                    onChange={e => setInputText(e.target.value)}
                    onKeyPress={handleKeyPress}
                    placeholder="Ask about FRC, WPILib, or Synthesis..."
                    variant="outlined"
                    size="small"
                    disabled={isTyping}
                    sx={{
                        "& .MuiOutlinedInput-root": {
                            bgcolor: "background.paper",
                        },
                    }}
                />
                <IconButton
                    onClick={handleSend}
                    disabled={!inputText.trim() || isTyping}
                    color="primary"
                    sx={{
                        bgcolor: "primary.main",
                        color: "white",
                        "&:hover": {
                            bgcolor: "primary.dark",
                        },
                        "&:disabled": {
                            bgcolor: "action.disabledBackground",
                        },
                    }}
                >
                    <FaPaperPlane />
                </IconButton>
            </Stack>
        </Stack>
    )
}

// Add CSS animation for typing indicator
const style = document.createElement("style")
style.textContent = `
    @keyframes typing {
        0%, 60%, 100% {
            opacity: 0.3;
            transform: translateY(0);
        }
        30% {
            opacity: 1;
            transform: translateY(-10px);
        }
    }
`
document.head.appendChild(style)

export default EducationChatPanel
