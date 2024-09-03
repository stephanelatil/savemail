import LoginForm from '@/components/LoginForm'
import { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Login'
}

const LoginPage: React.FC = async () => {
  return <LoginForm />;
}

export default LoginPage