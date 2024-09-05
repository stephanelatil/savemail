import { Metadata } from 'next';
import NewMailboxForm from "@/components/NewMailboxForm";

export const metadata: Metadata = {
    title: 'New Mailbox'
}

const NewMailboxPage:React.FC = () =>{
    return <NewMailboxForm />
}

export default NewMailboxPage;