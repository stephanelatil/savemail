'use client'

import { useState } from 'react'
import { useNotification } from './useNotification'
import { FetchError } from '@/services/fetchService'
import { Mail } from '@/models/mail'
import { getFolderMails } from '@/services/folderService'
import { PaginatedRequest } from '@/models/paginatedRequest'

export const useFolder = () => {
  const showNotification = useNotification();
  const [loading, setLoading] = useState(false);

  const getMails = async (folderId:number, pageNumber:number=1): Promise<PaginatedRequest<Mail>|null> => {
    try {
      setLoading(true);
      return await getFolderMails(folderId, pageNumber);
    } catch (error: unknown) {
      if (error instanceof FetchError){
        if (error.statusCode >= 500)
          showNotification("Backend issue: "+error.message, 'error');
        else if (error.statusCode == 401)
          showNotification(error.message, 'error');
      }
      return null;
    } finally {
      setLoading(false);
    }
  }

  return {
    loading,
    getMails
  };
}
