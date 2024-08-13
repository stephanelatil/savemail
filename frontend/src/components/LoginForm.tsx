'use client' 

import Head from 'next/head';
import { useForm, SubmitHandler } from 'react-hook-form';
import { useAuthentication } from '@/hooks/useAuthentication';
import { TextField, Button, Typography, Link, CircularProgress, Box, IconButton } from '@mui/material';
import { useRouter } from 'next/navigation';
import { Credentials } from '@/models/credentials';
import { useState } from 'react';
import { REGISTER_URL } from '@/constants/NavRoutes';
import { DarkMode, LightMode } from '@mui/icons-material';
import { useLightDarkModeSwitch } from '@/hooks/useLightDarkModeSwitch';

const LoginForm: React.FC = () => {
  const { register, handleSubmit, formState: { errors } } = useForm<Credentials>();
  const { login:loginService, loading } = useAuthentication();
  const { mode, toggleMode } = useLightDarkModeSwitch();
  const router = useRouter();
  const [errorText, setErrorText] = useState("");

  const onSubmit: SubmitHandler<Credentials> = async (data) => {
    try {
      if (await loginService(data))
        router.push('/'); // Redirect to home after successful login
    } catch (err) {
      console.error('Login failed:', err);
      if (err instanceof Error)
        setErrorText(err.message);
    }
  };

  return (
    <>
      <Head key="page_title">
        <title>Login</title>
      </Head>
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
          Don't have an account?{' '}
          <Link href={REGISTER_URL} underline="hover">
            Register
          </Link>
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