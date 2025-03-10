'use client'

import { useRouter, useSearchParams } from 'next/navigation'
import { Box, Button, Link, Typography } from "@mui/material";

const AfterRegisterInfo:React.FC = () => {
    const params = useSearchParams();
    const router = useRouter();

    return (
    <Box sx={{
      maxWidth: '400px',
      margin: '0 auto',
      padding: '2rem',
      display: 'flex',
      flexDirection: 'column',
      gap: '1rem'
    }}>
      <Typography variant='h3' textAlign='center'>
        Register successful
      </Typography>
      <Typography sx={{py:5}}>
        If email verification is enabled, verification email sent to <i>{params.get('email')}</i>
        <br/>
        <br/>
        Check your email and confirm your email address. Then go to the login page
      </Typography>
      <Button onClick={() => router.push('/auth/login')} variant="text">
        <Link underline="hover" variant='h6'>
          Log In Here!
        </Link>
      </Button>
    </Box>);
  }

export default AfterRegisterInfo;