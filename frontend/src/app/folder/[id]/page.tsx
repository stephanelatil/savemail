import MailListPage from '@/components/MailList';
import { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'View Emails'
}

const MailsInFolder:React.FC = () =>{
        return (<>
            <MailListPage />
        </>);
}

export default MailsInFolder;