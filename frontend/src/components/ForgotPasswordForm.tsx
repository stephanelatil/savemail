'use client'

import { useForm, SubmitHandler } from 'react-hook-form';
import { TextField, Button, Typography, Link, CircularProgress, Box, Divider, IconButton } from '@mui/material';
import { useRouter, useSearchParams } from 'next/navigation';
import { useState } from 'react';
import { useAuthentication } from '@/hooks/useAuthentication';
import { useLightDarkModeSwitch } from '@/hooks/useLightDarkModeSwitch';
import { DarkMode, LightMode } from '@mui/icons-material';
import { TextFieldElement } from 'react-hook-form-mui';

interface ForgotPasswordFormData {
  email: string;
}

const ForgotPasswordForm: React.FC = () => {
  const { control, handleSubmit } = useForm<ForgotPasswordFormData>();
  const { sendPasswordReset, loading } = useAuthentication();
  const router = useRouter();
  const searchParams = useSearchParams();
  const { mode, toggleMode } = useLightDarkModeSwitch();

  // Get email from URL if it was passed from login page
  const defaultEmail = searchParams.get('email') || '';

  const onSubmit: SubmitHandler<ForgotPasswordFormData> = async (data) => {
      const success = await sendPasswordReset(data.email);
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
        <Typography variant="h3" textAlign="center">
          Forgot Password
        </Typography>
        
        <span/>

        <Typography variant="body1" textAlign="center">
          Enter your email address and we'll send you instructions to reset your password.
        </Typography>
        <Typography variant='body2' textAlign='center' color='warning'>
          This will only work if you have confirmed your email address!
        </Typography>
        
        <TextFieldElement
          name='email'
          label="Email"
          control={control}
          defaultValue={defaultEmail}
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
          {loading ? <CircularProgress size={24} color="inherit" /> : 'Reset Password'}
        </Button>

        <Divider variant='middle'/>

        <Typography textAlign="center">
          {"Remember your password? "}
          <Button onClick={()=>{router.push('/auth/login');}}>
            Back to Login
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

export default ForgotPasswordForm;
