'use client'

import { useFolder } from "@/hooks/useFolder";
import { EmailAddress } from "@/models/emailAddress";
import { Mail } from "@/models/mail";
import { AttachFile, SkipNext, SkipPrevious } from "@mui/icons-material";
import { Card, CardContent, Stack, Typography, Box, ListItem, CircularProgress, Button, List, ListItemButton } from "@mui/material";
import { useEffect, useState } from "react";
import { useParams } from 'next/navigation';
import NotFound from "./NotFound";
import MailOverlay from "./MailOverlay";

interface MailParts{
    id:number,
    repliedFromId?: number|null,
    sender:EmailAddress,
    subject:string,
    body:string,
    hasAttachments:boolean,
    dateSent:Date
}

const MailListItem: React.FC<MailParts> = ({id,repliedFromId,sender,subject,body,hasAttachments,dateSent}) => {
    return (
        <ListItem key={"MAIL_"+id}>
            {/* Find a way to open mail overlay on click */}
            <ListItemButton >
                <Stack flexDirection="column" spacing={1} justifyContent="space-between">
                    <Stack flexDirection="row" spacing={2} justifyContent={"space-between"}>
                        <Typography variant="body1" color="textSecondary" maxWidth={'10em'} minWidth={"10%"}>
                            {sender?.fullName?.length > 0 ? sender.fullName : sender.address}
                        </Typography>
                        <Typography variant="h6" width='100%'>{((repliedFromId ?? 0) > 0 ? "RE: ": "") + subject}</Typography>
                        <Typography variant="body1" color="textSecondary">
                            {new Date(dateSent).toLocaleString() /*TODO convert utc date to local time*/ }
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
    setPageNum:(pageNum:number)=>void,
    mails:Mail[]
}

const MailListBox : React.FC<MailListPageInfo> = ({hasNext, hasPrev, pageNum, setPageNum, mails}) => {
    const [open, setOpen] = useState(false);

    return (
        <Box overflow='hidden'>
            <MailOverlay open={open} setOpen={setOpen} id={0}/>
            <Stack direction='row' justifyContent='right'>
                <Button onClick={() => setPageNum(pageNum-1)} disabled={!hasPrev}>
                    <SkipPrevious />
                </Button>
                <Typography variant='h6'>
                    {pageNum}
                </Typography>
                <Button onClick={() => setPageNum(pageNum+1)} disabled={!hasNext}>
                    <SkipNext />
                </Button>
            </Stack>
        <List>
            {mails.map(m => <MailListItem   
                                    key={"MAIL_LI_"+m.id}
                                    id={m.id}
                                    subject={m.subject}
                                    body={m.body}
                                    sender={m.sender}
                                    hasAttachments={m.attachments.length > 0}
                                    dateSent={m.dateSent}
                                          />)}
        </List>
    </Box>);
}

const MailListPage : React.FC = () => {
    const {id:folderId}:{id:string} = useParams();
    const {loading, getMails} = useFolder();
    const [mailList, setMailList] = useState(<CircularProgress />);
    const [pageNum, setPageNum] = useState(1);
    folderId
    useEffect(() => {
        async function fetchEmails(){
            const paginatedMail = await getMails(parseInt(folderId), pageNum);
            if (!paginatedMail)
            {
                setMailList(<NotFound/>);
                return;
            }
            const hasPrev = !!paginatedMail?.previousPage;
            const hasNext = !!paginatedMail?.nextPage;
            setMailList(<MailListBox mails={paginatedMail.items} hasNext={hasNext} hasPrev={hasPrev} pageNum={pageNum} setPageNum={setPageNum}/>);
        }
        fetchEmails();
    },[pageNum]);

    return <>{loading ? <CircularProgress /> : mailList}</>;
}

export default MailListPage