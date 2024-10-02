import { Metadata } from 'next';
import React from 'react';
import NewMailboxForm from "@/components/NewMailboxForm";
import Sidebar from '@/components/SideBar';

export const metadata: Metadata = {
    title: 'New Mailbox'
}

const NewMailboxPage:React.FC = () =>{
    return <>
                <Sidebar />
                <NewMailboxForm />
            </>
}

export default NewMailboxPage;