'use client'

import { useMails } from "@/hooks/useMails";
import { Mail } from "@/models/mail";
import { ArrowDropDown, ChevronRight, Close } from "@mui/icons-material";
import React from 'react';
import purify from 'dompurify'
import { Box, Button,  Collapse, Divider, List, ListItem, ListItemText, Modal, Paper, Skeleton, Stack, Typography } from "@mui/material";
import { Dispatch, SetStateAction, useEffect, useState } from "react";


const LoadingMailElement:React.FC<{mail?:Mail}> = ({mail}) => {
    return (<Box>
        </Box>);
}

const MailElement:React.FC<{id?:number|null,
                            loadedMails:number[],
                            setLoadedMails: Dispatch<SetStateAction<number[]>>}> = ({id, loadedMails, setLoadedMails}) => {
    const { loading, getMail } = useMails();
    const [ toFromOpen, setToFromOpen ] = useState(false);
    const [ mail, setMail ] = useState<Mail|null>();

    // only requery on first render and when mailID changes. Fill with error if not allowed
    useEffect(()=>{
        setMail(null);
        const populateMail = async () => {
            //dot not fetch if id is invalid or if mail is already fetched
            if (!id || id <= 0 || (!!mail && mail.id == id))
                return;
            if (loadedMails.includes(id))
                return;
            const fetchedMail:Mail|null = await getMail(id);

            //recheck in case another handled it
            if (!loadedMails.includes(id)){
                setMail(fetchedMail);
                setLoadedMails(loadedMails.concat([id]));
            }
        };

        populateMail();
    },[id]);
    return (<>
            {!mail ? <></> : //if mail is null do nothing. Most likely already loaded
            (
                loading ? <LoadingMailElement />: //show loading 
                <>
                    <MailElement id={mail.repliedFromId} loadedMails={loadedMails} setLoadedMails={setLoadedMails}/> {/*parent here*/}
                    <Box>
                        <Stack flexDirection='row'>
                            <Typography variant="h6" maxWidth='90%' flexWrap='wrap'>
                                {mail.subject}
                            </Typography>
                            <Typography variant='body1' minWidth='10em'>
                                {mail.dateSent.toLocaleString("en-Us", {
                                        dateStyle:"medium",
                                        timeStyle:"medium",
                                        dayPeriod:"short",
                                        hour:'2-digit',
                                        hourCycle:'h24',
                                    } as Intl.DateTimeFormatOptions)
                                }
                            </Typography>
                        </Stack>
                        <Button onClick={() => setToFromOpen(prev => !prev)} variant='text'>
                            <Typography variant="body1">
                                From: {mail.sender.fullName ?? "" + mail.sender.address}
                            </Typography>
                            { toFromOpen ? <ArrowDropDown /> : <ChevronRight /> }
                        </Button>

                        <Collapse in={toFromOpen}>
                            <List>
                                {mail.recipients.map(r => (
                                            <ListItem key={r.address}>
                                                <ListItemText>
                                                    <Typography variant="body2">
                                                        {r.fullName ?? "" + r.address}
                                                    </Typography>
                                                </ListItemText>
                                            </ListItem>
                                        ))}
                            </List>
                            { mail.recipientsCc?.length ?? 0 > 0 ?
                                <List>
                                    {mail.recipientsCc.map(r => (
                                        <ListItem key={r.address}>
                                            <ListItemText>
                                                <Typography variant="body2">
                                                    {r.fullName ?? "" + r.address}
                                                </Typography>
                                            </ListItemText>
                                        </ListItem> ))}
                                </List> :
                                <></> }
                        </Collapse>
                        <Box dangerouslySetInnerHTML={{ __html: purify.sanitize(mail.body)}}/>
                        {/* TODO Add attachments here! */}
                        <Divider />
                    </Box>
                    {/*child (reply) here. Removed to avoid recursive loading*/}
                    {/* <MailElement id={mail.replyId} loadedMails={loadedMails} setLoadedMails={setLoadedMails}/>  */}
                </>
            )
            }
            </>);
}

interface MailModalProps {
    id:number,
    open:boolean,
    setOpen:(value:boolean)=>void
}

const MailOverlay:React.FC<MailModalProps> = ({id, open, setOpen}) => {
    const handleClose = () => setOpen(false);
    const [loadedMails, setLoadedMails] = useState<number[]>([]);

    return (
        <Modal open={open}
               onClose={handleClose}
               closeAfterTransition
               slotProps={{
                backdrop:''
               }}>
            <Box sx={{alignItems:'center',
                      alignSelf:'center',
                      left:'0.5em',
                      p:'1em',
                      maxHeight:'90%',
                      width:'fit-content',
                      maxWidth:'95%',
                      overflow:'scroll',
                      bgcolor: 'white',
                      border: '2px solid #000',
                }}>
                <Paper elevation={0} variant="outlined" square>
                    <Button onClick={handleClose} disabled={!open}
                            sx={{
                                position:'sticky',
                                alignSelf:'end',
                                alignContent:'end'
                            }}>
                            <Close />
                    </Button>
                    <Paper  elevation={1}
                            square
                            sx={{
                                display:'flex',
                                overflow:'scroll'
                                }}>
                        <MailElement id={id} loadedMails={loadedMails} setLoadedMails={setLoadedMails}/>
                    </Paper>
                </Paper>
            </Box>
        </Modal>
        // TODO fix it not being centered
    );
}

export default MailOverlay;