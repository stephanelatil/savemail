export interface Credentials {
    email: string,
    password: string,
    passwordRepeat?:string,
    twoFactorCode?:string,
    twoFactorRecoveryCode?: string
}

export interface PasswordReset{
    email:string,
    resetCode: string,
    newPassword: string,
    newPasswordRepeat?:string
}

export interface ChangePassword{
    newPassword:string,
    oldPassword:string
}

export interface Init2FA{}

export interface Enable2FA{
    enable: boolean,
    twoFactorCode?: string
}

export interface Response2FA{
    sharedKey:string,
    recoveryCodesLeft:number
    recoveryCodes:string[],
    isTwoFactorEnabled:boolean,
    isMachineRemembered:boolean
}

export interface EmailConfirmed {
    email:string,
    isEmailConfirmed:boolean
}