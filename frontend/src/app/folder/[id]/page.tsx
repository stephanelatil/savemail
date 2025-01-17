import MailListPage2 from '@/components/MailList';
import Sidebar from '@/components/SideBar';
import { Stack } from '@mui/material';
import { Metadata } from 'next';
import React from 'react';

export const metadata: Metadata = {
  title: 'View Emails'
}

const MailsInFolder:React.FC = () =>{
        return (
          <Stack flexDirection='row'>
            <Sidebar />
            <MailListPage2 />
          </Stack>);
}

export default MailsInFolder;