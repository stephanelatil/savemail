import UserSettings from '@/components/UserSettings';
import { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Settings'
}

const UserSettingsPage: React.FC = () => {
    return <UserSettings />;
}

export default UserSettingsPage;