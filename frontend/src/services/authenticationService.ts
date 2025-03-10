import { ChangePassword, Credentials, Init2FA, Enable2FA, PasswordReset, Response2FA, EmailConfirmed } from '@/models/credentials';
import { FetchError as Error, apiFetchWithBody } from './fetchService';
import { get_frontend_url } from '@/constants/Envs';

const AUTH_ENDPOINT = '/api/auth/';

/**
 * Registers a new user
 * @param credentials The user data used for registering. All two factor fields will be ignored for registration
 * @returns true if successfully registered
 * @throws FetchError
 */
export const register = async (credentials: Credentials) : Promise<boolean> => {
    if (credentials.twoFactorCode)
        delete credentials.twoFactorCode;
    if (credentials.twoFactorRecoveryCode)
        delete credentials.twoFactorRecoveryCode;
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}register`, 'POST', credentials);

    if (response.status == 500)
    {
        const err = await response.json();
        console.error(err.message);
        throw new Error(err.message, 500)
    }
    else if (!response.ok) {
        const err = (await response.json()).errors;
        var errString = '';
        Object.values(err).forEach((val, index, arr)=>errString=errString.concat(val+'\n'));
        errString.trim()
        console.error(err);
        throw new Error(errString, response.status);
    }

    return true;
}

/**
 * Logs a user in
 * @param credentials The credentials to be used for logging in. 
 * @param rememberMe Whether to use session cookies only (if false) or store in regular longer-term cookies (if true)
 * @returns true if successful
 * @throws FetchError on invalid credentials
 */
export const login = async (credentials: Credentials, rememberMe:boolean=false): Promise<boolean> => {
    const remember:string = rememberMe ? "useCookies=true" : "useSessionCookies=true";
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}login?${remember}`, 'POST', credentials);

    if (response.status == 500)
    {
        const err = await response.json();
        console.error(err.message);
        throw new Error(err.message, 500)
    }
    else if (!response.ok) {
        if (credentials.twoFactorCode || credentials.twoFactorRecoveryCode)
            throw new Error("Incorrect username, password or 2FA", response.status);
        throw new Error("Incorrect username or password", response.status);
    }

    return true;
}

/**
 * Logs a user out. Has no effect if user is not logged in
 * @returns null (always)
 */
export const logout = async (): Promise<null> => {
    await apiFetchWithBody(`${AUTH_ENDPOINT}logout`, 'POST');
    return null;
}

/**
 * Sends an email only if the address is valid for a registered user
 * NOTE: Requires an email sending service like SendGrid!
 * @param email The email address of the user
 * @returns true (always)
 */
export const resendConfirmationEmail = async (email:string) : Promise<boolean> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}resendConfirmationEmail`, 'POST', {"email":email})

    return true;
}

/**
 * Generates an email that contains a password reset code. Send that code to /resetPassword with a new password.
 * NOTE: Requires an email sending service like SendGrid!
 * @param email The user email address
 * @returns true (always)
 */
export const sendPasswordReset = async (email:string) : Promise<boolean> => {
    const response = await apiFetchWithBody(`/api/AppUser/ForgotPassword`, 'POST',
                                            {
                                                "email":email,
                                                "redirectTo":`${get_frontend_url()}/auth/reset`
                                            });

    return true;
}

/**
 * Call this endpoint after getting a reset code by calling the /forgotPassword endpoint.
 * @param reset The email,, new password, and resetCode gotten from the /forgotPassword endpoint.
 * @returns true (always)
 * @throws FetchError if the reset code is invalid or expired, if the email is invalid, or if the password does not meet requirements
 */
export const passwordReset = async (reset:PasswordReset) : Promise<boolean> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}resetPassword`, 'POST', reset);

    if (response.status == 500)
    {
        const err = await response.json();
        console.error(err.message);
        throw new Error(err.message, 500)
    }
    else if (!response.ok){
        const err = (await response.json()).errors;
        var errString = '';
        Object.values(err).forEach((val,index, arr)=>errString=errString.concat(val+'\n'));
        errString.trim()
        throw new Error(errString, response.status);
    }

    return true;
}

/**
 * Updates the password of the logged-in user.
 * @param newPassword The old and new passwords
 * @returns true (always)
 * @throws FetchError if the current password is incorrect or if the new password does not meet requirements
 */
