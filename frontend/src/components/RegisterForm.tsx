'use client' 

import { useForm, SubmitHandler } from 'react-hook-form';
import { useAuthentication } from '@/hooks/useAuthentication';
import { TextField, Button, Typography, Link, CircularProgress, Box, IconButton } from '@mui/material';
import { useRouter } from 'next/navigation';
import { Credentials } from '@/models/credentials';
import { useState } from 'react';
import { DarkMode, LightMode } from '@mui/icons-material';
import { useLightDarkModeSwitch } from '@/hooks/useLightDarkModeSwitch';
import { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Register'
}

const RegisterForm: React.FC<{registerSuccess: (email:string)=>void}> = ({registerSuccess}) => {
  const { register, handleSubmit, formState: { errors }, watch } = useForm<Credentials>();
  const { register:registerService, loading } = useAuthentication();
  const { mode, toggleMode } = useLightDarkModeSwitch();
  const router = useRouter();
  const [errorText, setErrorText] = useState("");

  const onSubmit: SubmitHandler<Credentials> = async (data) => {
    try {
      if (await registerService(data)){
        const url = new URL('/auth/register-ok')
        url.searchParams.set('email', data.email);
        router.push(url.toString()); //action to run after register is successful
      }
    } catch (err) {
      console.error('Register failed:', err);
      if (err instanceof Error)
        setErrorText(err.message);
    }
  };
  var passwd = watch('password');

  return (
      <Box
        component="form"
        onSubmit={handleSubmit(onSubmit)}
        sx={{
          maxWidth: '400px',
          margin: '0 auto',
          padding: '2rem',
          display: 'flex',
          flexDirection: 'column',
          gap: '1rem'
        }}
      >
        <Typography variant="h3" component="h1" textAlign="center">
          Register
        </Typography>
        <Typography variant='h6' textAlign="left">
          {errorText}
        </Typography>
        <TextField
          label="Email"
          {...register('email', { required: {value:true, message:'Email is required'} })}
          error={!!errors.email}
          helperText={errors.email?.message}
          fullWidth
        />
        <TextField
          label="Password"
          type="password"
          {...register('password', { required: {value: true, message: 'Password is required'},
                                     minLength: {value: 6, message: "Password must have at least 6 characters"}
                                     
                                   })}
          error={!!errors.password}
          helperText={errors.password?.message}
          fullWidth
        />
        <TextField
          label="Confirm Password"
          type="password"
          {...register('passwordRepeat', { required: {value: true, message: 'Password is required'},
                                           validate: cp => passwd === cp || 'Passwords do not match!'})}
          error={!!errors.passwordRepeat}
          helperText={errors.passwordRepeat?.message}
          fullWidth
        />
        <Button
          type="submit"
          variant="contained"
          color="primary"
          fullWidth
          disabled={loading}
          sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}
          aria-busy={loading}
        >
          {loading ? <CircularProgress size={24} color="inherit" /> : 'Login'}
        </Button>
        <Typography textAlign="center">
          Already logged in?{' '}
          <Link href={'/auth/login'} underline="hover">
            Log In
          </Link>
        </Typography>
        <IconButton onClick={toggleMode} sx={{
          position: 'fixed',
          bottom: 16, left: 16,
          zIndex: 1000, /* ensure it stays on top */}}>
          {mode === 'light' ? <LightMode /> : <DarkMode />}
        </IconButton>
      </Box>);
};

export default RegisterForm;
