'use client'

import { getMail as getMailService, deleteMail as deleteMailService } from '@/services/mailService'
import { useState } from 'react'
import { useNotification } from './useNotification'
import { FetchError } from '@/services/fetchService'
import { Mail } from '@/models/mail'

export const useMails = () => {
  const showNotification = useNotification();
  const [loading, setLoading] = useState(false);

  const getMail = async (id:number): Promise<Mail|null> => {
    try {
      setLoading(true);
      return await getMailService(id);
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

  const deleteMail = async (id:number):Promise<boolean> => {
    try{
      setLoading(true);
      await deleteMailService(id);
      return true;
    } catch(error: unknown) {
      if (error instanceof FetchError){
        if (error.statusCode >= 500)
          showNotification("Backend issue: "+error.message, 'error');
        else if (error.statusCode == 404)
          showNotification("Mail not found. Maybe it has already been deleted", 'error');
        else if (error.statusCode == 401 || error.statusCode == 403)
          showNotification('Forbidden! You are not the owner of this email', 'warning');
        else{
          showNotification("Unexpected error occurred", 'error');
          console.error(error);
        }
      }
      else{
        showNotification("Unexpected error occurred", 'error');
        console.error(error);
      }
    } finally{
      setLoading(false);
    }
    return false;
  }

  return {
    loading,
    getMail,
    deleteMail
  };
}
