'use client'

import { useForm, SubmitHandler } from 'react-hook-form';
import { TextField, Button, Typography, Link, CircularProgress, Box } from '@mui/material';
import { useRouter, useSearchParams } from 'next/navigation';
import { useState } from 'react';
import { useAuthentication } from '@/hooks/useAuthentication';

interface ForgotPasswordFormData {
  email: string;
}

const ForgotPasswordForm: React.FC = () => {
  const { register, handleSubmit, formState: { errors } } = useForm<ForgotPasswordFormData>();
  const { sendPasswordReset, loading } = useAuthentication();
  const router = useRouter();
  const searchParams = useSearchParams();
  const [successMessage, setSuccessMessage] = useState("");
  const [errorText, setErrorText] = useState("");

  // Get email from URL if it was passed from login page
  const defaultEmail = searchParams.get('email') || '';

  const onSubmit: SubmitHandler<ForgotPasswordFormData> = async (data) => {
    try {
      const success = await sendPasswordReset(data.email);
      if (success) {
        setSuccessMessage("A Password reset link has been sent to your email.");
        setErrorText("");
      }
    } catch (err) {
      console.error('Password reset request failed:', err);
      if (err instanceof Error) {
        setErrorText(err.message);
        setSuccessMessage("");
      }
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
        gap: '1rem',
      }}
    >
      <Typography variant="h3" component="h1" textAlign="center">
        Forgot Password
      </Typography>
      
      {successMessage && (
        <Typography variant="body1" color="success.main" textAlign="center">
          {successMessage}
        </Typography>
      )}
      
      {errorText && (
        <Typography variant="body1" color="error.main" textAlign="center">
          {errorText}
        </Typography>
      )}

      <Typography variant="body1" textAlign="center">
        Enter your email address and we'll send you instructions to reset your password.
      </Typography>

      <TextField
        label="Email"
        defaultValue={defaultEmail}
        {...register('email', { 
          required: 'Email is required',
          pattern: {
            value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
            message: "Invalid email address"
          }
        })}
        error={!!errors.email}
        helperText={errors.email?.message}
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

      <Typography textAlign="center">
        {"Remember your password? "}
        <Button onClick={()=>{router.push('/auth/login');}}>
          Back to Login
        </Button>
      </Typography>
    </Box>
  );
};

export default ForgotPasswordForm;
