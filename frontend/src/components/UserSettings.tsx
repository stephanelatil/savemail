'use client'

import { useAppUserData } from "@/hooks/useAppUserData";
import { useAuthentication } from "@/hooks/useAuthentication";
import { useNotification } from "@/hooks/useNotification";
import { AppUser, EditAppUser } from "@/models/appUser";
import { Box, Button, CircularProgress, Container, Divider, Skeleton, Stack, TextField, Typography } from "@mui/material";
import { SubmitHandler, useForm } from "react-hook-form";
import useSWR from "swr";
import TwoFactorAuthFlows from "./2FAFlows";
import { BorderedTypography } from "./UtilComponents";
import React from "react";
import { PasswordElement, PasswordRepeatElement, TextFieldElement } from "react-hook-form-mui";
import { ChangePassword } from "@/models/credentials";
import { changeAccountEmail } from "@/services/authenticationService";

const SkeletonEditNames: React.FC = () =>{
    return (<Box sx={{
                width:'95%',
                maxWidth: '600px',
                margin: '0 auto',
                padding: '2rem',
                display: 'flex',
                flexDirection: 'column',
                gap: '1rem'
            }}>
                <Typography variant="h3" component="h1" textAlign="center">
                    Edit Name
                </Typography>

                {/* First Name Field Skeleton */}
                <Skeleton 
                    variant="rectangular" 
                    height={56} 
                    sx={{ borderRadius: 1 }} 
                />
                <Skeleton 
                    width="150px" 
                    height={20} 
                    sx={{ mt: 0.5 }} 
                />
                <Skeleton 
                    variant="rectangular" 
                    height={56} 
                    sx={{ borderRadius: 1 }} 
                />
                <Skeleton 
                    width="150px" 
                    height={20} 
                    sx={{ mt: 0.5 }} 
                />

                <Skeleton 
                    variant="rectangular" 
                    height={36} 
                    sx={{ borderRadius: 1 }} 
                />
            </Box>);
}
interface UserLoadingData {
    user?:AppUser,
    isLoading?:boolean
};

interface UserEdit {
    invalidateCache?:()=>void
};

const EditNames: React.FC<UserEdit & UserLoadingData> = ({user, isLoading, invalidateCache}) =>{
    const { editUser, loading } = useAppUserData();
    const { register, handleSubmit, formState: { errors } } = useForm<EditAppUser>(
        {
            defaultValues: {id:user?.id, firstName:user?.firstName, lastName:user?.lastName}
        }
    );
    const {showNotification} = useNotification();
    
    const onSubmit: SubmitHandler<EditAppUser> = async (u) => {
        if (await editUser(u))
        {
            showNotification("Successfully edited user", 'success');
            !!invalidateCache && invalidateCache();
        }
    };

    return (
        isLoading ? <SkeletonEditNames /> :
        <Box    onSubmit={handleSubmit(onSubmit)}
                component="form"
                sx={{
                    width:'95%',
                maxWidth: '600px',
                margin: '0 auto',
                padding: '2rem',
                display: 'flex',
                flexDirection: 'column',
                gap: '1rem'
            }}>
            <Typography variant="h5" textAlign="center">
                Edit Name
            </Typography>
            <TextField
                label="First Name"
                defaultValue={user?.firstName ?? ""}
                disabled={isLoading}
                {...register('firstName', { minLength:0 })}
                error={!!errors.firstName}
                helperText={errors.firstName?.message}
                fullWidth
            />
            <TextField
                label="Last Name"
                defaultValue={user?.lastName ?? ""}
                disabled={isLoading}
                {...register('lastName', { minLength:1,  })}
                error={!!errors.lastName}
                helperText={errors.lastName?.message}
                fullWidth
            />
            <Button
                type="submit"
                variant="contained"
                color="primary"
                fullWidth
                disabled={ loading|| isLoading }
                aria-busy={ loading || isLoading }
                sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}
                >
                {loading ? <CircularProgress size={24} color="inherit" /> : "Update"}
            </Button>
        </Box>);
}

//Add 2FA, password reset and email change/confirm
const ConfirmEmail: React.FC<UserLoadingData> = ({user, isLoading}) => {
    const {showNotification} = useNotification();
    const { resendConfirmationEmail, loading } = useAuthentication();
    
    if (isLoading)
        return <Box flexDirection='row'
                    sx={{
                        width: '95%',
                        maxWidth: '600px',
                        margin: '0 auto',
                        padding: '2rem',
                        display: 'flex',
                        gap: '1rem'
                    }}
                >
                    <Skeleton variant="text" width="50%" />
                    <Skeleton variant="text" width="30%" />
                </Box>;

    const resendEmailConfirmation = () => {
        async function resend(){
            await resendConfirmationEmail(user?.email??'');
            showNotification("A new confirmation email has been sent!", 'success');
        }
        resend();
    }

    return (<Box flexDirection='column'
                sx={{   
                    width:'95%',
                    maxWidth: '600px',
                    margin: '0 auto',
                    padding: '2rem',
                    display: 'flex',
                    gap: '1rem',
                    alignItems:'center'
            }}>
                <Stack flexDirection='row'
                        sx={{
                            justifyContent:'space-between',
                            margin: '0 auto',
                            display: 'flex',
                            gap: '1rem',
                            alignItems:'center'
                }}>
                    <Typography variant="h5" alignContent='right'>
                        Email Confirmation
                    </Typography>
                    {!user?.emailConfirmed ?
                        <BorderedTypography variant='body1'
                                            color='warning'
                                            maxWidth='12rem'>
                            Not Confirmed
                        </BorderedTypography> 
                        : 
                        <BorderedTypography variant='body1'
                                            color='success'
                                            align="right">
                            Confirmed
                        </BorderedTypography>
                    }
                </Stack>
                {!user?.emailConfirmed &&
                        <Button onClick={resendEmailConfirmation}
                                disabled={loading}
                                sx={{alignContent:'center'}}
                                variant="outlined">
                            {loading ? <CircularProgress size={20}/> : "Resend Email Confirmation"}
                        </Button>
                }
            </Box>);
}

