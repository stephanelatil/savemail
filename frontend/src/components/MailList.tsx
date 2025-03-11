'use client'

import { useFolder } from "@/hooks/useFolder";
import { EmailAddress } from "@/models/emailAddress";
import { Mail } from "@/models/mail";
import React, { Suspense } from 'react';
import { AttachFile, SkipNext, SkipPrevious } from "@mui/icons-material";
import { Stack, Typography, Box, ListItem, Button, List, ListItemButton, Paper, BottomNavigation, BottomNavigationAction, PaginationItem, TablePagination, Pagination, Skeleton } from "@mui/material";
import { useEffect, useState } from "react";
import { useParams, usePathname, useRouter, useSearchParams } from 'next/navigation';
import NotFound from "./NotFound";
import MailOverlay from "./MailOverlay";
import { PaginatedRequest } from "@/models/paginatedRequest";

interface MailParts{
    id:number,
    repliedFromId?: number|null,
    sender:EmailAddress,
    subject:string,
    body:string,
    hasAttachments:boolean,
    dateSent:Date,
    onClick:()=>void,
}

const MailListItem: React.FC<MailParts> = ({id,repliedFromId,sender,subject,body,hasAttachments,dateSent,onClick}) => {
    return (
        <ListItem key={"MAIL_"+id}>
            {/* Find a way to open mail overlay on click */}
            <ListItemButton onClick={onClick}>
                <Stack flexDirection="column" spacing={1} justifyContent="space-between">
                    <Stack flexDirection="row" spacing={2} justifyContent={"space-between"}>
                        <Typography variant="body1" color="textSecondary" maxWidth={'10em'} minWidth={"10%"}>
                            {sender?.fullName?.length > 0 ? sender.fullName : sender.address}
                        </Typography>
                        <Typography variant="h6" width='100%'>{((repliedFromId ?? 0) > 0 ? "RE: ": "") + subject}</Typography>
                        <Typography variant="body1" color="textSecondary">
                            {new Date(dateSent).toLocaleString()}
                        </Typography>
                    </Stack>
                    <Stack flexDirection="row" justifyContent="space-between">
                        <Typography width="95%">{body}</Typography>
                        { hasAttachments ? <AttachFile width="5%"/> : <></> }
                    </Stack>
                </Stack>
            </ListItemButton>
        </ListItem>);
}

interface MailListPageInfo {
    hasNext?:boolean,
    hasPrev?:boolean
    pageNum:number,
    mails:Mail[]
}

const MailListBox : React.FC<MailListPageInfo> = ({hasNext, hasPrev, pageNum, mails}) => {
    const [open, setOpen] = useState(false);
    const [mailId, setMailId] = useState(0);
    const router = useRouter();
    const basePathname = usePathname();

    return (
        <>
            <Stack overflow='hidden'>
                <List>
                    {mails.map(m => <MailListItem   
                                            key={"MAIL_LI_"+m.id}
                                            id={m.id}
                                            subject={m.subject}
                                            body={m.body}
                                            sender={m.sender}
                                            hasAttachments={m.attachments.length > 0}
                                            dateSent={m.dateSent}
                                            onClick={() => {setMailId(m.id); setOpen(true);}}
                                                />)}
                    </List>
                    <Stack direction='row' justifyContent='center' position='fixed'>
                        <Button onClick={() => router.push(`${basePathname}?page=${pageNum-1}`)} disabled={!hasPrev}>
                            <SkipPrevious />
                        </Button>
                        <Typography variant='h6'>
                            {pageNum}
                        </Typography>
                        <Button onClick={() => router.push(`${basePathname}?page=${pageNum+1}`)} disabled={!hasNext}>
                            <SkipNext />
                        </Button>
                    </Stack>
                <MailOverlay open={open} setOpen={setOpen} id={mailId}/>
            </Stack>
        </>);
}

const LoadingMailListBox: React.FC = () => {
    return (
        <Stack overflow="hidden" sx={{width:'100%'}}>
            <List sx={{width:'100%'}}>
                {Array.from({ length: 5 }).map((_, index) => (
                    <ListItem key={`loading-mail-${index}`} sx={{width:'100%'}}>
                        <Stack flexDirection="column" spacing={1} justifyContent="space-between" width='100%'>
                            <Stack flexDirection="row" spacing={2} justifyContent={"space-between"} width='100%'>
                                <Skeleton variant='text' width='25%'/>
                                <Skeleton variant="text" width='50%'/>
                                <Skeleton variant='rounded' width='20%' sx={{maxWidth:'10rem'}} animation='pulse'/>
                            </Stack>
                            <Stack flexDirection='row' justifyContent="space-between" gap={1} width='100%'>
                                <Skeleton variant='text' width="95%"/>
                                <Skeleton variant='rounded' animation='pulse' width='5%'/>
                            </Stack>
                        </Stack>
                    </ListItem>
                ))}
            </List>
        </Stack>
    );
};

const MailListPage : React.FC<{folderId:number, pageNum:number}> = ({folderId, pageNum}) => {
    const { loading, getMails } = useFolder();
    const [mailList, setMailList] = useState<PaginatedRequest<Mail>|null>(null);

    useEffect(() => {
        const load_mails = async () => getMails(folderId, pageNum)
                                            .then((mails) => setMailList(mails));
        load_mails();
    }, [folderId, pageNum])

    return loading ? <LoadingMailListBox />
                    : (!!mailList ? <MailListBox mails={mailList.items}
                                                hasNext={!!mailList.next}
                                                hasPrev={!!mailList.prev}
                                                pageNum={pageNum}/>
                                  : <NotFound objectName={`Page ${pageNum}`}/>);
}

export const MailListPage2: React.FC = () => {
    const {id:folder}:{id:string} = useParams();
    const searchParams = useSearchParams();
    const folderId:number = parseInt(folder);

    if (isNaN(folderId) || !isFinite(folderId))
        return <NotFound objectName={`Folder with ID ${folder}`}/>

    let mailPage = searchParams.has('page') ? parseInt(searchParams.get('page') ?? '1') : 1;
    mailPage = isNaN(mailPage) ? 1 : mailPage;

    return <Suspense fallback={<LoadingMailListBox />}>
                <MailListPage folderId={folderId} pageNum={mailPage}/>
            </Suspense>;
}

export default MailListPage2;