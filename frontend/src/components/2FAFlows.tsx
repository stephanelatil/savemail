'use client'

import { useAuthentication } from "@/hooks/useAuthentication";
import { ContentCopy } from "@mui/icons-material";
import { Box, Button, Container, IconButton, Modal, Paper, Skeleton, Stack, TextField, Tooltip, Typography, useTheme } from "@mui/material";
import { useQRCode } from "next-qrcode";
import { useCallback, useEffect, useState } from "react";
import { useForm } from "react-hook-form";

const DisableReset2FA: React.FC<{isLoading?:boolean, onDisable:()=>void}> = ({isLoading, onDisable}) => {
    if (isLoading)
        return <Box flexDirection='column'
                    alignItems='center'
                    sx={{   
                        width:'95%',
                        maxWidth: '600px',
                        margin: '0 auto',
                        padding: '2rem',
                        display: 'flex',
                        gap: '1rem'
                }}>
                    <Typography variant="h5" alignSelf='center'>
                        Two Factor Authentication
                    </Typography>
                    <Skeleton variant='text' width={'5rem'} height={40}/>
                    <Stack flexDirection='row'>
                        <Skeleton variant='rounded' animation='wave' sx={{mx:2}} width={'10rem'}/>
                        <Skeleton variant='rounded' animation='wave' sx={{mx:2}} width={'6rem'}/>
                    </Stack>
                </Box>;

    const { disable2FA, reset2FARecoveryCodes, loading } = useAuthentication();

    const disable = useCallback(()=>{
        async function disableAsync(){
            if (await disable2FA())
                onDisable();
        }
        disableAsync();
    }, []);
    
    return <Box flexDirection='column'
                alignItems='center'
                sx={{   
                    width:'95%',
                    maxWidth: '600px',
                    margin: '0 auto',
                    padding: '2rem',
                    display: 'flex',
                    gap: '1rem'
            }}>
                <Typography variant="h5">
                    Two Factor Authentication
                </Typography>
                <Typography color='success'>
                    Enabled!
                </Typography>
                <Stack flexDirection='row' letterSpacing={10}>
                    <Button onClick={reset2FARecoveryCodes}
                            disabled={loading}
                            color='warning'
                            variant="outlined"
                            sx={{mx:2}}>
                        Reset Recovery Codes
                    </Button>
                    <Button onClick={disable}
                            disabled={loading}
                            color='error'
                            variant='outlined'
                            sx={{mx:2}}>
                        Disable 2FA
                    </Button>
                </Stack>
            </Box>;
}

interface RecoveryCodesModalProps {
    codes?: string[];
    open: boolean;
    closeModal: () => void;
}

const RecoveryCodesModal: React.FC<RecoveryCodesModalProps> = ({
    codes,
    open,
    closeModal
}) => {
    const [showCopyText, setShowCopyText] = useState(0);
    const [showBeforeUnloadWarning, setShowBeforeUnloadWarning] = useState(true);

    // Prevent closing tab
    useEffect(() => {
        const handleBeforeUnload = (e: BeforeUnloadEvent) => {
            if (open && showBeforeUnloadWarning) {
                e.preventDefault();
            }
        };

        window.addEventListener('beforeunload', handleBeforeUnload);
        return () => window.removeEventListener('beforeunload', handleBeforeUnload);
    }, [open, showBeforeUnloadWarning]);

    // Handle copy to clipboard
    const handleCopy = useCallback(async () => {
        if (codes) {
            try {
                await navigator.clipboard.writeText(codes.join('\n'));
                setShowCopyText((prev) => prev+1);
                setTimeout(() => setShowCopyText(prev => prev > 0 ? prev-1 : 0), 2000);
            } catch (err) {
                console.error('Failed to copy:', err);
            }
        }
    }, [codes]);

    const handleFinalClose = () => {
        setShowCopyText(0);
        setShowBeforeUnloadWarning(false);
        closeModal();
    };

    return (
        <Modal
            open={open}
            onClose={()=>{}} //do not close on click outside the modal
            disableEscapeKeyDown
            aria-labelledby="recovery-codes-modal"
        >
            <Container maxWidth="sm" sx={{ mt: 4 }}>
                <Paper
                    elevation={3}
                    sx={{
                        p: 4,
                        display: 'flex',
                        flexDirection: 'column',
                        gap: 3
                    }}
                >
                    <Typography variant="h5" component="h2" color="error">
                        ⚠️ Important: Save Your Recovery Codes
                    </Typography>

                    <Typography>
                        These recovery codes will only be shown once. You must save them in a secure location to regain access to your account if you lose your 2FA device.
                    </Typography>

                    <Paper
                        elevation={1}
                        sx={{
                            p: 2,
                            backgroundColor: '#f5f5f5',
                            fontFamily: 'monospace'
                        }}
                    >
                        {codes?.map((code, index) => (
                            <Typography key={index} variant="body2">
                                {code}
                            </Typography>
                        )) ?? (
                            <Typography color="error">
                                Error getting recovery codes
                            </Typography>
                        )}
                    </Paper>

                    <Box sx={{ display: 'flex', justifyContent: 'center', position: 'relative' }}>
                        <Button
                            variant="contained"

                            startIcon={<ContentCopy />}
                            onClick={handleCopy}
                        >
                            {showCopyText > 0 ? 'Copied!' : 'Copy Codes'}
                        </Button>
                        {showCopyText > 0 && (
                            <Typography
                                variant="body2"
                                sx={{
                                    position: 'absolute',
                                    top: '50%',
                                    right: '-100px',
                                    transform: 'translateY(-50%)',
                                    color: 'success.main'
                                }}
                            >
                                Copied to clipboard!
                            </Typography>
                        )}
                    </Box>

                    <Button
                        onClick={handleFinalClose}
                        variant="contained"
                        color="warning"
                    >
                        I have saved my recovery codes
                    </Button>
                </Paper>
            </Container>
        </Modal>);
};

