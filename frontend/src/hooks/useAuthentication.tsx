'use client'

import { ChangePassword, Credentials, PasswordReset, Response2FA } from '@/models/credentials'
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
  reset2FARecoveryCodes as reset2FARecoveryCodesService,
  sendPasswordReset as sendPasswordResetService,
  passwordReset as passwordResetService
} from '@/services/authenticationService'
import { useState } from 'react'
import { useNotification } from './useNotification'

export const useAuthentication = () => {
  const {showNotification} = useNotification();
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
      setLoading(true);
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
      setLoading(true);
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

  /**
   * First step to the flow enabling 2FA 
   * @returns true/false depending if the action was successful
   */
  const init2FA = async ():Promise<Response2FA> => {
    try{
      setLoading(true);
      let result = await init2FAService({});
      return result;
    }catch (error){
      if (error instanceof Error) {
        showNotification("Unable to initialize 2FA flow: \n"+ error.message, 'error');
      }
    } finally {
      setLoading(false);
    }
    return {
        sharedKey:'',
        recoveryCodesLeft:-1,
        recoveryCodes:[],
        isTwoFactorEnabled:false,
        isMachineRemembered:false };
  }

  /**
   * First step to the flow enabling 2FA 
   * @returns true/false depending if the action was successful
   */
  const enable2FA = async (totp:string):Promise<Response2FA> => {
    try{
      setLoading(true);
      let result = await enable2FAService({enable:true, twoFactorCode:totp});
      return result;
    }catch (error){
      if (error instanceof Error) {
        showNotification("Unable to enable 2FA: "+ error.message, 'error');
      }
    } finally {
      setLoading(false);
    }
    return {
        sharedKey:'',
        recoveryCodesLeft:-1,
        recoveryCodes:[],
        isTwoFactorEnabled:false,
        isMachineRemembered:false };
  }

  const reset2FARecoveryCodes = async (): Promise<boolean> =>{
    try{
      setLoading(true);
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

  const sendPasswordReset = async (email:string): Promise<boolean> =>{
    try{
      setLoading(true);
      await sendPasswordResetService(email);
      return true;
    }catch (error){
      if (error instanceof Error) {
        showNotification("Unable to send password rest: "+ error.message, 'warning');
      }
    } finally {
      setLoading(false);
    }
    return false;
  }

  const changeAccountEmail = async (newEmail:string) :Promise<boolean> => {
    try{
      setLoading(true);
      await changeAccountEmailService(newEmail);
      return true;
    }catch (error){
      if (error instanceof Error) {
        showNotification("Unable change email (the old one is still in use): \n"+ error.message, 'warning');
      }
    } finally {
      setLoading(false);
    }
    return false;
  }

  const changePassword = async (newPassword:ChangePassword) :Promise<boolean> => {
    try{
      setLoading(true);
      await changePasswordService(newPassword);
      return true;
    }catch (error){
      if (error instanceof Error) {
        showNotification("Unable change email (the old one is still in use): \n"+ error.message, 'warning');
      }
    } finally {
      setLoading(false);
    }
    return false;
  }

  const resetPassword = async (reset:PasswordReset) :Promise<boolean> => {
    try{
      setLoading(true);
      await passwordResetService(reset);
      return true;
    }catch (error){
      if (error instanceof Error) {
        showNotification("Unable reset password:\n"+ error.message, 'warning');
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
    reset2FARecoveryCodes,
    disable2FA,
    init2FA,
    enable2FA,
    changeAccountEmail,
    changePassword,
    sendPasswordReset,
    resetPassword,
    loading
  };
}
