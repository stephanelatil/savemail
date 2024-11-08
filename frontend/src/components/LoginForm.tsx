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

const LoginForm: React.FC = () => {
  const { register, watch, handleSubmit, formState: { errors } } = useForm<Credentials>();
  const { login:loginService, loading } = useAuthentication();
  const { mode, toggleMode } = useLightDarkModeSwitch();
  const router = useRouter();
  const [errorText, setErrorText] = useState("");
  const [ rememberMe, setRememberMe ] = useState(false);
  const email = watch('email', '');
  const { getMailboxList } = useMailboxes();
  const { getCurrentlyLoggedInUser } = useAppUserData();

  const {mutate:mutateUser} = useSWR('/api/AppUser/me',
                                      getCurrentlyLoggedInUser,
                                      { fallbackData:null });
  const {mutate: mutateMb} = useSWR('/api/MailBox',
                                    getMailboxList,
                                    { fallbackData:[] });

  const onSubmit: SubmitHandler<Credentials> = async (data) => {
    try {
      if (await loginService(data, rememberMe)){
        //force SWR refresh on login
        mutateMb();
        mutateUser();
        router.push('/'); // Redirect to home after successful login
      }
    } catch (err) {
      console.error('Login failed:', err);
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
        <Typography variant='h6' textAlign="left">
          {errorText}
        </Typography>
        <TextField
          label="Email"
          {...register('email', { required: 'Username is required' })}
          error={!!errors.email}
          helperText={errors.email?.message}
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
        <TextField
          label="Two-Factor Code (if required)"
          {...register('twoFactorCode')}
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
          <Button variant='text' onClick={() => {router.push('/auth/forgotten?email='+email);}}>
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
