'use client'

import { useMailboxes } from "@/hooks/useMailboxes";
import { EditMailBox, ImapProvider, SecureSocketOptions } from "@/models/mailBox";
import { ExpandLess, ExpandMore } from "@mui/icons-material";
import { Box, Button, CircularProgress, Collapse, FormControl, FormHelperText, MenuItem, Select, TextField, Typography } from "@mui/material";
import Head from "next/head";
import { useRouter } from "next/router";
import { useState } from "react";
import { SubmitHandler, useForm } from "react-hook-form";

const EditMailboxForm:React.FC = () =>{
    const router = useRouter();
    const id = parseInt(router.query.id as string ?? '0', 10);
    const [defaultValues, setDefaultValues] = useState({
        id:id,
        imapDomain:"",
        imapPort:993,
        username:"",
        password:"",
        provider:0,
        secureSocketOptions:1
    } as EditMailBox);

    const { loading, editMailBox } = useMailboxes();
    const [ advancedOpen, setAdvancedOpen ] = useState(false);
    const [ errorText, setErrorText ] = useState("");
    const { register, handleSubmit, formState: { errors } } = useForm<EditMailBox>({defaultValues:defaultValues});


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
        <Head key="page_title">
            <title>Edit Mailbox</title>
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
            {...register('imapDomain', { required: 'IMAP Domain is required' })}
            error={!!errors.imapDomain}
            helperText={errors.imapDomain?.message}
            fullWidth
            />
            <TextField
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
                label="Username (Email address)"
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
            <Collapse in={advancedOpen}>
                <Box 
                    sx={{
                    maxWidth: '600px',
                    margin: '0 auto',
                    // padding: '2rem',
                    display: 'flex',
                    flexDirection: 'column',
                    gap: '1rem'}}
                >
                    <FormControl error={!!errors.secureSocketOptions} fullWidth>
                        <FormHelperText>
                        {errors.secureSocketOptions?.message ?? "Secure Socket Options"}
                        </FormHelperText>
                        <Select
                        label="Secure Socket Options"
                        {...register('secureSocketOptions', { required: 'Secure Socket Options value is required'})}
                        fullWidth>
                            <MenuItem value={0}>None</MenuItem>
                            <MenuItem value={1}>Auto</MenuItem>
                            <MenuItem value={2}>SslOnConnect</MenuItem>
                            <MenuItem value={3}>StartTls</MenuItem>
                            <MenuItem value={4}>StartTlsIfAvailable</MenuItem>
                        </Select>
                    </FormControl>

                    <FormControl error={!!errors.provider} fullWidth>
                        <FormHelperText>
                            {errors.provider?.message??'Imap Authentication Method'}
                        </FormHelperText>
                        <Select
                        label="Imap Authentication Method"
                        {...register('provider', { required: 'Secure Socket Options value is required'})}>
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

export default EditMailboxForm;