interface QRCodeData{
    secret: string
};

const TwoFA_QRCode: React.FC<QRCodeData> = ({secret}) => {
    const { SVG } = useQRCode();
    const theme = useTheme();
    const [copied, setCopied] = useState(false);
    
    const handleCopy = () => {
        navigator.clipboard.writeText(secret);
        setCopied(true);
        setTimeout(() => setCopied(false), 2000);
      };

    return <Container sx={{gap:'1rem'}}>
                <Typography variant="body1" textAlign="center">
                    Scan the QRCode with your Authenticator App and submit the application code it gives you.
                </Typography>
                <SVG 
                    text={'otpauth://totp/Savemail?issuer=Savemail&secret='+secret}
                    options={{

                    }}
                />
                <br/>
                <Typography>
                    Or enter the secret manually
                </Typography>
                <br/>
                <Paper elevation={3} sx={{p:1, paddingTop:2}}>
                    <Stack flexDirection='row'>
                        <Typography fontFamily='monospace' variant="body1">
                            {secret}
                        </Typography>
                        <Tooltip
                                title={"Copied!"}
                                onClose={() => setCopied(false)}
                                open={copied}
                                disableFocusListener
                                disableHoverListener
                                arrow>
                            <IconButton onClick={handleCopy} sx={{marginTop:-1}}>
                                <ContentCopy/>
                            </IconButton>
                        </Tooltip>
                    </Stack>
                </Paper>
            </Container>;
};

interface TOTPFormData {
    totp:string
};

const Enable2FAFlow: React.FC<{isLoading?:boolean, onEnable:()=>void}> = ({isLoading, onEnable}) => {
    const [ secretKey, setSecretKey ] = useState<string>('');
    const [ recoveryCodes, setRecoveryCodes ] = useState<string[]>([]);
    const [ modalOpen, setModalOpen ] = useState<boolean>(false);
    const { loading, init2FA, enable2FA } = useAuthentication();
    const { register, handleSubmit, formState: { errors } } = useForm<TOTPFormData>();

    function on2FAInit(){
        let startFlow = async () =>{
            let result = await init2FA();
            setSecretKey(result.sharedKey);
        }
        startFlow();
    }

    function closeModal(){
        onEnable();
        setModalOpen(false);
    }

    async function Enable2FAFlow(data:{totp:string}) {
        let result = await enable2FA(data.totp);
        setModalOpen(result.isTwoFactorEnabled);
        setRecoveryCodes(result.recoveryCodes);
    }

    if (isLoading)
        return <Skeleton/>;

    return (
        <Box flexDirection='column'
             alignItems='center'
             sx={{   
                width:'95%',
                maxWidth: '600px',
                margin: '0 auto',
                padding: '2rem',
                display: 'flex',
        }}>
            <Typography variant="h5">
                Two Factor Authentication
            </Typography>
            {  !secretKey 
                ? <Button onClick={on2FAInit}
                    disabled={loading}
                    color='success'
                    variant='outlined'
                    sx={{mx:2, my:3}}>
                    Enable 2FA
                </Button>
                : <>
                    <Box component="form"
                         sx={{
                                margin: '0 auto',
                                padding: '2rem',
                                display: 'flex',
                                flexDirection: 'column',
                                gap: '1rem'
                         }}
                         onSubmit={handleSubmit(Enable2FAFlow)}>

                        <TwoFA_QRCode secret={secretKey}/>

                        <TextField
                            label="One Time Code"
                            defaultValue={''}
                            {...register('totp', { 
                                required: 'The code is required',
                                minLength: 6,
                                pattern: {
                                value: /^[0-9]+$/g,
                                message: "Invalid code. It should only contain numbers"
                                }
                            })}
                            error={!!errors.totp}
                            helperText={errors.totp?.message}
                            fullWidth
                        />
                
                        <Button
                            type="submit"
                            variant="contained"
                            color="primary"
                            fullWidth
                            disabled={loading || modalOpen}
                            aria-busy={loading || modalOpen}
                            sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}
                        >
                            Submit and Enable 2FA
                        </Button>
                    </Box>
                    <RecoveryCodesModal open={modalOpen} codes={recoveryCodes} closeModal={closeModal}/>
                </>
            }
        </Box>
    );
}

export interface TwoFAInfo{
    twoFAEnabled?:boolean,
    isLoading?:boolean
}

const TwoFactorAuthFlows: React.FC<TwoFAInfo> = ({isLoading, twoFAEnabled}) => {
    const [ enabled, setEnabled ] = useState<boolean>(!!twoFAEnabled);

    return (
    <Container >
        {enabled ? <DisableReset2FA isLoading={!!isLoading} onDisable={() => {setEnabled(false);}}/>
                 : <Enable2FAFlow isLoading={!!isLoading} onEnable={() => {setEnabled(true);}}/>}
    </Container>);
}

export default TwoFactorAuthFlows;