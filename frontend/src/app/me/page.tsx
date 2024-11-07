import Sidebar from '@/components/SideBar';
import UserSettings from '@/components/UserSettings';
import React from 'react';
import { Metadata } from 'next';
import { Stack } from '@mui/material';

export const metadata: Metadata = {
  title: 'Settings'
}

const UserSettingsPage: React.FC = () => {
  return  <Stack flexDirection='row'>
              <Sidebar />
              <UserSettings />
          </Stack>;
}

export default UserSettingsPage;