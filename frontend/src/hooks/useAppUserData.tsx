'use client'

import { getLoggedInUser } from '@/services/appUserService'
import { AppUser } from '@/models/appUser'
import { useState } from 'react'
import { useNotification } from './useNotification'
import { useRouter } from 'next/navigation'
import { LOGIN_URL } from '@/constants/NavRoutes'

export const useAppUserData = () => {
  const showNotification = useNotification();
  const [loading, setLoading] = useState(false);
  const router = useRouter();

  const getCurrentlyLoggedInUser = async (): Promise<AppUser|null> => {
    try {
      setLoading(true);
      return await getLoggedInUser();
    } catch (error: unknown) {
      if (error instanceof Error && error.message.startsWith('50'))
        // 500 error backend issue notify user
        showNotification("Backend issue: "+error.message, 'error');
      else //unable to get user: 401/403 thus probably not logged in
        router.push(LOGIN_URL)
      return null;
    } finally {
      setLoading(false);
    }
  }

  //TODO add other functions to manage user here (to be used on the settings page)

  return {
    getCurrentlyLoggedInUser
  };
}
