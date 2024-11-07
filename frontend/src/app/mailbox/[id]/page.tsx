import { Metadata } from 'next';
import React from 'react';
import EditMailboxForm from "@/components/EditMailboxForm";
import Sidebar from '@/components/SideBar';
import { Stack } from '@mui/material';

export const metadata: Metadata = {
  title: 'Edit Mailbox'
}

const EditMailboxPage:React.FC = () =>{
    return  <Stack flexDirection='row'>
              <Sidebar/>
              <EditMailboxForm/>
            </Stack>;
}

export default EditMailboxPage;