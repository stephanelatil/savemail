'use client' 

import Head from 'next/head';
import { useForm, SubmitHandler } from 'react-hook-form';
import { useAuthentication } from '@/hooks/useAuthentication';
import { TextField, Button, Typography, Link, CircularProgress, Box } from '@mui/material';
import { useRouter } from 'next/navigation';
import { Credentials } from '@/models/credentials';
import { useState } from 'react';
import { useNotification } from '@/hooks/useNotification';

const RegisterForm: React.FC = () => {
  const { register, handleSubmit, formState: { errors }, watch } = useForm<Credentials>();
  const { register:registerService, loading } = useAuthentication();
  const router = useRouter();
  const [errorText, setErrorText] = useState("");

  const onSubmit: SubmitHandler<Credentials> = async (data) => {
    try {
      if (await registerService(data))
        router.push('/auth/login'); // Redirect to home after successful register
    } catch (err) {
      console.error('Register failed:', err);
      if (err instanceof Error)
        setErrorText(err.message);
    }
  };
  var passwd = watch('password');

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
        >
          {loading ? <CircularProgress size={24} color="inherit" /> : 'Login'}
        </Button>
        <Typography textAlign="center">
          Already logged in?{' '}
          <Link href="/auth/login" underline="hover">
            Log In
          </Link>
        </Typography>
      </Box>
    </>
  );
};

export default RegisterForm;
