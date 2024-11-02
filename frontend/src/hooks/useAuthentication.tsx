'use client'

import { Credentials } from '@/models/credentials'
import {
  login as loginService,
  logout as logoutService,
  register as registerService,
  resendConfirmationEmail as resendConfirmationEmailService,
  changeAccountEmail as changeAccountEmailService,
  changePassword as changePasswordService,
  disable2FA as disable2FAService,
  enable2FA as enable2FAService,
  init2FA as init2FAService,
  reset2FARecoveryCodes as reset2FARecoveryCodesService
} from '@/services/authenticationService'
import { useState } from 'react'
import { useNotification } from './useNotification'

export const useAuthentication = () => {
  const showNotification = useNotification();
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
        showNotification(error.message, 'error');
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
      if (error instanceof Error) {
        console.warn(error.message, 'error');
        showNotification(error.message, 'error');
      }
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
        showNotification(error.message, 'error');
      } else {
        showNotification('Could not register', 'error');
      }
    } finally {
      setLoading(false);
    }
    return false;
  }

  const resendConfirmationEmail = async (email:string):Promise<void> => {
    try{
      await resendConfirmationEmailService(email);
    }catch (error){
      if (error instanceof Error) {
        showNotification("Unable to send email", 'warning');
      }
    } finally {
      setLoading(false);
    }
  }

  /**
   * Disabled 2FA for the user
   * @returns true/false depending if the action was successful
   */
  const disable2FA = async ():Promise<boolean> => {
    try{
      await disable2FAService();
      return true;
    }catch (error){
      if (error instanceof Error) {
        showNotification("Unable to disable 2FA: "+ error.message, 'warning');
      }
    } finally {
      setLoading(false);
    }
    return false;
  }

  const reset2FARecoveryCodes = async (): Promise<boolean> =>{
    try{
      await reset2FARecoveryCodesService();
      return true;
    }catch (error){
      if (error instanceof Error) {
        showNotification("Unable to reset codes: "+ error.message+"\nYour old ones are still valid", 'warning');
      }
    } finally {
      setLoading(false);
    }
    return false;
  }

  return {
    login,
    register,
    logout,
    resendConfirmationEmail,
    disable2FA,
    loading
  };
}
