import { Box, Stack } from "@mui/material"
import type React from "react"
import { useState } from "react"
import { MdKeyboardArrowDown, MdKeyboardArrowRight } from "react-icons/md"

export interface TreeNode {
    id: string
    label: React.ReactNode
    children?: TreeNode[]
}

export interface TreeProps {
    nodes: TreeNode[]
    selectedId?: string
    onSelect?: (id: string) => void
}

interface TreeRowProps {
    node: TreeNode
    depth: number
    selectedId?: string
    onSelect?: (id: string) => void
    collapsed: Set<string>
    onToggle: (id: string) => void
}

const ROW_INDENT_PX = 18

const TreeRow: React.FC<TreeRowProps> = ({ node, depth, selectedId, onSelect, collapsed, onToggle }) => {
    const hasChildren = (node.children?.length ?? 0) > 0
    const isExpanded = hasChildren && !collapsed.has(node.id)
    const isSelected = node.id === selectedId

    return (
        <>
            <Stack
                direction="row"
                alignItems="center"
                gap={0.5}
                onClick={() => onSelect?.(node.id)}
                sx={{
                    pl: `${depth * ROW_INDENT_PX + 4}px`,
                    py: 0.25,
                    cursor: "pointer",
                    borderRadius: 1,
                    userSelect: "none",
                    bgcolor: isSelected ? "action.selected" : "transparent",
                    "&:hover": { bgcolor: isSelected ? "action.selected" : "action.hover" },
                }}
            >
                <Box
                    onClick={e => {
                        e.stopPropagation()
                        if (hasChildren) onToggle(node.id)
                    }}
                    sx={{ display: "flex", alignItems: "center", width: 18, flexShrink: 0 }}
                >
                    {hasChildren && (isExpanded ? <MdKeyboardArrowDown /> : <MdKeyboardArrowRight />)}
                </Box>
                <Box sx={{ minWidth: 0, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                    {node.label}
                </Box>
            </Stack>
            {isExpanded &&
                node.children!.map(child => (
                    <TreeRow
                        key={child.id}
                        node={child}
                        depth={depth + 1}
                        selectedId={selectedId}
                        onSelect={onSelect}
                        collapsed={collapsed}
                        onToggle={onToggle}
                    />
                ))}
        </>
    )
}

/**
 * Hierarchical node list, à la a CAD browser/design tree: expandable/collapsible rows nested by
 * parent-child relationship. Deliberately generic (no mix-and-match-specific knowledge) so any
 * feature with a tree-shaped structure can render through it.
 */
const Tree: React.FC<TreeProps> = ({ nodes, selectedId, onSelect }) => {
    // Nodes are expanded by default; only explicit collapses are tracked, so newly appearing nodes
    // (e.g. a freshly spawned part) don't need to be added to some "expanded" set to show up open.
    const [collapsed, setCollapsed] = useState<Set<string>>(new Set())

    const onToggle = (id: string) =>
        setCollapsed(prev => {
            const next = new Set(prev)
            if (next.has(id)) next.delete(id)
            else next.add(id)
            return next
        })

    return (
        <Stack direction="column">
            {nodes.map(node => (
                <TreeRow
                    key={node.id}
                    node={node}
                    depth={0}
                    selectedId={selectedId}
                    onSelect={onSelect}
                    collapsed={collapsed}
                    onToggle={onToggle}
                />
            ))}
        </Stack>
    )
}

export default Tree
