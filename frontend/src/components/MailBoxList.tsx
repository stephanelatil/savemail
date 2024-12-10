'use client'

import { Folder } from "@/models/folder";
import { MailBox } from "@/models/mailBox";
import { Archive as ArchiveIcon, CreateNewFolder, Delete as DeleteIcon, Email as EmailIcon, ExpandLess, ExpandMore, Folder as FolderIcon, Refresh, Send as SendIcon } from "@mui/icons-material";
import { Collapse, IconButton, List, ListItem, ListItemButton, ListItemIcon, ListItemText, Skeleton, Stack } from "@mui/material";
import { useParams, usePathname, useRouter } from "next/navigation";
import { useLayoutEffect, useMemo, useState } from "react";

const mapFolderIcon = (name:string) =>{
    switch (name.toLowerCase()) {
        case "sent":
            return <SendIcon/>
        case "trash":
            return <DeleteIcon/>
        case "archive":
            return <ArchiveIcon/> 
        default:
            return <FolderIcon/>;
    }
}

const LoadingMailListBox: React.FC = () => {
    return  (<>{Array(3).map((i) => 
                <ListItem key={`Skeleton_MB_${i}`}>
                    <Stack flexDirection='row' gap={1} justifyContent='space-evenly'>
                        <Skeleton width={30} variant='circular' />
                        <Skeleton width='calc(40 - 100%)' variant='rounded' />
                    </Stack>
                </ListItem>
            )}</>);
}

interface TreeNode {
    id: number;
    type: 'mailbox' | 'folder';
    name: string;
    children?: TreeNode[];
    parent?: TreeNode | null;
}

interface TreeItemProps {
    node: TreeNode;
    depth?: number;
    selectedNodeId?: number | null;
    expandedNodes: Set<number>;
    onToggleExpand: (nodeId: number) => void;
}

const TreeItem: React.FC<TreeItemProps> = ({ 
    node, 
    depth = 0,
    selectedNodeId, 
    expandedNodes, 
    onToggleExpand 
}) => {
    const router = useRouter();

    const [isSelected, setIsSelected] = useState(selectedNodeId === node.id);
    const isExpanded = expandedNodes.has(node.id);
    const hasChildren = node.children && node.children.length > 0;
    const handleSelect = (node: TreeNode) => {
        if (isSelected)
            return;
        setIsSelected(true);
        // Default navigation behavior
        if (node.type === 'mailbox') {
            router.push(`/mailbox/${node.id}`);
        } else {
            router.push(`/folder/${node.id}`);
        }
    };

    return (
        <>
            <ListItem sx={{ 
                paddingLeft: `${depth * 16}px`, 
                display: 'flex', 
                alignItems: 'center' 
            }}>
                <ListItemButton
                    selected={isSelected}
                    onClick={() => handleSelect(node)}
                    sx={{ display: 'flex', justifyContent: 'space-between' }}
                >
                    <div style={{ display: 'flex', alignItems: 'center' }}>
                        <ListItemIcon>
                            {node.type === 'mailbox' ? <EmailIcon /> : mapFolderIcon(node.name)}
                        </ListItemIcon>
                        <ListItemText primary={node.name} />
                    </div>
                    {hasChildren && (
                        <IconButton 
                            size="small" 
                            onClick={(e) => {
                                e.stopPropagation();
                                onToggleExpand(node.id);
                            }}
                        >
                            {isExpanded ? <ExpandLess /> : <ExpandMore />}
                        </IconButton>
                    )}
                </ListItemButton>
            </ListItem>
            
            {hasChildren && (
                <Collapse in={isExpanded} timeout="auto" unmountOnExit>
                    <List disablePadding>
                        {node.children?.map(childNode => (
                            <TreeItem
                                key={`${node.type}-${childNode.id}`}
                                node={childNode}
                                depth={depth + 1}
                                selectedNodeId={selectedNodeId}
                                expandedNodes={expandedNodes}
                                onToggleExpand={onToggleExpand}
                            />
                        ))}
                    </List>
                </Collapse>
            )}
        </>
    );
}

export const MailBoxList: React.FC<{loading?: boolean, mailboxes?: MailBox[]}> = ({ loading, mailboxes }) => {
    const router = useRouter();
    const pathname = usePathname();
    const params = useParams();

    const isFolderRoute = pathname.startsWith('/folder/');
    const currentId = parseInt(params.id as string);

    const [expandedNodes, setExpandedNodes] = useState<Set<number>>(new Set());

    // Transform mailbox data into a tree structure with parent references
    const treeData = useMemo(() => {
        const createTreeWithParents = (
            items: Folder[], 
            parent?: TreeNode | null
        ): TreeNode[] => 
            items.map(folder => {
                let f:TreeNode = {
                id: folder.id,
                type: 'folder',
                name: folder.name,
                parent: parent
            };
            if (!!folder.children)
                f.children = createTreeWithParents(folder.children, f);
            return f;

        });

        return mailboxes?.map(mb => {
            let mbNode:TreeNode = {
                id: mb.id,
                type: 'mailbox',
                name: mb.username
            }
            if (!!mb.folders?.length)
                mbNode.children = createTreeWithParents(mb.folders, mbNode);
            return mbNode;
        }) || [];
    }, [mailboxes]);

    // Auto-expand and select based on current route
    useLayoutEffect(() => {
        if (!treeData.length) return;
        if (isNaN(currentId)) return;

        // Determine current route type and ID
        const nodesToExpand:Set<number> = new Set<number>([currentId]);

        if (isFolderRoute) {
            // Set to expand: all
            const fillExpandSet = (nodes: TreeNode[])  => {
                for (const node of nodes) {
                    if (node.id === currentId) {
                        let parent = node.parent;
                        //set all parent path to be expanded
                        while (!!parent)
                        {
                            nodesToExpand.add(parent.id);
                            parent = parent.parent
                        }
                        return;
                    }
                    // look for current id node in children
                    if (!!node.children)
                        fillExpandSet(node.children);
                }
            };

            fillExpandSet(treeData);
            setExpandedNodes(nodesToExpand);
        }
    }, [currentId, isFolderRoute, treeData]);

    const handleToggleExpand = (nodeId: number) => {
        setExpandedNodes(prev => {
            const updated = new Set(prev);
            if (updated.has(nodeId)) {
                updated.delete(nodeId);
            } else {
                updated.add(nodeId);
            }
            return updated;
        });
    };

    const handleNewMailbox = () => {
        router.push('/mailbox/new');
    };

    return (
        <List sx={{ 
            height: '100%', 
            display: 'flex', 
            flexDirection: 'column', 
            flex: '1 1 auto', 
            overflowY: 'auto', 
            overflowX: 'hidden' 
        }}>
            <ListItem>
                <ListItemButton selected={pathname == '/mailbox/new'}
                                onClick={() => {pathname != '/mailbox/new' && router.push('/mailbox/new');}}>
                    <ListItemIcon>
                        <CreateNewFolder />
                    </ListItemIcon>
                    <ListItemText primary="Add New Mailbox" />
                </ListItemButton>
            </ListItem>

            {loading ? <LoadingMailListBox/>
                    : (treeData.map(mailbox => (
                            <TreeItem
                                key={`mailbox-${mailbox.id}`}
                                node={mailbox}
                                selectedNodeId={currentId}
                                expandedNodes={expandedNodes}
                                onToggleExpand={handleToggleExpand}
                            />)))
            }
        </List>
    );
}

export default MailBoxList;