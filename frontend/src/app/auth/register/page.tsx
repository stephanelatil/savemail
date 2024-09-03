import RegisterForm from '@/components/RegisterForm';
import { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Register'
}

const RegisterPage: React.FC = async () => {
  return <RegisterForm />
}

export default RegisterPage