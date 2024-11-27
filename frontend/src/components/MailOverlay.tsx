'use client'

import { useMails } from "@/hooks/useMails";
import { ArrowDropDown, ChevronRight, Close, Download } from "@mui/icons-material";
import React from 'react';
import purify from 'dompurify';
import { Box, Button, Container, Collapse, Divider, IconButton, Link, List, ListItem, ListItemText, Modal, Paper, Skeleton, Stack, Typography, useTheme } from "@mui/material";
import { Dispatch, SetStateAction, useState } from "react";
import useSWR from "swr";
import { Attachment } from "@/models/attachment";
import Grid2 from "@mui/material/Unstable_Grid2";

const AttachmentDownload:React.FC<{attachment:Attachment}> = ({attachment}) =>{
    function formatBytes(bytes:number, decimals = 2) {
        if (!+bytes) return '0B'
    
        const k = 1024
        const dm = decimals < 0 ? 0 : decimals
        const sizes = ['B', 'KB', 'MB', 'GB', 'TB', 'PB']
    
        const i = Math.floor(Math.log(bytes) / Math.log(k))
    
        return `${parseFloat((bytes / Math.pow(k, i)).toFixed(dm))} ${sizes[i]}`
    }

    return <Button startIcon={<Download />}
                   component={Link}
                   download={attachment.fileName}
                   href={`${process.env.NEXT_PUBLIC_BACKEND_URL}${attachment.downloadUrl}`}
                   variant='contained'>
                {`${attachment.fileName} (${formatBytes(attachment.fileSize)})`}
           </Button>;
};

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

          {/* Attachments */}
          <Divider sx={{ my: 2 }} />
          <Grid2 wrap="wrap" spacing={1}>
            {Array(3).fill(null).map((_, index) => (
              <Container key={index} component="div">
                <Skeleton variant="rectangular" width={200} height={36} />
              </Container>
            ))}
          </Grid2>
        </Stack>
      </Paper>
    );
};

const MailElement:React.FC<{id?:number|null,
                            loadedMails:number[],
                            setLoadedMails: Dispatch<SetStateAction<number[]>>}> = ({id, loadedMails, setLoadedMails}) => {
    const { getMail } = useMails();
    const [ toFromOpen, setToFromOpen ] = useState(false);
    const theme = useTheme();

    // only requery on first render and when mailID changes. Fill with error if not allowed
    const { data:mail, isLoading:loading} = useSWR(`/api/Mail/${id}`, () => {
        if (!id)
            return null;
        return getMail(id);
    });
      
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
            
            <Divider sx={{ my: 2 }} />
            <Grid2 wrap='wrap'
                   spacing={1}>
                {
                    mail.attachments.map((attachment) => {
                        return (<Container component='div' >
                                    <AttachmentDownload attachment={attachment}/>
                                </Container>); }
                    )
                }
            </Grid2>
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