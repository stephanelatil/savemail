import { Metadata } from 'next';
import React from 'react';
import NewMailboxForm from "@/components/NewMailboxForm";
import Sidebar from '@/components/SideBar';
import { Stack } from '@mui/material';

export const metadata: Metadata = {
    title: 'New Mailbox'
}

const NewMailboxPage:React.FC = () =>{
    return  <Stack flexDirection='row'>
                <Sidebar />
                <NewMailboxForm />
            </Stack>
}

export default NewMailboxPage;