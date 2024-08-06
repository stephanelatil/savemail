'use client'

import { AppUser } from '@/models/appUser'
import { Credentials } from '@/models/credentials'
import {
  login as loginService,
  logout as logoutService,
  register as registerService,
} from '@/services/authenticationService'
import { getLoggedInUser } from '@/services/appUserService'
import { useState } from 'react'

export const useAuthentication = () => {
  const [loading, setLoading] = useState(false);

  const login = async (credentials:Credentials, rememberMe:boolean=false): Promise<boolean> => {
    try {
      setLoading(true);
      if (!!credentials.passwordRepeat)
        delete credentials.passwordRepeat;
      await loginService(credentials, rememberMe);
      return true;
    } catch (error: unknown) {
      if (error instanceof Error) {
        console.warn(error.message, 'error');
      }
      return false;
    } finally {
      setLoading(false);
    }
  }

  const logout = async (): Promise<void> => {
    try {
      setLoading(true);
      await logoutService();
    } catch (error: unknown) {
      alert('Logout failed: ' + error);
    } finally {
      setLoading(false);
    }
  }

  const register = async (creds: Credentials): Promise<boolean> => {
    try {
      setLoading(true);
      if (!!creds.passwordRepeat)
        delete creds.passwordRepeat;
      await registerService(creds);
      return true;
    } catch (error: unknown) {
      if (error instanceof Error) {
        alert(error.message);
      } else {
        alert('Could not register');
      }
    } finally {
      setLoading(false);
    }
    return false;
  }

  const updateLoggedInUser = async (): Promise<void> => {
    try {
      setLoading(true);
      const user: AppUser = await getLoggedInUser();
      // setLoggedInUser(user);
    } catch (error: unknown) {
      // setLoggedInUser(null);
      console.error(error + ': failed to fetch logged in user');
    } finally {
      setLoading(false);
    }
  }

  return {
    login,
    register,
    logout,
    loading,
    updateLoggedInUser,
  };
}
