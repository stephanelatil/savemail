// 'use client'

// import { useMailboxes } from "@/hooks/useMailboxes";
import { Folder } from "@/models/folder";
import { Archive as ArchiveIcon, CreateNewFolder, Delete as DeleteIcon, Email as EmailIcon, ExpandLess, ExpandMore, Folder as FolderIcon, Send as SendIcon } from "@mui/icons-material";
import { CircularProgress, Collapse, Divider, List, ListItem, ListItemButton, ListItemIcon, ListItemText } from "@mui/material";
import { useParams, usePathname } from "next/navigation";
import React from "react";
import { usePersistentMailboxesState } from "./context/SidebarMailBoxListContext";

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

interface PartialFolderInfo{
    id:number,
    name:string,
    folderPathId?:string,
    children:Folder[],
    indent?:number
}

const FolderListItem: React.FC<PartialFolderInfo> = ({id, name, children, folderPathId, indent}) => {
    const hasChildren:boolean = children.length > 0;
    function idInSubfolders(folder:Folder, id:string): boolean {
        if (!folder || !folderPathId)
            return false;
        return folder.id+"" == folderPathId || folder.children?.some(f => idInSubfolders(f, folderPathId));
    };

    const open = !!folderPathId && (id+"" == folderPathId || !!children?.some(f => idInSubfolders(f, folderPathId)));
    
    return (
    <>
    <ListItem sx={{alignSelf:'center', px:0.5, paddingLeft: (indent || 0)}}>
        <ListItemButton key={'FOLDER_'+id}  href={`/folder/${id}`}>
            <ListItemIcon>
                {mapFolderIcon(name)}
            </ListItemIcon>
            <ListItemText primary={name} />
            {/* Here we only enable the expand button if there are children present */}
            {hasChildren && (open ? <ExpandLess/> : <ExpandMore/>) }
        </ListItemButton>
    </ListItem>
    {/* Only add sub-element (collapse) if the folder has children. Otherwise no need! */}
    {hasChildren ? 
        <Collapse in={open} timeout='auto' unmountOnExit>
            <List>
                {children.map(f => 
                    <FolderListItem key={'FOLDER_LI_'+f.id} id={f.id} name={f.name} children={f.children} indent={(indent|| 0) +1}/>
                )}
            </List>
        </Collapse>
        : <></>}
    </>);
}

interface PartialMailbox{
    id:number,
    username:string,
    folders?:Folder[]|null,
    indent?:number
}

const MailBoxListItem : React.FC<PartialMailbox> = ({id, username, folders, indent}) =>{
    const pathname = usePathname();
    const {id:pageId}:{id:string} = useParams();
    
    function idInSubfolders(folder:Folder, id:string): boolean {
        if (!folder)
            return false;
        return folder.id+"" == id || folder.children?.some(f => idInSubfolders(f, id));
    };

    let open = (RegExp("/mailbox/([0-9]+)").test(pathname)
                    && pageId == (id+''));
    let folderId:string = "";
    if (RegExp("/folder/([0-9]+)").test(pathname))
    {
        folderId = pageId;
        if (folderId)
            open = !!folders?.some(f => idInSubfolders(f, folderId));
    }

    return (
        <>
        <ListItem key={'MAILBOX_'+id} sx={{alignSelf:'center', py:0, px:0.5, paddingLeft:indent|| 0}}>
            <ListItemButton href={`/mailbox/${id}`}
                sx={{
                    justifyContent:'space-between',
                    paddingLeft:0
                }}>
                <ListItemIcon sx={{justifyContent:'center'}}>
                    <EmailIcon/>
                </ListItemIcon>
                <ListItemText primary={username}/>
            </ListItemButton>
        </ListItem>
        {
            open ? 
                <List>
                    { !!folders && folders.length > 0 ? folders?.map(f => <FolderListItem key={'FOLDER_LI_'+f.id} id={f.id} name={f.name} children={f.children} folderPathId={folderId} indent={(indent||0)+1}/>) : <></>}
                </List> 
                : <></>
        }
        <Divider/>
        </>
    );
}

const NewMailboxListItem:React.FC = () => {
    return (
        <ListItem sx={{alignSelf:'center', px:0.5}}>
            <ListItemButton key={'NEW'} 
                href="/mailbox/new"
                sx={{
                    minHeight:'3em',
                    justifyContent:'space-between',
                    px:2
            }}>
                <ListItemIcon>
                    <CreateNewFolder />
                </ListItemIcon>
                <ListItemText primary={'Add new Mailbox'} />
            </ListItemButton>
        </ListItem>);
}

const MailBoxList: React.FC = () => {
    // const {getMailboxList} = useMailboxes();
    // const [mailboxesList, setMailboxesList] = useState(<CircularProgress size={20} key='MAILBOX_LOADING' sx={{ alignSelf:'center'}}/>)

    // useEffect(()=> {
    //     async function populateMailboxes() {
    //         const mailboxes = await getMailboxList();
    //         if (mailboxes.length == 0)
    //             setMailboxesList(<></>);
    //         else
    //             setMailboxesList(<>{mailboxes.map(mb => <MailBoxListItem key={'MAILBOX_LI_'+mb.id} id={mb.id} folders={mb.folders} username={mb.username}/>)}</>);
    //     }
    //     if (mailboxesList?.key === 'MAILBOX_LOADING')
    //         populateMailboxes();
    // }, []);
    const {mailboxes} = usePersistentMailboxesState();

    return (
        <List sx={{ height:'100%',
                    display:'flex',
                    flexDirection:'column',
                    flex:'1 1 auto',
                    overflowY:'auto',
                    overflowX:'hidden'}}>
            <NewMailboxListItem key={'NEW_LI'}/>
            {mailboxes.map(mb => <MailBoxListItem key={'MAILBOX_LI_'+mb.id} id={mb.id} folders={mb.folders} username={mb.username}/>)}
        </List>
    );
}

export default MailBoxList;