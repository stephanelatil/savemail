import { ChangePassword, Credentials, Init2FA, Enable2FA, PasswordReset, Response2FA } from '@/models/credentials';
import { apiFetch, apiFetchWithBody } from './fetchService';

const AUTH_ENDPOINT = '/api/auth/';

export const register = async (credentials: Credentials) : Promise<boolean> => {
    if (credentials.twoFactorCode)
        delete credentials.twoFactorCode;
    if (credentials.twoFactorRecoveryCode)
        delete credentials.twoFactorRecoveryCode;
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}register`, 'POST', credentials);

    if (!response.ok) {
        const err = (await response.json()).errors;
        var errString = '';
        Object.values(err).forEach((val,index, arr)=>errString=errString.concat(val+'\n'));
        errString.trim()
        throw new Error(errString);
    }

    return true;
}

export const login = async (credentials: Credentials, rememberMe:boolean=false): Promise<boolean> => {
    const remember:string = rememberMe ? "useCookies=true" : "useSessionCookies=true";
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}login?${remember}`, 'POST', credentials);

    if (!response.ok) {
        if (credentials.twoFactorCode || credentials.twoFactorRecoveryCode)
            throw new Error("Incorrect username, password or 2FA");
        throw new Error("Incorrect username or password");
    }

    return true;
}

export const logout = async (): Promise<null> => {
    await apiFetchWithBody(`${AUTH_ENDPOINT}logout`, 'POST');
    return null;
}

export const resendConfirmationEmail = async (email:string) : Promise<boolean> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}resendConfirmationEmail`, 'POST', {"email":email})

    return true;
}

export const sendPasswordReset = async (email:string) : Promise<boolean> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}forgotPassword`, 'POST', {"email":email})

    return true;
}

export const passwordReset = async (reset:PasswordReset) : Promise<boolean> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}resetPassword`, 'POST', reset);

    if (!response.ok){
        const err = (await response.json()).errors;
        var errString = '';
        Object.values(err).forEach((val,index, arr)=>errString=errString.concat(val+'\n'));
        errString.trim()
        throw new Error(errString);
    }

    return true;
}

export const changePassword = async (newPassword:ChangePassword) : Promise<boolean> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}manage/info`, 'POST', newPassword);

    if (!response.ok){
        const err = (await response.json()).errors;
        var errString = '';
        Object.values(err).forEach((val,index, arr)=>errString=errString.concat(val+'\n'));
        errString.trim()
        throw new Error(errString);
    }

    return true;
}

export const init2FA = async (initial:Init2FA) : Promise<Response2FA> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}manage/2fa`, 'POST', initial);

    if (!response.ok){
        const err = (await response.json()).errors;
        var errString = '';
        Object.values(err).forEach((val,index, arr)=>errString=errString.concat(val+'\n'));
        errString.trim()
        throw new Error(errString);
    }

    return response.json();
}

export const enable2FA = async (edit2FA:Enable2FA) : Promise<Response2FA> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}manage/2fa`, 'POST', edit2FA);

    if (!response.ok){
        const err = (await response.json()).errors;
        var errString = '';
        Object.values(err).forEach((val,index, arr)=>errString=errString.concat(val+'\n'));
        errString.trim()
        throw new Error(errString);
    }

    return response.json();
}
