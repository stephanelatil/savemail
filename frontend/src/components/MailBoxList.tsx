'use client'

import { useMailboxes } from "@/hooks/useMailboxes";
import { Folder } from "@/models/folder";
import { Archive as ArchiveIcon, CreateNewFolder, Delete as DeleteIcon, Email as EmailIcon, ExpandLess, ExpandMore, Folder as FolderIcon, Send as SendIcon } from "@mui/icons-material";
import { CircularProgress, Collapse, Divider, List, ListItem, ListItemButton, ListItemIcon, ListItemText, Typography } from "@mui/material";
import React, { Suspense, useEffect, useMemo, useState } from "react";

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
    children:Folder[],
    showFolder?:(folderId:number)=>void
}

const FolderListItem: React.FC<PartialFolderInfo> = ({id, name, children, showFolder}) => {
    const hasChildren:boolean = children.length > 0;
    const [open, setOpen] = useState(false);

    const handleClick = ()=> setOpen(!open);

    return (
    <>
    <ListItem sx={{alignSelf:'center', px:0.5}}>
        <ListItemButton key={'FOLDER_'+id} onClick={handleClick}>
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
                    <FolderListItem id={f.id} name={f.name} children={f.children} showFolder={showFolder}/>
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
    showFolder?:(folderId:number)=>void
}

const MailBoxListItem : React.FC<PartialMailbox> = ({id, username, folders, showFolder}) =>{
    const [open, setOpen] = useState(false);

    const handleClick = ()=> setOpen(!open);

    return (
        <>
        <ListItem key={'MAILBOX_'+id} disablePadding  sx={{alignSelf:'center', px:0.5, display:'block'}} onClick={handleClick} >
            <ListItemButton sx={{
                minHeight:'3em',
                justifyContent:'space-between'
            }}>
                <ListItemIcon sx={{justifyContent:'center'}}>
                    <EmailIcon/>
                </ListItemIcon>
                <ListItemText primary={username}/>
            </ListItemButton>
        </ListItem>
        <Collapse in={open} timeout='auto' unmountOnExit>
            <List>
                { !!folders && folders.length > 0 ? folders?.map(f => <FolderListItem id={f.id} name={f.name} children={f.children} showFolder={showFolder}/>) : <></>}
            </List>
        </Collapse>
        <Divider/>
        </>
    );
}

const NewMailboxListItem:React.FC = () => {
    return (
        <ListItem sx={{alignSelf:'center', px:0.5}}>
            <ListItemButton key={'NEW'} sx={{
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

const MailBoxList: React.FC<{showFolder?:(folderId:number)=>void}> = ({showFolder}) => {
    const {getMailboxList} = useMailboxes();
    const [mailboxesList, setMailboxesList] = useState(<CircularProgress size={20} key='MAILBOX_LOADING' sx={{ alignSelf:'center'}}/>)

    useEffect(()=> {
        async function populateMailboxes() {
            const mailboxes = await getMailboxList();
            if (mailboxes.length == 0)
                setMailboxesList(<></>);
            else
                setMailboxesList(<>{mailboxes.map(mb => <MailBoxListItem id={mb.id} username={mb.username} showFolder={showFolder}/>)}</>);
        }
        if (mailboxesList?.key === 'MAILBOX_LOADING')
            populateMailboxes();
    }, []);

    return (
        <List sx={{height:'100%', display:'flex', flexDirection:'column', flex:'1 1 auto', overflowY:'auto', overflowX:'hidden'}}>
            <NewMailboxListItem />
            {mailboxesList}
        </List>
    );
}

export default MailBoxList;