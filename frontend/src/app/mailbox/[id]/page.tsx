import { Metadata } from 'next';
import React from 'react';
import EditMailboxForm from "@/components/EditMailboxForm";
import Sidebar from '@/components/SideBar';

export const metadata: Metadata = {
  title: 'Edit Mailbox'
}

const EditMailboxPage:React.FC = () =>{
    return  <>
              <Sidebar/>
              <EditMailboxForm/>
            </>;
}

export default EditMailboxPage;