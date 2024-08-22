import LoginForm from '@/components/LoginForm'
import Head from 'next/head';

const LoginPage: React.FC = async () => {
  return (
  <>
    <Head key="page_title">
        <title>Login</title>
    </Head>
    <LoginForm />
  </>);
}

export default LoginPage