'use client'

import { useMailboxes } from "@/hooks/useMailboxes";
import { EditMailBox } from "@/models/mailBox";
import { Google } from "@mui/icons-material";
import { Box, Button, CircularProgress, Divider, Typography } from "@mui/material";
import { useRouter } from "next/navigation";
import { SubmitHandler, useForm } from "react-hook-form";
import { PasswordElement, TextFieldElement } from "react-hook-form-mui";

const NewMailboxForm:React.FC = () =>{
    const router = useRouter();
    const { loading, createNewMailbox } = useMailboxes();
    const { control, handleSubmit } = useForm<EditMailBox>({defaultValues:{
        imapDomain:"",
        imapPort:993,
        username:"",
        password:""}});

    const onSubmit: SubmitHandler<EditMailBox> = async (mb) => {
        const createdMb = await createNewMailbox(mb);
        if (!!createdMb)
            router.push(`/mailbox/${createdMb.id}`);
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
            <TextFieldElement
                label="Email address (or Username)"
                name='username'
                control={control}
                required
                fullWidth
            />
            <PasswordElement
                label="Password"
                name='password'
                control={control}
                required
                fullWidth
            />
            <TextFieldElement
                label="Imap Domain"
                name='imapDomain'
                control={control}
                required
                fullWidth
            />
            <TextFieldElement
                label="Imap Port"
                type='number'
                name='imapPort'
                control={control}
                required
                rules={{
                    max:{value:65535, message:"Port must be less than 65535"},
                    min:{value:1, message: "Port must be greater than 0"}}}
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

            <Divider variant='middle' />

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
        </Box>);
}

export default NewMailboxForm;