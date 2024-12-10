import ForgotPasswordForm from '@/components/ForgotPasswordForm'
import { Metadata } from 'next';
import { Suspense } from 'react';

export const metadata: Metadata = {
  title: 'Forgot Password'
}

const ForgotPasswordPage: React.FC = async () => {
  return  <Suspense>
            <ForgotPasswordForm />
          </Suspense>;
}

export default ForgotPasswordPage