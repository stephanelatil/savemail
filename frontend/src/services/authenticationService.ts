import { ChangePassword, Credentials, Edit2FA, PasswordReset } from '@/models/credentials';
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
        err.reduce((prev:string, curr:string, idx:number) => prev.concat(curr));
        throw new Error(err);
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
        err.reduce((prev:string, curr:string, idx:number) => prev.concat(curr));
        throw new Error(err);
    }

    return true;
}

export const changePassword = async (newPassword:ChangePassword) : Promise<boolean> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}manage/info`, 'POST', newPassword);

    if (!response.ok){
        const err = (await response.json()).errors;
        err.reduce((prev:string, curr:string, idx:number) => prev.concat(curr));
        throw new Error(err);
    }

    return true;
}

export const edit2FA = async (edit2FA:Edit2FA) : Promise<boolean> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}manage/2fa`, 'POST', edit2FA);

    if (!response.ok){
        const err = (await response.json()).errors;
        err.reduce((prev:string, curr:string, idx:number) => prev.concat(curr));
        throw new Error(err);
    }

    return true;
}