export const changePassword = async (newPassword:ChangePassword) : Promise<boolean> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}manage/info`, 'POST', newPassword);

    if (response.status == 500)
    {
        const err = await response.json();
        console.error(err.message);
        throw new Error(err.message, 500)
    }
    else if (!response.ok){
        const err = (await response.json()).errors;
        var errString = '';
        Object.values(err).forEach((val,index, arr)=>errString=errString.concat(val+'\n'));
        errString.trim()
        throw new Error(errString, response.status);
    }

    return true;
}

/**
 * Updates the email address
 * @param email The new email address to use (should not be used by another user already)
 * @returns true (always)
 * @throws FetchError if the new email is already in use
 */
export const changeAccountEmail = async (email:string) : Promise<boolean> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}manage/info`, 'POST', {email:email});

    if (response.status == 500)
    {
        const err = await response.json();
        console.error(err.message);
        throw new Error(err.message, 500)
    }
    else if (!response.ok){
        const err = (await response.json()).errors;
        var errString = '';
        Object.values(err).forEach((val,index, arr)=>errString=errString.concat(val+'\n'));
        errString.trim()
        throw new Error(errString, response.status);
    }

    return true;
}

/**
 * Checks if the current account email address is confirmed
 * @returns The currently used email address and a boolean stating if it is confirmed
 */
export const emailIsConfirmed = async () : Promise<EmailConfirmed> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}manage/info`, 'GET');

    if (response.status == 500)
    {
        const err = await response.json();
        console.error(err.message);
        throw new Error(err.message, 500)
    }
    else if (!response.ok){
        const err = (await response.json()).errors;
        var errString = '';
        Object.values(err).forEach((val,index, arr)=>errString=errString.concat(val+'\n'));
        errString.trim()
        throw new Error(errString, response.status);
    }

    return response.json();
}

/**
 * Initializes setting up the 2fa process. Returns a shared key to be used to setup the user's TOTP provider
 * @param initial It is intentionally empty according to the IdentityAPI docs https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-8.0#use-the-post-manage2fa-endpoint
 * @returns The response body provides the sharedKey along with some other properties that aren't needed at this point. The shared key is used to set up the authenticator app
 */
export const init2FA = async (initial:Init2FA) : Promise<Response2FA> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}manage/2fa`, 'POST', initial);

    if (response.status == 500)
    {
        const err = await response.json();
        console.error(err.message);
        throw new Error(err.message, 500)
    }
    else if (!response.ok){
        const err = (await response.json()).errors;
        var errString = '';
        Object.values(err).forEach((val,index, arr)=>errString=errString.concat(val+'\n'));
        errString.trim()
        throw new Error(errString, response.status);
    }

    return response.json();
}

/**
 * Use the shared key to get a Time-based one-time password (TOTP). Send this one time password (with enable:true) to enable 2fa
 * @param edit2FA The TOTP and whether to enable 2fa
 * @returns The response body confirms that IsTwoFactorEnabled is true and provides the RecoveryCodes. The recovery codes are used to log in when the authenticator app isn't available
 */
export const enable2FA = async (edit2FA:Enable2FA) : Promise<Response2FA> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}manage/2fa`, 'POST', edit2FA);

    if (response.status == 500)
    {
        const err = await response.json();
        console.error(err.message);
        throw new Error(err.message, 500)
    }
    else if (!response.ok){
        const err = (await response.json()).errors;
        var errString = '';
        Object.values(err).forEach((val,index, arr)=>errString=errString.concat(val+'\n'));
        errString.trim()
        throw new Error(errString, response.status);
    }

    return response.json();
}

/**
 * Regenerates 2FA codes for the user (invalidates others)
 * @returns The response body confirms that IsTwoFactorEnabled is true and returns a list with the new recovery codes
 */
export const reset2FARecoveryCodes = async () : Promise<Response2FA> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}manage/2fa`, 'POST', {resetRecoveryCodes:true});

    if (response.status == 500)
    {
        const err = await response.json();
        console.error(err.message);
        throw new Error(err.message, 500)
    }
    else if (!response.ok){
        const err = (await response.json()).errors;
        var errString = '';
        Object.values(err).forEach((val,index, arr)=>errString=errString.concat(val+'\n'));
        errString.trim()
        throw new Error(errString, response.status);
    }

    return response.json();
}

/**
 * Disables 2FA if it is enabled
 * @returns The response body confirms that IsTwoFactorEnabled is false. Ignore the other fields
 */
export const disable2FA = async () : Promise<Response2FA> => {
    const response = await apiFetchWithBody(`${AUTH_ENDPOINT}manage/2fa`, 'POST', {resetSharedKey:true});

    if (response.status == 500)
    {
        const err = await response.json();
        console.error(err.message);
        throw new Error(err.message, 500)
    }
    else if (!response.ok){
        const err = (await response.json()).errors;
        var errString = '';
        Object.values(err).forEach((val,index, arr)=>errString=errString.concat(val+'\n'));
        errString.trim()
        throw new Error(errString, response.status);
    }

    return response.json();
}
