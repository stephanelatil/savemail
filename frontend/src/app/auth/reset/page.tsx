import ResetPasswordForm from '@/components/ResetPasswordForm';
import { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Reset Password'
};

const ResetPasswordPage: React.FC = async () => {
  return <ResetPasswordForm />
};

export default ResetPasswordPage;