'use client' 

import { useForm, SubmitHandler } from 'react-hook-form';
import { useAuthentication } from '@/hooks/useAuthentication';
import { TextField, Button, Typography, Link, CircularProgress, Box, IconButton, Divider, FormControlLabel, Checkbox } from '@mui/material';
import { useRouter } from 'next/navigation';
import { Credentials } from '@/models/credentials';
import { useState } from 'react';
import { DarkMode, LightMode } from '@mui/icons-material';
import { useLightDarkModeSwitch } from '@/hooks/useLightDarkModeSwitch';
import { useMailboxes } from '@/hooks/useMailboxes';
import { useAppUserData } from '@/hooks/useAppUserData';
import useSWR from 'swr';
import { CheckboxElement, PasswordElement, TextFieldElement } from 'react-hook-form-mui';

const LoginForm: React.FC = () => {
  const { control, handleSubmit, watch } = useForm<Credentials>();
  const { login:loginService, loading } = useAuthentication();
  const { mode, toggleMode } = useLightDarkModeSwitch();
  const router = useRouter();
  const [ rememberMe, setRememberMe ] = useState(false);
  const { getMailboxList } = useMailboxes();
  const { getCurrentlyLoggedInUser } = useAppUserData();

  const {mutate:mutateUser} = useSWR('/api/AppUser/me',
                                      getCurrentlyLoggedInUser,
                                      { fallbackData:null });
  const {mutate: mutateMb} = useSWR('/api/MailBox',
                                    getMailboxList,
                                    { fallbackData:[] });
  const email = watch('email');

  const onSubmit: SubmitHandler<Credentials> = async (data) => {
      if ((data.twoFactorCode?.trim()?.length ?? 0) > 6){
        // Choses whether 2FA code is a recovery code if len>6
        data.twoFactorRecoveryCode = data.twoFactorCode;
        delete data.twoFactorCode;
      }
      if (await loginService(data, rememberMe)){
        //force SWR refresh on login
        mutateMb();
        mutateUser();
        router.push('/'); // Redirect to home after successful login
      }
  };

  return (
    <>
      <Box
        component="form"
        onSubmit={handleSubmit(onSubmit)}
        sx={{
          maxWidth: '400px',
          margin: '0 auto',
          padding: '2rem',
          display: 'flex',
          flexDirection: 'column',
          gap: '1rem',
        }}
      >
        <Typography variant="h3" component="h1" textAlign="center">
          Login
        </Typography>

        <TextFieldElement
          label="Email"
          name='email'
          control={control}
          required
          fullWidth
        />
        <PasswordElement
          label="Password"
          name='password'
          control={control}
          fullWidth
        />
        <TextFieldElement
          label="Two-Factor Code (if required)"
          control={control}
          name='twoFactorCode'
          helperText='Your 6 digit TOTP or a single use recovery code'
          fullWidth
        />

        <FormControlLabel
          control={
            <Checkbox
              checked={rememberMe}
              onChange={(e, c) => setRememberMe(c)}
            />}
          label="Remember Me"
          labelPlacement="end"
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
          {loading ? <CircularProgress size={24} color="inherit" /> : 'Login'}
        </Button>
        <Typography textAlign="center">
          {'Forgot Login? '}
          <Button variant='text' onClick={() => {router.push('/auth/forgotten' + (!!email ? '?email='+email ? ''));}}>
            Reset Password
          </Button>
        </Typography>
        <Divider />
        <Typography textAlign="center">
          {"Don't have an account? "}
          <Button onClick={() => {router.push('/auth/register');}}>
            Register
          </Button>
        </Typography>
      </Box>
      <IconButton onClick={toggleMode} sx={{
        position: 'fixed',
        bottom: 16, left: 16,
        zIndex: 1000, /* ensure it stays on top */}}>
        {mode === 'light' ? <LightMode /> : <DarkMode />}
      </IconButton>
    </>
  );
};

export default LoginForm;
