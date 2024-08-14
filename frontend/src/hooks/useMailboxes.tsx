'use client'

import { getAllMailboxes } from '@/services/mailboxService'
import { useState } from 'react'
import { useNotification } from './useNotification'
import { useRouter } from 'next/navigation'
import { MailBox } from '@/models/mailBox'
import { FetchError } from '@/services/fetchService'

export const useMailboxes = () => {
  const showNotification = useNotification();
  const [loading, setLoading] = useState(false);
  const router = useRouter();

  const getMailboxList = async (): Promise<MailBox[]> => {
    try {
      setLoading(true);
      return await getAllMailboxes();
    } catch (error: unknown) {
      if (error instanceof FetchError){
        if (error.statusCode >= 500)
          showNotification("Backend issue: "+error.message, 'error');

        else if (error.statusCode == 401)
          showNotification(error.message, 'error');
      }
      return [];
    } finally {
      setLoading(false);
    }
  }

  return {
    loading,
    getMailboxList
  };
}
