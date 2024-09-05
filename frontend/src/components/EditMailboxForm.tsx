'use client'

import { useMailboxes } from "@/hooks/useMailboxes";
import { EditMailBox, ImapProvider } from "@/models/mailBox";
import { Google } from "@mui/icons-material";
import { Box, Button, CircularProgress, Divider, TextField, Typography } from "@mui/material";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { SubmitHandler, useForm } from "react-hook-form";


const EditMailboxForm:React.FC = () =>{
    const router = useRouter();
    const params = useParams();
    const mailboxPageId = parseInt(params.id as string ?? '0', 10);
    const [defaultValues, setDefaultValues] = useState<EditMailBox>({
        id:mailboxPageId,
        imapDomain:"",
        imapPort:993,
        username:"",
        password:""});

    const [ provider, setProvider ] = useState<ImapProvider|null>(null);
    const { loading, editMailBox, getMailbox, synchronizeMailbox } = useMailboxes();
    const [ errorText, setErrorText ] = useState("");
    const { register, handleSubmit, formState: { errors } } = useForm<EditMailBox>({defaultValues:defaultValues});

    useEffect(() => {
        const loadDefaults = async () => {
            const mb = await getMailbox(mailboxPageId);
            if(mb)
            {
                setDefaultValues({
                    id:mailboxPageId,
                    imapDomain:mb.imapDomain,
                    imapPort:mb.imapPort,
                    username:mb.username,
                    password:""
                });
                setProvider(mb.provider);
            }
            else
                router.push("/404")
        }

        loadDefaults();
    }, []);

    const onSubmit: SubmitHandler<EditMailBox> = async (mb) => {
        try {
            await editMailBox(mb);
        } catch (err) {
            if (err instanceof Error)
            setErrorText(err.message);
        }
    };

    return (
        <>
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
            <Typography variant="h3" component="h1" textAlign="center">
                Edit Mailbox
            </Typography>
            <Typography variant='h6' textAlign="left">
                {errorText}
            </Typography>
            <TextField
            disabled={provider !== ImapProvider.Simple}
            label="Imap Domain"
            {...register('imapDomain', { required: 'IMAP Domain is required' })}
            error={!!errors.imapDomain}
            helperText={errors.imapDomain?.message}
            fullWidth
            />
            <TextField
            disabled={provider !== ImapProvider.Simple}
            label="Imap Port"
            type='number'
            {...register('imapPort', { required: 'Port is required',
            max:{value:65535, message:"Port must be less than 65535"},
            min:{value:1, message: "Port must be greater than 0"}})}
            error={!!errors.imapPort}
            helperText={errors.imapPort?.message}
            fullWidth
            />
            <TextField
                disabled={provider !== ImapProvider.Simple}
                label="Username (Email address)"
                {...register('username', { required: 'Username is required' })}
                error={!!errors.username}
                helperText={errors.username?.message}
                fullWidth
            />

            {
                // return a password field, or a button to reauthenticate
                (() => {
                    switch(provider)
                    {
                        case ImapProvider.Simple:
                            return <TextField
                                label="Password"
                                type="password"
                                {...register('password', { required: 'Password is required' })}
                                error={!!errors.password}
                                helperText={errors.password?.message}
                                fullWidth
                            />
                        case ImapProvider.Google:
                            return <Button href={`${process.env.NEXT_PUBLIC_BACKEND_URL}/oauth/google/login/${mailboxPageId}`}
                                        variant='outlined'>
                                <Google />{' '}Re-Authenticate with Google
                            </Button>;
                        default:
                            return <></>;
                    }
                })()
            }

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
            <Divider  sx={{my:'2em'}}/>
            <Button 
                disabled={loading}
                variant="outlined"
                size='small'
                color='info'
                onClick={async (e) => {await synchronizeMailbox(mailboxPageId)}}
            >
                Synchronize mailbox
            </Button>
        </Box>
    </>);
}

export default EditMailboxForm;