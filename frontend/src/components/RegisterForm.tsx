'use client' 

import { useForm, SubmitHandler } from 'react-hook-form';
import { useAuthentication } from '@/hooks/useAuthentication';
import { TextField, Button, Typography, Link, CircularProgress, Box, IconButton } from '@mui/material';
import { useRouter } from 'next/navigation';
import { Credentials } from '@/models/credentials';
import { useState } from 'react';
import { DarkMode, LightMode } from '@mui/icons-material';
import { useLightDarkModeSwitch } from '@/hooks/useLightDarkModeSwitch';
import { PasswordElement, PasswordRepeatElement, TextFieldElement } from 'react-hook-form-mui';

const RegisterForm: React.FC = () => {
  const { control, handleSubmit } = useForm<Credentials>();
  const { register, loading } = useAuthentication();
  const { mode, toggleMode } = useLightDarkModeSwitch();
  const router = useRouter();

  const onSubmit: SubmitHandler<Credentials> = async (data) => {
    if (await register(data)){
      const url = new URL('/auth/register-ok')
      url.searchParams.set('email', data.email);
      router.push(url.toString()); //action to run after register is successful
    }
  };

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
        <Typography variant="h3" textAlign="center">
          Register
        </Typography>
        
        <TextFieldElement
          label="Email"
          name='email'
          control={control}
          required
          fullWidth
        />
        <PasswordElement
          control={control}
          label="Password"
          name='password'
          rules={{minLength: {value: 6, message: "Password must have at least 6 characters"},
                  required:{value: true, message: 'Password is required'}}}
          fullWidth
        />
        <PasswordRepeatElement
          label="Confirm Password"
          passwordFieldName='password'
          control={control}
          name='passwordRepeat'
          required
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
          {loading ? <CircularProgress size={24} color="inherit" /> : 'Register'}
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
