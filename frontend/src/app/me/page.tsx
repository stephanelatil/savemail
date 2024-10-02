import Sidebar from '@/components/SideBar';
import UserSettings from '@/components/UserSettings';
import React from 'react';
import { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Settings'
}

const UserSettingsPage: React.FC = () => {
    return  <>
                <Sidebar />
                <UserSettings />
            </>;
}

export default UserSettingsPage;