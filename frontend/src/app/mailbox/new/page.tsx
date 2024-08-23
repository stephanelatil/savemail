'use client'

import { useMailboxes } from "@/hooks/useMailboxes";
import { EditMailBox, ImapProvider } from "@/models/mailBox";
import { ExpandLess, ExpandMore } from "@mui/icons-material";
import { Box, Button, CircularProgress, Collapse, FormControl, FormHelperText, MenuItem, Select, TextField, Typography } from "@mui/material";
import Head from "next/head";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { SubmitHandler, useForm } from "react-hook-form";

const NewMailboxForm:React.FC = () =>{
    const router = useRouter();    
    const { loading, createNewMailbox } = useMailboxes();
    const [ advancedOpen, setAdvancedOpen ] = useState(false);
    const [ errorText, setErrorText ] = useState("");
    const { register, handleSubmit, formState: { errors } } = useForm<EditMailBox>({defaultValues:{
        imapDomain:"",
        imapPort:993,
        username:"",
        password:"",
        provider:0}});

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

    return (
        <>
        <Head key="page_title">
            <title>New Mailbox</title>
        </Head>
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
                onClick={() => setAdvancedOpen((prev) => !prev)}
                endIcon={advancedOpen ? <ExpandLess /> : <ExpandMore />}
                sx={{
                    textTransform: 'none',
                    color: 'text.primary',
                    '&:hover': { backgroundColor: 'transparent' },
                }}
            >
                <Typography variant="h6">Advanced</Typography>
            </Button>
            <Collapse in={advancedOpen} >
                <Box 
                    sx={{
                    maxWidth: '600px',
                    margin: '0 auto',
                    // padding: '2rem',
                    display: 'flex',
                    flexDirection: 'column',
                    gap: '1rem'}}
                >
                    <FormControl error={!!errors.provider} fullWidth>
                        <FormHelperText>
                            {errors.provider?.message??'Imap Authentication Method'}
                        </FormHelperText>
                        <Select
                        label="Imap Authentication Method"
                        defaultValue={ImapProvider.Simple}
                        {...register('provider', { required: 'Authentication value is required'})}>
                            <MenuItem value={0}>Simple</MenuItem>
                            <MenuItem value={4}>Gmail</MenuItem>
                            <MenuItem value={1}>Plain</MenuItem>
                            <MenuItem value={2}>SASL Login</MenuItem>
                            <MenuItem value={3}>Cram MD5</MenuItem>
                        </Select>
                    </FormControl>
                </Box>
            </Collapse>
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
        </Box>
    </>);
}

export default NewMailboxForm;