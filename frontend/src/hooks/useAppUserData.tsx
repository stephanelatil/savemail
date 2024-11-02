'use client'

import { getLoggedInUser, editUser as editUserService, deleteUser as deleteUserService } from '@/services/appUserService'
import { AppUser, EditAppUser } from '@/models/appUser'
import { useState } from 'react'
import { useNotification } from './useNotification'
import { useRouter } from 'next/navigation'
import { FetchError } from '@/services/fetchService'

export const useAppUserData = () => {
  const showNotification = useNotification();
  const [loading, setLoading] = useState(false);
  const router = useRouter();

  const getCurrentlyLoggedInUser = async (): Promise<AppUser|null> => {
    try {
      setLoading(true);
      return await getLoggedInUser();
    } catch (error: unknown) {
      if (error instanceof FetchError && error.statusCode >= 500)
        // 500 error backend issue notify user
        showNotification("Backend issue: "+error.message, 'error');
      else //unable to get user: 401/403 thus probably not logged in
        router.push('/auth/login');
      return null;
    } finally {
      setLoading(false);
    }
  }

  const editUser = async (user:EditAppUser): Promise<boolean> => {
    try{
      setLoading(true);
      return await editUserService(user);
    }
    catch(error){
      if (error instanceof FetchError)
      {
        if (error.statusCode == 500) // 500 error backend issue notify user
          showNotification("Backend issue: "+error.message, 'error');
        if (error.statusCode == 400)
          showNotification("Missing or invalid values: "+error.message, 'error');
        if (error.statusCode == 403)
          showNotification("Cannot edit another user's data: "+error.message, 'error');
        if (error.statusCode == 404)
          showNotification("Unknown user"+error.message, 'error');
      }
    }
    finally{
      setLoading(false);
    }
    return false;
  }

  const deleteUser = async (id:string) => {
    try{
      setLoading(true);
      return await deleteUserService(id);
    }
    catch(error){
      if (error instanceof FetchError)
      {
        if (error.statusCode == 500) // 500 error backend issue notify user
          showNotification("Backend issue: "+error.message, 'error');
        if (error.statusCode == 403)
          showNotification("Cannot edit another user's data: "+error.message, 'error');
        if (error.statusCode == 404)
          showNotification("Unknown user"+error.message, 'error');
      }
    }
    finally{
      setLoading(false);
    }
    return false;
  }

  return {
    loading,
    getCurrentlyLoggedInUser,
    editUser,
    deleteUser
  };
}
