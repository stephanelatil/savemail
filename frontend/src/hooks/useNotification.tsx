import { NotificationContext } from '@/components/context/NotificationContext';
import { useContext } from 'react';


export const useNotification = () => {
  return useContext(NotificationContext);
};
