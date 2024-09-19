import MailListPage from '@/components/MailList';
import NotFound from '@/components/NotFound';

const MailsInFolder:React.FC = () =>{
    try{
        return <MailListPage />;
    }catch{}
    return <NotFound />
}

export default MailsInFolder;