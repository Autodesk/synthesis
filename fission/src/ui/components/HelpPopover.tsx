import { Button, List, ListItemButton, ListItemIcon, ListItemText, Popover } from "@mui/material"
import { IoHelpCircle } from "react-icons/io5"
import { useCallback, useState } from "react"
import { type HelpOption, HelpOptionType } from "@/util/HelpOption"
import { FaDiscord, FaYoutube } from "react-icons/fa6"
import { MdArticle } from "react-icons/md"

function helpOptionIcon(type: HelpOptionType) {
    switch (type) {
        case HelpOptionType.YouTube:
            return <FaYoutube />
        case HelpOptionType.Codelab:
            return <MdArticle />
        case HelpOptionType.Discord:
            return <FaDiscord />
        case HelpOptionType.Website:
            return <IoHelpCircle />
        default:
            return <IoHelpCircle />
    }
}

const HelpPopover: React.FC<{ id: string; helpOptions?: HelpOption[] }> = ({ id, helpOptions }) => {
    const [helpPopoverAnchor, setHelpPopoverAnchor] = useState<HTMLButtonElement | null>(null)

    const handleHelpButton = useCallback((event: React.MouseEvent<HTMLButtonElement>) => {
        setHelpPopoverAnchor(event.currentTarget)
    }, [])

    const handleCloseHelpPopover = useCallback(() => {
        setHelpPopoverAnchor(null)
    }, [])

    const helpPopoverOpen = Boolean(helpPopoverAnchor)
    const helpPopoverId = helpPopoverOpen ? id : undefined

    return (
        helpOptions && (
            <>
                <Button variant="outlined" startIcon={<IoHelpCircle />} onClick={handleHelpButton}>
                    Help
                </Button>
                <Popover
                    id={helpPopoverId}
                    open={helpPopoverOpen}
                    anchorEl={helpPopoverAnchor}
                    onClose={handleCloseHelpPopover}
                    anchorOrigin={{
                        vertical: "bottom",
                        horizontal: "center",
                    }}
                >
                    <List
                        sx={{ width: "100%", maxWidth: 360, bgcolor: "background.paper" }}
                        component="nav"
                        aria-labelledby="nested-list-subheader"
                    >
                        {helpOptions.map(x => (
                            <ListItemButton onClick={() => window.open(x.link, "_blank", "noopener, noreferrer")}>
                                <ListItemIcon>{helpOptionIcon(x.type)}</ListItemIcon>
                                <ListItemText primary={x.text} />
                            </ListItemButton>
                        ))}
                    </List>
                </Popover>
            </>
        )
    )
}

export { HelpPopover }
