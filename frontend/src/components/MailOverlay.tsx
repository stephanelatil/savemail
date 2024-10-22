'use client'

import { useMails } from "@/hooks/useMails";
import { Mail } from "@/models/mail";
import { ArrowDropDown, ChevronRight, Close } from "@mui/icons-material";
import React from 'react';
import purify from 'dompurify';
import { Box, Button,  Collapse, Divider, IconButton, List, ListItem, ListItemText, Modal, Paper, Skeleton, Stack, Typography, useTheme } from "@mui/material";
import { Dispatch, SetStateAction, useEffect, useState } from "react";


const LoadingMailElement: React.FC = () => {
    const theme = useTheme();
    
    return (
      <Paper 
        elevation={0} 
        sx={{ 
          mb: 2, 
          p: 2, 
          borderRadius: 2, 
          bgcolor: theme.palette.background.paper,
        }}
      >
        <Stack spacing={2}>
          {/* Subject and Date */}
          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Skeleton variant="text" width="60%" height={32} />
            <Skeleton variant="text" width="20%" height={24} />
          </Stack>
          
          {/* From */}
          <Skeleton variant="text" width="40%" height={24} />
          
          {/* Email body paragraphs */}
          <Skeleton variant="text" width="100%" height={20} />
          <Skeleton variant="text" width="95%" height={20} />
          <Skeleton variant="text" width="98%" height={20} />
          <Skeleton variant="text" width="90%" height={20} />
        </Stack>
      </Paper>
    );
  };

const MailElement:React.FC<{id?:number|null,
                            loadedMails:number[],
                            setLoadedMails: Dispatch<SetStateAction<number[]>>}> = ({id, loadedMails, setLoadedMails}) => {
    const { loading, getMail } = useMails();
    const [ toFromOpen, setToFromOpen ] = useState(false);
    const [ mail, setMail ] = useState<Mail|null>();
    const theme = useTheme();

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
      
    if (loading) return <LoadingMailElement />;
    if (!mail) return <></>;

    return (
        <Paper 
        elevation={0} 
        sx={{ 
            mb: 2, 
            p: 2, 
            borderRadius: 2, 
            bgcolor: theme.palette.background.paper,
            color: theme.palette.text.primary
        }}
        >
        <MailElement id={mail.repliedFromId} loadedMails={loadedMails} setLoadedMails={setLoadedMails} />
        <Box>
            <Stack direction="row" justifyContent="space-between" alignItems="center" mb={1}>
            <Typography variant="h6" sx={{ fontWeight: 'bold', color: theme.palette.primary.main }}>
                {mail.subject}
            </Typography>
            <Typography variant="body2" sx={{ color: theme.palette.text.secondary }}>
                {mail.dateSent?.toLocaleString("en-US", {
                dateStyle: "medium",
                timeStyle: "short",
                }) ?? "?"}
            </Typography>
            </Stack>
            
            <Button 
            onClick={() => setToFromOpen(prev => !prev)} 
            variant='text' 
            sx={{ mb: 1, color: theme.palette.text.primary }}
            startIcon={toFromOpen ? <ArrowDropDown /> : <ChevronRight />}
            >
            <Typography variant="body2">
                From: {(mail.sender?.fullName || mail.sender?.address) ?? "UNKNOWN"}
            </Typography>
            </Button>

            <Collapse in={toFromOpen}>
            <Box sx={{ bgcolor: theme.palette.action.hover, borderRadius: 1, p: 1, mb: 1 }}>
                <Typography variant="subtitle2" sx={{ mb: 1 }}>To:</Typography>
                <List dense disablePadding>
                {mail.recipients?.map(r => (
                    <ListItem key={r.address} disableGutters>
                    <ListItemText 
                        primary={r.fullName || r.address} 
                        primaryTypographyProps={{ variant: 'body2' }}
                    />
                    </ListItem>
                ))}
                </List>
                {mail.recipientsCc?.length > 0 ? (
                    <>
                        <Typography variant="subtitle2" sx={{ mt: 1, mb: 1 }}>CC:</Typography>
                        <List dense disablePadding>
                        {mail.recipientsCc?.map(r => (
                            <ListItem key={r.address} disableGutters>
                            <ListItemText 
                                primary={r.fullName || r.address} 
                                primaryTypographyProps={{ variant: 'body2' }}
                            />
                            </ListItem>
                        ))}
                        </List>
                    </>
                ) : <></>}
            </Box>
            </Collapse>
            
            <Box 
            dangerouslySetInnerHTML={{ __html: purify.sanitize(mail.body) }}
            sx={{ 
                '& *': { 
                color: `${theme.palette.text.primary} !important`,
                backgroundColor: `${theme.palette.background.paper} !important`,
                },
                '& a': { 
                color: `${theme.palette.primary.main} !important`, 
                textDecoration: 'none' 
                },
                '& a:hover': { 
                textDecoration: 'underline' 
                },
            }}
            />
            
            {/* TODO: Add attachments here */}
            
            <Divider sx={{ my: 2 }} />
        </Box>
        </Paper>
    );
};

interface MailModalProps {
    id:number,
    open:boolean,
    setOpen:(value:boolean)=>void
}

const MailOverlay: React.FC<MailModalProps> = ({ id, open, setOpen }) => {
    const handleClose = () => setOpen(false);
    const [loadedMails, setLoadedMails] = useState<number[]>([]);

    return (
        <Modal
        open={open}
        onClose={handleClose}
        closeAfterTransition
        sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
        }}
        >
            <Box
                sx={{
                width: '90%',
                maxWidth: '800px',
                maxHeight: '90vh',
                bgcolor: 'background.paper',
                borderRadius: 2,
                boxShadow: 24,
                overflow: 'hidden',
                display: 'flex',
                flexDirection: 'column',
                }}
            >
                <Paper
                elevation={0}
                sx={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    p: 2,
                    borderBottom: '1px solid #e0e0e0',
                }}
                >
                <Typography variant="h6">Email</Typography>
                <IconButton onClick={handleClose} size="small">
                    <Close />
                </IconButton>
                </Paper>
                <Box sx={{ flexGrow: 1, overflow: 'auto', p: 2 }}>
                <MailElement id={id} loadedMails={loadedMails} setLoadedMails={setLoadedMails} />
                </Box>
            </Box>
        </Modal>
    );
}

export default MailOverlay;