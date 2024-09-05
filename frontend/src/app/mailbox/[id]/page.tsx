import { Metadata } from 'next';
import EditMailboxForm from "@/components/EditMailboxForm";

export const metadata: Metadata = {
  title: 'Edit Mailbox'
}

const EditMailboxPage:React.FC = () =>{
    return <EditMailboxForm/>
}

export default EditMailboxPage;