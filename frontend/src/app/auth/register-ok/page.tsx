import AfterRegisterInfo from '@/components/AfterRegisterInfo'
import { Box, Skeleton } from '@mui/material'
import { Metadata } from 'next'
import { Suspense } from 'react'

export const metadata:Metadata = {
  title:'Register successful'
}

const RegistrationSkeleton = () => {
  return (
    <Box
      sx={{
        maxWidth: '400px',
        margin: '0 auto',
        padding: '2rem',
        display: 'flex',
        flexDirection: 'column',
        gap: '1rem'
      }}
    >
      <Skeleton variant="text" width="60%" height={40} sx={{ mx: 'auto' }} />

      <Skeleton variant="rectangular" width="100%" height={80} sx={{ py: 5 }} />

      <Skeleton variant="text" width="40%" height={30} sx={{ mx: 'auto' }} />
    </Box>
  );
}


const RegisterOk: React.FC = async () => {

  return (<Suspense fallback={<RegistrationSkeleton />}>
            <AfterRegisterInfo />
          </Suspense>);
}

export default RegisterOk