const EditEmail: React.FC<UserEdit & UserLoadingData> = ({user, isLoading, invalidateCache}) => {
    const { loading, changeAccountEmail } = useAuthentication();
    const { control, handleSubmit } = useForm<{email:string}>({defaultValues:{email:user?.email}});

    async function onSubmit(data:{email:string}){
        if(await changeAccountEmail(data.email))
            !!invalidateCache && invalidateCache();
    }

    if (isLoading)
        return <Box flexDirection="column"
                    sx={{
                    width: '95%',
                    maxWidth: '600px',
                    margin: '0 auto',
                    padding: '2rem',
                    display: 'flex',
                    gap: '1rem'
                    }}
                    component='form'
                >
                    <Typography variant='h5' alignSelf='center'>
                        Change Email
                    </Typography>
                    <Skeleton variant="text" height={50}/>
                    <Skeleton variant="rounded" animation='wave' height={40}/>
                </Box>;

    return <Box flexDirection="column"
                sx={{
                width: '95%',
                maxWidth: '600px',
                margin: '0 auto',
                padding: '2rem',
                display: 'flex',
                gap: '1rem'
                }}
                component='form'
                onSubmit={handleSubmit(onSubmit)}
            >
                <Typography variant='h5' alignSelf='center'>
                    Change Email
                </Typography>
                <TextFieldElement 
                    control={control}
                    label='New Email'
                    name='email'
                    required
                    fullWidth
                />
                <Button
                    disabled={loading}
                    variant='contained'
                    sx={{maxWidth:'100%', minWidth:'200px'}}
                >
                    {loading ? <CircularProgress size={24}/> : "Edit Email"}
                </Button>
            </Box>;
}

const EditPassword: React.FC<{isLoading:boolean}> = ({isLoading}) => {
    const { loading, changePassword } = useAuthentication();
    const { control, handleSubmit } = useForm<ChangePassword&{newPasswordRepeat:string}>();

    if (isLoading)
        return <Box flexDirection="column"
                    sx={{
                    width: '95%',
                    maxWidth: '600px',
                    margin: '0 auto',
                    padding: '2rem',
                    display: 'flex',
                    gap: '1rem'
                    }}
                    component='form'
                >
                    <Typography variant='h5' alignSelf='center'>
                        Change Password
                    </Typography>
                    <Skeleton variant="text" height={50}/>
                    <Skeleton variant="text" height={50}/>
                    <Skeleton variant="text" height={50}/>
                    <Skeleton variant="rounded" animation='wave' height={40}/>
                </Box>;

    return <Box flexDirection="column"
                sx={{
                width: '95%',
                maxWidth: '600px',
                margin: '0 auto',
                padding: '2rem',
                display: 'flex',
                gap: '1rem'
                }}
                component='form'
                onSubmit={handleSubmit(changePassword)}
            >
                <Typography variant='h5' alignSelf='center'>
                    Change Password
                </Typography>
                <PasswordElement
                    control={control}
                    label='Old Password'
                    name='oldPassword'
                    required
                    fullWidth
                />
                <PasswordElement
                    control={control}
                    label='New Password'
                    name='newPassword'
                    required
                    fullWidth
                />
                <PasswordRepeatElement
                    control={control}
                    label='Confirm New Password'
                    name='newPasswordRepeat'
                    passwordFieldName='newPassword'
                    required
                    fullWidth
                />
                <Button
                    disabled={loading}
                    variant='contained'
                    sx={{maxWidth:'100%', minWidth:'200px'}}
                >
                    {loading ? <CircularProgress size={24}/> : "Change Password"}
                </Button>
            </Box>;
}

const UserSettings: React.FC = () => {
    const { getCurrentlyLoggedInUser } = useAppUserData();
    const { mutate, data:user, isLoading } = useSWR('/api/AppUser/me',
                                                    getCurrentlyLoggedInUser,
                                                    { fallbackData:{} as AppUser });
    return (
        <Container
            maxWidth='sm'
            sx={{marginTop:'2rem'}}
        >
            <Typography variant="h3" align="center" marginBlockEnd={2}>
                User Settings
            </Typography>
            <Divider variant='middle'/>
            <ConfirmEmail user={user ?? undefined} isLoading={isLoading}/>
            <Divider variant='middle'/>
            <EditNames user={user?? undefined} isLoading={isLoading} invalidateCache={() => mutate()}/>
            <Divider variant='middle'/>
            <EditPassword isLoading={isLoading}/>
            <Divider variant="middle"/>
            <EditEmail user={user?? undefined} isLoading={isLoading} invalidateCache={() => mutate()}/>
            <Divider variant="middle"/>
            <TwoFactorAuthFlows twoFAEnabled={!!user?.twoFactorEnabled} isLoading={isLoading}/>
        </Container>
    );
}

export default UserSettings;