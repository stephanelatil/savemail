'use client'

import { AppUser } from '@/models/appUser';
import { getLoggedInUser } from '@/services/appUserService';
import { useRouter } from 'next/router';
import { useEffect, useState } from 'react'

export const useLoginRedirect = () => {
  const [loading, setLoading] = useState(false);
  const router = useRouter();

  useEffect(() => {
    const redirectIfNotLoggedIn = async () : Promise<void> => {
      if (router.pathname !== '/auth/login' && router.pathname !== '/auth/register')
        return; //on login/or register page: ok

      try{
        setLoading(true);
        const user:AppUser = await getLoggedInUser();
        if (!!user.email)
          return; //logged in: ok
      }catch{} 
      //not logged in or invalid session cook
      finally{
        setLoading(false);
      }

      // If the user is not logged in
      router.push('/auth/login');
    };
    redirectIfNotLoggedIn();
    });
}