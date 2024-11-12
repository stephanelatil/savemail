'use client'

import { useMailboxes } from "@/hooks/useMailboxes";
import { EditMailBox, ImapProvider, MailBox } from "@/models/mailBox";
import { Google } from "@mui/icons-material";
import { Box, Button, CircularProgress, Divider, Skeleton, TextField, Typography } from "@mui/material";
import { useParams, useRouter } from "next/navigation";
import { SubmitHandler, useForm } from "react-hook-form";
import useSWR from "swr";
import NotFound from "./NotFound";
import { PasswordElement, TextFieldElement } from "react-hook-form-mui";


const EditMailboxFormBase:React.FC<{defaultValues:MailBox}> = ({defaultValues}) =>{
    const params = useParams();
    const mailboxPageId = parseInt(params.id as string ?? '0', 10);

    const { loading, editMailBox, synchronizeMailbox } = useMailboxes();
    const { control, handleSubmit } = useForm<EditMailBox>({defaultValues:defaultValues});
    const provider = defaultValues?.provider;

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
                <Typography variant='h5' textAlign="center">
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
                label="Username (Email address)"
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
                            return <Button href={`${process.env.NEXT_PUBLIC_BACKEND_URL}/oauth/google/login/${mailboxPageId}`}
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
                defaultValue={defaultValues.imapDomain}
                control={control}
                required
                fullWidth
            />
            <TextFieldElement
                disabled={provider !== ImapProvider.Simple}
                label="Imap Port"
                type='number'
                name='imapPort'
                defaultValue={defaultValues.imapPort}
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
            <Divider  sx={{my:'2em'}}/>
            <Button 
                disabled={loading}
                variant="outlined"
                size='small'
                color='info'
                onClick={async (e) => {await synchronizeMailbox(mailboxPageId)}}
            >
                Synchronize mailbox now
            </Button>
        </Box>);
}

const EditMailboxForm: React.FC = () => {
    const params = useParams();
    const mailboxPageId = parseInt(params.id as string ?? '0', 10);
    const { getMailbox } = useMailboxes();
    const {data, isLoading} = useSWR('/api/MailBox/'+mailboxPageId, () =>getMailbox(mailboxPageId));

    return isLoading
                ? <Skeleton />
                : !!data ? <EditMailboxFormBase defaultValues={data}/>
                         : <NotFound />
}

export default EditMailboxForm;