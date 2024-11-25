'use client'

import { useMailboxes } from "@/hooks/useMailboxes";
import { EditMailBox, ImapProvider, MailBox } from "@/models/mailBox";
import { Google } from "@mui/icons-material";
import { Box, Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Divider, Skeleton, TextField, Typography } from "@mui/material";
import { useParams, useRouter } from "next/navigation";
import { SubmitHandler, useForm } from "react-hook-form";
import useSWR from "swr";
import NotFound from "./NotFound";
import { PasswordElement, TextFieldElement } from "react-hook-form-mui";
import { useState } from "react";
import { useNotification } from "@/hooks/useNotification";


const EditMailboxFormBase:React.FC<{defaultValues:MailBox}> = ({defaultValues}) =>{
    const mailboxPageId = defaultValues.id;

    const { loading, editMailBox, synchronizeMailbox, deleteMailBox } = useMailboxes();
    const { control, handleSubmit } = useForm<EditMailBox>({defaultValues:defaultValues});
    const { control:controlDelete, handleSubmit:handleSubmitDelete } = useForm<{delete:string}>();
    const provider = defaultValues?.provider;
    const router = useRouter();
    const [deleteOpen, setDeleteOpen] = useState(false);
    const showNotif = useNotification();

    const onSubmit: SubmitHandler<EditMailBox> = async (mb) => {
        await editMailBox(mb);
    };

    return (
        <Box 
            component="form"
            onSubmit={handleSubmit(onSubmit)}
            sx={{
            maxWidth: '600px',
            margin: '0 auto',
            padding: '2rem',
            display: 'flex',
            flexDirection: 'column',
            gap: '1rem',
        }}>
            <Typography variant="h3" textAlign="center">
                Edit Mailbox
            </Typography>
            
            {
                defaultValues.needsReauth &&
                <Typography variant='h6' textAlign="center" color='warning'>
                    To continue syncing you need to update your credentials!
                    {provider === ImapProvider.Simple ? "\nPlease check that your Username and Password are still valid and that the Imap info is correct."
                                                      : "\nPlease Refresh your OAuth credentials with the button below."
                    }
                </Typography>
            }

            <TextFieldElement
                disabled={provider !== ImapProvider.Simple}
                name='username'
                control={control}
                label="Email address (or Username))"
                required
                fullWidth
            />
 
            {
                // return a password field, or a button to reauthenticate
                (() => {
                    switch(provider)
                    {
                        case ImapProvider.Simple:
                            return <PasswordElement
                                label="Password"
                                name="password"
                                required
                                fullWidth
                            />
                        case ImapProvider.Google:
                            const base = new URL(`${process.env.NEXT_PUBLIC_BACKEND_URL}/oauth/google/login/${mailboxPageId}`);
                            let hostname = process.env.NEXT_PUBLIC_FRONTEND_URL;
                            while (hostname?.charAt(hostname.length-1) == '/')
                                hostname = hostname.substring(hostname.length-1);
                            base.searchParams.set('next', `${hostname}/mailbox`);
                            return <Button href={base.toString()}
                                        variant='outlined'>
                                <Google />{' '}Re-Authenticate with Google
                            </Button>;
                        default:
                            return <></>;
                    }
                })()
            }

            <TextFieldElement
                disabled={provider !== ImapProvider.Simple}
                label="Imap Domain"
                name='imapDomain'
                control={control}
                required
                fullWidth
            />
            <TextFieldElement
                disabled={provider !== ImapProvider.Simple}
                label="Imap Port"
                type='number'
                name='imapPort'
                control={control}
                rules={{
                    max:{value:65535, message:"Port must be less than 65535"},
                    min:{value:1, message: "Port must be greater than 0"}
                }}
                required
                fullWidth
            />

            <Button
                type="submit"
                variant="contained"
                color="primary"
                fullWidth
                disabled={loading || provider !== ImapProvider.Simple}
                aria-busy={loading}
                sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}
                >
                {loading ? <CircularProgress size={24} color="inherit" /> : "Update"}
            </Button>
            <Divider variant="middle" sx={{my:'2rem'}}/>
            <Button 
                disabled={loading}
                variant="outlined"
                size='small'
                color='info'
                onClick={async (e) => {await synchronizeMailbox(mailboxPageId)}}
            >
                Synchronize mailbox now
            </Button>

            <Divider variant="middle" sx={{my:'3rem'}}/>
            <Button
                disabled={loading}
                variant='contained'
                size='small'
                sx={{py:'0.5rem'}}
                color='error'
                onClick={() => {setDeleteOpen(true);}}
            >
                Delete MailBox
            </Button>
            <Dialog
                open={deleteOpen}
                onClose={() => {setDeleteOpen(false);}}
                PaperProps={{
                    component: 'form'
                }}
                >
                <DialogTitle>Subscribe</DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        <Typography>
                            Are you sure you want to delete this mailbox? <br/>
                        </Typography>
                        <Typography color='error'>
                            This action is permanent and you will have to re-add and re-download all emails.
                        </Typography>
                        <Typography>
                            Write "delete" below then click the button
                        </Typography>
                    </DialogContentText>
                    <TextFieldElement
                        autoFocus
                        required
                        margin="dense"
                        control={controlDelete}
                        name="delete"
                        label='Write "delete" to confirm   '
                        fullWidth
                        variant="standard"
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => {setDeleteOpen(false);}}>Cancel</Button>
                    <Button onClick={handleSubmitDelete(async (data) => {
                        if (data.delete.trim() != 'delete')
                        {
                            showNotif('Write "delete" to confirm deleting', 'error');
                            return;
                        }
                        await deleteMailBox(mailboxPageId);
                        setDeleteOpen(false);
                        router.push('/mailbox/new');
                    })}>
                        Delete Mailbox
                        </Button>
                </DialogActions>

            </Dialog>
        </Box>);
}

const EditMailboxFormSkeleton: React.FC = () => (
    <Box 
        sx={{
            maxWidth: '600px',
            width:'95%',
            margin: '0 auto',
            padding: '2rem',
            display: 'flex',
            flexDirection: 'column',
            gap: '1rem',
        }}
    >
        <Typography variant="h3" textAlign="center">
            Edit Mailbox
        </Typography>

        <Skeleton variant="text" width="80%" height={30} sx={{ margin: '1rem auto'}} />

        <Skeleton variant="rectangular" height={56} />
        <Skeleton variant="rounded" animation='wave' height={40} />
        <Skeleton variant="rectangular" height={56} />
        <Skeleton variant="rectangular" height={56} />
        <Skeleton variant="rounded" animation='wave' height={40} />

        <Divider variant="middle" sx={{my:'2rem'}}/>
        <Skeleton variant="rounded" animation='wave' width="60%" height={30} sx={{ margin: '0 auto' }} />
    </Box>
);


const EditMailboxForm: React.FC = () => {
    const params = useParams();
    const mailboxPageId = parseInt(params.id as string ?? '0', 10);
    const { getMailbox } = useMailboxes();
    const {data, isLoading} = useSWR('/api/MailBox/'+mailboxPageId, () =>getMailbox(mailboxPageId));

    return isLoading
                ? <EditMailboxFormSkeleton />
                : !!data ? <EditMailboxFormBase defaultValues={data}/>
                         : <NotFound />
}

export default EditMailboxForm;