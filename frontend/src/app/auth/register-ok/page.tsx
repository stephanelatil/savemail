import AfterRegisterInfo from '@/components/AfterRegisterInfo'
import { Metadata } from 'next'

export const metadata:Metadata = {
  title:'Register successful'
}

const RegisterOk: React.FC = async () => {
  return <AfterRegisterInfo />
}

export default RegisterOk