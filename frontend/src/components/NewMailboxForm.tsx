'use client'

import { useMailboxes } from "@/hooks/useMailboxes";
import { EditMailBox } from "@/models/mailBox";
import { Google } from "@mui/icons-material";
import { Box, Button, CircularProgress, Divider, TextField, Typography } from "@mui/material";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { SubmitHandler, useForm } from "react-hook-form";

const NewMailboxForm:React.FC = () =>{
    const router = useRouter();
    const { loading, createNewMailbox } = useMailboxes();
    const [ errorText, setErrorText ] = useState("");
    const { register, handleSubmit, formState: { errors } } = useForm<EditMailBox>({defaultValues:{
        imapDomain:"",
        imapPort:993,
        username:"",
        password:""}});

    const onSubmit: SubmitHandler<EditMailBox> = async (mb) => {
        try {
            const createdMb = await createNewMailbox(mb);
            if (!!createdMb)
                router.push(`/mailbox/${createdMb.id}`);
        } catch (err) {
            if (err instanceof Error)
            setErrorText(err.message);
        }
    };

    const oauthUrl = (oauthProvider:string):string  =>{
        const base = new URL(`${process.env.NEXT_PUBLIC_BACKEND_URL}/oauth/${oauthProvider}/login`);
        
        let hostname = process.env.NEXT_PUBLIC_FRONTEND_URL;
        while (hostname?.charAt(hostname.length-1) == '/')
            hostname = hostname.substring(hostname.length-1);

        base.searchParams.set('next', `${hostname}/mailbox`);
        return base.toString();
    }

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
                New Mailbox
            </Typography>
            <Typography variant='h6' textAlign="left">
                {errorText}
            </Typography>
            <TextField
            label="Imap Domain"
            defaultValue=""
            {...register('imapDomain', { required: 'IMAP Domain is required' })}
            error={!!errors.imapDomain}
            helperText={errors.imapDomain?.message}
            fullWidth
            />
            <TextField
            label="Imap Port"
            type='number'
            defaultValue={993}
            {...register('imapPort', { required: 'Port is required',
            max:{value:65535, message:"Port must be less than 65535"},
            min:{value:1, message: "Port must be greater than 0"}})}
            error={!!errors.imapPort}
            helperText={errors.imapPort?.message}
            fullWidth
            />
            <TextField
                label="Username (Email address)"
                defaultValue=""
                {...register('username', { required: 'Username is required' })}
                error={!!errors.username}
                helperText={errors.username?.message}
                fullWidth
            />
            <TextField
                label="Password"
                type="password"
                {...register('password', { required: 'Password is required' })}
                error={!!errors.password}
                helperText={errors.password?.message}
                fullWidth
            />
            <Button
            type="submit"
            variant="contained"
            color="primary"
            fullWidth
            disabled={loading}
            aria-busy={loading}
            sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}
            >
            {loading ? <CircularProgress size={24} color="inherit" /> : "Create"}
            </Button>

            <Divider/>

            <Typography
                align='center'
                variant="h6"
            >
                Or link mailbox with OAuth
            </Typography>
            <Button
                variant='contained'
                href={oauthUrl('google')}
                startIcon={<Google />}
                >
                Google
            </Button>
        </Box>
    </>);
}

export default NewMailboxForm;