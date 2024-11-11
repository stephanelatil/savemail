'use client'

import { useSearchParams } from 'next/navigation'
import { Box, Link, Typography } from "@mui/material";

const AfterRegisterInfo:React.FC = () => {
    const params = useSearchParams();

    return (
    <Box sx={{
      maxWidth: '400px',
      margin: '0 auto',
      padding: '2rem',
      display: 'flex',
      flexDirection: 'column',
      gap: '1rem'
    }}>
      <Typography variant='h3' component='h1' textAlign='center'>
        Register successful
      </Typography>
      <Typography sx={{py:5}}>
        If email verification is enabled, verification email sent to <i>{params.get('email')}</i>
        
        Check your email and confirm your email address. Then go to the login page
      </Typography>
      <Typography textAlign="center" variant='h4'>
        <Link href={'/auth/login'}  underline="hover">
          Log In Here!
        </Link>
      </Typography>
    </Box>);
  }

export default AfterRegisterInfo;