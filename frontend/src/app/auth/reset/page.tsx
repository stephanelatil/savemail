import ResetPasswordForm from '@/components/ResetPasswordForm';
import { Metadata } from 'next';
import { Suspense } from 'react';

export const metadata: Metadata = {
  title: 'Reset Password'
};

const ResetPasswordPage: React.FC = async () => {
  return  <Suspense>
            <ResetPasswordForm />
            </Suspense>;
};

export default ResetPasswordPage;