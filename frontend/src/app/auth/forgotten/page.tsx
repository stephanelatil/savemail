import ForgotPasswordForm from '@/components/ForgotPasswordForm'
import { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Forgot Password'
}

const ForgotPasswordPage: React.FC = async () => {
  return <ForgotPasswordForm />;
}

export default ForgotPasswordPage